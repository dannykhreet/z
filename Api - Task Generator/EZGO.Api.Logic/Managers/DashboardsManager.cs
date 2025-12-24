using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// DashboardsManager; Contains the functionality for retrieving a data dashboard which can be used in a general overview (so no reporting but a first page entrance like overview).
    /// For now the only really dashboard is used within the CMS. 
    /// A dashboard contains a set of unlined data that can be displayed on page. 
    /// </summary>
    public class DashboardsManager : BaseManager<DashboardsManager>, IDashboardsManager
    {

        #region - properties -
        private string culture;
        public string Culture
        {
            get { return culture; }
            set { culture = _checklistManager.Culture = _auditManager.Culture = _taskManager.Culture = _actionManager.Culture = value; }
        }

        #endregion

        #region - privates -
        private readonly IStatisticsManager _statsManager;
        private readonly IGeneralManager _generalManager;
        private readonly IChecklistManager _checklistManager;
        private readonly IAuditManager _auditManager;
        private readonly ITaskManager _taskManager;
        private readonly IActionManager _actionManager;
        #endregion

        #region - constructor(s) -
        public DashboardsManager(IActionManager actionManager, ITaskManager taskManager, IChecklistManager checklistManager, IAuditManager auditManager, IGeneralManager generalManager, IStatisticsManager statsManager, ILogger<DashboardsManager> logger) : base(logger)
        {
            _statsManager = statsManager;
            _generalManager = generalManager;
            _checklistManager = checklistManager;
            _auditManager = auditManager;
            _taskManager = taskManager;
            _actionManager = actionManager;
        }
        #endregion

        #region - dashboard -
        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<Dashboard> GetDashboard(int companyId, int userId, DashboardFilters filters = null)
        {
            var dashboard = new Dashboard();
            //TODO set defaults (e.g. number of items retrieved to contants)
            if (filters != null && filters.UseStatisticsTotals) dashboard.StatisticTotals = await _statsManager.GetTotalStatisticsAsync();
            if (filters != null && filters.UseAnnouncements) dashboard.Announcements = await _generalManager.GetAnnouncements(companyId: companyId, limit: 3);
            if (filters != null && filters.UseCompanyOverview) dashboard.CompanyOverview = await _statsManager.GetTotalsOverviewByCompanyAsync(companyId: companyId);
            if (filters != null && filters.UseCompletedAudits) dashboard.CompletedAudits = await _auditManager.GetAuditsAsync(companyId: companyId,
                                                                                                                              userId: userId,
                                                                                                                              filters: new AuditFilters() { IsCompleted = true, Limit = 5 },
                                                                                                                              include: "language,tasks,areapaths,properties,propertyvalues,propertyuservalues,openfields,pictureproof",
                                                                                                                              useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId,
                                                                                                                                                                                              featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_AUDIT_STORAGE));
            if (filters != null && filters.UseCompletedChecklists) dashboard.CompletedChecklists = await _checklistManager.GetChecklistsAsync(companyId: companyId,
                                                                                                                                              userId: userId,
                                                                                                                                              filters: new ChecklistFilters() { IsCompleted = true, Limit = 5 },
                                                                                                                                              include: "language,tasks,areapaths,properties,propertyvalues,propertyuservalues,openfields,pictureproof",
                                                                                                                                              useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId,
                                                                                                                                                                                                              featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_CHECKLIST_STORAGE));
            if (filters != null && filters.UseCompletedTasks) dashboard.CompletedTasks = await _taskManager.GetLatestTasks(companyId: companyId, 5, include: "language,steps,areapaths,properties,propertyvalues,propertyuservalues,pictureproof");
            if (filters != null && filters.UseActions) dashboard.Actions = await _actionManager.GetActionsAsync(companyId: companyId, userId: userId, new ActionFilters() { Limit = 100 });

            //add specific sorting.
            if (dashboard.Actions != null && dashboard.Actions.Count > 0) {
                dashboard.Actions = dashboard.Actions.Count > 3 ? dashboard.Actions.OrderByDescending(x => x.CreatedAt).Take(3).ToList() : dashboard.Actions.OrderByDescending(x => x.CreatedAt).ToList();
            }

            return dashboard;
        }

        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<Dashboard> GetDashboardAnnouncements(int companyId, int userId, DashboardFilters filters = null)
        {
            var dashboard = new Dashboard();
            
            if (filters != null && filters.UseAnnouncements) dashboard.Announcements = await _generalManager.GetAnnouncements(companyId: companyId, limit: 3);

            return dashboard;
        }

        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<Dashboard> GetDashboardCompletedItems(int companyId, int userId, DashboardFilters filters = null)
        {
            var dashboard = new Dashboard();

            //TODO set defaults (e.g. number of items retrieved to contants)
            if (filters != null && filters.UseCompletedAudits) dashboard.CompletedAudits = await GetDashboardCompletedAudits(companyId: companyId, userId: userId);
            if (filters != null && filters.UseCompletedChecklists) dashboard.CompletedChecklists = await GetDashboardCompletedChecklists(companyId: companyId, userId: userId);
            if (filters != null && filters.UseCompletedTasks) dashboard.CompletedTasks = await GetDashboardCompletedTasks(companyId: companyId, userId: userId);
            if (filters != null && filters.UseActions) dashboard.Actions = await GetDashboardActions(companyId: companyId, userId: userId);

            return dashboard;
        }

        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<List<Models.Audit>> GetDashboardCompletedAudits(int companyId, int userId)
        {
            return await _auditManager.GetAuditsAsync(companyId: companyId,
                                                      userId: userId,
                                                      filters: new AuditFilters() { IsCompleted = true, Limit = 5 },
                                                      include: "language,tasks,areapaths,properties,propertyvalues,propertyuservalues,openfields,pictureproof",
                                                      useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId,
                                                                                                                                                                                              featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_AUDIT_STORAGE));
        }

        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<List<Models.Checklist>> GetDashboardCompletedChecklists(int companyId, int userId)
        {
            return await _checklistManager.GetChecklistsAsync(companyId: companyId,
                                                              userId: userId,
                                                              filters: new ChecklistFilters() { IsCompleted = true, Limit = 5 },
                                                              include: "tasks,areapaths,properties,propertyvalues,propertyuservalues,openfields,pictureproof,language",
                                                              useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId,
                                                                                                                                                                                                              featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_CHECKLIST_STORAGE));
        }

        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<List<Models.TasksTask>> GetDashboardCompletedTasks(int companyId, int userId)
        {
            return await _taskManager.GetLatestTasks(companyId: companyId, 5, include: "language,steps,areapaths,properties,propertyvalues,propertyuservalues,pictureproof");
        }

        /// <summary>
        /// GetDashboard; Get dashboard information for use in CMS or certain overviews.
        /// Note! Due to the number of queries used, do not implement in auto refresh structures under 5 minutes.
        /// </summary>
        /// <param name="companyId">CompanyId of the dashboard</param>
        /// <param name="userId">User connected.</param>
        /// <param name="filters">Filters used</param>
        /// <returns>A dashboard object containing several lists and collections of statistics.</returns>
        public async Task<List<Models.ActionsAction>> GetDashboardActions(int companyId, int userId)
        {
            var actions = await _actionManager.GetLatestActionsAsync(companyId: companyId, userId: userId, new ActionFilters() { Limit = 3 });

            return actions;
        }

        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_statsManager.GetPossibleExceptions());
                listEx.AddRange(_generalManager.GetPossibleExceptions());
                listEx.AddRange(_checklistManager.GetPossibleExceptions());
                listEx.AddRange(_auditManager.GetPossibleExceptions());
                listEx.AddRange(_taskManager.GetPossibleExceptions());
                listEx.AddRange(_actionManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion
    }
}
