using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Reports;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// ReportManager; Contains report methods for generating several reports. These are used within the app and or CMSs.
    /// ReportManager is mainly used withing the reporting parts in the client apps and / or cms. 
    /// </summary>
    public class ReportManager : BaseManager<ReportManager>, IReportManager
    {
        //TODO sort methods and reset regions (or move methods)

        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;

        #endregion

        #region - constructor(s) -
        public ReportManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ILogger<ReportManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;

        }
        #endregion

        #region - public methods Tasks Reports -
        /// <summary>
        /// GetTaskOverviewReportAsync; Get overview report; Report is based on several different datasets. The datasets are buildup in separate calls and methods.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">Timestamp where of the periode representing the report.</param>
        /// <returns>TaskOverviewReport, containing several datasets.</returns>
        public async Task<TaskOverviewReport> GetTaskOverviewReportAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new TaskOverviewReport();
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            //var TaskCurrentTaskAsync = GetTaskOverviewReportCurrentItemsAsync(companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            var taskitems = await GetTaskOverviewReportTaskItemsAsync(companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid); ;
            output.Month = taskitems.Where(x => x.Type == "month").ToList();
            output.Week = taskitems.Where(x => x.Type == "week").ToList();
            output.Shifts = taskitems.Where(x => x.Type == "shifts").ToList();
            //week month combination
            output.WeekMonth = taskitems.Where(y => y.Type == "week" || y.Type == "month").GroupBy(x => x.Status).Select(g => new TaskOverviewReportItem() { Status = g.First().Status, NrOfItems = g.Sum(c => c.NrOfItems) }).ToList();

            var past = await GetTaskOverviewReportPastItemsAsync(storedProcedure:"report_get_tasks_overview_past", companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);
            output.Overdue = await GetTaskOverdueReportsAsync(companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid, allowedOnly: allowedOnly); ;
            output.OverDueTasks = output.Overdue.Select(g => g.NrOfItems).Sum();
            output.LastShift = past.Where(x => x.SourceType == "task_overview_previous_shift").Select(x => new TaskOverviewReportItem() { Status =x.Status, NrOfItems = x.NrOfItems }).ToList();
            output.LastWeek = past.Where(x => x.SourceType == "task_overview_lastweek").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();
            output.Yesterday = past.Where(x => x.SourceType == "task_overview_yesterday").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();


            var current = await GetTaskOverviewReportCurrentItemsAsync(companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            output.Today = current.Where(x => x.SourceType == "task_overview_today").Select(x => new TaskOverviewReportItem() { Status = x.Status, Type = x.Type, NrOfItems = x.NrOfItems }).ToList();
            output.TodayTotal = output.Today.GroupBy(x => x.Status).Select(g => new TaskOverviewReportItem() { Status = g.First().Status, NrOfItems = g.Sum(c => c.NrOfItems) }).ToList();
            output.ThisShift = current.Where(x => x.SourceType == "task_overview_this_shift").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();

            return output;
        }

        /// <summary>
        /// NOTE: LEGACY CODE
        /// GetPastTaskOverviewReportAsync; Get past task overview report data for past dataset.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp which is used for determining the data range.</param>
        /// <param name="allowedOnly">AllowedOnly bit based is there needs to be a extra filter.</param>
        /// <returns>TaskOverviewReport object containing all the data.</returns>
        public async Task<TaskOverviewReport> GetPastTaskOverviewReportAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new TaskOverviewReport();
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            var past = await GetTaskOverviewReportPastItemsAsync(storedProcedure: "report_get_tasks_overview_past", companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            output.LastShift = past.Where(x => x.SourceType == "task_overview_previous_shift").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();
            output.LastWeek = past.Where(x => x.SourceType == "task_overview_lastweek").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();
            output.Yesterday = past.Where(x => x.SourceType == "task_overview_yesterday").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();

            return output;
        }

        /// <summary>
        /// GetPastTaskOverviewReportPreviousShiftAsync; Get past task overview report data for the previous shift.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp which is used for determining the data range.</param>
        /// <param name="allowedOnly">AllowedOnly bit based is there needs to be a extra filter.</param>
        /// <returns>TaskOverviewReport object containing all the data.</returns>
        public async Task<List<TaskOverviewReportItem>> GetPastTaskOverviewReportPreviousShiftAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new TaskOverviewReport();
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            var past = await GetTaskOverviewReportPastItemsAsync(storedProcedure: "report_get_tasks_overview_past_previous_shift", companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            return past.Where(x => x.SourceType == "task_overview_previous_shift").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();
        }

        /// <summary>
        /// GetPastTaskOverviewReportLastWeekAsync; Get past task overview report data for last week.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp which is used for determining the data range.</param>
        /// <param name="allowedOnly">AllowedOnly bit based is there needs to be a extra filter.</param>
        /// <returns>TaskOverviewReport object containing all the data.</returns>
        public async Task<List<TaskOverviewReportItem>> GetPastTaskOverviewReportLastWeekAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new TaskOverviewReport();
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            var past = await GetTaskOverviewReportPastItemsAsync(storedProcedure: "report_get_tasks_overview_past_last_week", companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            return past.Where(x => x.SourceType == "task_overview_lastweek").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();
        }

        /// <summary>
        /// GetPastTaskOverviewReportYesterdayAsync; Get past task overview report data for yesterday.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp which is used for determining the data range.</param>
        /// <param name="allowedOnly">AllowedOnly bit based is there needs to be a extra filter.</param>
        /// <returns>TaskOverviewReport object containing all the data.</returns>
        public async Task<List<TaskOverviewReportItem>> GetPastTaskOverviewReportYesterdayAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new TaskOverviewReport();
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            var past = await GetTaskOverviewReportPastItemsAsync(storedProcedure: "report_get_tasks_overview_past_yesterday", companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            return past.Where(x => x.SourceType == "task_overview_yesterday").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();
        }

        /// <summary>
        /// GetCurrentTaskOverviewReportAsync; Get the task overview report data for current data set.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp which is used for determining the data range.</param>
        /// <param name="allowedOnly">AllowedOnly bit based is there needs to be a extra filter.</param>
        /// <returns>TaskOverviewReport object containing all the data.</returns>
        public async Task<TaskOverviewReport> GetCurrentTaskOverviewReportAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new TaskOverviewReport();
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            var current = await GetTaskOverviewReportCurrentItemsAsync(companyId: companyId, userId: allowedOnly.HasValue && allowedOnly.Value ? userId : null, timestamp: timestamp, areaid: areaid);

            output.Today = current.Where(x => x.SourceType == "task_overview_today").Select(x => new TaskOverviewReportItem() { Status = x.Status, Type = x.Type, NrOfItems = x.NrOfItems }).ToList();
            output.TodayTotal = output.Today.GroupBy(x => x.Status).Select(g => new TaskOverviewReportItem() { Status = g.First().Status, NrOfItems = g.Sum(c => c.NrOfItems) }).ToList();
            output.ThisShift = current.Where(x => x.SourceType == "task_overview_this_shift").Select(x => new TaskOverviewReportItem() { Status = x.Status, NrOfItems = x.NrOfItems }).ToList();

            return output;
        }

        /// <summary>
        /// GetTaskOverviewReportCurrentItemsAsync; Get  items for task overview;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">Timestamp where of the periode representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        private async Task<List<TaskOverviewReportItem>> GetTaskOverviewReportTaskItemsAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if(areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString(), Type = dr["type"].ToString() }) ;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetTaskOverviewReportCurrentItems(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetTaskOverviewReportPastItemsAsync; Get list of TaskOverviewReportItems for use with a TaskOverviewReport
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem;</returns>
        private async Task<List<TaskOverviewReportItem>> GetTaskOverviewReportPastItemsAsync(string storedProcedure, int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                if(string.IsNullOrEmpty(storedProcedure)) { storedProcedure = "report_get_tasks_overview_past"; }
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString(), SourceType = dr["pasttype"].ToString() });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetTaskOverviewReportPastItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetTaskOverviewReportCurrentItemsAsync; Get a list of TaskOverviewReportItem for a current data set. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem</returns>
        private async Task<List<TaskOverviewReportItem>> GetTaskOverviewReportCurrentItemsAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks_overview_current", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString(), Type = dr["type"].ToString(), SourceType = dr["sourcetype"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetTaskOverviewReportCurrentItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// GetThisShiftTasksAsync; Get tasks overview based on current shift structure.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        private async Task<List<TaskOverviewReportItem>> GetThisShiftTasksAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks_this_shift", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString()});
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetThisShiftTasks(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetPreviousShiftTasksAsync; Gets the previous shift counts.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem;</returns>
        private async Task<List<TaskOverviewReportItem>> GetPreviousShiftTasksAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks_previous_shift", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString() });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetPreviousShiftTasksAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetTodayTasksAsync; Get tasks for today
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        private async Task<List<TaskOverviewReportItem>> GetTodayTasksAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks_today", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString(), Type = dr["type"].ToString() });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetLastWeekTasks(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetLastWeekTasksAsync; Get Last Week task overview report items.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        private async Task<List<TaskOverviewReportItem>> GetLastWeekTasksAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks_last_week", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetLastWeekTasksAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetYesterdaysTasksAsync; Get list of TaskOverviewReportItems. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        private async Task<List<TaskOverviewReportItem>> GetYesterdaysTasksAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_tasks_yesterday", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetYesterdaysTasksAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetTasksOverdueReportAsync; Get list of TaskOverviewReportItem for overdue reports.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        public async Task<List<TaskOverviewReportItem>> GetTaskOverdueReportsAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            var output = new List<TaskOverviewReportItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                using (dr = await _manager.GetDataReader("report_get_overdue_tasks", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new TaskOverviewReportItem() { NrOfItems = Convert.ToInt32(dr["amount"]), Status = dr["status"].ToString() });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetTaskOverdueReportsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetOverdueTaskReportsAsync; Get overdue tasks number 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp where of the period representing the report.</param>
        /// <returns>List of TaskOverviewReportItem items</returns>
        public async Task<int> GetOverdueTaskReportsAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null)
        {
            if (timestamp == null || timestamp.Value == DateTime.MinValue)
            {
                timestamp = DateTime.Now;
            }

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                if (areaid.HasValue && areaid.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaid.Value));
                }
                if (allowedOnly.HasValue && allowedOnly.Value && userId.HasValue && userId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }

                var outcome = (int)await _manager.ExecuteScalarAsync("report_get_overdue_tasks", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return outcome;

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetOverdueTaskReportsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }

            return 0; //something went wrong, return 0.
        }

        /// <summary>
        /// GetAuditsDeviationReportAsync; Get a report audit deviation report.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <param name="taskTemplateId">TaskTemplateId (DB: tasks_tasktemplate.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="timespanInDays">Time span in days.</param>
        /// <returns>ReportAuditDeviations object containing all data</returns>
        public async Task<ReportAuditDeviations> GetAuditsDeviationReportAsync(int companyId, int? auditTemplateId = null, int? taskTemplateId = null, int? areaId = null, int? timespanInDays = null)
        {
            var output = new ReportAuditDeviations();
            output.Deviations = await GetReportAuditDeviationItemsAsync(companyId: companyId, areaId: areaId, timespanInDays: timespanInDays); ;
            output.DeviationsSkipped = await GetReportAuditTaskStatusSkippedDeviationItemsAsync(companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);

            if (auditTemplateId.HasValue || taskTemplateId.HasValue)
            {
                //filter certain data
                output = FilterReportAuditDeviationItemsInReportAsync(report: output, auditTemplateId: auditTemplateId, taskTemplateId: taskTemplateId);
                output = FilterReportDeviationItemInReport(report: output, auditTemplateId: auditTemplateId, taskTemplateId: taskTemplateId);
            }

            return output;
        }

        /// <summary>
        /// GetChecklistDeviationReportAsync; Get ReportChecklistDeviations.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">Timestamp where of the period representing the report.</param>
        /// <returns>ReportChecklistDeviations object containing all data</returns>
        public async Task<ReportChecklistDeviations> GetChecklistDeviationReportAsync(int companyId, int? checklistTemplateId = null, int? taskTemplateId = null, int? areaId = null, int? timespanInDays = null)
        {
            var output = new ReportChecklistDeviations();

            output = await GetAndAppendReportChecklistTaskStatusDeviationItemsAsync(report: output, companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);

            if (checklistTemplateId.HasValue || taskTemplateId.HasValue)
            {
                //filter certain data
                output = FilterReportChecklistDeviationItemsInReportAsync(report: output, checklistTemplateId: checklistTemplateId, taskTemplateId: taskTemplateId);
            }

            return output;
        }

        /// <summary>
        /// GetTasksDeviationReportAsync; Get's a ReportTaskDeviations
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaid">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">Timestamp where of the period representing the report.</param>
        /// <returns>ReportTaskDeviations object containing all data</returns>
        public async Task<ReportTaskDeviations> GetTasksDeviationReportAsync(int companyId, int? areaId = null, int? timespanInDays = null)
        {
            var output = new ReportTaskDeviations();

            output = await GetAndAppendReportTasksTaskStatusDeviationItems(report: output, companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);

            return output;
        }
        #endregion

        #region - private audit deviation reports -
        /// <summary>
        /// GetReportAuditDeviationItemsAsync; Get's deviation report for audit items. The data will be presented as a collection of ReportAuditDeviationItem items.
        /// After the data is retreived from the database a collection will be added to the ReportAuditDeviations object.
        /// NOTE! if timespanInDays is not supplied all items will be returned. Depending on company this may take a while.
        /// In normal circumstances a timespan SHOULD ALWAYS be supplied. For load reasons internally any number may be used.
        /// External apps should always use the numbers represented in the <see cref="TimespanTypeEnum">TimespanTypeEnum</see> due to optimizations for these specific time spans.
        /// </summary>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>A collection of ReportAuditDeviationItem items.</returns>
        private async Task<List<ReportAuditDeviationItem>> GetReportAuditDeviationItemsAsync(int companyId, int? areaId = null, int? timespanInDays = null)
        {

            var output = new List<ReportAuditDeviationItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (timespanInDays.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timespanindays", timespanInDays));
                }
                if (areaId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                using (dr = await _manager.GetDataReader("report_audits_deviance_scores_new_calculation", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = new ReportAuditDeviationItem();

                        item.ActionCount = Convert.ToInt32(dr["action_count"]);
                        item.ActionDoneCount = Convert.ToInt32(dr["action_count_done"]);
                        item.AuditTemplateId = Convert.ToInt32(dr["audit_template_id"]);
                        item.AuditTemplateName = dr["audit_template_name"].ToString();
                        item.DeviationScore = Convert.ToDouble(dr["deviance_nr_total"]);
                        item.DeviationPercentage = Convert.ToDouble(dr["deviance_percentage_avg"]);
                        item.NumberOfQuestions = Convert.ToInt32(dr["nr_of_questions"]);
                        item.TaskTemplateId = Convert.ToInt32(dr["task_template_id"]);
                        item.TaskTemplateName = dr["task_template_name"].ToString();


                        output.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetReportAuditDeviationItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetReportAuditTaskStatusDeviationItemsAsync; Get ReportDeviationTaskStatusItems collection.
        /// </summary>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>List of ReportDeviationTaskStatusItems</returns>
        private async Task<List<ReportDeviationTaskStatusItem>> GetReportAuditTaskStatusDeviationItemsAsync(int companyId, int? areaId = null, int? timespanInDays = null)
        {
            return await GetReportTaskStatusDeviationItemsAsync(sp: "report_audits_deviance_scores_other", companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);
        }

        /// <summary>
        /// GetReportAuditTaskStatusSkippedDeviationItemsAsync; Get skipped items based on all task items.
        /// </summary>
        /// <param name="report">Report where ReportDeviationItem need to be added.</param>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>Report with ReportDeviationItems </returns>
        private async Task<List<ReportDeviationItem>> GetReportAuditTaskStatusSkippedDeviationItemsAsync( int companyId, int? areaId = null, int? timespanInDays = null)
        {

            var output = new List<ReportDeviationItem>();
            var items = await GetReportAuditTaskStatusDeviationItemsAsync(companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);

            foreach (var item in items.Where(x => x.CountSkipped > 0).OrderByDescending(o => o.CountSkipped))
            {
                output.Add(new ReportDeviationItem()
                {
                    ActionCount = item.ActionCount,
                    ActionDoneCount = item.ActionDoneCount,
                    CountNr = item.CountSkipped,
                    Id = item.TaskTemplateId,
                    Name = item.TaskTemplateName,
                    ParentTemplateId = item.ParentTemplateId,
                    ParentTemplateName = item.ParentTemplateName,
                    Percentage = item.PercentageSkipped,
                    Status = "skipped"
                });
            }

            return output;
        }

        /// <summary>
        /// FilterReportAuditDeviationItemsInReportAsync; Fitlered ReportAuditDeviations report;
        /// </summary>
        /// <param name="report">Report containing deviance data</param>
        /// <param name="auditTemplateId">AuditTemplateId used for filtering</param>
        /// <param name="taskTemplateId">TaskTemplateId used for filtering</param>
        /// <returns>Returns filtered ReportAuditDeviations</returns>
        private ReportAuditDeviations FilterReportAuditDeviationItemsInReportAsync(ReportAuditDeviations report, int? auditTemplateId = null, int? taskTemplateId = null)
        {
            if (report != null && report.Deviations != null && report.Deviations.Count > 0)
            {
                //filter based on audit template
                if (auditTemplateId.HasValue)
                {
                    report.Deviations = report.Deviations.Where(x => x.AuditTemplateId == auditTemplateId).ToList();
                }
                //filter based on task template
                if (taskTemplateId.HasValue)
                {
                    report.Deviations = report.Deviations.Where(x => x.TaskTemplateId == taskTemplateId).ToList();
                }
            }
            return report;
        }

        /// <summary>
        /// FilterReportChecklistDeviationItemsInReportAsync; Fitlered ReportChecklistDeviations report;
        /// </summary>
        /// <param name="report">Report containing deviance data</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId used for filtering</param>
        /// <param name="taskTemplateId">TaskTemplateId used for filtering</param>
        /// <returns>Returns filtered ReportChecklistDeviations</returns>
        private ReportChecklistDeviations FilterReportChecklistDeviationItemsInReportAsync(ReportChecklistDeviations report, int? checklistTemplateId = null, int? taskTemplateId = null)
        {
            if (report != null && report.DeviationsNotOk != null && report.DeviationsNotOk.Count > 0)
            {
                //filter based on checklist template
                if (checklistTemplateId.HasValue)
                {
                    report.DeviationsNotOk = report.DeviationsNotOk.Where(x => x.ParentTemplateId == checklistTemplateId).ToList();
                }
                //filter based on task template
                if (taskTemplateId.HasValue)
                {
                    report.DeviationsNotOk = report.DeviationsNotOk.Where(x => x.Id == taskTemplateId).ToList();
                }
            }

            if (report != null && report.DeviationsSkipped != null && report.DeviationsSkipped.Count > 0)
            {
                //filter based on checklist template
                if (checklistTemplateId.HasValue)
                {
                    report.DeviationsSkipped = report.DeviationsSkipped.Where(x => x.ParentTemplateId == checklistTemplateId).ToList();
                }
                //filter based on task template
                if (taskTemplateId.HasValue)
                {
                    report.DeviationsSkipped = report.DeviationsSkipped.Where(x => x.Id == taskTemplateId).ToList();
                }
            }
            
            return report;
        }
        #endregion

        #region - private checklist deviation reports -
        /// <summary>
        /// GetReportChecklistTaskStatusDeviationItemsAsync; Get ReportDeviationTaskStatusItems collection.
        /// </summary>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>List of ReportDeviationTaskStatusItems</returns>
        private async Task<List<ReportDeviationTaskStatusItem>> GetReportChecklistTaskStatusDeviationItemsAsync(int companyId, int? areaId = null, int? timespanInDays = null)
        {
            return await GetReportTaskStatusDeviationItemsAsync(sp: "report_checklists_deviance_scores", companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);
        }

        /// <summary>
        /// GetAndAppendReportChecklistTaskStatusDeviationItemsAsync; Get skipped and not ok items based on all task items.
        /// NOTE; if report is null it will be created.
        /// </summary>
        /// <param name="report">Report where ReportDeviationItem need to be added.</param>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>Report with ReportDeviationItems </returns>
        private async Task<ReportChecklistDeviations> GetAndAppendReportChecklistTaskStatusDeviationItemsAsync(ReportChecklistDeviations report, int companyId, int? areaId = null, int? timespanInDays = null)
        {
            if (report == null)
            {
                report = new ReportChecklistDeviations();
            }
            var items = await GetReportChecklistTaskStatusDeviationItemsAsync(companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);

            foreach (var item in items.Where(x => x.CountSkipped > 0).OrderByDescending(o => o.CountSkipped))
            {
                report.DeviationsSkipped.Add(new ReportDeviationItem()
                {
                    ActionCount = item.ActionCount,
                    ActionDoneCount = item.ActionDoneCount,
                    CountNr = item.CountSkipped,
                    Id = item.TaskTemplateId,
                    Name = item.TaskTemplateName,
                    ParentTemplateId = item.ParentTemplateId,
                    ParentTemplateName = item.ParentTemplateName,
                    Percentage = item.PercentageSkipped,
                    Status = "skipped"
                });
            }

            foreach (var item in items.Where(x => x.CountNotOk > 0).OrderByDescending(o => o.CountNotOk))
            {
                report.DeviationsNotOk.Add(new ReportDeviationItem()
                {
                    ActionCount = item.ActionCount,
                    ActionDoneCount = item.ActionDoneCount,
                    CountNr = item.CountNotOk,
                    Id = item.TaskTemplateId,
                    Name = item.TaskTemplateName,
                    ParentTemplateId = item.ParentTemplateId,
                    ParentTemplateName = item.ParentTemplateName,
                    Percentage = item.PercentageNotOk,
                    Status = "not ok"
                });
            }

            return report;
        }
        #endregion

        #region - private tasks deviation reports-
        /// <summary>
        /// GetReportTasksTaskStatusDeviationItems; Get ReportDeviationTaskStatusItems collection.
        /// </summary>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>List of ReportDeviationTaskStatusItems</returns>
        private async Task<List<ReportDeviationTaskStatusItem>> GetReportTasksTaskStatusDeviationItems(int companyId, int? areaId = null, int? timespanInDays = null)
        {
            return await GetReportTaskStatusDeviationItemsAsync(sp: "report_tasks_deviance_scores", companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);
        }

        /// <summary>
        /// GetAndAppendReportTasksTaskStatusDeviationItems; Get skipped / not ok / todo items based on all task items.
        /// NOTE; if report is null it will be created.
        /// </summary>
        /// <param name="report">Report where ReportDeviationItem need to be added.</param>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>Report with ReportDeviationItems </returns>
        private async Task<ReportTaskDeviations> GetAndAppendReportTasksTaskStatusDeviationItems(ReportTaskDeviations report, int companyId, int? areaId = null, int? timespanInDays = null)
        {
            if (report == null)
            {
                report = new ReportTaskDeviations();
            }
            var items = await GetReportTasksTaskStatusDeviationItems(companyId: companyId, areaId: areaId, timespanInDays: timespanInDays);

            foreach (var item in items.Where(x => x.CountSkipped > 0).OrderByDescending(o => o.CountSkipped))
            {
                report.DeviationsSkipped.Add(new ReportDeviationItem()
                {
                    ActionCount = item.ActionCount,
                    ActionDoneCount = item.ActionDoneCount,
                    CountNr = item.CountSkipped,
                    Id = item.TaskTemplateId,
                    Name = item.TaskTemplateName,
                    ParentTemplateId = item.ParentTemplateId,
                    ParentTemplateName = item.ParentTemplateName,
                    Percentage = item.PercentageSkipped,
                    Status = "skipped"
                });
            }

            foreach (var item in items.Where(x => x.CountNotOk > 0).OrderByDescending(o => o.CountNotOk))
            {
                report.DeviationsNotOk.Add(new ReportDeviationItem()
                {
                    ActionCount = item.ActionCount,
                    ActionDoneCount = item.ActionDoneCount,
                    CountNr = item.CountNotOk,
                    Id = item.TaskTemplateId,
                    Name = item.TaskTemplateName,
                    ParentTemplateId = item.ParentTemplateId,
                    ParentTemplateName = item.ParentTemplateName,
                    Percentage = item.PercentageNotOk,
                    Status = "not ok"
                });
            }

            foreach (var item in items.Where(x => x.CountTodo > 0).OrderByDescending(o => o.CountTodo))
            {
                report.DeviationsTodo.Add(new ReportDeviationItem()
                {
                    ActionCount = item.ActionCount,
                    ActionDoneCount = item.ActionDoneCount,
                    CountNr = item.CountTodo,
                    Id = item.TaskTemplateId,
                    Name = item.TaskTemplateName,
                    ParentTemplateId = item.ParentTemplateId,
                    ParentTemplateName = item.ParentTemplateName,
                    Percentage = item.PercentageTodo,
                    Status = "todo"
                });
            }

            return report;
        }
        #endregion

        #region - private general methods -
        /// <summary>
        /// GetReportTaskStatusDeviationItemsAsync; Get a collection of ReportDeviationTaskStatusItems.
        /// </summary>
        /// <param name="sp">Stored procedure name (will differ for tasks, checklist and audits)</param>
        /// <param name="companyId">Company id of connecting user.</param>
        /// <param name="areaId">Optional areaId used for in-query-filtering</param>
        /// <param name="timespanInDays">Optional timespanIndays representing a number of days used for calculating a timespan. If not supplied all items will be returned.</param>
        /// <returns>List of ReportDeviationTaskStatusItems</returns>
        private async Task<List<ReportDeviationTaskStatusItem>> GetReportTaskStatusDeviationItemsAsync(string sp, int companyId, int? areaId = null, int? timespanInDays = null)
        {
            var output = new List<ReportDeviationTaskStatusItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (timespanInDays.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timespanindays", timespanInDays));
                }
                if (areaId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = new ReportDeviationTaskStatusItem();

                        item.ActionCount = Convert.ToInt32(dr["action_count"]);
                        item.ActionDoneCount = Convert.ToInt32(dr["action_count_done"]);
                        if (dr.HasColumn("parent_template_id")) item.ParentTemplateId = Convert.ToInt32(dr["parent_template_id"]);
                        if(dr.HasColumn ("parent_template_name")) item.ParentTemplateName = dr["parent_template_name"].ToString();
                        item.TaskTemplateId = Convert.ToInt32(dr["task_template_id"]);
                        item.TaskTemplateName = dr["task_template_name"].ToString();
                        if (dr.HasColumn("percentage_todo")) item.PercentageTodo = Convert.ToDouble(dr["percentage_todo"]);
                        if (dr.HasColumn("percentage_ok")) item.PercentageOk = Convert.ToDouble(dr["percentage_ok"]);
                        if (dr.HasColumn("percentage_notok")) item.PercentageNotOk = Convert.ToDouble(dr["percentage_notok"]);
                        if (dr.HasColumn("percentage_skipped")) item.PercentageSkipped = Convert.ToDouble(dr["percentage_skipped"]);
                        if (dr.HasColumn("count_todo")) item.CountTodo = Convert.ToInt32(dr["count_todo"]);
                        if (dr.HasColumn("count_ok")) item.CountOk = Convert.ToInt32(dr["count_ok"]);
                        if (dr.HasColumn("count_notok")) item.CountNotOk = Convert.ToInt32(dr["count_notok"]);
                        if (dr.HasColumn("count_skipped")) item.CountSkipped = Convert.ToInt32(dr["count_skipped"]);
                        item.CountNr = Convert.ToInt32(dr["count_nr"]);

                        output.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ReportManager.GetReportTaskStatusDeviationItems(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// FilterReportDeviationItemInReport; Filter ReportAuditDeviations
        /// </summary>
        /// <param name="report">Report containing deviance data</param>
        /// <param name="auditTemplateId">AuditTemplateId used for filtering</param>
        /// <param name="taskTemplateId">TaskTemplateId used for filtering</param>
        /// <returns>a ReportAuditDeviations item</returns>
        private ReportAuditDeviations FilterReportDeviationItemInReport(ReportAuditDeviations report, int? auditTemplateId = null, int? taskTemplateId = null)
        {
            if (report != null && report.DeviationsSkipped != null && report.DeviationsSkipped.Count > 0)
            {
                //filter based on audit template
                if(auditTemplateId.HasValue)
                {
                    report.DeviationsSkipped = report.DeviationsSkipped.Where(x => x.ParentTemplateId.HasValue && x.ParentTemplateId == auditTemplateId.Value).ToList();
                }

                //filter based on task template
                if (taskTemplateId.HasValue)
                {
                    report.DeviationsSkipped = report.DeviationsSkipped.Where(x => x.Id.HasValue && x.Id.Value == taskTemplateId.Value).ToList();
                }

            }
            return report;
        }

        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}

