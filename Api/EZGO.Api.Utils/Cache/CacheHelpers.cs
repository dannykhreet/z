using EZGO.Api.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.Cache
{
    /// <summary>
    /// CacheHelpers; contains a set of methods for helping with cache creating.
    /// </summary>
    public static class CacheHelpers
    {
        /// <summary>
        /// StoredKeys; list of stored cache keys for ez getting and deleting keys.
        /// Currently no key list available from the caching structure.
        /// Seeing reflecting like querying through the caching structure will probably slower, just maintain a list of keys.
        /// </summary>
        public static List<string> StoredKeys { get; set; } = new List<string>();

        /// <summary>
        /// GetMemoryCacheDefaultEntryOptions; get the default cache options for creating a memory cache item. This consist of the sliding expiration setting based on default number of seconds.
        /// </summary>
        /// <returns>A MemoryCacheEntryOptions object.</returns>
        public static MemoryCacheEntryOptions GetMemoryCacheDefaultEntryOptions()
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();
            // Keep in cache for this time, reset time if accessed.
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSettings.CacheTimeDefaultInSeconds);
            return cacheEntryOptions;
        }

        /// <summary>
        /// GenerateCacheKey; generate a cache key for use with a cache item.
        /// When used for business purposes, if the cache-able dataset is company specific add company id as first item.
        /// </summary>
        /// <param name="baseKey">base key represented by a string</param>
        /// <param name="parmeters">parameters that are added to the key.</param>
        /// <returns>A generated key in uppercase format</returns>
        public static string GenerateCacheKey(params object[] parameters)
        {
            var output = string.Empty;
            if(parameters.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                for(var i = 0; i < parameters.Length; i++)
                {
                    sb.Append("_");
                    if(parameters[i] != null)
                    {
                        sb.Append(parameters[i].ToString());
                    }

                }
                output = sb.ToString(); //string.Concat(key, string.Join("_", parameters.Select(x => string.Concat(x.GetType().Name, '_' ,x.ToString()).ToList())));
                sb.Clear();
                sb = null;
            }

            output = output.ToUpper();

            if (!StoredKeys.Contains(output)) StoredKeys.Add(output.ToString());
            return output;

        }

        /// <summary>
        /// ResetCacheByKey, reset the cache based on a key
        /// </summary>
        /// <param name="memoryCache">memory cache object being used.</param>
        /// <param name="key">Key of cache item to be removed.</param>
        /// <param name="logger">Logger for logging issues. This is optional.</param>
        /// <returns>bool, true/false depending on error occurred.</returns>
        public static bool ResetCacheByKey(IMemoryCache memoryCache, string key, ILogger logger = null)
        {
            try
            {
                memoryCache.Remove(key);
                return true;
            } catch (Exception ex)
            {
                //If logger available log error, if not just ignore it.
                if (logger != null) { logger.LogError(exception: ex, message: "Error occurred GetTasksDataTableByCompanyAndDateAsync()"); }
            }
            return false;

        }

        /// <summary>
        /// ResetCacheByKeyByKeyStart; resets the cachekey based on the start part of a key. For mass removing a certain object.
        /// </summary>
        /// <param name="memoryCache">memory cache object being used.</param>
        /// <param name="key">Key of cache item to be removed.</param>
        /// <param name="logger">Logger for logging issues. This is optional.</param>
        /// <returns>bool, true/false depending on error occurred.</returns>
        public static bool ResetCacheByKeyByKeyStart(IMemoryCache memoryCache, string key, ILogger logger = null)
        {
            try
            {
                var keysToRemoved = new List<string>();
                foreach(var cacheKey in StoredKeys.Where(x => x.StartsWith (key))) {
                    keysToRemoved.Add(cacheKey);
                }

                foreach(var removingkey in keysToRemoved)
                {
                    memoryCache.Remove(removingkey);
                    StoredKeys.Remove(removingkey);
                }
                return true;
            }
            catch (Exception ex)
            {
                //If logger available log error, if not just ignore it.
                if (logger != null) { logger.LogError(exception: ex, message: "Error occurred ResetCacheByKeyByKeyStart()"); }
            }
            return false;

        }

    }
}
