using EZGO.Api.Models.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Menu
{
    public interface IMenuService : IDisposable
    {
        Task<SideMenu> GetSideMenuAsync(int? id);
    }
}
