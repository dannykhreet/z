using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Classes;

public static class QueueManager<T> where T : class, IQueueableItem
{
    private static readonly ConcurrentQueue<T> ItemsQueue;
    private static readonly SemaphoreSlim fileSemaphore = new SemaphoreSlim(1, 1);
    private static readonly string fileName = "ItemsQueue_" + typeof(T).Name + ".json";

    static QueueManager()
    {
        ItemsQueue = new ConcurrentQueue<T>();
        LoadItemsFromFile();
    }

    public static ConcurrentQueue<T> GetQueue()
    {
        return ItemsQueue;
    }

    // Enqueue checklist items to the queue
    public static async Task EnqueueItemAsync(T item)
    {
        ItemsQueue.Enqueue(item);
        await SerializeQueueToFileAsync();
    }

    public static bool HasItems()
    {
        return !ItemsQueue.IsEmpty;
    }

    public static bool Contains(IQueueableItem item)
    {
        return ItemsQueue.Any(x => x.LocalGuid == item.LocalGuid);
    }

    public static async Task<T> DequeueItemAsync()
    {
        T dequeuedItem = null;
        bool result = ItemsQueue.TryDequeue(out dequeuedItem);
        if (result)
            await SerializeQueueToFileAsync();

        return dequeuedItem;
    }

    public static T PeekItem()
    {
        T dequeuedItem = null;
        ItemsQueue.TryPeek(out dequeuedItem);
        return dequeuedItem;
    }

    // Serialize the entire queue to the file directly
    private static async Task SerializeQueueToFileAsync()
    {
        await fileSemaphore.WaitAsync();
        try
        {
            // Snapshot the current queue items to ensure no modifications during serialization
            var queueSnapshot = ItemsQueue.ToList();

            // Write the queue snapshot to a file 
            await SaveToFileAsync(queueSnapshot);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
        }
        finally
        {
            fileSemaphore.Release();
        }
    }

    // Load saved items from file into the queue 
    public static async Task<string> GetItemsFromFile()
    {
        await fileSemaphore.WaitAsync();
        try
        {
            using var scope = App.Container.CreateScope();
            var _fileService = scope.ServiceProvider.GetService<IFileService>();
            string itemsJson = await _fileService.ReadFromInternalStorageAsync(fileName, Constants.PersistentDataDirectory);

            if (string.IsNullOrWhiteSpace(itemsJson))
                return null;

            return itemsJson;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
            return null;
        }
        finally
        {
            fileSemaphore.Release();
        }
    }

    // Load saved items from file into the queue 
    private static void LoadItemsFromFile()
    {
        fileSemaphore.Wait();
        try
        {
            using var scope = App.Container.CreateScope();
            var _fileService = scope.ServiceProvider.GetService<IFileService>();
            string itemsJson = _fileService.ReadFromInternalStorage(fileName, Constants.PersistentDataDirectory);

            if (string.IsNullOrWhiteSpace(itemsJson))
                return;

            var savedItems = JsonSerializer.Deserialize<List<T>>(itemsJson) ?? new List<T>();

            // Enqueue each item into the queue
            foreach (var item in savedItems)
            {
                ItemsQueue.Enqueue(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }
        finally
        {
            fileSemaphore.Release();
        }
    }

    // Helper method to save data to a file 
    private static async Task SaveToFileAsync(List<T> items)
    {
        string itemsJson = JsonSerializer.Serialize(items);

        using var scope = App.Container.CreateScope();
        var _fileService = scope.ServiceProvider.GetService<IFileService>();
        await _fileService.SaveFileToInternalStorageAsync(itemsJson, fileName, Constants.PersistentDataDirectory);
    }
}
