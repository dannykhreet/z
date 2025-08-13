using System.IO;
using System.Reflection;
using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Extensions;

namespace EZGO.Maui.Core.Classes.LanguageResources.ResourceTypes
{
    public class EmbeddedLanguageResource : BaseLanguageResource
    {
        public EmbeddedLanguageResource()
        {
        }

        /// <summary>
        /// Get Default Embedded Language Resource
        /// </summary>
        /// <returns>Returns LanguageResource</returns>
        public override LanguageResource GetLanguageResource()
        {
            LanguageResource language = null;
            try
            {
                string languageJson = string.Empty;
                string languageFile = "DefaultLanguage.json";

                var assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream($"EZGO.Maui.Core.{languageFile}");

                using (var reader = new StreamReader(stream))
                {
                    languageJson = reader.ReadToEnd();
                }

                if (!languageJson.IsNullOrEmpty())
                {
                    language = JsonSerializer.Deserialize<LanguageResource>(languageJson);
                }
            }
            catch (Exception ex)
            {
                DebugService.WriteLine(ex.Message, "EmbeddedLanguageResource");
            }
            return language;
        }
    }
}
