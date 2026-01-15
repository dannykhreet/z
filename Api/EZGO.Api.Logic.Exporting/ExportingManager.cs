using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Logic.Exporting.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Settings;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Exporting
{
    /// <summary>
    /// ExportingManager, contains all functionality used for creating data overviews/ export collection for use in Reporting/Exporting.
    /// Depending on settings most will output a datatable or dataset for further processing (most of the times to excel file or csv).
    /// </summary>
    public class ExportingManager : BaseManager<ExportingManager>, IExportingManager
    {
        private readonly IExportingDataManager _dataManager;
        private readonly IDatabaseAccessHelper _dbmanager;
        private readonly IShiftManager _shiftManager;
        private readonly ITaskManager _taskManager;
        private readonly IGeneralManager _generalManager;
        private readonly IAutomatedExportingManager _automatedExportManager;
        private Features _features;

        public ExportingManager(IDatabaseAccessHelper dbmanager, IAutomatedExportingManager automatedExportManager, IExportingDataManager datamanager, IShiftManager shiftmanager, ITaskManager taskmanager, IGeneralManager generalManager, ILogger<ExportingManager> logger) : base(logger)
        {
            _dataManager = datamanager;
            _dbmanager = dbmanager;
            _shiftManager = shiftmanager;
            _taskManager = taskmanager;
            _generalManager = generalManager;
            _automatedExportManager = automatedExportManager;
        }

        /// <summary>
        /// GetTaskOverviewByCompanyAndDateAsync; Get tasks data in datetable.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="from">from datetime, if not supplied default now -60 days will be used.</param>
        /// <param name="to">to datetime, if not supplied now will be used.</param>
        /// <returns>DataTable for futher conversion.</returns>
        public async System.Threading.Tasks.Task<DataTable> GetTaskOverviewByCompanyAndDateAsync(int companyId, DateTime? from, DateTime? to)
        {
            if (!(companyId > 0))
            {
                //TODO add logging
                return new DataTable();
            }

            if (!from.HasValue)
            {
                from = DateTime.Now.AddDays(-60);
            }

            if (!to.HasValue)
            {
                to = DateTime.Now;
            }

            return await _dataManager.GetTasksDataTableByCompanyAndDateAsync(companyId, from.Value, to.Value);

        }

        /// <summary>
        /// GetTaskDetailsOverviewByCompanyAndDateAsync; Get Task Details in a DataTable
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="from">from datetime, if not supplied default now -60 days will be used.</param>
        /// <param name="to">to datetime, if not supplied now will be used.</param>
        /// <returns>DataTable for futher conversion.</returns>
        public async System.Threading.Tasks.Task<DataTable> GetTaskDetailsOverviewByCompanyAndDateAsync(int companyId, DateTime? from, DateTime? to)
        {
            if (!(companyId > 0))
            {
                //TODO add logging
                return new DataTable();
            }

            if (!from.HasValue)
            {
                from = DateTime.Now.AddDays(-60);
            }

            if (!to.HasValue)
            {
                to = DateTime.Now;
            }

            return await _dataManager.GetTasksDetailsDataTableByCompanyAndDateAsync(companyId, from.Value, to.Value);

        }

        /// <summary>
        /// GetTaskTemplateDetailsOverviewByCompanyAsync; Get a datatable containing template detail information. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>DataTable for conversion.</returns>
        public async System.Threading.Tasks.Task<DataTable> GetTaskTemplateDetailsOverviewByCompanyAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            if (!(companyId > 0))
            {
                //TODO add logging
                return new DataTable();
            }

            return await _dataManager.GetTaskTemplateDetailsDataTableByCompanyAsync(companyId);

        }

        /// <summary>
        /// GetTaskTemplateOverviewAsync; Get a overview of tasks as a DataSet. DataSet contains one or more tables.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <returns>A multi-datatable dataset structure.</returns>
        public async Task<DataSet> GetTaskTemplateOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var ds = new DataSet();
            var dt_nonshifts = await _dataManager.GetTaskTemplateOverviewDataTableByCompanyAsync(companyId: companyId, overviewType: ExportTaskTemplateOverviewTypeEnum.NonShifts);
            var dt_shifts = await _dataManager.GetTaskTemplateOverviewDataTableByCompanyAsync(companyId: companyId, overviewType: ExportTaskTemplateOverviewTypeEnum.Shifts);
            var shifts = await _shiftManager.GetShiftsAsync(companyId: companyId);
            var templateshiftrelation = await _taskManager.GetShiftRelationsWithTaskTemplatesAsync(companyId: companyId);

            dt_nonshifts.TableName = "Week-Month tasks";
            dt_shifts.TableName = "Shift tasks";

            dt_nonshifts = AppendHeaderRowsColumns(dtSource: dt_nonshifts);

            dt_shifts = AppendDataTableColumns(dtSource: dt_shifts, shifts.Select(x => string.Concat("shift_", x.Id.ToString())).ToList());
            dt_shifts = AppendDataTaskTemplateOverview(dtSource: dt_shifts, tasktemplateShiftsRelations: templateshiftrelation);
            dt_shifts = AppendHeaderRowsShiftsTaskTemplateOverview(dtSource: dt_shifts, shifts: shifts);

            ds.Tables.Add(dt_nonshifts);
            ds.Tables.Add(dt_shifts);

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_TAGS"))
            {
                var parametersTaskTemplateTagsOverview = _dbmanager.GetBaseParameters(companyId: companyId);

                var dt_tags = await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_tasktemplates_tags_overview",
                                                            parameters: parametersTaskTemplateTagsOverview,
                                                            dataTableName: "TASK TEMPLATE TAGS");

                dt_tags = AppendHeaderRowsColumns(dtSource: dt_tags);
                ds.Tables.Add(dt_tags);
            }

            return ds;
        }

        /// <summary>
        /// GetTasksOverviewAsync; Get tasks overview in a dataset. Tasks, Actions, Comments and Value registration will be loaded.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing multiple tables for futher processing.</returns>
        public async Task<DataSet> GetTaskOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            await this.InitFeatures(companyId: companyId);

            var output = new DataSet();

            var parametersTasksOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTasksActionsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTasksCommentsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTasksPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTaskTagsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            
            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersTasksActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersTasksCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersTasksPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersTaskTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            } else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
                parametersTasksActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
                parametersTasksCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
                parametersTasksPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
                parametersTaskTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersTasksActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersTasksCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersTasksPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersTaskTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            } else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
                parametersTasksActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
                parametersTasksCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
                parametersTasksPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
                parametersTaskTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_overview",
                                            parameters: parametersTasksOverview,
                                            dataTableName: "TASKS"));

            if(_features.ActionsEnabled.HasValue && _features.ActionsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_tasks_actions_overview",
                                           parameters: parametersTasksActionsOverview,
                                           dataTableName: "ACTIONS"));
            }

            if(_features.EasyCommentsEnabled.HasValue && _features.EasyCommentsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_tasks_comments_overview",
                                           parameters: parametersTasksCommentsOverview,
                                           dataTableName: "COMMENTS"));
            }

            if(_features.TasksPropertyValueRegistrationEnabled.HasValue && _features.TasksPropertyValueRegistrationEnabled.Value || _features.TasksPropertyEnabled.HasValue && _features.TasksPropertyEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_properties_overview",
                               parameters: parametersTasksPropertiesOverview,
                               dataTableName: "VALUE REGISTRATION"));

            }

            if (_features.TagsEnabled == true)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_tasks_tags_overview",
                                                           parameters: parametersTaskTagsOverview,
                                                           dataTableName: "TASKS TAGS"));
            }

            return output;
        }

        /// <summary>
        /// GetTaskPropertyOverviewAsync; Get data set containing Task Property information.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing a table with task property data.</returns>
        public async Task<DataSet> GetTaskPropertyOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersTasksOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_properties_overview",
                                              parameters: parametersTasksOverview,
                                              dataTableName: "TASK_PROPERTIES"));

            return output;
        }

        /// <summary>
        /// GetTaskChecklistPropertyOverviewAsync; Get data set containing checklist task property information.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing a table with cehcklist task property data.</returns>
        public async Task<DataSet> GetTaskChecklistPropertyOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersTasksOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_checklist_properties_overview",
                                              parameters: parametersTasksOverview,
                                              dataTableName: "TASK_PROPERTIES"));

            return output;
        }

        /// <summary>
        /// GetTaskAuditPropertyOverviewAsync; Get data set containing audit task property information.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing a table with audit task property data.</returns>
        public async Task<DataSet> GetTaskAuditPropertyOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersTasksOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_audit_properties_overview",
                                              parameters: parametersTasksOverview,
                                              dataTableName: "TASK_PROPERTIES"));

            return output;
        }

        /// <summary>
        /// GetTaskOverviewCustomerSpecificAsync; Get TaskOverview based on customer; This is a customer specific call.
        /// Note! for now only 1 implemented.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>DAtaSet containing a task data for futher processing.</returns>
        public async Task<DataSet> GetTaskOverviewCustomerSpecificAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersTasksOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }
            else
            {
                parametersTasksOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
            }

            //TODO add customer switch for specifics.

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_overview_custom",
                                              parameters: parametersTasksOverview,
                                              dataTableName: "TASKS"));

            return output;
        }

        /// <summary>
        /// GetChecklistAuditOverviewByCompanyAsync; Get a Dataset containing multiple DataTables which can be used for generating a multiworkbook xlsx.
        /// DataTables are based on checklists and audits.
        /// The ExcelSheet will contain 4 worksheets.
        /// - Checklists (a list of all checklist with some extra information)
        /// - Checklist Items (a list of all tasks with all checklists.)
        /// - Audits (a list of all audits with some extra information)
        /// - Audits Items (a list of all tasks with all audits.)
        /// </summary>
        /// <param name="companyid">CompanyId of the company where all checklists and audits must be loaded.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>A multidatatable dataset structure.</returns>
        public async Task<DataSet> GetChecklistAuditOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersChecklistOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            if(from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklists_overview",
                                                          parameters: parametersChecklistOverview,
                                                          dataTableName: "CHECKLISTS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklistitems_overview",
                                                            parameters: parametersChecklistItemsOverview,
                                                            dataTableName: "CHECKLIST ITEMS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audits_overview",
                                              parameters: parametersAuditsOverview,
                                              dataTableName: "AUDITS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audititems_overview",
                                                            parameters: parametersAuditItemsOverview,
                                                            dataTableName: "AUDIT ITEMS"));

            return output;
        }

        /// <summary>
        /// GetAuditOverviewByCompanyAsync; Get a Dataset containing multiple DataTables which can be used for generating a multiworkbook xlsx.
        /// DataTables are based on audits.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Return dataset containing audit information.</returns>
        public async Task<DataSet> GetAuditOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            await this.InitFeatures(companyId: companyid);

            var output = new DataSet();

            var parametersAuditsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsActionsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsCommentsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsOpenfieldsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersAuditsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {

                parametersAuditsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audits_overview",
                                                            parameters: parametersAuditsOverview,
                                                            dataTableName: "AUDITS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audititems_overview",
                                                            parameters: parametersAuditItemsOverview,
                                                            dataTableName: "AUDIT ITEMS"));

            if (_features.ActionsEnabled.HasValue && _features.ActionsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audititems_actions_overview",
                                                                parameters: parametersAuditItemsActionsOverview,
                                                                dataTableName: "ACTIONS"));
            }

            if (_features.EasyCommentsEnabled.HasValue && _features.EasyCommentsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audititems_comments_overview",
                                                                parameters: parametersAuditItemsCommentsOverview,
                                                                dataTableName: "COMMENTS"));
            }

            if (_features.TasksPropertyValueRegistrationEnabled.HasValue && _features.TasksPropertyValueRegistrationEnabled.Value || _features.TasksPropertyEnabled.HasValue && _features.TasksPropertyEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_audit_properties_overview",
                                                                parameters: parametersAuditItemsPropertiesOverview,
                                                                dataTableName: "VALUE REGISTRATION"));
            }

            if (_features.AuditsPropertyEnabled.HasValue && _features.AuditsPropertyEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audit_openfields_properties_overview",
                                                                parameters: parametersAuditItemsOpenfieldsPropertiesOverview,
                                                                dataTableName: "OPEN FIELDS"));
            }

            if (_features.TagsEnabled == true)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audits_tags_overview",
                                                           parameters: parametersAuditTagsOverview,
                                                           dataTableName: "AUDIT TAGS"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audititems_tags_overview",
                                                           parameters: parametersAuditItemsTagsOverview,
                                                           dataTableName: "AUDIT ITEM TAGS"));
            }

            return output;
        }

        /// <summary>
        /// GetChecklistOverviewByCompanyAsync; Get a Dataset containing multiple DataTables which can be used for generating a multiworkbook xlsx.
        /// DataTables are based on checklists.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Return dataset containing checklist information.</returns>
        public async Task<DataSet> GetChecklistOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            await this.InitFeatures(companyId: companyid);

            var output = new DataSet();

            var parametersChecklistOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsActionsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsCommentsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsOpenfieldsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            var parametersChecklistStagesOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistStageItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistStageItemTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistStagesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistStageItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistStageItemTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistStagesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistStageItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistStageItemTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }


            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklists_overview",
                                                            parameters: parametersChecklistOverview,
                                                            dataTableName: "CHECKLISTS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklistitems_overview",
                                                            parameters: parametersChecklistItemsOverview,
                                                            dataTableName: "CHECKLIST ITEMS"));

            if (_features.ChecklistStagesEnabled == true)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checkliststages_overview",
                                                               parameters: parametersChecklistStagesOverview,
                                                               dataTableName: "CHECKLISTS STAGES"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checkliststages_items_overview",
                                                               parameters: parametersChecklistStageItemsOverview,
                                                               dataTableName: "CHECKLISTS STAGES ITEMS"));
            }

            if (_features.ActionsEnabled.HasValue && _features.ActionsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklistitems_actions_overview",
                                                                parameters: parametersChecklistItemsActionsOverview,
                                                                dataTableName: "ACTIONS"));
            }

            if (_features.EasyCommentsEnabled.HasValue && _features.EasyCommentsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklistitems_comments_overview",
                                                                parameters: parametersChecklistItemsCommentsOverview,
                                                                dataTableName: "COMMENTS"));
            }

            if (_features.TasksPropertyValueRegistrationEnabled.HasValue && _features.TasksPropertyValueRegistrationEnabled.Value || _features.TasksPropertyEnabled.HasValue && _features.TasksPropertyEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_task_checklist_properties_overview",
                                                                parameters: parametersChecklistItemsPropertiesOverview,
                                                                dataTableName: "VALUE REGISTRATION"));
            }

            if (_features.AuditsPropertyEnabled.HasValue && _features.AuditsPropertyEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklist_openfields_properties_overview",
                                                                parameters: parametersChecklistItemsOpenfieldsPropertiesOverview,
                                                                dataTableName: "OPEN FIELDS"));
            }

            if (_features.TagsEnabled == true)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklists_tags_overview",
                                                           parameters: parametersChecklistTagsOverview,
                                                           dataTableName: "CHECKLIST TAGS"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklistitems_tags_overview",
                                                           parameters: parametersChecklistItemsTagsOverview,
                                                           dataTableName: "CHECKLIST ITEM TAGS"));

                if (_features.ChecklistStagesEnabled == true)
                {
                    output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checkliststages_tags_overview", //implemented
                                                                    parameters: parametersChecklistStageItemTagsOverview,
                                                                    dataTableName: "CHECKLISTS STAGES TAGS"));
                }
            }

            return output;
        }

        /// <summary>
        /// GetChecklistAuditTemplatesOverviewByCompanyAsync; Get a Dataset containing multiple DataTables which can be used for generating a multiworkbook xlsx.
        /// DataTables are based on the templates of checklists and audits.
        /// The ExcelSheet will contain 4 worksheets.
        /// - Checklists (a list of all checklist templates with some extra information)
        /// - Checklist Items (a list of all tasks with all checklists.)
        /// - Audits (a list of all audits templates with some extra information)
        /// - Audits Items (a list of all tasks with all audits.)
        /// </summary>
        /// <param name="companyid">CompanyId of the company where all checklists and audits must be loaded.</param>
        /// <returns>A multidatatable dataset structure.</returns>
        public async Task<DataSet> GetChecklistAuditTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersChecklistOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }


            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplates_overview",
                                                            parameters: parametersChecklistOverview,
                                                            dataTableName: "CHECKLISTS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplateitems_overview",
                                                            parameters: parametersChecklistItemsOverview,
                                                            dataTableName: "CHECKLIST ITEMS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audittemplates_overview",
                                                            parameters: parametersAuditsOverview,
                                                            dataTableName: "AUDIT"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audittemplateitems_overview",
                                                            parameters: parametersAuditItemsOverview,
                                                            dataTableName: "AUDIT ITEMS"));

            return output;
        }

        /// <summary>
        /// GetChecklistTemplatesOverviewByCompanyAsync; Get a data set with checklist data.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>DataSet containing checklist information.</returns>
        public async Task<DataSet> GetChecklistTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            await this.InitFeatures(companyId: companyid);

            var output = new DataSet();

            var parametersChecklistOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistOverview2 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistOverview3 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistOverview4 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistOverview5 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistOverview6 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersChecklistOverview7 = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersChecklistOverview2.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersChecklistOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersChecklistOverview2.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplates_overview",
                                                            parameters: parametersChecklistOverview,
                                                            dataTableName: "CHECKLISTS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplateitems_overview",
                                                           parameters: parametersChecklistOverview2,
                                                           dataTableName: "CHECKLISTS ITEMS"));

            if (_features.ChecklistStagesEnabled == true)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplatestages_overview",
                                                               parameters: parametersChecklistOverview5,
                                                               dataTableName: "CHECKLISTS STAGES"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplatestages_items_overview",
                                                               parameters: parametersChecklistOverview6,
                                                               dataTableName: "CHECKLISTS STAGES ITEMS"));
            }

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyid, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplates_tags_overview",
                                                                parameters: parametersChecklistOverview3,
                                                                dataTableName: "CHECKLISTS TAGS"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplateitems_tags_overview",
                                                                parameters: parametersChecklistOverview4,
                                                                dataTableName: "CHECKLISTS ITEMS TAGS"));

                if (_features.ChecklistStagesEnabled == true)
                {
                    output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_checklisttemplatestages_tags_overview", //implemented
                                                                    parameters: parametersChecklistOverview7,
                                                                    dataTableName: "CHECKLISTS STAGES TAGS"));
                }
            }

            return output;
        }

        /// <summary>
        /// GetAuditTemplatesOverviewByCompanyAsync; Get a data set with audit data.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>DataSet containing audit information.</returns>
        public async Task<DataSet> GetAuditTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersAuditOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditOverview2 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditOverview3 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAuditOverview4 = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersAuditOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAuditOverview2.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersAuditOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAuditOverview2.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audittemplates_overview",
                                                            parameters: parametersAuditOverview,
                                                            dataTableName: "AUDITS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audittemplateitems_overview",
                                                           parameters: parametersAuditOverview2,
                                                           dataTableName: "AUDITS ITEMS"));

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyid, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audittemplates_tags_overview",
                                                           parameters: parametersAuditOverview3,
                                                           dataTableName: "AUDITS TAGS"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_audittemplateitems_tags_overview",
                                                           parameters: parametersAuditOverview4,
                                                           dataTableName: "AUDITS ITEMS TAGS"));
            }

            return output;
        }


        /// <summary>
        /// GetWorkInstructionTemplatesOverviewByCompanyAsync; Get a data set with workinstruction template data.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>DataSet containing workinstruction template information.</returns>
        public async Task<DataSet> GetWorkInstructionTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersWIToverview1 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersWIToverview2 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersWIToverview3 = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersWIToverview1.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersWIToverview2.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersWIToverview3.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersWIToverview1.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersWIToverview2.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersWIToverview3.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_workinstructiontemplates_overview",
                                                            parameters: parametersWIToverview1,
                                                            dataTableName: "WORK INSTRUCTION TEMPLATES"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_workinstructiontemplateitems_overview",
                                                           parameters: parametersWIToverview2,
                                                           dataTableName: "WORK INSTRUCTION TEMPLATE ITEMS"));

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyid, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_workinstructiontemplates_tags_overview",
                                                           parameters: parametersWIToverview3,
                                                           dataTableName: "WORK INSTRUCTION TAGS"));
            }

            return output;
        }


        public async Task<DataSet> GetWorkInstructionChangeNotificationsOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersWIToverview1 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersWIToverview2 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersWIToverview3 = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersWIToverview1.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersWIToverview2.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersWIToverview3.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersWIToverview1.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersWIToverview2.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersWIToverview3.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_wi_change_notifications_overview",
                                                            parameters: parametersWIToverview1,
                                                            dataTableName: "WI CHANGES"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_wi_change_notifications_viewed_overview",
                                                           parameters: parametersWIToverview2,
                                                           dataTableName: "WI CHANGES VIEWED"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_wi_change_notifications_data_overview",
                                                           parameters: parametersWIToverview3,
                                                           dataTableName: "WI CHANGES DATA"));

            return output;
        }


        /// <summary>
        /// GetAssessmentTemplatesOverviewByCompanyAsync; Get a data set with workinstruction template data.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>DataSet containing workinstruction template information.</returns>
        public async Task<DataSet> GetAssessmentTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersASToverview1 = _dbmanager.GetBaseParameters(companyId: companyid);
            //var parametersWIToverview2 = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAssessmentTemplateTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersASToverview1.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                //parametersWIToverview2.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersASToverview1.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                //parametersWIToverview2.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessmenttemplates_overview",
                                                            parameters: parametersASToverview1,
                                                            dataTableName: "ASSESSMENT TEMPLATES"));

            //output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessmenttemplateitems_overview",
            //                                               parameters: parametersWIToverview2,
            //                                               dataTableName: "ASSESSMENT TEMPLATE ITEMS"));

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyid, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessmenttemplates_tags_overview",
                                                           parameters: parametersAssessmentTemplateTagsOverview,
                                                           dataTableName: "ASSESSMENT TEMPLATES TAGS"));
            }

            return output;
        }


        /// <summary>
        /// GetAssessmentOverviewByCompanyAsync; Get a Dataset containing multiple DataTables which can be used for generating a multiworkbook xlsx.
        /// DataTables are based on checklists.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Return dataset containing checklist information.</returns>
        public async Task<DataSet> GetAssessmentOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            await this.InitFeatures(companyId: companyid);

            var output = new DataSet();

            var parametersAssessmentsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAssessmentItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAssessmentInstructionItemsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAssessmentTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);
            var parametersAssessmentInstructionTagsOverview = _dbmanager.GetBaseParameters(companyId: companyid);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersAssessmentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAssessmentItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAssessmentInstructionItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAssessmentTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersAssessmentInstructionTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersAssessmentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAssessmentItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAssessmentInstructionItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAssessmentTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersAssessmentInstructionTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessments_overview",
                                                            parameters: parametersAssessmentsOverview,
                                                            dataTableName: "ASSESSMENTS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessmentitems_overview",
                                                            parameters: parametersAssessmentItemsOverview,
                                                            dataTableName: "ASSESSMENT INSTRUCTIONS"));

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessmentinstructionitems_overview",
                                                            parameters: parametersAssessmentInstructionItemsOverview,
                                                            dataTableName: "ASSESSMENT INSTRUCTION ITEMS"));

            if (_features.TagsEnabled == true)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessments_tags_overview",
                                                           parameters: parametersAssessmentTagsOverview,
                                                           dataTableName: "ASSESSMENT TAGS"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_assessmentinstructions_tags_overview",
                                                           parameters: parametersAssessmentInstructionTagsOverview,
                                                           dataTableName: "ASSESSMENT INSTRUCTION TAGS"));
            }

            return output;
        }

        //
        /// <summary>
        /// GetMatrixSkillsUserOverviewByCompanyAsync; Get a dataset containing the matrix, skills and user information for company.
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>DataSet containing workinstruction template information.</returns>
        public async Task<DataSet> GetMatrixSkillsUserOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parameters = _dbmanager.GetBaseParameters(companyId: companyid);


            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parameters.Add(new NpgsqlParameter("@_starttimestamp", from.Value)); //ignored for now, will be added to SP lateron if needed.
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parameters.Add(new NpgsqlParameter("@_endtimestamp", to.Value));//ignored for now, will be added to SP lateron if needed.
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_matrix_user_skills_overview",
                                                            parameters: parameters,
                                                            dataTableName: "MATRIX USER SKILLS"));


            return output;
        }

        /// <summary>
        /// GetLanguageResourcesAsync; Get language resource table as excel
        /// </summary>
        /// <returns>DataTable containing the language resources.</returns>
        public async Task<DataTable> GetLanguageResourcesAsync()
        {
            return await _dbmanager.GetDataTable(procedureNameOrQuery: "export_language_resources",
                                                            parameters: new List<NpgsqlParameter>(),
                                                            dataTableName: "LANGUAGE RESOURCES");
        }

        /// <summary>
        /// GetLanguageImportQueriesAsync; Get language query exports;
        /// </summary>
        /// <returns>DataTable containing the language update queries.</returns>
        public async Task<DataTable> GetLanguageImportQueriesAsync()
        {
            return await _dbmanager.GetDataTable(procedureNameOrQuery: "export_language_resources_update_queries",
                                                            parameters: new List<NpgsqlParameter>(),
                                                            dataTableName: "LANGUAGE UPDATE QUERIES");
        }


        /// <summary>
        ///GetManagementCompanyOverview; Get a management overview containing specific data for use in management portal.
        /// </summary>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <returns></returns>
        public async Task<DataTable> GetManagementCompanyOverview(DateTime? from = null, DateTime? to = null)
        {
            var output = new DataTable();

            var parametersCompanyManagementOverview = new List<NpgsqlParameter>();

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersCompanyManagementOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }
            else
            {
                parametersCompanyManagementOverview.Add(new NpgsqlParameter("@_starttimestamp", DateTime.Now.AddDays(-7)));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersCompanyManagementOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }
            else
            {
                parametersCompanyManagementOverview.Add(new NpgsqlParameter("@_endtimestamp", DateTime.Now));
            }

            output = await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_company_management_overview",
                                                   parameters: parametersCompanyManagementOverview,
                                                   dataTableName: "COMPANY_OVERVIEW");

            return output;
        }

        #region - action / comments -
        /// <summary>
        /// GetActionCommentOverviewAsync; Get action comments data set
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing action/comments information.</returns>
        public async Task<DataSet> GetActionCommentOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null) {
            await this.InitFeatures(companyId: companyId);

            var output = new DataSet();

            var parametersActionOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersCommentOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersActionsTagsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersCommentsTagsOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersActionOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersCommentOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersActionsTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersCommentsTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersActionOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersCommentOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersActionsTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersCommentsTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            if (_features.ActionsEnabled.HasValue && _features.ActionsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_actions_overview",
                                                            parameters: parametersActionOverview,
                                                            dataTableName: "ACTIONS"));
            }

            if (_features.EasyCommentsEnabled.HasValue && _features.EasyCommentsEnabled.Value)
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_comments_overview",
                                                            parameters: parametersCommentOverview,
                                                            dataTableName: "COMMENTS"));
            }

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_actions_tags_overview",
                                                               parameters: parametersActionsTagsOverview,
                                                               dataTableName: "ACTIONS TAGS"));

                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_comments_tags_overview",
                                                               parameters: parametersCommentsTagsOverview,
                                                               dataTableName: "COMMENTS TAGS"));
            }

            return output;
        }

        /// <summary>
        /// GetActionOverviewAsync; Get action data set
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing action information.</returns>
        public async Task<DataSet> GetActionOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersActionOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersCommentOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersActionsTagsOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersActionOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersCommentOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersActionsTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersActionOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersCommentOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersActionsTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }


            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_actions_overview",
                                                            parameters: parametersActionOverview,
                                                            dataTableName: "ACTIONS"));

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_actions_tags_overview",
                                                               parameters: parametersActionsTagsOverview,
                                                               dataTableName: "ACTIONS TAGS"));
            }

            return output;
        }

        /// <summary>
        /// GetCommentOverviewAsync; Get comments data set
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing comment information.</returns>
        public async Task<DataSet> GetCommentOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersActionOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersCommentOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersCommentsTagsOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersActionOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersCommentOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                parametersCommentsTagsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersActionOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersCommentOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                parametersCommentsTagsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_comments_overview",
                                                            parameters: parametersCommentOverview,
                                                            dataTableName: "COMMENTS"));

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_TAGS"))
            {
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_comments_tags_overview",
                                                               parameters: parametersCommentsTagsOverview,
                                                               dataTableName: "COMMENTS TAGS"));
            }

            return output;
        }

        #endregion

        #region - auditing log -

        /// <summary>
        /// GetAuditingLogOverviewAsync; Get auditing log data set
        /// </summary>
        /// <param name="companyid">CompanyId of the company.</param>
        /// <param name="from">from datetime.</param>
        /// <param name="to">to datetime.</param>
        /// <param name="timespanInDays">Timespan from now in days.</param>
        /// <returns>Data set containing data auditing log information.</returns>
        public async Task<DataSet> GetAuditingLogOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null)
        {
            var output = new DataSet();

            var parametersAuditingOverview = _dbmanager.GetBaseParameters(companyId: companyId);


            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersAuditingOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));


            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersAuditingOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));

            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_auditinglog_overview",
                                                            parameters: parametersAuditingOverview,
                                                            dataTableName: "AUDITING LOG"));

            return output;
        }

        #endregion

        #region - automated exports -

        public async Task<DataSet> GetAutomatedTaskExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = await _automatedExportManager.GetAutomatedTaskExportsOverview(holdingId: holdingId, companyId: companyId, from: from, to: to, dataType: dataType);
            return output;
        }

        public async Task<DataSet> GetAutomatedChecklistExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = await _automatedExportManager.GetAutomatedChecklistExportsOverview(holdingId: holdingId, companyId: companyId, from: from, to: to, dataType: dataType);
            return output;
        }

        public async Task<DataSet> GetAutomatedAuditExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = await _automatedExportManager.GetAutomatedAuditExportsOverview(holdingId: holdingId, companyId: companyId, from: from, to: to, dataType: dataType);
            return output;
        }

        public async Task<DataSet> GetAutomatedDatawarehouseExport(int holdingId, int companyId, string module, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = await _automatedExportManager.GetAutomatedDatawarehouseExport(holdingId: holdingId, companyId: companyId, module: module, from: from, to: to, dataType: dataType);
            return output;
        }

        public async Task<DataSet> GetAutomatedDataCollectionExport(int companyId, string module, DateTime? from = null)
        {
            var output = await _automatedExportManager.GetAutomatedDatCollectionExport(companyId: companyId, module: module, from: from);
            return output;
        }

        public async Task<DataTable> GetAutomatedExportAtoss(int holdingId, int companyId, string module, List<int> validCompanyIds)
        {
            DataTable output = new DataTable();
            
            if(module == "masterdata") {
                output = await _automatedExportManager.GetAutomatedMasterDataExportForAtoss(holdingId: holdingId, companyId: companyId, validCompanyIds: validCompanyIds, validHoldingIds: (new int[] { holdingId }).ToList(), checkValidIds: validCompanyIds != null && validCompanyIds.Count > 0);
            } else if (module == "scoredata") {
                output = await _automatedExportManager.GetAutomatedScoreDataExportForAtoss(holdingId: holdingId, companyId: companyId, validCompanyIds: validCompanyIds, validHoldingIds: (new int[] { holdingId }).ToList(), checkValidIds: validCompanyIds != null && validCompanyIds.Count > 0);
            } else if (module == "user") {
                output = await _automatedExportManager.GetAutomatedUserExportForAtoss(holdingId: holdingId, companyId: companyId, validCompanyIds: validCompanyIds, validHoldingIds: (new int[] { holdingId }).ToList(), checkValidIds: validCompanyIds != null && validCompanyIds.Count > 0);
            }

            return output;
        }

        #endregion

        #region - configuration
        public async Task<DataSet> GetCompanyActiveAreas(int companyid)
        {
            var output = new DataSet();

            var parametersActiveAreas = _dbmanager.GetBaseParameters(companyId: companyid);

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_data_company_areas",
                                                            parameters: parametersActiveAreas,
                                                            dataTableName: "Company areas"));

            return output;
        }
        #endregion

        #region - manual exports to datawarehouse -
        /// <summary>
        /// Exports data to the data warehouse using the specified stored procedure.
        /// </summary>
        /// <remarks>This method supports exporting data for either a single company or all companies
        /// within a holding. If <paramref name="holdingId"/> is greater than 0, the method retrieves all company IDs
        /// associated with the holding and performs the export for each company. Otherwise, the export is performed for
        /// the specified company only.</remarks>
        /// <param name="holdingId">The ID of the holding. If greater than 0, the method exports data for all companies within the holding. If 0
        /// or less, the method exports data for the specified company.</param>
        /// <param name="companyId">The ID of the company for which data should be exported. This parameter is used only if <paramref
        /// name="holdingId"/> is 0 or less.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute for the export operation.</param>
        /// <param name="fromTime">The start time for the data export range. If null, the export will not be time-bound.</param>
        /// <param name="toTime">The end time for the data export range. If null, the export will not be time-bound.</param>
        /// <returns><see langword="true"/> if the export operation succeeds for all companies; otherwise, <see
        /// langword="false"/>.</returns>
        public async Task<bool> ExportToDatawarehouse(int holdingId, int companyId, string storedProcedureName, DateTime? fromTime = null, DateTime? toTime = null)
        {
            if(holdingId > 0)
            {
                //companies within holding
                var output = false;
                foreach(int cId in await GetHoldingCompanyIdsForDW(holdingId: holdingId))
                {
                    var companyOutput = await _automatedExportManager.ExportToDatawarehouse(holdingId: holdingId, companyId: cId, storedProcedureName: storedProcedureName, fromTime: fromTime.Value, toTime: toTime.Value);
                    if (companyOutput) output = true;
                }
                return output;
            } else
            {
                //company only
                return await _automatedExportManager.ExportToDatawarehouse(holdingId: holdingId, companyId: companyId, storedProcedureName: storedProcedureName, fromTime: fromTime.Value, toTime: toTime.Value);
            }  
        }

        /// <summary>
        /// Retrieves a list of company IDs associated with the specified holding ID for data warehouse processing.
        /// </summary>
        /// <remarks>This method executes a stored procedure to fetch company IDs from the database.
        /// Ensure that the database connection is properly configured and accessible.</remarks>
        /// <param name="holdingId">The ID of the holding company for which associated company IDs are to be retrieved. Must be a valid,
        /// non-negative integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of integers, where each
        /// integer is the ID of a company associated with the specified holding ID. If no companies are associated, the
        /// list will be empty.</returns>
        private async Task<List<int>> GetHoldingCompanyIdsForDW(int holdingId)
        {
            List<int> companyIds = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));

                using (dr = await _dbmanager.GetDataReader("get_companies_for_dw_with_holding", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                         companyIds.Add(Convert.ToInt32(dr["company_id"]));
                        
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ExportManager.GetHoldingCompanyIdsForDW(): ", ex.Message));
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return companyIds;
        }
        #endregion


        #region - private methods -
        /// <summary>
        /// AppendDataTableColumns; Append datatable colums to source.
        /// </summary>
        /// <param name="dtSource">Source where items needs to be added.</param>
        /// <param name="columnNames">Columns to be added.</param>
        /// <returns>Return datatable.</returns>
        private DataTable AppendDataTableColumns(DataTable dtSource, List<string> columnNames)
        {
            foreach(var columnName in columnNames)
            {
                dtSource.Columns.Add(columnName);
            }
            return dtSource;
        }

        //TODO move custom implementations to export object for every specific export. For now it can stay here.

        /// <summary>
        /// AppendDataTaskTemplateOverview; Add data to the database set for TaskTemplate;
        /// </summary>
        /// <param name="dtSource">Source DataTable</param>
        /// <returns>DataTable with appended data.</returns>
        private DataTable AppendDataTaskTemplateOverview(DataTable dtSource, List<TaskTemplateRelationShift> tasktemplateShiftsRelations)
        {
            if(dtSource!=null && tasktemplateShiftsRelations != null && tasktemplateShiftsRelations.Any())
            {
                foreach(DataRow dr in dtSource.Rows)
                {
                    if(dr["TaskID"] != DBNull.Value && dr["TaskID"] != null && !string.IsNullOrEmpty(dr["TaskID"].ToString()))
                    {
                        foreach (TaskTemplateRelationShift item in tasktemplateShiftsRelations.Where(x => x.TaskTemplateId == Convert.ToInt32(dr["TaskID"])).ToList()) {
                            var columnName = string.Concat("shift_", item.ShiftId.ToString());
                            dr[columnName] = 1;
                        }
                    }
                }
            }
            return dtSource;
        }

        /// <summary>
        /// AppendHeaderRowsTaskTemplateOverview; Append extra header rows to datatables.
        /// </summary>
        /// <param name="dtSource">Source DataTable</param>
        /// <param name="shifts">Collection of shift objects.</param>
        /// <returns>DataTable with appended data.</returns>
        private DataTable AppendHeaderRowsShiftsTaskTemplateOverview(DataTable dtSource, List<Shift> shifts)
        {

            var drWeekNames = dtSource.NewRow();
            var drStartTimes = dtSource.NewRow();
            var drEndTimes = dtSource.NewRow();
            var drShiftNumbers = dtSource.NewRow();

            foreach(var shift in shifts)
            {
                string columnName = string.Concat("shift_", shift.Id.ToString());
                drWeekNames[columnName] = CultureInfo.GetCultureInfo("en-GB").DateTimeFormat.DayNames[shift.Weekday == 6 ? 0 : shift.Weekday + 1].Substring (0,2).ToUpper(); //TODO make culture info dynamic
                drStartTimes[columnName] = shift.Start.ToString();
                drEndTimes[columnName] = shift.End.ToString();
                drShiftNumbers[columnName] = shift.ShiftNr.ToString();
            }

            foreach(DataColumn column in dtSource.Columns)
            {
                if(!column.ColumnName.StartsWith("shift"))
                    drShiftNumbers[column.ColumnName] = column.ColumnName;
            }

            // reverse add all rows to datatable (at position 0);
            dtSource.Rows.InsertAt(drShiftNumbers, 0);
            dtSource.Rows.InsertAt(drEndTimes, 0);
            dtSource.Rows.InsertAt(drStartTimes, 0);
            dtSource.Rows.InsertAt(drWeekNames, 0);

            return dtSource;
        }

        /// <summary>
        /// AppendHeaderRowsColumns; Append a header row to the dataset.
        /// </summary>
        /// <param name="dtSource">Source DataTable</param>
        /// <returns>DataTable with appended data.</returns>
        private DataTable AppendHeaderRowsColumns(DataTable dtSource)
        {
            var drFirstRow = dtSource.NewRow();

            foreach (DataColumn column in dtSource.Columns)
            {

                drFirstRow[column.ColumnName] = column.ColumnName;
            }

            dtSource.Rows.InsertAt(drFirstRow, 0);

            return dtSource;
        }

        /// <summary>
        /// Init features object.
        /// </summary>
        /// <param name="companyId">CompanyId (DB.companies_company.id)</param>
        private async Task InitFeatures(int companyId)
        {
            _features = await _generalManager.GetFeatures(companyId: companyId, userId: null);
        }
         #endregion

    }
}
