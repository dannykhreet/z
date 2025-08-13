using System.Linq;
using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Classes.LanguageResources
{
    /// <summary>
    /// Sets language dictionaries
    /// </summary>
    public class LanguageSetter
    {
        public void SetLanguageDictionary(LanguageResource language)
        {
            Statics.LanguageDictionary = language.ResourceItems.ToDictionary(item => item.ResourceKey, item => item.ResourceValue);
        }

        public void SetDefaultLanguageResource(LanguageResource language)
        {
            Statics.DefaultLanguageDictionary = language.ResourceItems.ToDictionary(item => item.ResourceKey, item => item.ResourceValue);
        }
    }
}
