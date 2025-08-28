using System;
using System.Diagnostics;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Cache;
using Syncfusion.Maui.DataSource.Extensions;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class CachingService : ICachingService
    {
        private const string cacheFolder = "cache";

        /// <summary>
        /// Clears the cache.
        /// </summary>
        /// <param name="request">The request.</param>
        public void ClearCache(string request = null)
        {
            try
            {
                string folder = Path.Combine(FileSystem.Current.AppDataDirectory, cacheFolder); ;

                if (Directory.Exists(folder))
                {
                    if (string.IsNullOrWhiteSpace(request))
                    {
                        var files = Directory.GetFiles(folder);
                        if (files.Any())
                        {
                            foreach (var file in files)
                            {
                                File.Delete(file);
                            }
                        }
                        Directory.Delete(folder, true);
                    }
                    else
                    {
                        // Only clear the cached files for the given request
                        string[] fileNames = Directory.GetFiles(folder);

                        string searchName = $"-{request.RemoveSpecialCharacters().Replace(" ", "_")}.req";

                        if (fileNames.Any())
                        {
                            IEnumerable<FileInfo> files = fileNames.Where(item => item.EndsWith(searchName)).Select(item => new FileInfo(item));

                            foreach (FileInfo file in files)
                            {
                                file.Delete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Crashes.TrackError(ex);
            }
        }

        /// <summary>
        /// Caches the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The cache duration.</param>
        public void CacheRequest(string request, string response, TimeSpan duration)
        {
            // Clear the old cached files first
            ClearCache(request);

            string filename = GetFilenameToCacheRequest(request, duration);

            File.WriteAllText(filename, response);
        }

        /// <summary>
        /// Caches the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="duration">The cache duration.</param>
        public void CacheRequest(string request, byte[] response, TimeSpan duration)
        {
            // Clear the old cached files first
            ClearCache(request);

            string filename = GetFilenameToCacheRequest(request, duration);

            File.WriteAllBytes(filename, response);
        }

        /// <summary>
        /// Gets the response as string.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The response as string.
        /// </returns>
        public string GetResponseAsString(string request)
        {
            string result = string.Empty;

            string filename = GetFilenameToRetrieveRequest(request);

            if (!string.IsNullOrWhiteSpace(filename))
                result = File.ReadAllText(filename);

            return result;
        }

        public async Task AlterCachedRequestListAsync<T>(string request, Action<T> alteringFunction, Func<T, bool> predicate = null)
        {
            try
            {
                var fileName = GetFilenameToRetrieveRequest(request);
                if (fileName.IsNullOrEmpty())
                    return;

                var content = File.ReadAllText(fileName);
                var httpResponse = new HttpResponseMessage { Content = new StringContent(content) };
                var data = await httpResponse.Content.ReadAsJsonAsync<IEnumerable<T>>().ConfigureAwait(false);


                IEnumerable<T> filtered = data;

                if (predicate != null)
                    filtered = data.Where(predicate);

                filtered.ForEach(alteringFunction);

                var serialized = JsonSerializer.Serialize(data);
                File.WriteAllText(fileName, serialized);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                Debug.WriteLine("[CachingService]: " + ex.Message);
            }
        }

        public async Task AlterCachedRequestAsync<T>(string request, Action<T> alteringFunction)
        {
            try
            {
                var fileName = GetFilenameToRetrieveRequest(request);
                if (fileName.IsNullOrEmpty())
                    return;

                var content = File.ReadAllText(fileName);
                var httpResponse = new HttpResponseMessage { Content = new StringContent(content) };
                var data = await httpResponse.Content.ReadAsJsonAsync<T>();

                alteringFunction(data);

                var serialized = JsonSerializer.Serialize(data);
                File.WriteAllText(fileName, serialized);
            }
            catch
            { }
        }

        /// <summary>
        /// Gets the response as bytes.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The response as bytes.
        /// </returns>
        public byte[] GetResponseAsBytes(string request)
        {
            byte[] result = null;

            string filename = GetFilenameToRetrieveRequest(request);

            if (!string.IsNullOrWhiteSpace(filename))
                result = File.ReadAllBytes(filename);

            return result;
        }

        /// <summary>
        /// Gets the filenames of cached items.
        /// </summary>
        /// <returns>
        /// Collection of filenames.
        /// </returns>
        public IEnumerable<string> GetCachedFilenames()
        {
            List<string> result = new List<string>();

            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, cacheFolder); ;

            if (Directory.Exists(folder))
            {
                string[] filenames = Directory.GetFiles(folder);

                foreach (string filename in filenames)
                {
                    string filenameWithoutPathAndTime = filename.Split("/").Last().Split("-").Last();
                    result.Add(filenameWithoutPathAndTime);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the request cache filename.
        /// This is the filename that get's used to store the request in the cache.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The request cache filename.
        /// </returns>
        public string GetRequestCacheFilename(string request)
        {
            string filename = $"{request.RemoveSpecialCharacters().Replace(" ", "_")}.req";

            return filename;
        }

        /// <summary>
        /// Clears the out of date cache.
        /// </summary>
        public void ClearOutOfDateCache()
        {
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, cacheFolder); ;

            if (Directory.Exists(folder))
            {
                string[] filenames = Directory.GetFiles(folder);

                foreach (string filename in filenames)
                {
                    string timestamp = filename.Split("/").Last().Split("-").First();
                    long fileTime = long.Parse(timestamp);

                    DateTime datetime = DateTime.FromFileTimeUtc(fileTime);

                    if (datetime < DateTime.UtcNow)
                        File.Delete(filename);
                }
            }
        }

        /// <summary>
        /// Gets the filename to retrieve request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Filename.</returns>
        private static string GetFilenameToRetrieveRequest(string request)
        {
            string result = string.Empty;

            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, cacheFolder); ;
            string searchName = $"-{request.RemoveSpecialCharacters().Replace(" ", "_")}.req";

            if (Directory.Exists(folder))
            {
                long current = DateTimeHelper.UtcNow.ToFileTimeUtc();

                long[] startTimes = null;

                string[] fileNames = Directory.GetFiles(folder);

                IEnumerable<FileInfo> files = null;

                // Get the files that have the same request
                if (fileNames.Any())
                {
                    files = fileNames.Where(item => item.EndsWith(searchName)).Select(item => new FileInfo(item));

                    if (files.Any())
                        startTimes = files.Select(item => long.Parse(item.Name.Split("-", StringSplitOptions.RemoveEmptyEntries).First())).ToArray();
                }

                if (startTimes != null)
                {
                    long startTime = startTimes.Where(item => current <= item).OrderByDescending(i => i).FirstOrDefault();

                    // Filename
                    string filename = startTime + searchName;

                    if (files.Any())
                    {
                        FileInfo responseFile = files.SingleOrDefault(item => item.Name == filename);

                        if (responseFile != null)
                            result = responseFile.FullName;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the filename to cache request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="duration">The duration.</param>
        /// <returns>The filename to cache a request.</returns>
        private static string GetFilenameToCacheRequest(string request, TimeSpan duration)
        {
            string filename = $"{DateTime.UtcNow.Add(duration).ToFileTimeUtc()}-{request.RemoveSpecialCharacters().Replace(" ", "_")}.req";

            // Save files to internal storage; no other user or apps can access these files.
            // Unlike the external storage directories, your app does not require any system permissions to read and write to the internal directories returned by these methods.
            string folder = Path.Combine(FileSystem.Current.AppDataDirectory, cacheFolder); ;

            Java.IO.File newFile = new Java.IO.File(folder, filename);

            if (!newFile.Exists())
                newFile.ParentFile?.Mkdirs();

            return newFile.Path;
        }
    }
}

