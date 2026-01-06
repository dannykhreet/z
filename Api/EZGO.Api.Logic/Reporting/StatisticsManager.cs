using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Reports;
using EZGO.Api.Models.Stats;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace EZGO.Api.Logic.Exporting
{
    /// <summary>
    /// StatisticsManager; Statistics manager contains functionality to get basic statistics based on database information. These can be per company, per user or statistics that span the entire database
    /// (which then can be used in management portals). Some of these statistics can also be used within the reporting parts of the several client applications.
    /// NOTE! If used please call this manager from the reporting manager and make some kind of pass through call.
    /// NOTE! Only use this manager directly in specific statistics functionality or controllers.
    /// NOTE! This manager can contain a lot of specific smaller calls. Use it wisely.
    /// </summary>
    public class StatisticsManager : BaseManager<StatisticsManager>, IStatisticsManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        #endregion

        #region - constructor(s) -
        public StatisticsManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ILogger<StatisticsManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
        }
        #endregion

        /// <summary>
        /// GetTotalsOverviewByCompanyAsync; Get a totals overview based on raw database information. Can be used for dashboards and a like.
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <returns>A list of statistic items containing a number, possbile reference id and somekind of type.</returns>
        public async Task<List<StatisticTypeItem>> GetTotalsOverviewByCompanyAsync(int companyId = 0)
        {
            var output = new List<StatisticTypeItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("report_statistics_overview_totals", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new StatisticTypeItem() { Type = dr["type"].ToString(), CountNr = Convert.ToInt32(dr["count_nr"]) });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetTotalsOverviewByCompany(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetTotalsOverviewByCompanyAsync; Get a totals overview based on raw database information. Can be used for dashboards and a like.
        /// </summary>
        /// <param name="holdingId">The HoldingId (holding of connected user)</param>
        /// <returns>A list of statistic items containing a number, possbile reference id and somekind of type.</returns>
        public async Task<List<StatisticTypeItem>> GetTotalsOverviewByHoldingAsync(int holdingId = 0)
        {
            var output = new List<StatisticTypeItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));

                using (dr = await _manager.GetDataReader("report_statistics_holding_overview_totals", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new StatisticTypeItem() { Type = dr["type"].ToString(), CountNr = Convert.ToInt32(dr["count_nr"]) });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetTotalsOverviewByHoldingAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetUserActivityTotalsByCompanyAsync; Get the basic user activity numbers, these are totals of the number of object in the database that are active and are connected to the user.
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <returns>A list of statistic items containing a number, possbile reference id and somekind of type.</returns>
        public async Task<List<StatisticUserItem>> GetUserActivityTotalsByCompanyAsync(int companyId)
        {
            var output = new List<StatisticUserItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("report_statistics_user_activity_totals", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(new StatisticUserItem() { Name = dr["name"].ToString(), CountNr = Convert.ToInt32(dr["count_nr"]), UserId = Convert.ToInt32(dr["user_id"]) });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetUserActivityTotalsByCompany(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetGenericStatisticsCollectionAsync; Will get a collection of StatisticGenericItems;
        /// Every item will contain an Id and Count. Depending on what stored procedure reference is used the Id can reference an totally different database object.
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <param name="auditTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="checklistTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="taskTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="storedProcedureReference">Reference to database functionality. </param>
        /// <returns>a List of StatisticGenericItems</returns>
        public async Task<List<StatisticGenericItem>> GetGenericStatisticsCollectionAsync(int companyId, string storedProcedureReference, DateTime? timestamp = null, int? areaId = null, int? auditTemplateId = null, int? checklistTemplateId = null, int? taskTemplateId = null, int? timespanInDays = null)
        {
            //StoredProcedureReference must be available in StatisticReferences or else method will return null;
            if (Settings.StatisticSettings.StatisticReferences.Contains(storedProcedureReference))
            {
                var storedProcedureName = string.Concat("report_statistics_", storedProcedureReference.ToLower());
                return await GetStatistics(companyId: companyId, storedProcedureName: storedProcedureName, timestamp: timestamp, areaId: areaId, auditTemplateId: auditTemplateId, checklistTemplateId: checklistTemplateId, taskTemplateId: taskTemplateId, timespanInDays: timespanInDays);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetAverageStatisticsCollectionAsync; Will get a collection of StatisticAverageItems;
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <param name="storedProcedureReference">Reference to database functionality. </param>
        /// <param name="auditTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="checklistTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="taskTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <returns>a List of StatisticAverageItems</returns>
        public async Task<List<StatisticGenericItem>> GetAverageStatisticsCollectionAsync(int companyId, string storedProcedureReference, int? areaId = null, int? auditTemplateId = null, int? checklistTemplateId = null, int? taskTemplateId = null, int? timespanInDays = null)
        {
            //StoredProcedureReference must be available in StatisticReferences or else method will return null;
            if (Settings.StatisticSettings.StatisticReferences.Contains(storedProcedureReference))
            {
                var storedProcedureName = string.Concat("report_statistics_", storedProcedureReference.ToLower());
                return await GetAverageStatistics(companyId: companyId, storedProcedureName: storedProcedureName, areaId: areaId, auditTemplateId: auditTemplateId, checklistTemplateId: checklistTemplateId, taskTemplateId: taskTemplateId, timespanInDays: timespanInDays);
            }
            else
            {
                return null;
            }
        }

        public async Task<TasksReport> GetTasksStatisticsAsync(int companyId, DateTime? timestamp = null, int? areaId = null)
        {
            var storedProcedureName = "report_statistics_taskscount_gen4";
            var output = new TasksReport();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                
                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillTasksReportFromReader(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetTasksStatisticsAsync(", storedProcedureName, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
            }

            return output;
        }

        public async Task<AuditsReport> GetAuditsStatisticsAsync(int companyId, DateTime? timestamp = null, int? areaId = null, int? templateId = null)
        {
            var storedProcedureName = "report_statistics_auditsaverage_gen4";
            AuditsReport output = null;

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                if(templateId.HasValue && templateId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_templateid", templateId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillAuditsReportFromReader(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetAuditsStatisticsAsync(", storedProcedureName, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
            }

            return output;
        }

        public async Task<ChecklistsReport> GetChecklistsStatisticsAsync(int companyId, DateTime? timestamp = null, int? areaId = null)
        {
            var storedProcedureName = "report_statistics_checklistitemscount_gen4";
            var output = new ChecklistsReport();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillChecklistItemsReportFromReader(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetChecklistsStatisticsAsync(", storedProcedureName, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
            }

            return output;
        }

        public async Task<ActionsReport> GetActionsStatisticsAsync(int companyId, int userId, DateTime? timestamp = null, int? areaId = null)
        {
            var storedProcedureName = "report_statistics_actionscount_gen4";
            var output = new ActionsReport();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (userId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillActionsReportFromReader(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetActionsStatisticsAsync(", storedProcedureName, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
            }

            return output;
        }

        public async Task<ChecklistsReportExtended> GetTaskChecklistsStatisticsExtendedAsync(int companyId, DateTime? timestamp = null, int? areaId = null, string periodType = "last12days", string reportType = "tasks")
        {
            string spNameExtendedSP = "report_statistics_taskscount_extended_gen4";
            string spNameDeviationsSP = "report_statistics_taskscount_deviations_gen4";

            switch (reportType)
            {
                case "checklists":
                    spNameExtendedSP = "report_statistics_checklistitemscount_extended_gen4";
                    spNameDeviationsSP = "report_statistics_checklistitemscount_deviations_gen4";
                    break;
                case "tasks":
                    spNameExtendedSP = "report_statistics_taskscount_extended_gen4";
                    spNameDeviationsSP = "report_statistics_taskscount_deviations_gen4";
                    break;
            }

            var output = new ChecklistsReportExtended();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parametersExtendedSP = new List<NpgsqlParameter>();
                List<NpgsqlParameter> parametersDeviationsSP = new List<NpgsqlParameter>();
                parametersExtendedSP.Add(new NpgsqlParameter("@_companyid", companyId));
                parametersDeviationsSP.Add(new NpgsqlParameter("@_companyid", companyId));

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                if (!string.IsNullOrEmpty(periodType) && (periodType == "last12days" || periodType == "last12weeks" || periodType == "last12months" || periodType == "thisyear"))
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                }

                using (dr = await _manager.GetDataReader(spNameExtendedSP, commandType: CommandType.StoredProcedure, parameters: parametersExtendedSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendMainStatsToChecklistReportExtendedFromReader(dr, checklistsReportExtended: output);
                    }
                }

                using (dr = await _manager.GetDataReader(spNameDeviationsSP, commandType: CommandType.StoredProcedure, parameters: parametersDeviationsSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendDeviationsToChecklistReportExtendedFromReader(dr, checklistsReportExtended: output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetTaskChecklistsStatisticsExtendedAsync(", spNameExtendedSP, ", " + spNameDeviationsSP + "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
            }

            return output;
        }

        public async Task<AuditsReportExtended> GetAuditsStatisticsExtendedAsync(int companyId, DateTime? timestamp = null, int? areaId = null, string periodType = "last12days", int? templateId = null)
        {
            var spNameExtendedSP = "report_statistics_auditscount_extended_gen4";
            var spNameDeviationsSP = "report_statistics_auditscount_deviations_gen4";

            var output = new AuditsReportExtended();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parametersExtendedSP = new List<NpgsqlParameter>();
                List<NpgsqlParameter> parametersDeviationsSP = new List<NpgsqlParameter>();
                parametersExtendedSP.Add(new NpgsqlParameter("@_companyid", companyId));
                parametersDeviationsSP.Add(new NpgsqlParameter("@_companyid", companyId));

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                if (!string.IsNullOrEmpty(periodType) && (periodType == "last12days" || periodType == "last12weeks" || periodType == "last12months" || periodType == "thisyear"))
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                }

                if (templateId.HasValue && templateId.Value > 0)
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_templateid", templateId.Value));
                    parametersDeviationsSP.Add(new NpgsqlParameter("@_templateid", templateId.Value));
                }


                using (dr = await _manager.GetDataReader(spNameExtendedSP, commandType: CommandType.StoredProcedure, parameters: parametersExtendedSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendMainStatsToAuditReportExtendedFromReader(dr, auditsReportExtended: output);
                    }
                }

                using (dr = await _manager.GetDataReader(spNameDeviationsSP, commandType: CommandType.StoredProcedure, parameters: parametersDeviationsSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendDeviationsToAuditReportExtendedFromReader(dr, auditsReportExtended: output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetAuditsStatisticsAsync(", spNameExtendedSP, ", " + spNameDeviationsSP + "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                ;
            }

            return output;
        }

        public async Task<ActionsReportExtended> GetActionsStatisticsExtendedAsync(int companyId, DateTime? timestamp = null, int? areaId = null, string periodType = "last12days")
        {
            var spNameExtendedSP = "report_statistics_actionscount_extended_gen4";
            var spNameOverallSP = "report_statistics_actionscount_overallstats_gen4";
            var spNameTopFiveSP = "report_statistics_actionscount_top5_gen4";

            var output = new ActionsReportExtended();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parametersExtendedSP = _manager.GetBaseParameters(companyId);
                List<NpgsqlParameter> parametersOverallSP = _manager.GetBaseParameters(companyId);
                List<NpgsqlParameter> parametersTopFiveSP = _manager.GetBaseParameters(companyId);

                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                    parametersOverallSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                    parametersTopFiveSP.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parametersOverallSP.Add(new NpgsqlParameter("@_areaid", areaId));
                }

                if (!string.IsNullOrEmpty(periodType))
                {
                    parametersExtendedSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                    parametersOverallSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                    parametersTopFiveSP.Add(new NpgsqlParameter("@_timerangetype", periodType));
                }

                using (dr = await _manager.GetDataReader(spNameExtendedSP, commandType: CommandType.StoredProcedure, parameters: parametersExtendedSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendMainStatsToActionReportExtendedFromReader(dr, actionsReportExtended: output);
                    }
                }

                using (dr = await _manager.GetDataReader(spNameOverallSP, commandType: CommandType.StoredProcedure, parameters: parametersOverallSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendOverallStatsToAuditReportExtendedFromReader(dr, actionsReportExtended: output);
                    }
                }

                using (dr = await _manager.GetDataReader(spNameTopFiveSP, commandType: CommandType.StoredProcedure, parameters: parametersTopFiveSP))
                {
                    while (await dr.ReadAsync())
                    {
                        output = AppendTop5StatsToAuditReportExtendedFromReader(dr, actionsReportExtended: output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetActionsStatisticsAsync(", spNameExtendedSP, ", ", spNameOverallSP, ", ", spNameTopFiveSP, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                ;
            }

            return output;
        }

        /// <summary>
        /// GetDateStatisticsCollectionAsync; Get a list of statistics containing data
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <param name="storedProcedureReference">Reference to database functionality. </param>
        /// <param name="startDateTime">StartDate of range</param>
        /// <param name="endDateTime">End date of range</param>
        /// <returns></returns>
        public async Task<List<StatisticMonthYearItem>> GetDateStatisticsCollectionAsync(int companyId, int holdingId, string storedProcedureReference, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            if (Settings.StatisticSettings.StatisticReferences.Contains(storedProcedureReference))
            {
                var storedProcedureName = holdingId > 0 ? string.Concat("report_stats_h_", storedProcedureReference.ToLower()) : string.Concat("report_statistics_", storedProcedureReference.ToLower());
                var output = new List<StatisticMonthYearItem>();

                NpgsqlDataReader dr = null;

                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    if (holdingId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                    }

                    if (startDateTime.HasValue && startDateTime.Value != DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_startdatetime", startDateTime.Value));
                    }

                    if (endDateTime.HasValue && endDateTime.Value != DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_enddatetime", endDateTime.Value));
                    }

                    using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                    {
                        while (await dr.ReadAsync())
                        {
                            output.Add(CreateOrFillMonthYearStatisticFromReader(dr));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetDateStatisticsCollectionAsync(", storedProcedureName, "):", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
                finally
                {
                    if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
                }

                return output;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="storedProcedureReference"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task<List<StatisticMonthYearItem>> GetDateStatisticsCollectionAsync(string storedProcedureReference, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            if (Settings.StatisticSettings.StatisticReferences.Contains(storedProcedureReference))
            {
                var storedProcedureName = string.Concat("report_statistics_", storedProcedureReference.ToLower());
                var output = new List<StatisticMonthYearItem>();

                NpgsqlDataReader dr = null;

                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                    if (startDateTime.HasValue && startDateTime.Value != DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_startdatetime", startDateTime.Value));
                    }

                    if (endDateTime.HasValue && endDateTime.Value != DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_enddatetime", endDateTime.Value));
                    }

                    using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                    {
                        while (await dr.ReadAsync())
                        {
                            output.Add(CreateOrFillMonthYearStatisticFromReader(dr));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetDateStatisticsCollectionAsync(", storedProcedureName, "):", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
                finally
                {
                    if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
                }

                return output;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetMyStatisticsCollectionAsync; Get statistics for a specific user (usually called by that user)
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <param name="userId">UserId of the my 'data'</param>
        /// <returns>a list of StatisticGenericItem</returns>
        public async Task<List<StatisticGenericItem>> GetMyStatisticsCollectionAsync(int companyId, int userId, int? areaId = null, int? timespanInDays = null)
        {
            var output = new List<StatisticGenericItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                using (dr = await _manager.GetDataReader("report_statistics_my", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillGenericStatisticFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetMyStatisticsCollectionAsync():", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetMyEZFeedStatisticsCollectionAsync; Get EZ Feed statistics for a specific user (usually called by that user)
        /// </summary>
        /// <param name="companyId">The CompanyId (company of connected user)</param>
        /// <param name="userId">UserId of the my 'data'</param>
        /// <returns>a list of StatisticGenericItem</returns>
        public async Task<List<StatisticGenericItem>> GetMyEZFeedStatisticsCollectionAsync(int companyId, int userId)
        {
            var output = new List<StatisticGenericItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("report_statistics_my_ezfeed", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillGenericStatisticFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetMyEZFeedStatisticsCollectionAsync():", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetMyStatisticsCollectionAsync; Get my statistic collection
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="storedProcedureReference">Stored procedure name, to be executed.</param>
        /// <returns>List of StatisticGenericItem</returns>
        public async Task<List<StatisticGenericItem>> GetMyStatisticsCollectionAsync(int companyId, int userId, string storedProcedureReference, int? areaId = null, int? timespanInDays = null)
        {
            //StoredProcedureReference must be available in StatisticReferences or else method will return null;
            if (Settings.StatisticSettings.StatisticReferences.Contains(storedProcedureReference))
            {
                var storedProcedureName = string.Concat("report_statistics_", storedProcedureReference.ToLower());
                return await GetStatistics(companyId: companyId, userId: userId, storedProcedureName: storedProcedureName, areaId: areaId, timespanInDays: timespanInDays);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetLoggingRequestStatisticsCollectionAsync(); Get statistics about the available request and response logging information.
        /// NOTE! this is based on the logging that is recorded, this logging can be reset or disabled at any time, only use for 'general' checking of request.
        /// Do NOT use for any further logic or business related stuff
        /// </summary>
        /// <returns>A list of StatisticGenericItems that contain basic logging information</returns>
        public async Task<List<StatisticGenericItem>> GetLoggingRequestStatisticsCollectionAsync()
        {
            var output = new List<StatisticGenericItem>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("report_statistics_logging_requestresponse", commandType: CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillGenericStatisticFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetLoggingRequestStatisticsCollectionAsync():", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetActionCountStatistics; Gets a ActionsCountStatistic object containing several counts based on dates or states of the actions.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="timespanInDays">Int, number in days.</param>
        /// <returns>Return ActionsCountStatistic item.</returns>
        public async Task<ActionsCountStatistic> GetActionCountStatistics(int companyId, int? areaId = null, int? timespanInDays = null)
        {
            var output = new ActionsCountStatistic();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }

                if (timespanInDays.HasValue && timespanInDays.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_timespanindays", timespanInDays.Value));
                }

                using (dr = await _manager.GetDataReader("report_statistics_actionscount", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.CountNr = dr["actions_count"] != DBNull.Value ? Convert.ToInt32(dr["actions_count"]) : 0;
                        output.CountNrOverdue = dr["overdue_actions_count"] != DBNull.Value ? Convert.ToInt32(dr["overdue_actions_count"]) : 0;
                        output.CountNrUnresolved = dr["unresolved_actions_count"] != DBNull.Value ? Convert.ToInt32(dr["unresolved_actions_count"]) : 0;
                        output.CountNrResolved = dr["resolved_actions_count"] != DBNull.Value ? Convert.ToInt32(dr["resolved_actions_count"]) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetActionCountStatistics():", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetTotalStatisticsAsync; Get total statistics based on all companies.
        /// </summary>
        /// <returns>StatsTotals object.</returns>
        public async Task<StatsTotals> GetTotalStatisticsAsync()
        {
            var output = new StatsTotals();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("report_statistics_totals", commandType: CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        output.ActionCommentNr = Convert.ToInt32(dr["actioncomment_nr"]);
                        output.ActionsNr = Convert.ToInt32(dr["actions_nr"]);
                        output.AnnounceNr = Convert.ToInt32(dr["announcement_nr"]);
                        output.AuditNr = Convert.ToInt32(dr["audit_nr"]);
                        output.AuditTemplateNr = Convert.ToInt32(dr["audittemplate_nr"]);
                        output.ChecklistNr = Convert.ToInt32(dr["checklist_nr"]);
                        output.ChecklistTemplateNr = Convert.ToInt32(dr["checklisttemplate_nr"]);
                        output.CommentNr = Convert.ToInt32(dr["comment_nr"]);
                        output.CompanyNr = Convert.ToInt32(dr["company_nr"]);
                        output.FactoryFeedMessagesNr = Convert.ToInt32(dr["factoryfeedmessage_nr"]);
                        output.FactoryFeedNr = Convert.ToInt32(dr["factoryfeed_nr"]);
                        output.TasksNr = Convert.ToInt32(dr["tasks_nr"]);
                        output.TaskTemplateNr = Convert.ToInt32(dr["tasktemplate_nr"]);
                        output.UserNr = Convert.ToInt32(dr["user_nr"]);
                        output.AssessmentNr = Convert.ToInt32(dr["assessment_nr"]);
                        output.AssessmentTemplateNr = Convert.ToInt32(dr["assessmenttemplate_nr"]);
                        output.HoldingNr = Convert.ToInt32(dr["holding_nr"]);
                        output.AreaNr = Convert.ToInt32(dr["area_nr"]);
                        output.WorkInstructionNr = Convert.ToInt32(dr["workinstruction_nr"]);
                        output.MatricesNr = Convert.ToInt32(dr["matrices_nr"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetTotalStatistics():", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetCompanyReports; Get company report information for use in management tooling
        /// </summary>
        /// <returns>List of company report items, containing statistics per company.</returns>
        public async Task<List<CompanyReport>> GetCompanyReports(DateTime startTime, DateTime endTime)
        {
            List<CompanyReport> output = new List<CompanyReport>();

            if (startTime == DateTime.MinValue)
            {
                startTime = new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day);
            }

            if (endTime == DateTime.MinValue)
            {
                endTime = new DateTime(year: DateTime.Now.AddMonths(1).Year, month: DateTime.Now.AddMonths(1).Month, day: DateTime.Now.AddMonths(1).Day);
            }

            List<string> headerFields = new List<string>();

            //export_data_company_management_overview
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_starttimestamp", startTime));
                parameters.Add(new NpgsqlParameter("@_endtimestamp", endTime));

                using (dr = await _manager.GetDataReader("export_data_company_management_overview", commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    headerFields = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).Where(x => x != "id" && x != "name").ToList(); //get statistics without id and name
                    while (await dr.ReadAsync())
                    {
                        var companyReport = new CompanyReport();
                        companyReport.CompanyId = Convert.ToInt32(dr["id"]);
                        companyReport.Name = dr["name"].ToString();

                        companyReport.Statistics = new List<StatsItem>();

                        foreach (var item in headerFields)
                        {
                            companyReport.Statistics.Add(new StatsItem() { Statistic = Convert.ToInt32(dr[item]), Title = item });
                        }

                        output.Add(companyReport);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetTotalStatistics():", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        #region - private methods -



        /// <summary>
        /// GetStatistics; general methods for getting statistics data based on the Count - Id structure.
        /// </summary>
        /// <param name="companyId">CompanyId where the statistics belong to. Will be used within SP parameter.</param>
        /// <param name="storedProcedureName">Postgresql function name.</param>
        /// <param name="auditTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="checklistTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="taskTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <returns>A list of StatisticGenericItem</returns>
        private async Task<List<StatisticGenericItem>> GetStatistics(int companyId, string storedProcedureName, int userId = 0, DateTime? timestamp = null, int? areaId = null, int? auditTemplateId = null, int? checklistTemplateId = null, int? taskTemplateId = null, int? timespanInDays = null)
        {
            var output = new List<StatisticGenericItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (userId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId));
                }
                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }
                if (timespanInDays.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timespanindays", timespanInDays));
                }

                if (auditTemplateId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_audittemplateid", auditTemplateId.Value));
                }
                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));
                }
                if (checklistTemplateId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId.Value));
                }
                if (taskTemplateId.HasValue)
                {
                    //not yet used
                    //parameters.Add(new NpgsqlParameter("@_tasktemplateid", taskTemplateId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {

                        output.Add(CreateOrFillGenericStatisticFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetStatistics(", storedProcedureName, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetAverageStatistics; Get average statistics from database;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="storedProcedureName"></param>
        /// <param name="auditTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="checklistTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <param name="taskTemplateId">Optional; only use when getting specific statistic for specific data set</param>
        /// <returns>List of average statistic items.</returns>
        private async Task<List<StatisticGenericItem>> GetAverageStatistics(int companyId, string storedProcedureName, int? areaId = null, int? auditTemplateId = null, int? checklistTemplateId = null, int? taskTemplateId = null, int? timespanInDays = null)
        {
            var output = new List<StatisticGenericItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (areaId.HasValue && areaId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                }
                if (timespanInDays.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timespanindays", timespanInDays));
                }

                if (auditTemplateId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_audittemplateid", auditTemplateId.Value));
                }
                if (checklistTemplateId.HasValue)
                {
                    //not yet used
                    //parameters.Add(new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId.Value));
                }
                if (taskTemplateId.HasValue)
                {
                    //not yet used
                    //parameters.Add(new NpgsqlParameter("@_tasktemplateid", taskTemplateId.Value));
                }

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillGenericStatisticFromReader(dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetAverageStatistics(", storedProcedureName, "):", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); };
            }


            return output;
        }

        /// <summary>
        /// CreateOrFillGenericStatisticFromReader; Create a stats item based on a data reader; If stats item is not supplied it will be created.
        /// Note! depending on dataset the values will be filled. Unfillable values will remain null.
        /// </summary>
        /// <param name="dr">dr containing all information.</param>
        /// <param name="stat">Statistic item, if not supplied will be created.</param>
        /// <returns>StatisticGenericItem containing information.</returns>
        private StatisticGenericItem CreateOrFillGenericStatisticFromReader(NpgsqlDataReader dr, StatisticGenericItem stat = null)
        {
            if (stat == null) stat = new StatisticGenericItem();

            if (dr.HasColumn("count_nr"))
            {
                if (dr["count_nr"] != DBNull.Value)
                {
                    stat.CountNr = Convert.ToInt32(dr["count_nr"]);
                }
            }

            if (dr.HasColumn("average_nr"))
            {
                if (dr["average_nr"] != DBNull.Value)
                {
                    stat.AverageNr = Convert.ToDecimal(dr["average_nr"]);
                }
            }

            if (dr.HasColumn("id"))
            {
                if (dr["id"] != DBNull.Value)
                {
                    stat.Id = Convert.ToInt32(dr["id"]);
                }
            }

            if (dr.HasColumn("name"))
            {
                if (dr["name"] != DBNull.Value && !string.IsNullOrEmpty(dr["name"].ToString()))
                {
                    stat.Name = dr["name"].ToString();
                }
            }

            if (dr.HasColumn("day"))
            {
                if (dr["day"] != DBNull.Value)
                {
                    stat.Day = Convert.ToInt32(dr["day"]);
                }
            }

            if (dr.HasColumn("month"))
            {
                if (dr["month"] != DBNull.Value)
                {
                    stat.Month = Convert.ToInt32(dr["month"]);
                }
            }

            if (dr.HasColumn("week"))
            {
                if (dr["week"] != DBNull.Value)
                {
                    stat.Week = Convert.ToInt32(dr["week"]);
                }
            }

            if (dr.HasColumn("year"))
            {
                if (dr["year"] != DBNull.Value)
                {
                    stat.Year = Convert.ToInt32(dr["year"]);
                }
            }

            if (dr.HasColumn("status"))
            {
                if (dr["status"] != DBNull.Value && !string.IsNullOrEmpty(dr["status"].ToString()))
                {
                    stat.Status = dr["status"].ToString();
                }
            }

            return stat;

        }


        private StatisticMonthYearItem CreateOrFillMonthYearStatisticFromReader(NpgsqlDataReader dr, StatisticMonthYearItem stat = null)
        {
            if (stat == null) stat = new StatisticMonthYearItem();

            if (dr.HasColumn("count_nr"))
            {
                if (dr["count_nr"] != DBNull.Value)
                {
                    stat.CountNr = Convert.ToInt32(dr["count_nr"]);
                }
            }

            if (dr.HasColumn("year_nr"))
            {
                if (dr["year_nr"] != DBNull.Value)
                {
                    stat.Year = Convert.ToInt32(dr["year_nr"]);
                }
            }

            if (dr.HasColumn("month_nr"))
            {
                if (dr["month_nr"] != DBNull.Value)
                {
                    stat.Month = Convert.ToInt32(dr["month_nr"]);
                }
            }

            //Debug.WriteLine(string.Concat(stat.CountNr, " ",stat.Month, " ", stat.Year));

            return stat;

        }

        private TasksReport CreateOrFillTasksReportFromReader(NpgsqlDataReader dr, TasksReport tasksReport = null)
        {
            if (tasksReport == null) tasksReport = new TasksReport();
            if (tasksReport.Today == null) tasksReport.Today = new TaskStatusCountStatistic();
            if (tasksReport.Last7Days == null) tasksReport.Last7Days = new TaskStatusCountStatistic();
            if (tasksReport.Last30Days == null) tasksReport.Last30Days = new TaskStatusCountStatistic();


            if (dr.HasColumn("tasks_executed_today"))
            {
                if (dr["tasks_executed_today"] != DBNull.Value)
                {
                    tasksReport.TasksExecutedToday = Convert.ToInt32(dr["tasks_executed_today"]);
                }
            }

            if (dr.HasColumn("tasks_skipped_today"))
            {
                if (dr["tasks_skipped_today"] != DBNull.Value)
                {
                    tasksReport.TasksSkippedToday = Convert.ToInt32(dr["tasks_skipped_today"]);
                }
            }


            if (dr.HasColumn("today_todo"))
            {
                if (dr["today_todo"] != DBNull.Value)
                {
                    tasksReport.Today.TodoCount = Convert.ToInt32(dr["today_todo"]);
                }
            }

            if (dr.HasColumn("today_ok"))
            {
                if (dr["today_ok"] != DBNull.Value)
                {
                    tasksReport.Today.OkCount = Convert.ToInt32(dr["today_ok"]);
                }
            }

            if (dr.HasColumn("today_not_ok"))
            {
                if (dr["today_not_ok"] != DBNull.Value)
                {
                    tasksReport.Today.NotOkCount = Convert.ToInt32(dr["today_not_ok"]);
                }
            }

            if (dr.HasColumn("today_skipped"))
            {
                if (dr["today_skipped"] != DBNull.Value)
                {
                    tasksReport.Today.SkippedCount = Convert.ToInt32(dr["today_skipped"]);
                }
            }

            if (dr.HasColumn("today_total"))
            {
                if (dr["today_total"] != DBNull.Value)
                {
                    tasksReport.Today.TotalCount = Convert.ToInt32(dr["today_total"]);
                }
            }


            if (dr.HasColumn("seven_days_todo"))
            {
                if (dr["seven_days_todo"] != DBNull.Value)
                {
                    tasksReport.Last7Days.TodoCount = Convert.ToInt32(dr["seven_days_todo"]);
                }
            }

            if (dr.HasColumn("seven_days_ok"))
            {
                if (dr["seven_days_ok"] != DBNull.Value)
                {
                    tasksReport.Last7Days.OkCount = Convert.ToInt32(dr["seven_days_ok"]);
                }
            }

            if (dr.HasColumn("seven_days_not_ok"))
            {
                if (dr["seven_days_not_ok"] != DBNull.Value)
                {
                    tasksReport.Last7Days.NotOkCount = Convert.ToInt32(dr["seven_days_not_ok"]);
                }
            }

            if (dr.HasColumn("seven_days_skipped"))
            {
                if (dr["seven_days_skipped"] != DBNull.Value)
                {
                    tasksReport.Last7Days.SkippedCount = Convert.ToInt32(dr["seven_days_skipped"]);
                }
            }

            if (dr.HasColumn("seven_days_total"))
            {
                if (dr["seven_days_total"] != DBNull.Value)
                {
                    tasksReport.Last7Days.TotalCount = Convert.ToInt32(dr["seven_days_total"]);
                }
            }


            if (dr.HasColumn("thirty_days_todo"))
            {
                if (dr["thirty_days_todo"] != DBNull.Value)
                {
                    tasksReport.Last30Days.TodoCount = Convert.ToInt32(dr["thirty_days_todo"]);
                }
            }

            if (dr.HasColumn("thirty_days_ok"))
            {
                if (dr["thirty_days_ok"] != DBNull.Value)
                {
                    tasksReport.Last30Days.OkCount = Convert.ToInt32(dr["thirty_days_ok"]);
                }
            }

            if (dr.HasColumn("thirty_days_not_ok"))
            {
                if (dr["thirty_days_not_ok"] != DBNull.Value)
                {
                    tasksReport.Last30Days.NotOkCount = Convert.ToInt32(dr["thirty_days_not_ok"]);
                }
            }

            if (dr.HasColumn("thirty_days_skipped"))
            {
                if (dr["thirty_days_skipped"] != DBNull.Value)
                {
                    tasksReport.Last30Days.SkippedCount = Convert.ToInt32(dr["thirty_days_skipped"]);
                }
            }

            if (dr.HasColumn("thirty_days_total"))
            {
                if (dr["thirty_days_total"] != DBNull.Value)
                {
                    tasksReport.Last30Days.TotalCount = Convert.ToInt32(dr["thirty_days_total"]);
                }
            }

            return tasksReport;
        }

        private ChecklistsReport CreateOrFillChecklistItemsReportFromReader(NpgsqlDataReader dr, ChecklistsReport checklistsReport = null)
        {
            if (checklistsReport == null) checklistsReport = new ChecklistsReport();
            if (checklistsReport.Today == null) checklistsReport.Today = new ChecklistItemStatusCountStatistic();
            if (checklistsReport.Last7Days == null) checklistsReport.Last7Days = new ChecklistItemStatusCountStatistic();
            if (checklistsReport.Last30Days == null) checklistsReport.Last30Days = new ChecklistItemStatusCountStatistic();


            if (dr.HasColumn("checklists_executed_today"))
            {
                if (dr["checklists_executed_today"] != DBNull.Value)
                {
                    checklistsReport.ChecklistsExecutedToday = Convert.ToInt32(dr["checklists_executed_today"]);
                }
            }

            if (dr.HasColumn("checklists_executed_today"))
            {
                if (dr["checklists_executed_today"] != DBNull.Value)
                {
                    checklistsReport.Today.TotalChecklistCount = Convert.ToInt32(dr["checklists_executed_today"]);
                }
            }

            if (dr.HasColumn("checklists_executed_7days"))
            {
                if (dr["checklists_executed_7days"] != DBNull.Value)
                {
                    checklistsReport.Last7Days.TotalChecklistCount = Convert.ToInt32(dr["checklists_executed_7days"]);
                }
            }

            if (dr.HasColumn("checklists_executed_30days"))
            {
                if (dr["checklists_executed_30days"] != DBNull.Value)
                {
                    checklistsReport.Last30Days.TotalChecklistCount = Convert.ToInt32(dr["checklists_executed_30days"]);
                }
            }


            if (dr.HasColumn("today_todo"))
            {
                if (dr["today_todo"] != DBNull.Value)
                {
                    checklistsReport.Today.TodoCount = Convert.ToInt32(dr["today_todo"]);
                }
            }

            if (dr.HasColumn("today_ok"))
            {
                if (dr["today_ok"] != DBNull.Value)
                {
                    checklistsReport.Today.OkCount = Convert.ToInt32(dr["today_ok"]);
                }
            }

            if (dr.HasColumn("today_not_ok"))
            {
                if (dr["today_not_ok"] != DBNull.Value)
                {
                    checklistsReport.Today.NotOkCount = Convert.ToInt32(dr["today_not_ok"]);
                }
            }

            if (dr.HasColumn("today_skipped"))
            {
                if (dr["today_skipped"] != DBNull.Value)
                {
                    checklistsReport.Today.SkippedCount = Convert.ToInt32(dr["today_skipped"]);
                }
            }

            if (dr.HasColumn("today_total"))
            {
                if (dr["today_total"] != DBNull.Value)
                {
                    checklistsReport.Today.TotalItemsCount = Convert.ToInt32(dr["today_total"]);
                }
            }


            if (dr.HasColumn("seven_days_todo"))
            {
                if (dr["seven_days_todo"] != DBNull.Value)
                {
                    checklistsReport.Last7Days.TodoCount = Convert.ToInt32(dr["seven_days_todo"]);
                }
            }

            if (dr.HasColumn("seven_days_ok"))
            {
                if (dr["seven_days_ok"] != DBNull.Value)
                {
                    checklistsReport.Last7Days.OkCount = Convert.ToInt32(dr["seven_days_ok"]);
                }
            }

            if (dr.HasColumn("seven_days_not_ok"))
            {
                if (dr["seven_days_not_ok"] != DBNull.Value)
                {
                    checklistsReport.Last7Days.NotOkCount = Convert.ToInt32(dr["seven_days_not_ok"]);
                }
            }

            if (dr.HasColumn("seven_days_skipped"))
            {
                if (dr["seven_days_skipped"] != DBNull.Value)
                {
                    checklistsReport.Last7Days.SkippedCount = Convert.ToInt32(dr["seven_days_skipped"]);
                }
            }

            if (dr.HasColumn("seven_days_total"))
            {
                if (dr["seven_days_total"] != DBNull.Value)
                {
                    checklistsReport.Last7Days.TotalItemsCount = Convert.ToInt32(dr["seven_days_total"]);
                }
            }


            if (dr.HasColumn("thirty_days_todo"))
            {
                if (dr["thirty_days_todo"] != DBNull.Value)
                {
                    checklistsReport.Last30Days.TodoCount = Convert.ToInt32(dr["thirty_days_todo"]);
                }
            }

            if (dr.HasColumn("thirty_days_ok"))
            {
                if (dr["thirty_days_ok"] != DBNull.Value)
                {
                    checklistsReport.Last30Days.OkCount = Convert.ToInt32(dr["thirty_days_ok"]);
                }
            }

            if (dr.HasColumn("thirty_days_not_ok"))
            {
                if (dr["thirty_days_not_ok"] != DBNull.Value)
                {
                    checklistsReport.Last30Days.NotOkCount = Convert.ToInt32(dr["thirty_days_not_ok"]);
                }
            }

            if (dr.HasColumn("thirty_days_skipped"))
            {
                if (dr["thirty_days_skipped"] != DBNull.Value)
                {
                    checklistsReport.Last30Days.SkippedCount = Convert.ToInt32(dr["thirty_days_skipped"]);
                }
            }

            if (dr.HasColumn("thirty_days_total"))
            {
                if (dr["thirty_days_total"] != DBNull.Value)
                {
                    checklistsReport.Last30Days.TotalItemsCount = Convert.ToInt32(dr["thirty_days_total"]);
                }
            }

            return checklistsReport;
        }

        private ChecklistsReportExtended AppendMainStatsToChecklistReportExtendedFromReader(NpgsqlDataReader dr, ChecklistsReportExtended checklistsReportExtended = null)
        {
            if (checklistsReportExtended == null) checklistsReportExtended = new ChecklistsReportExtended();
            if (checklistsReportExtended.ExecutionStats == null) checklistsReportExtended.ExecutionStats = new List<ListExecutedStatistic>();
            if (checklistsReportExtended.ItemTappings == null) checklistsReportExtended.ItemTappings = new List<TaskTappingsStatistic>();

            var checklistsExecuted = new ListExecutedStatistic();

            var checklistItemTapping = new TaskTappingsStatistic();

            checklistItemTapping.TotalCount = 0;

            if (dr.HasColumn("period_start"))
            {
                if (dr["period_start"] != DBNull.Value)
                {
                    checklistsExecuted.PeriodStart = Convert.ToDateTime(dr["period_start"]);
                    checklistItemTapping.PeriodStart = Convert.ToDateTime(dr["period_start"]);
                }
            }

            if (dr.HasColumn("period_end"))
            {
                if (dr["period_end"] != DBNull.Value)
                {
                    checklistsExecuted.PeriodEnd = Convert.ToDateTime(dr["period_end"]);
                    checklistItemTapping.PeriodEnd = Convert.ToDateTime(dr["period_end"]);
                }
            }

            if (dr.HasColumn("label_text"))
            {
                if (dr["label_text"] != DBNull.Value)
                {
                    checklistsExecuted.LabelText = Convert.ToString(dr["label_text"]);
                    checklistItemTapping.LabelText = Convert.ToString(dr["label_text"]);
                }
            }

            if (dr.HasColumn("tasks_planned_count"))
            {
                if (dr["tasks_planned_count"] != DBNull.Value)
                {
                    checklistsExecuted.PlannedCount = Convert.ToInt32(dr["tasks_planned_count"]);
                }
            }

            if (dr.HasColumn("checklists_executed_count"))
            {
                if (dr["checklists_executed_count"] != DBNull.Value)
                {
                    checklistsExecuted.ExecutedCount = Convert.ToInt32(dr["checklists_executed_count"]);
                }
            }
            else if (dr.HasColumn("tasks_executed_count"))
            {
                if (dr["tasks_executed_count"] != DBNull.Value)
                {
                    checklistsExecuted.TaskExecutedCount = Convert.ToInt32(dr["tasks_executed_count"]);
                }
            }

            if (dr.HasColumn("ok_count"))
            {
                if (dr["ok_count"] != DBNull.Value)
                {
                    checklistItemTapping.OkCount = Convert.ToInt32(dr["ok_count"]);
                    checklistItemTapping.TotalCount += (int) checklistItemTapping.OkCount;
                }
            }

            if (dr.HasColumn("not_ok_count"))
            {
                if (dr["not_ok_count"] != DBNull.Value)
                {
                    checklistItemTapping.NotOkCount = Convert.ToInt32(dr["not_ok_count"]);
                    checklistItemTapping.TotalCount += (int)checklistItemTapping.NotOkCount;
                }
            }

            if (dr.HasColumn("skipped_count"))
            {
                if (dr["skipped_count"] != DBNull.Value)
                {
                    checklistItemTapping.SkippedCount = Convert.ToInt32(dr["skipped_count"]);
                    checklistItemTapping.TotalCount += (int)checklistItemTapping.SkippedCount;
                }
            }

            if (dr.HasColumn("todo_count"))
            {
                if (dr["todo_count"] != DBNull.Value)
                {
                    checklistItemTapping.TodoCount = Convert.ToInt32(dr["todo_count"]);
                    checklistItemTapping.TotalCount += (int)checklistItemTapping.TodoCount;
                }
            }


            checklistsReportExtended.ExecutionStats.Add(checklistsExecuted);
            checklistsReportExtended.ItemTappings.Add(checklistItemTapping);

            return checklistsReportExtended;
        }

        private ChecklistsReportExtended AppendDeviationsToChecklistReportExtendedFromReader(NpgsqlDataReader dr, ChecklistsReportExtended checklistsReportExtended = null)
        {
            if (checklistsReportExtended == null) checklistsReportExtended = new ChecklistsReportExtended();

            var statistic = new TaskStatusStatistic();

            if (dr.HasColumn("name"))
            {
                if (dr["name"] != DBNull.Value)
                {
                    statistic.TaskName = Convert.ToString(dr["name"]);
                }
            }

            if (dr.HasColumn("template_id"))
            {
                if (dr["template_id"] != DBNull.Value)
                {
                    statistic.TemplateId = Convert.ToInt32(dr["template_id"]);
                }
            }

            if (dr.HasColumn("status_count"))
            {
                if (dr["status_count"] != DBNull.Value)
                {
                    statistic.StatusCount = Convert.ToInt32(dr["status_count"]);
                }
            }

            if (dr.HasColumn("total_count"))
            {
                if (dr["total_count"] != DBNull.Value)
                {
                    statistic.TotalCount = Convert.ToInt32(dr["total_count"]);
                }
            }

            if (dr.HasColumn("total_actions_count"))
            {
                if (dr["total_actions_count"] != DBNull.Value)
                {
                    statistic.ActionsCount = Convert.ToInt32(dr["total_actions_count"]);
                }
            }

            if (dr.HasColumn("resolved_actions_count"))
            {
                if (dr["resolved_actions_count"] != DBNull.Value)
                {
                    statistic.ResolvedActionsCount = Convert.ToInt32(dr["resolved_actions_count"]);
                }
            }

            switch (Convert.ToString(dr["status"]))
            {
                case "not ok":
                    if (checklistsReportExtended.NotOkStats == null) checklistsReportExtended.NotOkStats = new List<TaskStatusStatistic>();
                    checklistsReportExtended.NotOkStats.Add(statistic);
                    break;
                case "skipped":
                    if (checklistsReportExtended.SkippedStats == null) checklistsReportExtended.SkippedStats = new List<TaskStatusStatistic>();
                    checklistsReportExtended.SkippedStats.Add(statistic);
                    break;
                case "todo":
                    if (checklistsReportExtended.TodoStats == null) checklistsReportExtended.TodoStats = new List<TaskStatusStatistic>();
                    checklistsReportExtended.TodoStats.Add(statistic);
                    break;
            }

            return checklistsReportExtended;
        }

        private AuditsReportExtended AppendMainStatsToAuditReportExtendedFromReader(NpgsqlDataReader dr, AuditsReportExtended auditsReportExtended = null)
        {
            if (auditsReportExtended == null) auditsReportExtended = new AuditsReportExtended();
            if (auditsReportExtended.AuditsExecuted == null) auditsReportExtended.AuditsExecuted = new List<ListExecutedStatistic>();
            if (auditsReportExtended.AuditAverageScores == null) auditsReportExtended.AuditAverageScores = new List<AuditAverageScoreStatistic>();

            var checklistsExecuted = new ListExecutedStatistic();

            var auditAverageScores = new AuditAverageScoreStatistic();

            if (dr.HasColumn("period_start"))
            {
                if (dr["period_start"] != DBNull.Value)
                {
                    checklistsExecuted.PeriodStart = Convert.ToDateTime(dr["period_start"]);
                    auditAverageScores.PeriodStart = Convert.ToDateTime(dr["period_start"]);
                }
            }

            if (dr.HasColumn("period_end"))
            {
                if (dr["period_end"] != DBNull.Value)
                {
                    checklistsExecuted.PeriodEnd = Convert.ToDateTime(dr["period_end"]);
                    auditAverageScores.PeriodEnd = Convert.ToDateTime(dr["period_end"]);
                }
            }

            if (dr.HasColumn("label_text"))
            {
                if (dr["label_text"] != DBNull.Value)
                {
                    checklistsExecuted.LabelText = Convert.ToString(dr["label_text"]);
                    auditAverageScores.LabelText = Convert.ToString(dr["label_text"]);
                }
            }

            if (dr.HasColumn("audits_executed_count"))
            {
                if (dr["audits_executed_count"] != DBNull.Value)
                {
                    checklistsExecuted.ExecutedCount = Convert.ToInt32(dr["audits_executed_count"]);
                }
            }

            if (dr.HasColumn("average_score"))
            {
                if (dr["average_score"] != DBNull.Value)
                {
                    auditAverageScores.AverageScore = Convert.ToInt32(dr["average_score"]);
                }
            }

            auditsReportExtended.AuditsExecuted.Add(checklistsExecuted);
            auditsReportExtended.AuditAverageScores.Add(auditAverageScores);

            return auditsReportExtended;
        }

        private AuditsReportExtended AppendDeviationsToAuditReportExtendedFromReader(NpgsqlDataReader dr, AuditsReportExtended auditsReportExtended = null)
        {
            if (auditsReportExtended == null) auditsReportExtended = new AuditsReportExtended();
            if (auditsReportExtended.DeviationStats == null) auditsReportExtended.DeviationStats = new List<TaskStatusStatistic>();
            if (auditsReportExtended.SkippedStats == null) auditsReportExtended.SkippedStats = new List<TaskStatusStatistic>();

            var statistic = new TaskStatusStatistic();

            if (dr.HasColumn("name"))
            {
                if (dr["name"] != DBNull.Value)
                {
                    statistic.TaskName = Convert.ToString(dr["name"]);
                }
            }


            if (dr.HasColumn("template_id"))
            {
                if (dr["template_id"] != DBNull.Value)
                {
                    statistic.TemplateId = Convert.ToInt32(dr["template_id"]);
                }
            }

            if (dr.HasColumn("status_count"))
            {
                if (dr["status_count"] != DBNull.Value)
                {
                    statistic.StatusCount = Convert.ToInt32(dr["status_count"]);
                }
            }

            if (dr.HasColumn("total_count"))
            {
                if (dr["total_count"] != DBNull.Value)
                {
                    statistic.TotalCount = Convert.ToInt32(dr["total_count"]);
                }
            }

            if (dr.HasColumn("average_percentage"))
            {
                if (dr["average_percentage"] != DBNull.Value)
                {
                    statistic.AveragePercentage = Math.Round(Convert.ToDouble(dr["average_percentage"]), 2);
                }
            }

            if (dr.HasColumn("total_actions_count"))
            {
                if (dr["total_actions_count"] != DBNull.Value)
                {
                    statistic.ActionsCount = Convert.ToInt32(dr["total_actions_count"]);
                }
            }

            if (dr.HasColumn("resolved_actions_count"))
            {
                if (dr["resolved_actions_count"] != DBNull.Value)
                {
                    statistic.ResolvedActionsCount = Convert.ToInt32(dr["resolved_actions_count"]);
                }
            }


            var status = "";

            if (dr.HasColumn("status"))
            {
                if (dr["status"] != DBNull.Value)
                {
                    status = Convert.ToString(dr["status"]);
                }
            }

            if (status == "deviations")
            {
                auditsReportExtended.DeviationStats.Add(statistic);
            }
            else if(status == "skipped")
            {
                auditsReportExtended.SkippedStats.Add(statistic);
            }

            return auditsReportExtended;
        }

        private ActionsReportExtended AppendMainStatsToActionReportExtendedFromReader(NpgsqlDataReader dr, ActionsReportExtended actionsReportExtended)
        {
            actionsReportExtended ??= new ActionsReportExtended();
            actionsReportExtended.ActionsStartedAndResolved ??= new List<ActionsStartedResolvedStatistic>();

            var statistic = new ActionsStartedResolvedStatistic()
            {
                PeriodStart = Convert.ToDateTime(dr["period_start"]),
                PeriodEnd = Convert.ToDateTime(dr["period_end"]),
                LabelText = Convert.ToString(dr["label_text"]),
                TotalCount = Convert.ToInt32(dr["total_count"]),
                UnresolvedCount = Convert.ToInt32(dr["unresolved_count"]),
                ResolvedCount = Convert.ToInt32(dr["resolved_count"])
            };

            actionsReportExtended.ActionsStartedAndResolved.Add(statistic);

            return actionsReportExtended;
        }
        private ActionsReportExtended AppendOverallStatsToAuditReportExtendedFromReader(NpgsqlDataReader dr, ActionsReportExtended actionsReportExtended)
        {
            actionsReportExtended ??= new ActionsReportExtended();

            actionsReportExtended.ActionCounts = new ActionCountsStatistic()
            {
                TotalActions = Convert.ToInt32(dr["total_actions_count"]),
                ResolvedActions = Convert.ToInt32(dr["resolved_actions_count"]),
                OpenActions = Convert.ToInt32(dr["open_actions_count"]),
                OverdueActions = Convert.ToInt32(dr["overdue_actions_count"])
            };

            return actionsReportExtended;
        }
        private ActionsReportExtended AppendTop5StatsToAuditReportExtendedFromReader(NpgsqlDataReader dr, ActionsReportExtended actionsReportExtended)
        {
            actionsReportExtended ??= new ActionsReportExtended();
            actionsReportExtended.ActionsCreated ??= new ActionsUsersStatistic();
            actionsReportExtended.ActionsAssigned ??= new ActionsUsersStatistic();

            String statType = Convert.ToString(dr["top5Type"]);

            ActionsUserStatistic statistic = new ActionsUserStatistic()
            {
                UserId = Convert.ToInt32(dr["user_id"]),
                FullName = Convert.ToString(dr["user_name"]),
                Count = Convert.ToInt32(dr["total_count"])
            };

            switch (Convert.ToString(dr["top5Type"]))
            {
                case "actionscreated":
                    actionsReportExtended.ActionsCreated.TopUsers.Add(statistic);
                    actionsReportExtended.ActionsCreated.TotalCount += statistic.Count;
                    break;
                case "actionsassigned":
                    actionsReportExtended.ActionsAssigned.TopUsers.Add(statistic);
                    actionsReportExtended.ActionsAssigned.TotalCount += statistic.Count;
                    break;
            }

            return actionsReportExtended;
        }


        private AuditsReport CreateOrFillAuditsReportFromReader(NpgsqlDataReader dr, AuditsReport auditsReport = null)
        {
            if (auditsReport == null) auditsReport = new AuditsReport();
            if (auditsReport.Today == null) auditsReport.Today = new AuditScoreStatistic();
            if (auditsReport.Last7Days == null) auditsReport.Last7Days = new AuditScoreStatistic();
            if (auditsReport.Last30Days == null) auditsReport.Last30Days = new AuditScoreStatistic();


            if (dr.HasColumn("audits_executed_today"))
            {
                if (dr["audits_executed_today"] != DBNull.Value)
                {
                    auditsReport.AuditsExecutedToday = Convert.ToInt32(dr["audits_executed_today"]);
                }
            }


            if (dr.HasColumn("today_average_score"))
            {
                if (dr["today_average_score"] != DBNull.Value)
                {
                    auditsReport.Today.AverageScore = Convert.ToDouble(dr["today_average_score"]);
                }
            }

            if (dr.HasColumn("today_total_count"))
            {
                if (dr["today_total_count"] != DBNull.Value)
                {
                    auditsReport.Today.TotalAuditsCount = Convert.ToInt32(dr["today_total_count"]);
                }
            }


            if (dr.HasColumn("seven_days_average_score"))
            {
                if (dr["seven_days_average_score"] != DBNull.Value)
                {
                    auditsReport.Last7Days.AverageScore = Convert.ToDouble(dr["seven_days_average_score"]);
                }
            }

            if (dr.HasColumn("seven_days_total_count"))
            {
                if (dr["seven_days_total_count"] != DBNull.Value)
                {
                    auditsReport.Last7Days.TotalAuditsCount = Convert.ToInt32(dr["seven_days_total_count"]);
                }
            }


            if (dr.HasColumn("thirty_days_average_score"))
            {
                if (dr["thirty_days_average_score"] != DBNull.Value)
                {
                    auditsReport.Last30Days.AverageScore = Convert.ToDouble(dr["thirty_days_average_score"]);
                }
            }

            if (dr.HasColumn("thirty_days_total_count"))
            {
                if (dr["thirty_days_total_count"] != DBNull.Value)
                {
                    auditsReport.Last30Days.TotalAuditsCount = Convert.ToInt32(dr["thirty_days_total_count"]);
                }
            }

            return auditsReport;
        }

        private ActionsReport CreateOrFillActionsReportFromReader(NpgsqlDataReader dr, ActionsReport actionsReport = null)
        {
            if (actionsReport == null) actionsReport = new ActionsReport();
            if (actionsReport.Today == null) actionsReport.Today = new ActionStatusCountsStatistic();
            if (actionsReport.Last7Days == null) actionsReport.Last7Days = new ActionStatusCountsStatistic();
            if (actionsReport.Last30Days == null) actionsReport.Last30Days = new ActionStatusCountsStatistic();
            if (actionsReport.MyActions == null) actionsReport.MyActions = new MyActionsStatusCountsStatistic();


            if (dr.HasColumn("today_new_count"))
            {
                if (dr["today_new_count"] != DBNull.Value)
                {
                    actionsReport.Today.CreatedCount = Convert.ToInt32(dr["today_new_count"]);
                }
            }

            if (dr.HasColumn("today_resolved_count"))
            {
                if (dr["today_resolved_count"] != DBNull.Value)
                {
                    actionsReport.Today.ResolvedCount = Convert.ToInt32(dr["today_resolved_count"]);
                }
            }


            if (dr.HasColumn("seven_days_new_count"))
            {
                if (dr["seven_days_new_count"] != DBNull.Value)
                {
                    actionsReport.Last7Days.CreatedCount = Convert.ToInt32(dr["seven_days_new_count"]);
                }
            }

            if (dr.HasColumn("seven_days_resolved_count"))
            {
                if (dr["seven_days_resolved_count"] != DBNull.Value)
                {
                    actionsReport.Last7Days.ResolvedCount = Convert.ToInt32(dr["seven_days_resolved_count"]);
                }
            }


            if (dr.HasColumn("thirty_days_new_count"))
            {
                if (dr["thirty_days_new_count"] != DBNull.Value)
                {
                    actionsReport.Last30Days.CreatedCount = Convert.ToInt32(dr["thirty_days_new_count"]);
                }
            }

            if (dr.HasColumn("thirty_days_resolved_count"))
            {
                if (dr["thirty_days_resolved_count"] != DBNull.Value)
                {
                    actionsReport.Last30Days.ResolvedCount = Convert.ToInt32(dr["thirty_days_resolved_count"]);
                }
            }


            if (dr.HasColumn("my_started_count"))
            {
                if (dr["my_started_count"] != DBNull.Value)
                {
                    actionsReport.MyActions.StartedCount = Convert.ToInt32(dr["my_started_count"]);
                }
            }

            if (dr.HasColumn("my_resolved_count"))
            {
                if (dr["my_resolved_count"] != DBNull.Value)
                {
                    actionsReport.MyActions.ResolvedCount = Convert.ToInt32(dr["my_resolved_count"]);
                }
            }

            if (dr.HasColumn("my_open_count"))
            {
                if (dr["my_open_count"] != DBNull.Value)
                {
                    actionsReport.MyActions.OpenCount = Convert.ToInt32(dr["my_open_count"]);
                }
            }

            if (dr.HasColumn("my_overdue_count"))
            {
                if (dr["my_overdue_count"] != DBNull.Value)
                {
                    actionsReport.MyActions.OverdueCount = Convert.ToInt32(dr["my_overdue_count"]);
                }
            }


            if (dr.HasColumn("total_count"))
            {
                if (dr["total_count"] != DBNull.Value)
                {
                    actionsReport.TotalCount = Convert.ToInt32(dr["total_count"]);
                }
            }

            if (dr.HasColumn("unresolved_count"))
            {
                if (dr["unresolved_count"] != DBNull.Value)
                {
                    actionsReport.UnresolvedCount = Convert.ToInt32(dr["unresolved_count"]);
                }
            }

            if (dr.HasColumn("resolved_count"))
            {
                if (dr["resolved_count"] != DBNull.Value)
                {
                    actionsReport.ResolvedCount = Convert.ToInt32(dr["resolved_count"]);
                }
            }

            if (dr.HasColumn("overdue_count"))
            {
                if (dr["overdue_count"] != DBNull.Value)
                {
                    actionsReport.OverdueCount = Convert.ToInt32(dr["overdue_count"]);
                }
            }


            return actionsReport;
        }
        #endregion

        #region - datawarehouse statistics - 
        /// <summary>
        /// Retrieves statistics data from the data warehouse for a given company and holding, based on the specified statistics reference and date range.
        /// </summary>
        /// <param name="companyId">The ID of the company for which to retrieve statistics.</param>
        /// <param name="holdingId">The ID of the holding for which to retrieve statistics.</param>
        /// <param name="statsReference">A string reference indicating the type of statistics to retrieve (e.g., "action", "audit").</param>
        /// <param name="startDateTime">The start date of the statistics range.</param>
        /// <param name="endDateTime">The end date of the statistics range.</param>
        /// <returns>A <see cref="StatisticsData"/> object containing the requested statistics data.</returns>
        public async Task<StatisticsData> GetStatisticsDataWarehouse(int companyId, int holdingId, string statsReference, DateTime startDateTime, DateTime endDateTime)
        {
            var statsData = new StatisticsData();

            var connectionDwString = _manager.GetWriterConnection().Result.ConnectionString;

            //Get active connectionstring, which should be the one to the normal EZGO db, seeing this query will run cross database, will replace the ezgo db with the ez_dw database. 
            //Currently there is no separate connection handler for the API to the DW database, so this will be used with a own connection.
            //NOTE: THIS WILL BREAK IF THE DW DB IS ON A DIFFERENT SERVER THAN THE PRODUCTION DATABASE!!!
            if (!string.IsNullOrEmpty(connectionDwString))
            {
                //map to ez_dw database. 
                connectionDwString = connectionDwString.Replace("Database=ezgo", "Database=ez_dw");
            }

            NpgsqlConnection connDW = new NpgsqlConnection(connectionDwString);
            try
            {
                await connDW.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(GenerateStatisticsQuery(statsReference:statsReference, companyId:companyId, holdingId:holdingId, startDateTime:startDateTime, endDateTime:endDateTime), connDW))
                {
                    cmd.CommandType = CommandType.Text; //normal query, generated by the GenerateStatisticsQuery method.
                    if(startDateTime != DateTime.MinValue && endDateTime != DateTime.MinValue)
                    {
                        //add parameters to the query, if start and end date are set.
                        cmd.Parameters.Add(new NpgsqlParameter("@_startdate", startDateTime));
                        cmd.Parameters.Add(new NpgsqlParameter("@_enddate", endDateTime));
                    }

                    NpgsqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    DataTable dt = new DataTable();
                    dt.Load(dr); //load in datatable, dataset can be different based on the query.

                    await dr.CloseAsync(); dr = null;
                    if (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0)
                    {
                        //convert to data structure for use in CMS. 
                        statsData.Columns = new List<string>();
                        statsData.ColumnTypes = new List<string>();
                        statsData.Data = new List<List<string>>();

                        foreach (DataColumn column in dt.Columns)
                        {
                            statsData.Columns.Add(column.ColumnName.ToString());
                            statsData.ColumnTypes.Add(column.DataType.ToString());
                        }

                        foreach (DataRow datarow in dt.Rows)
                        {
                            var row = new List<string>();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                row.Add(datarow[i].ToString());
                            }
                            statsData.Data.Add(row);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred GetStatisticsDataWarehouse()");
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

            return statsData;
        }

        /// <summary>
        /// Generates a SQL query string for retrieving statistics data from the data warehouse.
        /// The query is dynamically constructed based on the provided statistics reference (e.g., "action", "audit"),
        /// company ID, holding ID, and optional date range. The resulting query targets the appropriate data warehouse
        /// tables and applies a date filter if start and end dates are specified.
        /// </summary>
        /// <param name="statsReference">A string reference indicating the type of statistics to retrieve (e.g., "action", "audit").</param>
        /// <param name="companyId">The ID of the company for which to retrieve statistics.</param>
        /// <param name="holdingId">The ID of the holding for which to retrieve statistics.</param>
        /// <param name="startDateTime">The start date of the statistics range.</param>
        /// <param name="endDateTime">The end date of the statistics range.</param>
        /// <returns>A SQL query string for retrieving the requested statistics from the data warehouse.</returns>
        private string GenerateStatisticsQuery(string statsReference, int companyId, int holdingId, DateTime startDateTime, DateTime endDateTime)
        {
            //Queries are semi-dynamic, seeing DW uses generated tables (and not statis tables) based on holding and company ids these will be generated for the database.
            //These queries will change the coming releases of the datawarehouse due to addons and changes in structure. 
            const string STATISTICS_QUERY_ACTIONS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_action_overview_{0}_{1} {2}) AS overview,(SELECT COUNT(*) FROM data_action_tags_overview_{0}_{1} {2}) AS tags) AS S";
            const string STATISTICS_QUERY_AREAS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_area_overview_{0}_{1} {2}) AS overview) AS S";
            const string STATISTICS_QUERY_ASSESSMENTS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_assessment_overview_{0}_{1} {2}) AS overview, (SELECT COUNT(*) FROM data_assessment_instruction_overview_{0}_{1} {2}) AS instructionoverview, (SELECT COUNT(*) FROM data_assessment_instruction_items_overview_{0}_{1} {2}) AS instructionitemoverview, (SELECT COUNT(*) FROM data_assessment_instruction_items_tags_overview_{0}_{1} {2}) AS intructionitemtagsoverview, (SELECT COUNT(*) FROM data_assessment_instruction_tags_overview_{0}_{1} {2}) AS instructiontagsoverview, (SELECT COUNT(*) FROM data_assessment_tags_overview_{0}_{1} {2}) AS tagoverview) AS S";
            const string STATISTICS_QUERY_AUDITS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_audit_overview_{0}_{1} {2}) AS overview, (SELECT COUNT(*) FROM data_audit_items_overview_{0}_{1} {2}) AS itemsoverview, (SELECT COUNT(*) FROM data_audit_items_actions_overview_{0}_{1} {2}) AS itemsactionsoverview, (SELECT COUNT(*) FROM data_audit_items_comments_overview_{0}_{1} {2}) AS itemscommentsoverview, (SELECT COUNT(*) FROM data_audit_items_pictureproof_overview_{0}_{1} {2}) AS itemspictureproofoverview, (SELECT COUNT(*) FROM data_audit_items_properties_overview_{0}_{1} {2}) AS itemspictureproofoverview, (SELECT COUNT(*) FROM data_audit_items_tags_overview_{0}_{1} {2}) AS itemstagsoverview, (SELECT COUNT(*) FROM data_audit_openfields_properties_overview_{0}_{1} {2}) AS openfieldspropertiesoverview, (SELECT COUNT(*) FROM data_audit_tags_overview_{0}_{1} {2}) AS tagsoverview) AS S";
            const string STATISTICS_QUERY_CHECKLISTS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_checklist_overview_{0}_{1} {2}) AS overview, (SELECT COUNT(*) FROM data_checklist_items_overview_{0}_{1} {2}) AS itemsoverview, (SELECT COUNT(*) FROM data_checklist_items_actions_overview_{0}_{1} {2}) AS itemsactionsoverview, (SELECT COUNT(*) FROM data_checklist_items_comments_overview_{0}_{1} {2}) AS itemscommentsoverview, (SELECT COUNT(*) FROM data_checklist_items_pictureproof_overview_{0}_{1} {2}) AS itemspictureproofoverview, (SELECT COUNT(*) FROM data_checklist_items_properties_overview_{0}_{1} {2}) AS itemspictureproofoverview, (SELECT COUNT(*) FROM data_checklist_items_tags_overview_{0}_{1} {2}) AS itemstagsoverview, (SELECT COUNT(*) FROM data_checklist_openfields_properties_overview_{0}_{1} {2}) AS openfieldspropertiesoverview, (SELECT COUNT(*) FROM data_checklist_tags_overview_{0}_{1} {2}) AS tagsoverview) AS S";
            const string STATISTICS_QUERY_COMMENTS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_comment_overview_{0}_{1} {2}) AS overview, (SELECT COUNT(*) FROM data_comment_tags_overview_{0}_{1} {2}) AS tags) AS S";
            const string STATISTICS_QUERY_COMPANIES = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_company_overview_{0}_{1} {2}) AS overview) AS S";
            const string STATISTICS_QUERY_PICTUREPROOF = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_pictureproof_overview_{0}_{1} {2}) AS overview) AS S";
            const string STATISTICS_QUERY_SHIFTS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_shift_overview_{0}_{1} {2}) AS overview) AS S";
            const string STATISTICS_QUERY_TAGS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_tag_overview_{0}_{1} {2}) AS overview, (SELECT COUNT(*) FROM data_tag_items_overview_{0}_{1} {2}) AS items) AS S";
            const string STATISTICS_QUERY_TASKS = "SELECT * FROM (SELECT (SELECT COUNT(*) FROM data_task_overview_{0}_{1} {2}) AS overview, (SELECT COUNT(*) FROM data_task_actions_overview_{0}_{1} {2}) AS actionoverview, (SELECT COUNT(*) FROM data_task_comments_overview_{0}_{1} {2}) AS commentoverview, (SELECT COUNT(*) FROM data_task_linked_overview_{0}_{1} {2}) AS linkedoverview, (SELECT COUNT(*) FROM data_task_pictureproof_overview_{0}_{1} {2}) AS pictureproofoverview, (SELECT COUNT(*) FROM data_task_properties_overview_{0}_{1} {2}) AS propertiesoverview, (SELECT COUNT(*) FROM data_task_tags_overview_{0}_{1} {2}) AS tagsoverview) AS S";

            const string QUERY_WHERE = " WHERE import_date >= @_startdate AND import_date <= @_enddate";

            string query = string.Empty; 
            
            switch(statsReference)
            {
                case "action" : query = STATISTICS_QUERY_ACTIONS; break;
                case "area" : query = STATISTICS_QUERY_AREAS; break;
                case "assessment" : query = STATISTICS_QUERY_ASSESSMENTS; break;
                case "audit" : query = STATISTICS_QUERY_AUDITS; break;
                case "checklist" : query = STATISTICS_QUERY_CHECKLISTS; break;
                case "comment": query = STATISTICS_QUERY_COMMENTS; break;
                case "company": query = STATISTICS_QUERY_COMPANIES; break;
                case "pictureproof": query = STATISTICS_QUERY_PICTUREPROOF; break;
                case "shift": query = STATISTICS_QUERY_SHIFTS; break;
                case "tag": query = STATISTICS_QUERY_TAGS; break;
                case "task": query = STATISTICS_QUERY_TASKS; break;
            }

            string queryWhere = startDateTime != DateTime.MinValue && endDateTime != DateTime.MinValue ? QUERY_WHERE : string.Empty;

            return string.Format(query, holdingId, companyId, queryWhere); //"SELECT * FROM ", rawReference, " WHERE period_start >= @startdatetime AND period_end <= @enddatetime")
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