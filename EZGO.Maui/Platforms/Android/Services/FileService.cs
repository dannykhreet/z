using System;
using Android.Content.Res;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Platforms.Android.Services;
using static System.Environment;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class FileService : IFileService
    {
        public void ClearInternalStorageFolder(string directoryName)
        {
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);

            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        public async Task<FileInfo[]> GetFilesFromAssetsAsync(string directoryName)
        {
            FileInfo[] fileInfos = null;

            using (AssetManager assets = MainActivity.CurrentActivity.ApplicationContext.Assets)
            {
                string[] files = await assets.ListAsync(directoryName);

                if (files != null && files.Any())
                    fileInfos = files.Select(item => new FileInfo(item)).OrderBy(item => item.Name).ToArray();
            }

            return fileInfos;
        }

        public async Task<byte[]> ReadFromAssetsAsBytesAsync(string filename)
        {
            byte[] file;

            using (AssetManager assets = MainActivity.CurrentActivity.ApplicationContext.Assets)
            using (MemoryStream memoryStream = new MemoryStream())
            using (Stream stream = assets.Open(filename))
            {
                await stream.CopyToAsync(memoryStream);
                file = memoryStream.ToArray();
            }

            return file;
        }

        public FileInfo[] GetFilesFromInternalStorage(string directoryName)
        {
            FileInfo[] fileInfos = null;
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);

            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder);

                if (files != null && files.Any())
                    fileInfos = files.Select(item => new FileInfo(item)).OrderBy(item => item.Name).ToArray();
            }

            return fileInfos;
        }

        public async Task<string> ReadFromInternalStorageAsync(string filename, string directoryName)
        {
            string fileContents = string.Empty;
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, filename);

            if (newFile.Exists())
                fileContents = await File.ReadAllTextAsync(newFile.Path).ConfigureAwait(false);

            return fileContents;
        }

        public string ReadFromInternalStorage(string filename, string directoryName)
        {
            string fileContents = string.Empty;
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, filename);

            if (newFile.Exists())
                fileContents = File.ReadAllText(newFile.Path);

            return fileContents;
        }

        public async Task<Stream> ReadFromInternalStorageAsBytesAsync(string filename, string directoryName)
        {
            Stream fileContents = null;
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, filename);

            if (newFile.Exists())
                fileContents = File.OpenRead(newFile.Path);

            return fileContents;
        }

        public string SaveFileToInternalStorage(byte[] file, string fileName, string directoryName)
        {
            // Save files to internal storage; no other user or apps can access these files.
            // Unlike the external storage directories, your app does not require any system permissions to read and write to the internal directories returned by these methods.
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, fileName);

            if (!newFile.Exists())
                newFile.ParentFile?.Mkdirs();

            File.WriteAllBytes(newFile.Path, file);

            return newFile.Path;
        }

        public async Task<string> SaveFileToInternalStorageAsync(byte[] file, string fileName, string directoryName)
        {
            // Save files to internal storage; no other user or apps can access these files.
            // Unlike the external storage directories, your app does not require any system permissions to read and write to the internal directories returned by these methods.
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, fileName);

            if (!newFile.Exists())
                newFile.ParentFile?.Mkdirs();

            await File.WriteAllBytesAsync(newFile.Path, file);

            return newFile.Path;
        }

        public async Task<string> SaveFileToInternalStorageAsync(string fileContents, string fileName, string directoryName)
        {
            // Save files to internal storage; no other user or apps can access these files.
            // Unlike the external storage directories, your app does not require any system permissions to read and write to the internal directories returned by these methods.
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, fileName);

            if (!newFile.Exists())
                newFile.ParentFile?.Mkdirs();

            await File.WriteAllTextAsync(newFile.Path, fileContents).ConfigureAwait(false);

            return newFile.Path;
        }

        public async Task<Stream> ReadFromInternalStorageAsStreamAsync(string filename, string directoryName)
        {
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, filename);

            if (newFile.Exists())
            {
                var fileContents = new FileStream(newFile.Path, FileMode.Open);
                if (fileContents != null)
                {
                    var memoryStream = new MemoryStream();
                    await fileContents.CopyToAsync(memoryStream);

                    fileContents.Close();
                    fileContents.Dispose();

                    return memoryStream;
                }
            }

            return null;
        }

        public bool CheckIfFileExists(string filename, string directoryName)
        {
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, filename);

            return newFile.Exists();
        }

        public void DeleteFile(string filename, string directoryName)
        {
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
            Java.IO.File newFile = new Java.IO.File(folder, filename);

            if (newFile.Exists())
                newFile.Delete();
        }

        public void DeleteFile(string path)
        {
            Java.IO.File newFile = new Java.IO.File(path);

            if (newFile.Exists())
                newFile.Delete();
        }

        public async Task IsFileReady(string filename, string directoryName)
        {
            await Task.Run(() =>
            {
                string folder = Path.Combine(FileSystem.Current.AppDataDirectory, directoryName);
                Java.IO.File newFile = new Java.IO.File(folder, filename);

                if (!newFile.Exists())
                {
                    throw new IOException("File does not exist!");
                }

                var isReady = false;

                while (!isReady)
                {
                    // If the file can be opened for exclusive access it means that the file
                    // is no longer locked by another process.
                    try
                    {
                        using (var fileContents = new FileStream(newFile.Path, FileMode.Open))
                            isReady = fileContents.Length > 0;
                    }
                    catch (Exception e)
                    {
                        // Check if the exception is related to an IO error.
                        if (e.GetType() == typeof(IOException))
                        {
                            isReady = false;
                        }
                        else
                        {
                            // Rethrow the exception as it's not an exclusively-opened-exception.
                            throw;
                        }
                    }
                }
            });
        }

        public string GetAssetsPath()
        {
            return "file:///android_asset/";
        }
    }
}

