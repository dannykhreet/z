using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Export;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Logs;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tools;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Raw
{
    public class ToolsManager : BaseManager<ToolsManager>, IToolsManager
    {
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ITaskGenerationManager _taskGenerationManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IDatabaseLogWriter _databaseLogWriter;

        public ToolsManager(IDatabaseAccessHelper manager, IDatabaseAccessHelper dwManager, IConfigurationHelper configurationHelper, IDataAuditing dataAuditing, ITaskGenerationManager taskGenerationManager, IDatabaseLogWriter databaseLogWriter, ILogger<ToolsManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _taskGenerationManager = taskGenerationManager;
            _dataAuditing = dataAuditing;
            _databaseLogWriter = databaseLogWriter;
        }

        /// <summary>
        /// GetLatestLogsAsJsonAsync; Get logs (last 25-ish) as a JSON string.
        /// </summary>
        /// <returns>String containing logs.</returns>
        public async Task<string> GetLatestLogsAsJsonAsync()
        {
            var possibleLogs = await _manager.ExecuteScalarAsync("get_tools_latest_logs", commandType: System.Data.CommandType.StoredProcedure);
            return possibleLogs.ToString();
        }

        /// <summary>
        /// GetLatestLogsRequestResponseAsJsonAsync; Get logs (last 25-ish) as a JSON string.
        /// </summary>
        /// <returns>String containing logs.</returns>
        public async Task<string> GetLatestLogsRequestResponseAsJsonAsync()
        {
            var possibleLogs = await _manager.ExecuteScalarAsync("get_tools_latest_logs_requestresponses", commandType: System.Data.CommandType.StoredProcedure);
            return possibleLogs.ToString();
        }

        /// <summary>
        /// GetLatestAuditingLogs; Get latests audit logs (last month) over all companies. This can be used for supervising users, not for general use
        /// </summary>
        /// <returns>A list of logitems.</returns>
        public async Task<List<LogAuditingItem>> GetLatestAuditingLogs(bool includeData = false)
        {
            var logs = new List<LogAuditingItem>();

            NpgsqlDataReader dr = null;


            try
            {
                using (dr = await _manager.GetDataReader("get_tools_latest_auditlogs", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {

                        logs.Add(CreateOrFillLogAuditingItemFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetLatestAuditingLogs(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return logs;
        }

        /// <summary>
        /// GetLatestAuditingLogsForUser; Get latest audit logs (last month) for a specific user.
        /// </summary>
        /// <param name="userId">UserId of user (normally logged in user)</param>
        /// <param name="companyId">CompanyId of user</param>
        /// <param name="includeData">Include the data of the log record (before/after)</param>
        /// <returns>A list of logitems.</returns>
        public async Task<List<LogAuditingItem>> GetLatestAuditingLogsForUser(int userId, int companyId, bool includeData = false)
        {
            var logs = new List<LogAuditingItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("get_tools_latest_auditlogs_user", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {

                        logs.Add(CreateOrFillLogAuditingItemFromReader(dr, includeData: includeData));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetLatestAuditingLogsForUser(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return logs;
        }

        /// <summary>
        /// GetLatestAuditingLogsForUser; Get latest audit logs (last month) for a specific user.
        /// </summary>
        /// <param name="companyId">CompanyId</param>
        /// <param name="includeData">Include the data of the log record (before/after)</param>
        /// <returns>A list of logitems.</returns>
        public async Task<List<LogAuditingItem>> GetLatestAuditingLogsForCompany(int companyId, bool includeData = false)
        {
            var logs = new List<LogAuditingItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_tools_latest_auditlogs_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {

                        logs.Add(CreateOrFillLogAuditingItemFromReader(dr, includeData: includeData));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetLatestAuditintLogsForCompany(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return logs;
        }

        public async Task<List<LogShortOutput>> GetLatestESLogs()
        {
            var logs = new List<LogShortOutput>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                using (dr = await _manager.GetDataReader("get_tools_latest_logs_es", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        logs.Add(new LogShortOutput() { CreatedOn = Convert.ToDateTime(dr["created_on"]), Message = dr["description"].ToString() });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetLatestESLogs(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return logs;
        }


        /// <summary>
        /// GetDatabaseSupportedTimezones; Get all timezones that can be used. This list is needed for correct configuration of a company.
        /// </summary>
        /// <returns>A list of all timezones</returns>
        public async Task<List<DatabaseTimezoneItem>> GetDatabaseSupportedTimezones()
        {
            var items = new List<DatabaseTimezoneItem>();
            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_tools_database_timezones", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        items.Add(CreateOrFillDatabaseTimezoneItemFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetDatabaseSupportedTimezones(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY))
                {
                    this.Exceptions.Add(ex);
                }
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }
            }

            return items;
        }

        /// <summary>
        /// Get a list of .net core related timezones
        /// </summary>
        /// <returns>A list of timezone items</returns>
        public async Task<List<DatabaseTimezoneItem>> GetCoreSupportedTimezones()
        {
            var items = new List<DatabaseTimezoneItem>();
            var timeZones = TimeZoneInfo.GetSystemTimeZones();

            foreach (var timeZone in timeZones)
            {
                DatabaseTimezoneItem timezoneItem = new DatabaseTimezoneItem();
                timezoneItem.IsDayLightSavingsTime = timeZone.SupportsDaylightSavingTime;
                timezoneItem.Name = timeZone.Id;
                timezoneItem.UtcOffset = timeZone.BaseUtcOffset.ToString();
                timezoneItem.Abbrivation = timeZone.StandardName;
                items.Add(timezoneItem);
            }

            await Task.CompletedTask;

            items = items.OrderBy(x => x.Name).ToList();
            return items;
        }

        /// <summary>
        /// GetSupportedLanguages; Get a list of strings containing all languages that are active.
        /// </summary>
        /// <returns>A list of active languages.</returns>
        public async Task<string> GetSupportedLanguages()
        {
            var output = "";
            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_settingkey", "TECH_ENABLED_LANGUAGES"));

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var values = dr["settingvalue"].ToString();
                            if (values != null)
                            {
                                output = values;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetSupportedLanguages(): ", ex.Message));

                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">The id of the company which to reduce the generation configs for</param>
        /// <returns></returns>
        public async Task<bool> ReduceTaskGenerationConfigsForCompany(int companyId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var rowsaffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("reduce_generation_configs_for_company", parameters: parameters, commandType: CommandType.StoredProcedure));

            return (rowsaffected > 0);
        }

        /// <summary>
        /// ResetConnectionPool; Reset conncetion pools based on the NpgSqlConnection
        /// </summary>
        /// <param name="resetAll">Reset all bool</param>
        /// <returns>Reset connection pool if needed. Only use for debug option.</returns>
        public async Task<bool> ResetConnectionPool(bool resetAll)
        {
            try
            {
                if (!resetAll)
                {
                    var conn = await _manager.GetConnection();
                    NpgsqlConnection.ClearPool(conn);
                }
                else
                {
                    NpgsqlConnection.ClearAllPools();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.ResetConnectionPool(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY))
                {
                    this.Exceptions.Add(ex);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// CreateOrFillLogAuditingItemFromReader; Create a log item based on a reader.
        /// </summary>
        /// <param name="dr">Reader containing data</param>
        /// <param name="logitem">LogItem that is supplied or will be created</param>
        /// <param name="includeData">Include mutation data yes or no.</param>
        /// <returns>LogItem</returns>
        private LogAuditingItem CreateOrFillLogAuditingItemFromReader(NpgsqlDataReader dr, LogAuditingItem logitem = null, bool includeData = false)
        {
            if (logitem == null) logitem = new LogAuditingItem();

            if (dr["id"] != DBNull.Value)
            {
                logitem.Id = Convert.ToInt64(dr["id"]);
            }

            if (includeData) //only include if supplied.
            {
                if (dr["original_object"] != DBNull.Value && !string.IsNullOrEmpty(dr["original_object"].ToString()))
                {
                    logitem.OriginalObject = dr["original_object"].ToString();
                }

                if (dr["mutated_object"] != DBNull.Value && !string.IsNullOrEmpty(dr["mutated_object"].ToString()))
                {
                    logitem.MutatedObject = dr["mutated_object"].ToString();
                }
            }

            if (dr["object_type"] != DBNull.Value && !string.IsNullOrEmpty(dr["object_type"].ToString()))
            {
                logitem.ObjectType = dr["object_type"].ToString();
            }

            if (dr["object_id"] != DBNull.Value)
            {
                logitem.ObjectId = Convert.ToInt32(dr["object_id"]);
            }

            if (dr["company_id"] != DBNull.Value)
            {
                logitem.CompanyId = Convert.ToInt32(dr["company_id"]);
            }

            if (dr["user_id"] != DBNull.Value)
            {
                logitem.UserId = Convert.ToInt32(dr["user_id"]);
            }

            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                logitem.Description = dr["description"].ToString();
            }

            if (dr["created_on"] != DBNull.Value)
            {
                logitem.CreatedOnUTC = Convert.ToDateTime(dr["created_on"]);
            }

            return logitem;
        }

        /// <summary>
        /// CreateOrFillDatabaseTimezoneItemFromReader; Create a DatabaseTimezoneItem from reader;
        /// </summary>
        /// <param name="dr">Reader containing the data</param>
        /// <param name="databaseTimezoneItem">Item to fill (if not supplied, create)</param>
        /// <returns>Filled DatabaseTimezoneItem</returns>
        private DatabaseTimezoneItem CreateOrFillDatabaseTimezoneItemFromReader(NpgsqlDataReader dr, DatabaseTimezoneItem databaseTimezoneItem = null)
        {
            if (databaseTimezoneItem == null) databaseTimezoneItem = new DatabaseTimezoneItem();

            if (dr["abbrev"] != DBNull.Value)
            {
                databaseTimezoneItem.Abbrivation = dr["abbrev"].ToString();
            }

            if (dr["name"] != DBNull.Value)
            {
                databaseTimezoneItem.Name = dr["name"].ToString();
            }

            if (dr["utc_offset"] != DBNull.Value)
            {
                databaseTimezoneItem.UtcOffset = dr["utc_offset"].ToString();
            }

            if (dr["is_dst"] != DBNull.Value)
            {
                databaseTimezoneItem.IsDayLightSavingsTime = Convert.ToBoolean(dr["is_dst"]);
            }

            return databaseTimezoneItem;
        }

        #region - raw data-
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="rawReference"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task<RawData> GetRawData(int companyId, string rawReference, DateTime startDateTime, DateTime endDateTime)
        {
            var rawProcedure = string.Concat("raw_", rawReference);
            var rawData = new RawData();
            try
            {
                if (startDateTime == DateTime.MinValue)
                {
                    startDateTime = DateTime.Now.Date;
                }

                if (endDateTime == DateTime.MinValue)
                {
                    endDateTime = DateTime.Now.AddDays(1).Date;
                }

                var parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_startdate", startDateTime));
                parameters.Add(new NpgsqlParameter("@_enddate", endDateTime));

                var dt = await _manager.GetDataTable(procedureNameOrQuery: rawProcedure,
                                                       parameters: parameters,
                                                       dataTableName: rawReference.ToUpper());

                if (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    rawData.Columns = new List<string>();
                    rawData.ColumnTypes = new List<string>();
                    rawData.Data = new List<List<string>>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        rawData.Columns.Add(column.ColumnName.ToString());
                        rawData.ColumnTypes.Add(column.DataType.ToString());
                    }

                    foreach (DataRow datarow in dt.Rows)
                    {
                        var row = new List<string>();
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            row.Add(datarow[i].ToString());
                        }
                        rawData.Data.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetRawData(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return rawData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="rawReference"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task<RawData> GetRawDataFromDataWarehouse(int companyId, string rawReference, DateTime startDateTime, DateTime endDateTime)
        {
            var rawData = new RawData();

            var rawProcedure = string.Concat("raw_", rawReference);

            var connectionDwString = _manager.GetWriterConnection().Result.ConnectionString;

            //TODO, replace with full connection string when config of API has been changes on deployment. 
            //currently same user for connections can be used. 

            //NOTE: THIS WILL BREAK IF THE DW DB IS ON A DIFFERENT SERVER THAN THE PRODUCTION DATABASE!!!
            if (!string.IsNullOrEmpty(connectionDwString))
            {
                connectionDwString = connectionDwString.Replace("Database=ezgo", "Database=ez_dw");
            }

            NpgsqlConnection connDW = new NpgsqlConnection(connectionDwString);
            try
            {
                await connDW.OpenAsync();

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_startdate", startDateTime));
                parameters.Add(new NpgsqlParameter("@_enddate", endDateTime));

                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText:DataConnectorHelper.WrapFunctionCommand(rawProcedure, parameters), connDW))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    NpgsqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    await dr.CloseAsync(); dr = null;
                    if (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0)
                    {
                        rawData.Columns = new List<string>();
                        rawData.ColumnTypes = new List<string>();
                        rawData.Data = new List<List<string>>();

                        foreach (DataColumn column in dt.Columns)
                        {
                            rawData.Columns.Add(column.ColumnName.ToString());
                            rawData.ColumnTypes.Add(column.DataType.ToString());
                        }

                        foreach (DataRow datarow in dt.Rows)
                        {
                            var row = new List<string>();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                row.Add(datarow[i].ToString());
                            }
                            rawData.Data.Add(row);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred GetRawDataFromDataWarehouse()");
                if (connDW != null)
                {
                    await connDW.CloseAsync();
                    connDW.Close();
                    connDW = null;
                }
            }
            finally
            {
                if (connDW != null)
                {
                    await connDW.CloseAsync();
                    connDW.Close();
                    connDW = null;
                }
            }

            return rawData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<CalendarSchedule> GetRawScheduleData(int companyId, DateTime startDateTime, DateTime endDateTime)
        {

            var shifts = await RetrieveShifts();
            var timezones = await RetrieveCompanyTimeZones();
            var exportScheduleHoldings = await RetrieveHoldingSchedules();
            var exportScheduleCompanies = await RetrieveCompanySchedules();
            var taskGenerationRunnableType = await _taskGenerationManager.GetRunnableType();
            var companies = await RetrieveCompanies();
            var holdings = await RetrieveHoldings();
            var taskGenerationTimeNumber = taskGenerationRunnableType.ToUpper() == "MINUTES" || taskGenerationRunnableType.ToUpper() == "MINUTE" ?
                                                await _taskGenerationManager.GetRunnableMinutes() :
                                                await _taskGenerationManager.GetRunnableHours();

            //paletton: https://paletton.com/#uid=7000u0kllllaFw0g0qFqFg0w0aF

            List<ExportSchedule> possibleExportSchedules = new List<ExportSchedule>();
            if (exportScheduleHoldings != null && exportScheduleHoldings.Count > 0) possibleExportSchedules.AddRange(exportScheduleHoldings);
            if (exportScheduleCompanies != null && exportScheduleCompanies.Count > 0) possibleExportSchedules.AddRange(exportScheduleCompanies);

            //set defaults
            var schedule = new CalendarSchedule();
            schedule.Days = new List<CalendarDay>();
            for (var i = 0; i < 7; i++)
            {
                var day = new CalendarDay();
                day.ScheduleItems = new List<CalendarItem>();
                day.DayName = ((DayOfWeek)i).ToString();
                day.DayNumber = i;
                schedule.Days.Add(day);
            }


            if (taskGenerationRunnableType.ToUpper() == "MINUTES")
            {
                for (var i = 0; i < 7; i++)
                {
                    var day = schedule.Days.Where(x => x.DayNumber == i).FirstOrDefault();

                    for (var h = 0; h < 24; h++)
                    {
                        foreach (var m in taskGenerationTimeNumber)
                        {
                            day.ScheduleItems.Add(new CalendarItem() { Color = "#AA3939", Title = "Task Generation", Description = "Task Generation Process Execution based on minute schedule.", StartTime = string.Concat(h.ToString("00"), ":", m.ToString("00")), EndTime = "N/A", ItemType = "TASKGENERATION" });
                        }
                    }


                }
            }
            if (taskGenerationRunnableType.ToUpper() == "HOURS")
            {
                for (var i = 0; i < 7; i++)
                {
                    var day = schedule.Days.Where(x => x.DayNumber == i).FirstOrDefault();

                    foreach (var h in taskGenerationTimeNumber)
                    {
                        day.ScheduleItems.Add(new CalendarItem() { Color = "#AA3939", Title = "Task Generation", Description = "Task Generation Process Execution based on hour schedule.", StartTime = string.Concat(h.ToString("00"), ":", "00"), EndTime = "N/A", ItemType = "TASKGENERATION" });
                    }
                }
            }

            foreach (var exportSchedule in possibleExportSchedules)
            {
                if (exportSchedule.ScheduleItems != null)
                {
                    foreach (var exportScheduleItem in exportSchedule.ScheduleItems)
                    {
                        if (exportScheduleItem.DayOfWeek != null && exportScheduleItem.DayOfWeek.Count > 0)
                        {
                            foreach (var i in exportScheduleItem.DayOfWeek)
                            {
                                var day = schedule.Days.Where(x => x.DayNumber == i).FirstOrDefault();

                                var starttime = CalculateRunTime(exportScheduleItem.ScheduleRunTime, exportSchedule.TimeZone).ToString("0000");
                                starttime = starttime.Insert(2, ":");
                                day.ScheduleItems.Add(new CalendarItem()
                                {
                                    Color = "#FF6666",
                                    Title = "Export",
                                    Description = string.Format("Export for holding: [{3}]({0}) | company: [{1}]({2})", exportSchedule.HoldingId, exportSchedule.CompanyId != 0 ? companies.Where(x => x.Id == exportSchedule.CompanyId).FirstOrDefault().Name : "", exportSchedule.CompanyId, exportSchedule.HoldingId != 0 ? holdings.Where(x => x.Id == exportSchedule.HoldingId).FirstOrDefault().Name : ""),
                                    StartTime = starttime,
                                    EndTime = "N/A",
                                    ItemType = "EXPORT"
                                });

                            }
                        }
                        else
                        {
                            for (var i = 0; i < 7; i++)
                            {

                                var day = schedule.Days.Where(x => x.DayNumber == i).FirstOrDefault();

                                var starttime = CalculateRunTime(exportScheduleItem.ScheduleRunTime, exportSchedule.TimeZone).ToString("0000");
                                starttime = starttime.Insert(2, ":");
                                day.ScheduleItems.Add(new CalendarItem()
                                {
                                    Color = "#FF6666",
                                    Title = "Export",
                                    Description = string.Format("Export for holding: [{3}]({0}) | company: [{1}]({2})", exportSchedule.HoldingId, exportSchedule.CompanyId != 0 ? companies.Where(x => x.Id == exportSchedule.CompanyId).FirstOrDefault().Name : "", exportSchedule.CompanyId, exportSchedule.HoldingId != 0 ? holdings.Where(x => x.Id == exportSchedule.HoldingId).FirstOrDefault().Name : ""),
                                    StartTime = starttime,
                                    EndTime = "N/A",
                                    ItemType = "EXPORT"
                                });

                            }
                        }

                    }
                }

            }

            foreach (var shift in shifts)
            {
                var convertedShift = CalculateScheduleTimeOfShift(currentShift: shift, "Europe/Amsterdam");

                var day = schedule.Days.Where(x => x.DayNumber == convertedShift.ConvertedShiftDay).FirstOrDefault();

                var starttime = convertedShift.ConvertedShiftStart;

                starttime = starttime.Insert(2, ":");

                starttime = starttime.Insert(2, ":");
                if ((shift.CompanyId.HasValue && companies.Select(x => x.Id).ToList().Contains(shift.CompanyId.Value)))
                {
                    day.ScheduleItems.Add(new CalendarItem() { Color = "#AA6C39", Title = "Shift", Description = string.Format("Shift for [{0}]({4}) | Original customer start {1} - end {2} on {3}", companies.Where(x => x.Id == convertedShift.CompanyId).FirstOrDefault().Name, convertedShift.Start, convertedShift.End, ((DayOfWeek)(convertedShift.Day == 0 ? 6 : convertedShift.Day - 1)).ToString(), convertedShift.CompanyId), StartTime = convertedShift.ConvertedShiftStart, EndTime = convertedShift.ConvertedShiftEnd, ItemType = "SHIFT" });
                }
             }

            await Task.CompletedTask;

            return schedule;
        }
        #endregion

        #region - data retrieval for raw schedule -
        private async Task<List<ExportSchedule>> RetrieveHoldingSchedules()
        {
            var output = new List<ExportSchedule>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_holding_setting_schedules", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var exportSchedule = new ExportSchedule();
                        if (dr["json_data"] != DBNull.Value && !string.IsNullOrEmpty(dr["json_data"].ToString()))
                        {
                            var json = dr["json_data"].ToString();
                            exportSchedule = json.ToObjectFromJson<ExportSchedule>();
                        }
                        exportSchedule.HoldingId = Convert.ToInt32(dr["holding_id"]);

                        output.Add(exportSchedule);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.RetrieveHoldingSchedules(): ", ex.Message));

                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        private async Task<List<ExportSchedule>> RetrieveCompanySchedules()
        {
            var output = new List<ExportSchedule>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_companies_setting_schedules", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var exportSchedule = new ExportSchedule();
                        if (dr["json_data"] != DBNull.Value && !string.IsNullOrEmpty(dr["json_data"].ToString()))
                        {
                            var json = dr["json_data"].ToString();
                            exportSchedule = json.ToObjectFromJson<ExportSchedule>();
                        }
                        exportSchedule.CompanyId = Convert.ToInt32(dr["company_id"]);

                        output.Add(exportSchedule);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.RetrieveCompanySchedules(): ", ex.Message));

                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        private async Task<List<CalendarShift>> RetrieveShifts()
        {
            var output = new List<CalendarShift>();

            NpgsqlDataReader dr = null;

            try
            {

                using (dr = await _manager.GetDataReader("get_shifts_all_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var shift = new CalendarShift();

                        shift.Day = Convert.ToInt32(dr["day"]);
                        shift.Weekday = Convert.ToInt32(dr["weekday"]);
                        shift.CompanyId = Convert.ToInt32(dr["company_id"]);
                        if (dr["area_id"] != DBNull.Value)
                        {
                            shift.AreaId = Convert.ToInt32(dr["area_id"]);
                        }
                        shift.Start = dr["start"].ToString();
                        shift.End = dr["end"].ToString();
                        if (dr["shiftnr"] != DBNull.Value)
                        {
                            shift.ShiftNr = Convert.ToInt32(dr["shiftnr"]);
                        }
                        output.Add(shift);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.RetrieveShifts(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        private async Task<List<CompanyBasic>> RetrieveCompanies()
        {
            var output = new List<CompanyBasic>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var company = new CompanyBasic();

                        company.Id = Convert.ToInt32(dr["id"]);
                        company.Name = dr["name"].ToString();
                        if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
                        {
                            company.Picture = dr["picture"].ToString();
                        }
                        output.Add(company);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.RetrieveCompanies(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        private async Task<List<Holding>> RetrieveHoldings()
        {
            var output = new List<Holding>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_holdings", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var holding = new Holding();

                        holding.Id = Convert.ToInt32(dr["id"]);
                        holding.Name = dr["name"].ToString();

                        output.Add(holding);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.RetrieveCompanies(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        private async Task<List<SettingResourceItem>> RetrieveCompanyTimeZones()
        {
            var output = new List<SettingResourceItem>();

            NpgsqlDataReader dr = null;

            try
            {

                using (dr = await _manager.GetDataReader("get_timezones_all_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var setting = new SettingResourceItem();

                        setting.CompanyId = Convert.ToInt32(dr["company_id"]);
                        setting.Value = dr["value"].ToString();

                        output.Add(setting);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetCompanyTimeZones(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        private CalendarShift CalculateScheduleTimeOfShift(CalendarShift currentShift, string timeZone)
        {
            if (!string.IsNullOrEmpty(timeZone))
            {
                try
                {
                    var systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                    DateTime currentStartDateTime = new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: 15);
                    currentStartDateTime = currentStartDateTime.AddHours(Convert.ToInt32(currentShift.Start.Substring(0, 2)));
                    currentStartDateTime = currentStartDateTime.AddMinutes(Convert.ToInt32(currentShift.Start.Substring(3, 2)));

                    var dateTimeStartUnspec = DateTime.SpecifyKind(currentStartDateTime, DateTimeKind.Unspecified);
                    var utcStartDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeStartUnspec, systemTimeZone);

                    currentShift.ConvertedShiftStart = utcStartDateTime.ToString("HH:mm");

                    Console.WriteLine(string.Concat(currentShift.Start, "-", currentShift.ConvertedShiftStart));

                    DateTime currentEndDateTime = new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: 15);
                    currentEndDateTime = currentEndDateTime.AddHours(Convert.ToInt32(currentShift.End.Substring(0, 2)));
                    currentEndDateTime = currentEndDateTime.AddMinutes(Convert.ToInt32(currentShift.End.Substring(3, 2)));

                    var dateTimeEndUnspec = DateTime.SpecifyKind(currentEndDateTime, DateTimeKind.Unspecified);
                    var utcEndDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeEndUnspec, systemTimeZone);

                    currentShift.ConvertedShiftEnd = utcEndDateTime.ToString("HH:mm");

                    if (utcStartDateTime.Date < currentStartDateTime.Date)
                    {
                        if ((currentShift.Day) == 1) { currentShift.ConvertedShiftDay = 6; }
                        else { currentShift.ConvertedShiftDay = (currentShift.Day - 1) - 1; }
                    }
                    else if (utcStartDateTime.Date > currentStartDateTime.Date)
                    {
                        if ((currentShift.Day) == 7) { currentShift.ConvertedShiftDay = 0; }
                        else { currentShift.ConvertedShiftDay = (currentShift.Day - 1) + 1; }
                    }
                    else
                    {
                        currentShift.ConvertedShiftDay = currentShift.Day - 1;
                        if (currentShift.ConvertedShiftDay < 0) { currentShift.ConvertedShiftDay = 6; }
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    //ignore offset, timezone not correctly configured.
                }

            }
            return currentShift;
        }

        private int CalculateRunTime(int currentTime, string timeZone)
        {
            if (!string.IsNullOrEmpty(timeZone))
            {
                if (!string.IsNullOrEmpty(timeZone))
                {
                    try
                    {
                        DateTime currentDateTime = DateTime.Now.Date;
                        currentDateTime = currentDateTime.AddHours(Convert.ToInt32(currentTime.ToString("0000").Substring(0, 2)));
                        currentDateTime = currentDateTime.AddMinutes(Convert.ToInt32(currentTime.ToString("0000").Substring(2, 2)));

                        var dateTimeUnspec = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Unspecified);
                        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspec, TimeZoneInfo.FindSystemTimeZoneById(timeZone));

                        return Convert.ToInt32(utcDateTime.ToString("HHmm"));
                    }
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
                        //ignore offset, timezone not correctly configured.
                    }
                }
            }

            return currentTime;
        }
        #endregion

        #region - bulk updates -
        public async Task<int> CleanupLoggingTable()
        {
            // cleanup query for logging, delete all records older than last 50000;
            //NOTE, this is not a specific SP, due to this query will change during time depending on requirements.
            var output = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("WITH rows AS (SELECT id FROM logging_log WHERE id NOT IN (SELECT id FROM logging_log ORDER BY id DESC LIMIT 50000) ORDER BY id LIMIT 1000) DELETE FROM logging_log WHERE id IN (SELECT id FROM rows);", commandType: CommandType.Text));
            return output;
        }

        public async Task<int> CleanupLoggingGenerationTable()
        {
            //cleanup query for logging, delete all records older than last 50000;
            //NOTE, this is not a specific SP, due to this query will change during time depending on requirements.
            var output = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("WITH rows AS (SELECT id FROM logging_generation WHERE id NOT IN (SELECT id FROM logging_generation ORDER BY id DESC LIMIT 50000) ORDER BY id LIMIT 1000) DELETE FROM logging_generation WHERE id IN (SELECT id FROM rows);", commandType: CommandType.Text));
            return output;
        }

        public async Task<int> CleanupLoggingRequestResponseTable()
        {
            //cleanup query for logging, delete all records older than last 50000;
            //NOTE, this is not a specific SP, due to this query will change during time depending on requirements.
            var output = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("WITH rows AS (SELECT id FROM logging_requestresponse WHERE id NOT IN (SELECT id FROM logging_requestresponse ORDER BY id DESC LIMIT 50000) ORDER BY id LIMIT 1000) DELETE FROM logging_requestresponse WHERE id IN (SELECT id FROM rows);", commandType: CommandType.Text));
            return output;
        }

        public async Task<int> CleanupLoggingMigrationTable()
        {
            //cleanup query for logging, delete all records older than last 50000;
            //NOTE, this is not a specific SP, due to this query will change during time depending on requirements.
            var output = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("WITH rows AS (SELECT id FROM logging_migration WHERE id NOT IN (SELECT id FROM logging_migration ORDER BY id DESC LIMIT 50000) ORDER BY id LIMIT 1000) DELETE FROM logging_migration WHERE id IN (SELECT id FROM rows);", commandType: CommandType.Text));
            return output;
        }

        public async Task<int> CleanupLoggingSecurityTable()
        {
            //cleanup query for logging, delete all records older than last 50000;
            //NOTE, this is not a specific SP, due to this query will change during time depending on requirements.
            var output = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("WITH rows AS (SELECT id FROM logging_security WHERE id NOT IN (SELECT id FROM logging_security ORDER BY id DESC LIMIT 50000) ORDER BY id LIMIT 1000) DELETE FROM logging_security WHERE id IN (SELECT id FROM rows);", commandType: CommandType.Text));
            return output;
        }

        public async Task<int> CleanupLoggingExportTable()
        {
            //cleanup query for logging, delete all records older than last 50000;
            //NOTE, this is not a specific SP, due to this query will change during time depending on requirements.
            var output = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("WITH rows AS (SELECT id FROM logging_export WHERE id NOT IN (SELECT id FROM logging_export ORDER BY id DESC LIMIT 50000) ORDER BY id LIMIT 1000) DELETE FROM logging_export WHERE id IN (SELECT id FROM rows);", commandType: CommandType.Text));
            return output;
        }


        public async Task<int> UpdateModifiedActions(ToolFilter toolFilter)
        {


            List<int> actions = new List<int>();
            actions.AddRange(await GetActionIds(companyId: toolFilter.CompanyId, fromDate: toolFilter.StartAt.Value, toDate: toolFilter.EndAt.Value));
            var resultCount = 0;
            foreach (int id in actions)
            {
                resultCount += await UpdateModifiedAtAction(companyId: toolFilter.CompanyId, actionId: id);
            }
            return resultCount;
        }


        public async Task<bool> GenerateUserProfileGuids()
        {
            //gernate GUID for user records where no GUID exists, should be used once when GUIDS are correctly implemented or else use multiple times. 
            NpgsqlDataReader dr = null;
            int count = 0;

            try
            {
                using (dr = await _manager.GetDataReader("SELECT id FROM profiles_user WHERE guid IS NULL", commandType: System.Data.CommandType.Text))
                {
                    while (await dr.ReadAsync())
                    {
                        await _manager.ExecuteNonQueryAsync(string.Format("UPDATE profiles_user SET GUID = '{0}' WHERE id = {1};", Guid.NewGuid().ToString(), dr["id"].ToString()), commandType: CommandType.Text);
                        count = count + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GenerateUserProfileGuids(): ", ex.Message));
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return count > 0;
        }

        public async Task<int> UpdateModifiedBaseStructures(int companyId, int holdingId)
        {
            if (holdingId > 0)
            {
                List<int> companies = new List<int>();
                companies.AddRange(await GetHoldingCompanies(holdingId: holdingId));
                var resultCount = 0;
                foreach (int id in companies)
                {
                    resultCount += await UpdateModifiedBaseStructureCompany(companyId: id);
                }
                return resultCount;

            }
            else
            {
                return await UpdateModifiedBaseStructureCompany(companyId: companyId);
            }
        }

        private async Task<int> UpdateModifiedBaseStructureCompany(int companyId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            var output = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_modified_base_structures", parameters: parameters));
            return output;
        }

        private async Task<int> UpdateModifiedAtAction(int companyId, int actionId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);

            //add auditing
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            var output = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_action_modifiedat", parameters: parameters));

            if (output > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action.ToString(), objectId: actionId, userId: 0, companyId: companyId, description: "Modification date update by SYSTEM.");

            }

            return output;
        }

        private async Task<List<int>> GetHoldingCompanies(int holdingId)
        {
            var output = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_holding_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (Convert.ToInt32(dr["holding_id"]) == holdingId) //only add specific data
                        {
                            output.Add(Convert.ToInt32(dr["company_id"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetHoldingCompanies(): ", ex.Message));
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        private async Task<List<int>> GetActionIds(int companyId, DateTime fromDate, DateTime toDate)
        {
            var output = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", companyId));

                using (dr = await _manager.GetDataReader("get_action_ids_by_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        if (Convert.ToInt32(dr["companyId"]) == companyId) //only add specific data
                        {
                            output.Add(Convert.ToInt32(dr["action_id"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.GetHoldingCompanies(): ", ex.Message));
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }
        #endregion

        #region - user tooling -
        /// <summary>
        /// CreateServiceUserForCompany; Will generate a service account with pre-generated data; 
        /// </summary>
        /// <returns>String containing name of generated user for use in display.</returns>
        public async Task<string> CreateServiceUserForCompany(int companyId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("create_serviceaccount_user", parameters: parameters));

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), possibleId);
            await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.profiles_user.ToString(), objectId: possibleId, userId: 0, companyId: companyId, description: "Added service account by EZF.");

            var generatedName = await RetrieveServiceAccountName(companyId: companyId, userId: possibleId);
            return generatedName;
        }

        /// <summary>
        /// CreateSystemUsers; Create system users with all companies.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateSystemUsers()
        {
            NpgsqlDataReader dr = null;
            int count = 0;

            try
            {
                List<int> ids = new List<int>();
                using (dr = await _manager.GetDataReader("SELECT id FROM companies_company", commandType: System.Data.CommandType.Text))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr["id"].ToString()));
                       
                    }
                }

                foreach(var cid in ids)
                {
                    await _manager.ExecuteNonQueryAsync(string.Format("SELECT * FROM create_system_user({0});", cid.ToString()), commandType: CommandType.Text);
                    count = count + 1;
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ToolsManager.CreateSystemUsers(): ", ex.Message));
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return count > 0;
        }

        /// <summary>
        /// RetrieveServiceAccountName; retrieve service account based on id; method should only be used for internal usage within other functionalities.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<string> RetrieveServiceAccountName(int companyId, int userId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            var output = (await _manager.ExecuteScalarAsync("retrieve_serviceaccount_user_name", parameters: parameters)).ToString();
            return output;
        }
        #endregion

        #region - logging -
        public async Task WriteToLog(string domain, string path, string query, string status, string header, string request, string response)
        {
            await _databaseLogWriter.WriteToLog(domain: domain, path: path, query: query, status: status, header: header, request: request, response: response);
        }
        #endregion

        #region - -
        public async Task<bool> FixAudtingData()
        {
            var result = await _manager.ExecuteScalarAsync("fix_auditing_data_task");
            return (Convert.ToInt32(result) > 0);
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
