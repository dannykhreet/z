using EZGO.Maui.Core.Models.Menu;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Menu
{
    public interface IMainMenuService
    {
        Task<MainMenuModel> GetMainMenuAsync(string url = null);
    }
}
