using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.LanguageResources.ResourceTypes
{
    public class ApiLanguageResource : BaseLanguageResource
    {
        /// <summary>
        /// Gets the language from API asynchronous.
        /// </summary>
        /// <param name="languageTag">The language tag.</param>
        /// <returns>Language.</returns>
        public override async Task<LanguageResource> GetLanguageResource(string languageTag)
        {
            LanguageResource language = null;
            string filename = string.Format(languageFilename, languageTag);

            IFileService fileService = DependencyService.Get<IFileService>();

            string languageJson = string.Empty;

            if (await InternetHelper.HasInternetConnection())
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(5);
                        httpClient.BaseAddress = new Uri(Statics.ApiUrl);
                        httpClient.DefaultRequestHeaders.Accept.Clear();

                        string requestUri = $"app/resources/language/?language={languageTag}";

                        var request = new HttpRequestMessage(requestUri: requestUri, method: HttpMethod.Get);
                        request.Headers.AddEzgoHeaders();

                        using (HttpResponseMessage response = await httpClient.SendAsync(request, completionOption: HttpCompletionOption.ResponseContentRead))
                        {
                            languageJson = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                languageJson = string.Empty;
                            }
                            else
                            {
                                language = JsonSerializer.Deserialize<LanguageResource>(languageJson);

                                await fileService.SaveFileToInternalStorageAsync(languageJson, filename, languagesDirectoryName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error has occured during retriving dictionary from API. Ex: {e.Message}", "[LanguageAPI]");
                }
            }
            else
            {
                languageJson = await fileService.ReadFromInternalStorageAsync(filename, languagesDirectoryName);

                language = JsonSerializer.Deserialize<LanguageResource>(languageJson);
            }

            return language;
        }
    }
}
