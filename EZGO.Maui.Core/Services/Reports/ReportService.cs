using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Utils;
using System.Globalization;

namespace EZGO.Maui.Core.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public ReportService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }

        #region my statistics

        public async Task<List<ReportsCount>> GetMyStatisticsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/my?areaid={Settings.AreaSettings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region dashboard

        public async Task<List<ReportsCount>> GetTasksCountAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string timestamp = DateTimeHelper.Now.ToString(Constants.ApiDateTimeFormat, null);

            string uri = $"reporting/statistics/generic/taskscount_per_state?areaid={Settings.AreaSettings.ReportWorkAreaId}&timestamp={timestamp}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsAverage>> GetAuditsAverageAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/average/auditsaverage_per_state?areaid={Settings.AreaSettings.ReportWorkAreaId}";

            List<ReportsAverage> result = await _apiRequestHandler.HandleListRequest<ReportsAverage>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetAuditsCountAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/auditscount_per_audit_state?areaid={Settings.AreaSettings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetActionsCountPerStateAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/actionscount_per_action_state";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region tasks
        public async Task<List<ReportsCount>> GetTasksLastMonthCountAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/taskscount_per_state_per_date_last_month?areaid={Settings.AreaSettings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetTasksCountPerStatePerDateAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/taskscount_per_state_per_date?areaid={Settings.AreaSettings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<ReportTaskDeviationsModel> GetTaskDeviationsAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/deviations/tasks?timespantype={(int)period}&areaid={Settings.AreaSettings.ReportWorkAreaId}";

            ReportTaskDeviationsModel result = await _apiRequestHandler.HandleRequest<ReportTaskDeviationsModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region actions

        public async Task<List<ReportsCount>> GetActionsCountStartedResolvedPerDateAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/actionscount_started_resolved_per_date";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<ReportsCount> GetActionsCountAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/count/actions?timespantype={(int)period}";

            ReportsCount result = await _apiRequestHandler.HandleRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetActionsCountPerUserAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/actionscount_per_user?timespantype={(int)period}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetActionsCountPerAssignedUserAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/actionscount_per_assigned_user?timespantype={(int)period}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region Checklists

        public async Task<List<ReportsCount>> GetChecklistItemsCountAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistitemscount_per_state?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }
        public async Task<List<ReportsCount>> GetChecklistItemsCountPerStatePerDateAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistitemscount_per_state_per_date?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetChecklistsCountAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistscount_per_checklist_state?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetChecklistsCountPerDateAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistscount_per_date?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }


        public async Task<List<ReportsCount>> GetChecklistsCountPerDayOfYearAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistscount_per_day?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetChecklistsCountPerWeekOfYearAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistscount_per_week?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsCount>> GetChecklistsCountPerMonthOfYearAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/checklistscount_per_month?areaid={Settings.ReportWorkAreaId}";

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<ReportChecklistDeviationsModel> GetChecklistDeviationsAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/deviations/checklists?timespantype={(int)period}&areaid={Settings.ReportWorkAreaId}";

            ReportChecklistDeviationsModel result = await _apiRequestHandler.HandleRequest<ReportChecklistDeviationsModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }


        #endregion

        #region audit
        public async Task<List<ReportsCount>> GetAuditsCountPerDate(int audittemplateid = 0, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/generic/auditscount_per_date?";

            List<string> parameters = new List<string>();
            parameters.Add($"areaid={Settings.ReportWorkAreaId}");
            if (audittemplateid != 0) parameters.Add($"audittemplateid={audittemplateid}");

            uri += parameters.Aggregate((a, b) => a + '&' + b);

            List<ReportsCount> result = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ReportsAverage>> GetAuditsAveragePerDate(int audittemplateid = 0, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/average/auditsaverage_per_date?";

            List<string> parameters = new List<string>();
            parameters.Add($"areaid={Settings.ReportWorkAreaId}");
            if (audittemplateid != 0) parameters.Add($"audittemplateid={audittemplateid}");

            uri += parameters.Aggregate((a, b) => a + '&' + b);

            List<ReportsAverage> result = await _apiRequestHandler.HandleListRequest<ReportsAverage>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<ReportAuditDeviationsModel> GetAuditDeviationsAsync(TimespanTypeEnum period, int audittemplateid = 0, int tasktemplateid = 0, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/deviations/audits?";

            List<string> parameters = new List<string>();
            parameters.Add($"areaid={Settings.ReportWorkAreaId}");
            parameters.Add($"timespantype={(int)period}");
            if (audittemplateid != 0) parameters.Add($"audittemplateid={audittemplateid}");
            if (tasktemplateid != 0) parameters.Add($"tasktemplateid={tasktemplateid}");

            uri += parameters.Aggregate((a, b) => a + '&' + b);

            ReportAuditDeviationsModel result = await _apiRequestHandler.HandleRequest<ReportAuditDeviationsModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region pre filled collections

        private List<ReportsCount> GetPrefilled12Days(DateTime date)
        {
            List<ReportsCount> result = new List<ReportsCount>();
            if (date == DateTime.MinValue) { date = DateTime.Today; }

            for (int i = 0; i < 12; i++)
            {
                result.Add(new ReportsCount { ReportDate = date.Date, Day = date.Day, DayOfYear = date.DayOfYear, Month = date.Month, Year = date.Year, Week = WeekOfYear.GetIso8601WeekOfYear(date), Subscript = date.ToString("d/MM", CultureInfo.CurrentUICulture), Name = String.Format("count per day of year {0}-{1}", date.Year, date.DayOfYear) });
                date = date.AddDays(-1);
            }

            return result.OrderBy(x => x.ReportDate).ToList();
        }

        private List<ReportsCount> GetPrefilled12Weeks(DateTime date)
        {
            List<ReportsCount> result = new List<ReportsCount>();
            if (date == DateTime.MinValue) { date = DateTime.Today; }

            int weeknr = WeekOfYear.GetIso8601WeekOfYear(date);
            date = WeekOfYear.FirstDateOfWeekISO8601(date.Year, weeknr);

            for (int i = 0; i < 12; i++)
            {
                weeknr = WeekOfYear.GetIso8601WeekOfYear(date);
                result.Add(new ReportsCount { ReportDate = date.Date, Day = date.Day, DayOfYear = date.DayOfYear, Month = date.Month, Year = date.Year, Week = weeknr, Subscript = String.Format("W{0}", weeknr), Name = String.Format("count per week of year {0}-{1}", date.Year, weeknr.ToString("D2")) });
                date = date.AddDays(-7);
            }

            return result.OrderBy(x => x.ReportDate).ToList();
        }

        private static List<ReportsCount> GetPrefilled12Months(DateTime date)
        {
            List<ReportsCount> result = new List<ReportsCount>();
            if (date == DateTime.MinValue) { date = DateTime.Today; }

            date = new DateTime(date.Year, date.Month, 1);
            for (int i = 0; i < 12; i++)
            {
                result.Add(new ReportsCount { ReportDate = date.Date, Day = date.Day, DayOfYear = date.DayOfYear, Month = date.Month, Year = date.Year, Week = WeekOfYear.GetIso8601WeekOfYear(date), Subscript = date.ToString("MMM", CultureInfo.CurrentUICulture), Name = String.Format("count per month of year {0}-{1}", date.Year, date.Month.ToString("D2")) });
                date = date.AddMonths(-1);
            }

            return result.OrderBy(x => x.ReportDate).ToList();
        }

        private List<ReportsCount> GetPrefilledYear(DateTime date)
        {
            List<ReportsCount> result = new List<ReportsCount>();
            if (date == DateTime.MinValue) { date = DateTime.Today; }
            int year = date.Year;
            int month = date.Month;

            for (int i = 0; i < 12; i++)
            {
                date = new DateTime(year, (i + 1), 1);
                if (i >= month) { break; }
                result.Add(new ReportsCount { ReportDate = date.Date, Day = date.Day, DayOfYear = date.DayOfYear, Month = date.Month, Year = date.Year, Week = WeekOfYear.GetIso8601WeekOfYear(date), Subscript = date.ToString("MMM", CultureInfo.CurrentUICulture), Name = String.Format("count per month of year {0}-{1}", date.Year, date.Month.ToString("D2")) });
            }

            return result;
        }

        public List<ReportsCount> GetIntervalCollection(TimespanTypeEnum interval, DateTime date)
        {
            switch (interval)
            {
                case TimespanTypeEnum.LastTwelveDays:
                    return GetPrefilled12Days(date);
                case (TimespanTypeEnum.LastTwelveWeeks):
                    return GetPrefilled12Weeks(date);
                case (TimespanTypeEnum.LastTwelveMonths):
                    return GetPrefilled12Months(date);
                case (TimespanTypeEnum.ThisYear):
                    return GetPrefilledYear(date);
                default:
                    return new List<ReportsCount>();
            }
        }

        #endregion

        #region ez feed stats

        public async Task<MyEzFeedStats> GetMyEzFeedStatsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"reporting/statistics/my/ezfeed";

            List<ReportsCount> stats = await _apiRequestHandler.HandleListRequest<ReportsCount>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            var result = new MyEzFeedStats(stats);

            return result;
        }

        #endregion

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }
    }
}
