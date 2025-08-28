using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.Menu;
using EZGO.Maui.Core.Models.Menu;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Services.Menu
{
    public class MainMenuService : IMainMenuService
    {
        private readonly IApiClient _restClient;

        public MainMenuService(IApiClient restClient)
        {
            _restClient = restClient;
        }

        public Task<MainMenuModel> GetMainMenuAsync(string url = null)
        {
            throw new NotImplementedException();
            //return _restClient.HttpGetSingleObjectAsync<MainMenuModel>($"app/mainmenu");
        }
    }
}
