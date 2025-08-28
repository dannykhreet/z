using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Polly;

namespace EZGO.Maui.Core.Utils
{
    public static class RetryWrapper
    {
        private static readonly TimeSpan[] _pauseBetweenFailures = new TimeSpan[]
        {
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(20),
        };

        public static async Task ExecuteAsync(Func<Task> func)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_pauseBetweenFailures, (exception, timeSpan) =>
                {
                    Debug.WriteLine(exception, "Retrying\n\t");
                });
            await retryPolicy.ExecuteAsync(async () => await func());
        }
    }
}
