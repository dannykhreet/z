using EZGO.Api.Settings;
using EZGO.Api.Utils.Cache;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EZGO.Api.Utils
{
    public static class ProtectionHelper
    {
        public const string HAMMER_MESSAGE = "To many attempts. Please try again later.";

        /// <summary>
        /// IsHammering; Hammer check method. Based on a string a hammer protection is calculated based on the MemCache. If hammering is occurring return true so it can be handled properly.
        /// </summary>
        /// <param name="hammerCheck">String to be checked. Can be a specific value (e.g. UserName) or a combination of values.</param>
        /// <returns>true if hammering is probably occurring.</returns>
        public static bool IsHammering(IMemoryCache memoryCache, string hammerCheck)
        {
            bool hammering = false;
            var cacheKeyHammer = CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyHammerProtection, hammerCheck);

            int count = 0;
            if (memoryCache.TryGetValue(cacheKeyHammer, out count))
            {
                if (count < ApiSettings.HAMMER_MAX_NUMBER_OF_ATTEMPTS)
                {
                    count = count + 1;
                }
                else
                {
                    hammering = true;
                }
            }

            memoryCache.Set(cacheKeyHammer, count, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ApiSettings.HAMMER_ELAPSED_TIME_IN_SECONDS) });

            return hammering;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="requestCheck"></param>
        /// <returns></returns>
        public static bool CheckRunningRequest(IMemoryCache memoryCache, string requestCheck)
        {
            bool alreadyRunning = false;
            var cacheKeyRequest = CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyRequestCheck, requestCheck);
            if (memoryCache.TryGetValue(cacheKeyRequest, out alreadyRunning))
            {
                Debug.WriteLine(string.Concat("AlreadyRunning ", requestCheck));
                return alreadyRunning;
            } else
            {
                Debug.WriteLine(string.Concat("Added to cache ", requestCheck));
                //add run item to cache, run is started, no existing runnning item is found
                memoryCache.Set(cacheKeyRequest, true, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ApiSettings.REQUESTCHECK_ELAPSED_TIME_IN_SECONDS) });
                alreadyRunning = false;
                return alreadyRunning;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="requestCheck"></param>
        public static void RemoveRunningRequest(IMemoryCache memoryCache, string requestCheck)
        {
            Debug.WriteLine(string.Concat("Removed from cache ", requestCheck));
            memoryCache.Remove(CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyRequestCheck, requestCheck));
        }
    }
}
