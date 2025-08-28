using System.Threading.Tasks;
using EZGO.Api.Models.Settings;

namespace EZGO.Maui.Core.Classes.LanguageResources.ResourceTypes
{
    public abstract class BaseLanguageResource
    {
        protected const string languagesDirectoryName = "languages";
        protected const string languageFilename = "language-{0}";

        public BaseLanguageResource()
        {
        }

        public virtual LanguageResource GetLanguageResource()
        {
            return null;
        }

        public virtual async Task<LanguageResource> GetLanguageResource(string languageTag)
        {
            return await Task.FromResult<LanguageResource>(null);
        }
    }
}
