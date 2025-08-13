using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Cache
{
    /// <summary>
    /// Caching service.
    /// </summary>
    public interface ICachingService
    {
        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="request">The request.</param>
        void ClearCache(string request = null);

        /// <summary>
        /// Caches the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The cache duration.</param>
        void CacheRequest(string request, string response, TimeSpan duration);

        /// <summary>
        /// Caches the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The cache duration.</param>
        void CacheRequest(string request, byte[] response, TimeSpan duration);

        /// <summary>
        /// Alters list type of data in cache.
        /// </summary>
        /// <typeparam name="T">The type of list element.</typeparam>
        /// <param name="request">Request URL</param>
        /// <param name="alteringFunction">The function that is applied to every item in the filtered collection.</param>
        /// <param name="predicate">Predicate for the collection filter. 
        /// If <see langword="null"/> the <paramref name="alteringFunction"/> will be applied to all the objects in the list.</param>
        /// <returns>Task object that can be awaited.</returns>
        Task AlterCachedRequestListAsync<T>(string request, Action<T> alteringFunction, Func<T, bool> predicate = null);


        /// <summary>
        /// Alters data in cache.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="request">Request URL</param>
        /// <param name="alteringFunction">The function that is applied to item.</param>        
        /// <returns>Task object that can be awaited.</returns>
        Task AlterCachedRequestAsync<T>(string request, Action<T> alteringFunction);

        /// <summary>
        /// Gets the response as string.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response as string.</returns>
        string GetResponseAsString(string request);

        /// <summary>
        /// Gets the response as bytes.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response as bytes.</returns>
        byte[] GetResponseAsBytes(string request);

        /// <summary>
        /// Gets the filenames of cached items.
        /// </summary>
        /// <returns>Collection of filenames.</returns>
        IEnumerable<string> GetCachedFilenames();

        /// <summary>
        /// Gets the request cache filename.
        /// This is the filename that get's used to store the request in the cache.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The request cache filename.</returns>
        string GetRequestCacheFilename(string request);

        /// <summary>
        /// Clears the out of date cache.
        /// </summary>
        void ClearOutOfDateCache();
    }
}
