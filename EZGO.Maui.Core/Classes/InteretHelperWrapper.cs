using System.Threading.Tasks;

namespace EZGO.Maui.Core.Classes
{
    public class InteretHelperWrapper : IInternetHelper
    {
        public Task<bool> HasInternetConnection()
        {
            return InternetHelper.HasInternetConnection();
        }
    }

    public interface IInternetHelper
    {
        Task<bool> HasInternetConnection();
    }
}

