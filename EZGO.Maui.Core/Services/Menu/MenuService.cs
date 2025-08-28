using EZGO.Api.Models.UI;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Menu;
using System;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Services.Menu
{
    public class MenuService : IMenuService
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public MenuService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }

        public async Task<SideMenu> GetSideMenuAsync(int? id)
        {
            string uri = $"app/sidemenu?areaid={id}";

            SideMenu result = await _apiRequestHandler.HandleRequest<SideMenu>(uri);

            return result;
        }
    }
}
