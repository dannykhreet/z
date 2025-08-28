using EZGO.Api.Models.Stats;
using EZGO.Api.Models.UI;
using EZGO.Maui.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Home
{
    public interface IHomeService : IDisposable
    {
        Task<List<StatsItem>> GetStatsAsync();
        Task<MainMenu> GetMainMenuAsync(int? id);
    }
}
