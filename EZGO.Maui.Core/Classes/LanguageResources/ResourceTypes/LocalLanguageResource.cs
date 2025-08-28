using System.Threading.Tasks;
using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Interfaces.File;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.LanguageResources.ResourceTypes
{
    public class LocalLanguageResource : BaseLanguageResource
    {

        public LocalLanguageResource()
        {
        }

        /// <summary>
        /// Get Local Language Resource from Internal Storage
        /// </summary>
        /// <param name="languageTag">Language tag to determinate which language to get</param>
        /// <returns>A LanguageResource </returns>
        public override async Task<LanguageResource> GetLanguageResource(string languageTag = null)
        {
            LanguageResource language = null;

            IFileService fileService = DependencyService.Get<IFileService>();

            string languageJson = await fileService.ReadFromInternalStorageAsync(string.Format(languageFilename, languageTag), languagesDirectoryName);

            if (!string.IsNullOrWhiteSpace(languageJson))
                language = JsonSerializer.Deserialize<LanguageResource>(languageJson);

            return language;
        }
    }
}
