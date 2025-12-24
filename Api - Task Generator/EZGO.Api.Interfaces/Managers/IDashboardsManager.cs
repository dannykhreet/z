using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IDashboardsManager
    {
        string Culture { get; set; }
        Task<Dashboard> GetDashboard(int companyId, int userId, DashboardFilters filters = null);
        Task<Dashboard> GetDashboardAnnouncements(int companyId, int userId, DashboardFilters filters = null);
        Task<Dashboard> GetDashboardCompletedItems(int companyId, int userId, DashboardFilters filters = null);
        Task<List<Models.Audit>> GetDashboardCompletedAudits(int companyId, int userId);
        Task<List<Models.Checklist>> GetDashboardCompletedChecklists(int companyId, int userId);
        Task<List<Models.TasksTask>> GetDashboardCompletedTasks(int companyId, int userId);
        Task<List<Models.ActionsAction>> GetDashboardActions(int companyId, int userId);
        List<Exception> GetPossibleExceptions();
    }
}
