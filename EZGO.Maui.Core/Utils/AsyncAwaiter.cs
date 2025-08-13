using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Utils
{
    /// <summary>
    /// Provides the ability to safely await tasks that need limited thread access.
    /// </summary>
    public static class AsyncAwaiter
    {
        #region Private Members 

        /// <summary>
        /// Self lock to keep the internal dictionary of semaphores safe.
        /// </summary>
        private static readonly SemaphoreSlim SelfLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Dictionary of all created semaphores
        /// </summary>
        /// <remarks>Semaphores are not deleted after they have been used.</remarks>
        private static readonly Dictionary<string, SemaphoreSlim> Semaphores = new Dictionary<string, SemaphoreSlim>();

        #endregion

        /// <summary>
        /// Awaits for any tasks to complete that are accessing the same key then runs the given task and returns it's value.
        /// </summary>
        /// <typeparam name="T">Return type of the awaited task.</typeparam>
        /// <param name="key">The key to await on.</param>
        /// <param name="task">The task to perform with limited thread access.</param>
        /// <param name="maxAccessCount">Max number of thread that can run the task concurrently. 
        /// <para>Only used if it's the first call with the given key.</para></param>
        /// <returns>An awaitable task.</returns>
        public static async Task<T> AwaitResultAsync<T>(string key, Func<Task<T>> task, int maxAccessCount = 1)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key cannot be null or empty.", nameof(key));

            if (task == null)
                throw new ArgumentNullException("The task cannot be null.", nameof(task));

            await CreateSemaphoreIfNotExistsAsync(key, maxAccessCount);

            // Get the semaphore for the given key
            var taskSemaphore = Semaphores[key];

            // Wait for the access
            await taskSemaphore.WaitAsync();

            try
            {
                // Do the given job and return the result of the task
                return await task().ConfigureAwait(false);
            }
            // Optionally catch the exception
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Crash in {nameof(AsyncAwaiter)}!\r\n{ex}", nameof(AsyncAwaiter));
                Debug.WriteLine("Key", key);
                Debugger.Break();
#endif
                // Throw as usual
                throw ex;
            }
            finally
            {
                // Release the task semaphore
                taskSemaphore.Release();
            }

        }

        /// <summary>
        /// Awaits for any tasks to complete that are accessing the same key then runs the given task.
        /// </summary>
        /// <param name="key">The key to await on.</param>
        /// <param name="task">The task to perform with limited thread access.</param>
        /// <param name="maxAccessCount">Max number of thread that can run the task concurrently. 
        /// <para>Only used if it's the first call with the given key.</para></param>
        /// <returns>An awaitable task.</returns>
        public static async Task AwaitAsync(string key, Func<Task> task, int maxAccessCount = 1)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key cannot be null or empty.", nameof(key));

            if (task == null)
                throw new ArgumentNullException("The task cannot be null.", nameof(task));

            #region Create Semaphore

            await CreateSemaphoreIfNotExistsAsync(key, maxAccessCount);

            #endregion

            // Get the semaphore for the given key
            var taskSemaphore = Semaphores[key];

            // Wait for the access
            await taskSemaphore.WaitAsync();

            try
            {
                // Do the given job and return the result of the task
                await task().ConfigureAwait(false);
            }
            // Optionally catch the exception
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Crash in {nameof(AsyncAwaiter)}.\r\n{ex}", nameof(AsyncAwaiter));
                Debugger.Break();
#endif
                // Throw as usual
                throw ex;

            }
            finally
            {
                // Release the task semaphore
                taskSemaphore.Release();
            }
        }

        public static async Task ExecuteIfPossibleAsync(string key, Func<Task> task, int maxAccessCount = 1)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key cannot be null or empty.", nameof(key));

            if (task == null)
                throw new ArgumentNullException("The task cannot be null.", nameof(task));

            await CreateSemaphoreIfNotExistsAsync(key, maxAccessCount);

            // Get the semaphore for the given key
            var taskSemaphore = Semaphores[key];

            // Try to enter immediately 
            var entered = taskSemaphore.Wait(0);

            // If managed to enter
            if (entered)
            {
                try
                {
                    // Do the given job and return the result of the task
                    await task().ConfigureAwait(false);
                }
                // Optionally catch the exception
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine($"Crash in {nameof(AsyncAwaiter)}!\r\n{ex}", nameof(AsyncAwaiter));
                    Debugger.Break();
#endif
                    // Throw as usual
                    throw ex;
                }
                finally
                {
                    // Release the task semaphore
                    taskSemaphore.Release();
                }
            }

            // Otherwise don't do the task at all
        }

        public static async Task ExecuteIfPossibleAsync(string key, Action task, int maxAccessCount = 1)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key cannot be null or empty.", nameof(key));

            if (task == null)
                throw new ArgumentNullException("The task cannot be null.", nameof(task));

            await CreateSemaphoreIfNotExistsAsync(key, maxAccessCount);

            // Get the semaphore for the given key
            var taskSemaphore = Semaphores[key];

            // Try to enter immediately 
            var entered = taskSemaphore.Wait(0);

            // If managed to enter
            if (entered)
            {
                try
                {
                    // Do the given job
                    task();
                }
                // Optionally catch the exception
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine($"Crash in {nameof(AsyncAwaiter)}!\r\n{ex}", nameof(AsyncAwaiter));
                    Debugger.Break();
#endif
                    // Throw as usual
                    throw ex;
                }
                finally
                {
                    // Release the task semaphore
                    taskSemaphore.Release();
                }
            }

            // Otherwise don't do the task at all
        }

        private static async Task CreateSemaphoreIfNotExistsAsync(string key, int maxAccessCount)
        {
            // First wait for the on the self locking semaphore
            await SelfLock.WaitAsync();

            try
            {
                // If the semaphore doesn't exist for that key
                if (!Semaphores.ContainsKey(key))
                {
                    // Create it using the provided max count
                    Semaphores.Add(key, new SemaphoreSlim(maxAccessCount, maxAccessCount));
                }
            }
            finally
            {
                // Release the self lock
                SelfLock.Release();
            }
        }
    }
}
