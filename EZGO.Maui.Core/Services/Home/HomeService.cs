using EZGO.Api.Models.Stats;
using EZGO.Api.Models.UI;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.Home;
using EZGO.Maui.Core.Services.Api;
using EZGO.Maui.Core.Utils;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly IApiClient apiClient;

        public HomeService() : this(Statics.AppHttpClient)
        {

        }

        public HomeService(HttpClient httpClient)
        {
            apiClient = new ApiClient(httpClient, true);
        }

        public void Dispose()
        {
            apiClient.Dispose();
        }

        public async Task<MainMenu> GetMainMenuAsync(int? id)
        {
            string uri = $"app/mainmenu?areaid={id}";
            MainMenu result = await apiClient.GetAsync<MainMenu>(uri);
            return result;
        }

        public async Task<List<StatsItem>> GetStatsAsync()
        {
            string id = Settings.WorkAreaId.ToString();
            string datetime = DateTimeHelper.Now.ToString(Constants.ApiDateTimeFormat, null);
            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();

            string uri = $"app/mainmenu/statistics?areaid={id}&timestamp={datetime}&allowedonly={allowed}";
            List<StatsItem> result = await apiClient.GetAsync<List<StatsItem>>(uri);
            return result;
        }
    }
}
