using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Utils
{
    public static class PerformanceTracker
    {
        private static readonly Dictionary<string, Stopwatch> _timers = new();
        private static readonly object _lock = new object();

        public static void StartOperation(string operationName)
        {
            lock (_lock)
            {
                if (_timers.ContainsKey(operationName))
                {
                    _timers[operationName].Restart();
                }
                else
                {
                    _timers[operationName] = Stopwatch.StartNew();
                }
            }
        }

        public static void EndOperation(string operationName)
        {
            lock (_lock)
            {
                if (_timers.TryGetValue(operationName, out var timer))
                {
                    timer.Stop();
                    Debug.WriteLine($"[Performance] Operation {operationName} took {timer.ElapsedMilliseconds}ms");
                }
            }
        }

        public static async Task<T> TrackOperationAsync<T>(string operationName, Func<Task<T>> operation)
        {
            StartOperation(operationName);
            try
            {
                return await operation();
            }
            finally
            {
                EndOperation(operationName);
            }
        }

        // New method to track async Task operations
        public static async Task TrackOperationAsync(string operationName, Func<Task> operation)
        {
            StartOperation(operationName);
            try
            {
                await operation();
            }
            finally
            {
                EndOperation(operationName);
            }
        }
    }
} 