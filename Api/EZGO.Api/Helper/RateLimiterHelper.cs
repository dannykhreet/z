using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace EZGO.Api.Helper
{
    public class RateLimiterHelper
    {
        private readonly IMemoryCache _cache;
        private readonly int _limit;
        private readonly TimeSpan _window;

        public RateLimiterHelper(IMemoryCache cache, IConfiguration config)
        {
            _cache = cache;
            _limit = config.GetValue<int>("FeedRateLimiting:LimitPerUser", 5);
            _window = TimeSpan.FromSeconds(config.GetValue<int>("FeedRateLimiting:TimeframeSeconds", 60));
        }

        public (bool isLimited, string message) CheckLimit(string userId, string endpoint)
        {
            var key = $"ratelimit:{userId}:{endpoint}";
            var entry = _cache.GetOrCreate(key, e =>
            {
                e.AbsoluteExpirationRelativeToNow = _window;
                return new Counter { Count = 0, Expiry = DateTime.UtcNow.Add(_window) };
            });

            entry.Count++;
            _cache.Set(key, entry, entry.Expiry);

            if (entry.Count > _limit)
            {
                var remainingSeconds = (int)Math.Max((entry.Expiry - DateTime.UtcNow).TotalSeconds, 0);
                var msg = $"Rate limit reached. Please try again in {remainingSeconds} seconds.";
                return (true, msg);
            }

            var remaining = Math.Max(_limit - entry.Count, 0);
            return (false, $"Request accepted. You have {remaining} requests left in this time window.");
        }
    }
}