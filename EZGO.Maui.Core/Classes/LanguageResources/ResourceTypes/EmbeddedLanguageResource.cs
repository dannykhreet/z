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
            try
            {
                string languageFile = "DefaultLanguage.json";
                var assembly = Assembly.GetExecutingAssembly();

                using Stream? stream = assembly.GetManifestResourceStream($"EZGO.Maui.Core.{languageFile}");
                if (stream == null)
                {
                    DebugService.WriteLine($"Resource not found: {languageFile}", "EmbeddedLanguageResource");
                    return new LanguageResource(); // fallback
                }

                using var reader = new StreamReader(stream);
                string languageJson = reader.ReadToEnd();

                if (!languageJson.IsNullOrEmpty())
                {
                    return JsonSerializer.Deserialize<LanguageResource>(languageJson) ?? new LanguageResource();
                }

                DebugService.WriteLine($"Resource {languageFile} is empty.", "EmbeddedLanguageResource");
                return new LanguageResource();
            }
            catch (Exception ex)
            {
                DebugService.WriteLine($"Failed to load language resource. Exception: {ex}", "EmbeddedLanguageResource");
                return new LanguageResource(); // fallback
            }
        }
    }
}
