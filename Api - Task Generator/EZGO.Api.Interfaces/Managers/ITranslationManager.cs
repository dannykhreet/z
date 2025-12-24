using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ITranslationManager
    {
        Task<bool> TranslateAndSaveObjectAsync(int objectId, string type);

        Task<(bool hasTranslation, object translation)> GetTranslationAsync(int objectId, int companyId, string language, string functionName, object component);
    }
}
