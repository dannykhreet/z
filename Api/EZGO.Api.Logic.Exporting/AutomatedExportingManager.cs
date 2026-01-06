using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Exporting.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EZGO.Api.Logic.Exporting
{
    /// <summary>
    /// AutomatedExportingManager, contains all functionality used for creating data overviews/ export collection for use in Reporting/Exporting.
    /// Depending on settings most will output a datatable or dataset for further processing (most of the times to excel file or csv).
    /// </summary>
    public class AutomatedExportingManager : BaseManager<AutomatedExportingManager>, IAutomatedExportingManager
    {
        public enum ExportDataTabTypeEnum
        {
            Overview = 0,
            Items = 1,
            Actions = 2,
            Comments = 3,
            ValueRegistration = 4,
            OpenFields = 5,
            Tags = 6,
            PictureProof = 7
        }

        public enum ExportDataWarehouseModulesEnum
        {
            tasks, audits, checklists, assessments, actions, comments, areas, companies, shifts
        }

        private readonly IDatabaseAccessHelper _dbmanager;
        private readonly IConfigurationHelper _configHelper;
        private readonly IConnectionHelper _connectionHelper;

        public AutomatedExportingManager(IDatabaseAccessHelper dbmanager, IConfigurationHelper configHelper, IConnectionHelper connectionHelper, ILogger<AutomatedExportingManager> logger) : base(logger)
        {
            _dbmanager = dbmanager;
            _configHelper = configHelper;
            _connectionHelper = connectionHelper;
        }

        #region - automated exports -

        public async Task<DataSet> GetAutomatedTaskExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = new DataSet();

            var parametersTaskOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTaskItemsActionsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTaskItemsCommentsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersTaskItemsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersTaskOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersTaskItemsActionsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersTaskItemsCommentsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersTaskItemsPropertiesOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersTaskOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersTaskItemsActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersTaskItemsCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersTaskItemsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersTaskOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersTaskItemsActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersTaskItemsCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersTaskItemsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_task_overview",
                                                            parameters: parametersTaskOverview,
                                                            dataTableName: "TASKS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_task_actions_overview",
                                                            parameters: parametersTaskItemsActionsOverview,
                                                            dataTableName: "ACTIONS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_task_comments_overview",
                                                            parameters: parametersTaskItemsCommentsOverview,
                                                            dataTableName: "COMMENTS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_task_properties_overview",
                                                            parameters: parametersTaskItemsPropertiesOverview,
                                                            dataTableName: "VALUE REGISTRATION"));
            return output;
        }

        public async Task<DataSet> GetAutomatedChecklistExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = new DataSet();

            var parametersChecklistOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersChecklistItemsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersChecklistItemsActionsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersChecklistItemsCommentsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersChecklistItemsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersChecklistItemsOpenfieldsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersChecklistOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower()) parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersChecklistItemsActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersChecklistItemsCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersChecklistItemsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower()) parametersChecklistItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersChecklistOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower()) parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersChecklistItemsActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersChecklistItemsCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersChecklistItemsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower()) parametersChecklistItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersChecklistOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower()) parametersChecklistItemsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersChecklistItemsActionsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersChecklistItemsCommentsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersChecklistItemsPropertiesOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower()) parametersChecklistItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_checklist_overview",
                                                            parameters: parametersChecklistOverview,
                                                            dataTableName: "CHECKLISTS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_checklist_items_overview",
                                                            parameters: parametersChecklistItemsOverview,
                                                            dataTableName: "CHECKLIST ITEMS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_checklist_items_actions_overview",
                                                            parameters: parametersChecklistItemsActionsOverview,
                                                            dataTableName: "ACTIONS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_checklist_items_comments_overview",
                                                            parameters: parametersChecklistItemsCommentsOverview,
                                                            dataTableName: "COMMENTS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_checklist_items_properties_overview",
                                                            parameters: parametersChecklistItemsPropertiesOverview,
                                                            dataTableName: "VALUE REGISTRATION"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_checklist_openfields_properties_overview",
                                                            parameters: parametersChecklistItemsOpenfieldsPropertiesOverview,
                                                            dataTableName: "OPEN FIELDS"));

            return output;
        }

        public async Task<DataSet> GetAutomatedAuditExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = new DataSet();

            var parametersAuditOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersAuditItemsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersAuditItemsActionsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersAuditItemsCommentsOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersAuditItemsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyId);
            var parametersAuditItemsOpenfieldsPropertiesOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersAuditOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower()) parametersAuditItemsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersAuditItemsActionsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersAuditItemsCommentsOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersAuditItemsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower()) parametersAuditItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));

            }

            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersAuditOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower()) parametersAuditItemsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersAuditItemsActionsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersAuditItemsCommentsOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersAuditItemsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
                if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower()) parametersAuditItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower()) parametersAuditOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower()) parametersAuditItemsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower()) parametersAuditItemsActionsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower()) parametersAuditItemsCommentsOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower()) parametersAuditItemsPropertiesOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower()) parametersAuditItemsOpenfieldsPropertiesOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Overview.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_audit_overview",
                                                            parameters: parametersAuditOverview,
                                                            dataTableName: "AUDITS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Items.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_audit_items_overview",
                                                            parameters: parametersAuditItemsOverview,
                                                            dataTableName: "AUDITS ITEMS"));

            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Actions.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_audit_items_actions_overview",
                                                            parameters: parametersAuditItemsActionsOverview,
                                                            dataTableName: "ACTIONS"));


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.Comments.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_audit_items_comments_overview",
                                                            parameters: parametersAuditItemsCommentsOverview,
                                                            dataTableName: "COMMENTS"));


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.ValueRegistration.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_audit_items_properties_overview",
                                                            parameters: parametersAuditItemsPropertiesOverview,
                                                            dataTableName: "VALUE REGISTRATION"));


            if (string.IsNullOrEmpty(dataType) || dataType.ToLower() == ExportDataTabTypeEnum.OpenFields.ToString().ToLower())
                output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_audit_openfields_properties_overview",
                                                            parameters: parametersAuditItemsOpenfieldsPropertiesOverview,
                                                            dataTableName: "OPEN FIELDS"));

            return output;
        }

        #endregion

        #region - automated exports datawarehouse -
        /// <summary>
        /// GetAutomatedDatawarehouseExport; Retrieve data based on the datawarehouse snapper functionality. 
        /// </summary>
        /// <param name="holdingId"></param>
        /// <param name="companyId"></param>
        /// <param name="module"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="dataType"></param>
        /// <returns>DataSet containing the data.</returns>
        public async Task<DataSet> GetAutomatedDatawarehouseExport(int holdingId, int companyId, string module, DateTime? from = null, DateTime? to = null, string dataType = null)
        {
            var output = new DataSet();
            var types = new List<string>();  
            var companies = new List<int>();

            //check if specific data type needs to be retrieved
            if(string.IsNullOrEmpty(dataType))
            {
                types = await GetFromModuleDataTypes(module: module);
            } else
            {
                types.Add(dataType);
            }

            //check if there is a holding, if so retrieve all underlying companies.
            if(holdingId > 0)
            { 
                companies.AddRange(await GetHoldingCompanies(holdingId: holdingId));
            } else
            {
                companies.Add(companyId);
            }

            foreach (string dtype in types)
            {
                var dt = new DataTable(tableName: dtype.ToUpper());
                var spName = string.Concat(ConvertModuleToDatabaseNamePart(module), "_", ConvertModuleDataTypeToDatabaseNamePart(module, dtype));
                var sp = string.Concat("export_data_dw_", spName); 

                foreach (var cid in companies)
                {
                    var parameters = await GetAutomatedDatawarehouseParameters(holdingId: holdingId, companyId: companyId, from: from, to: to);

                    //if there is 1 company or no rows or new table, just retrieve table and overwrite
                    if (companies.Count == 1 || dt.Rows == null || dt.Rows.Count == 0)
                    {
                        dt = await _dbmanager.GetDataTable(procedureNameOrQuery: sp,
                                                        parameters: parameters,
                                                        dataTableName: spName.ToLower());
                    } else
                    {
                        //else retrieve table and merge with current table.
                        dt.Merge(await _dbmanager.GetDataTable(procedureNameOrQuery: sp,
                                                        parameters: parameters,
                                                        dataTableName: spName.ToLower()));
                    }
                }

                //convert all names to lower to normalize data.
                foreach(DataColumn col in dt.Columns)
                {
                    col.ColumnName = col.ColumnName.ToLower();
                }

                output.Tables.Add(dt); //add the table to the collection.
            }

            return output;
        }

        /// <summary>
        /// GetAutomatedDatawarehouseParameters; Retrieve a set of parameters that can be used with the datawarehouse retrieval stored procedures. 
        /// </summary>
        /// <param name="holdingId"></param>
        /// <param name="companyId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>List of Npgsql Parameters</returns>
        private async Task<List<NpgsqlParameter>> GetAutomatedDatawarehouseParameters(int holdingId, int companyId, DateTime? from = null, DateTime? to = null)
        {
            var parametersTaskOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            parametersTaskOverview.Add(new NpgsqlParameter("@_holdingid", holdingId));
          
            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersTaskOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
              
            }
            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersTaskOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            await Task.CompletedTask;

            return parametersTaskOverview;
        }

        /// <summary>
        /// GetFromModuleDataTypes; Get available datatypes (tabs) for specific modules. 
        /// </summary>
        /// <param name="module">Module as string where the datatypes need to be retrieved.</param>
        /// <returns>List of datatypes as strings.</returns>
        private async Task<List<string>> GetFromModuleDataTypes(string module)
        {
            var output = new List<string>();
            //Overview, Items, Actions, Comments, ValueRegistration, OpenFields, Tags, PictureProof

            switch (module)
            {
                case "checklists": case "audits": output.AddRange(new[] { "overview", "items", "actions", "comments", "valueregistration", "openfields", "tags", "itemtags" }.ToList());  break;
                case "tasks": output.AddRange(new[] { "overview", "actions", "comments", "valueregistration", "tags", "linked" }.ToList()); break;
                case "actions": case "comments": output.AddRange(new[] { "overview", "tags" }.ToList()); break;
                case "areas": case "shifts": case "companies": output.AddRange(new[] { "overview" }.ToList()); break;
                case "assessments" : output.AddRange(new[] { "overview", "instructions", "items", "tags" }.ToList()); break;
            }

            await Task.CompletedTask;

            return output;
        }

        /// <summary>
        /// ConvertModuleToDatabaseNamePart; Convert a module to a stored procedure name part for datawarehouse stored procedures. 
        /// </summary>
        /// <param name="module">Module as string what needs to be converted.</param>
        /// <returns>String containing a part of a datawarehouse retrieval stored procedure.</returns>
        private string ConvertModuleToDatabaseNamePart(string module)
        {
            var output = "";
            switch (module)
            {
                case "checklists": output = "checklist"; break;
                case "audits": output = "audit"; break;
                case "tasks": output = "task"; break;
                case "actions": output = "action"; break;
                case "comments": output = "comment"; break;
                case "areas": output = "area"; break;
                case "shifts": output = "shift"; break;
                case "companies": output = "company"; break;
                case "assessments": output = "assessment"; break;
            }
            return output;
        }

        /// <summary>
        /// ConvertModuleDataTypeToDatabaseNamePart; Convert datatype (and module) to a part of a datawarehouse stored procedure. 
        /// </summary>
        /// <param name="module">Module (as string) needed for certain conversion rules.</param>
        /// <param name="dataType">Datatype that needs to be converted.</param>
        /// <returns>String containing a part of a datawarehouse retrieval stored procedure.</returns>
        private string ConvertModuleDataTypeToDatabaseNamePart(string module, string dataType)
        {
            var output = "";
            if (dataType == "overview")
            {
                output = "overview";
            } else
            {
                if(module == "tasks")
                {
                    if (dataType == "openfields")
                    {
                        output = "openfields_properties_overview";
                    }
                    else if (dataType == "valueregistration")
                    {
                        output = string.Concat("properties", "_overview");
                    } else
                    {
                        output = string.Concat(dataType.ToLower(), "_overview");
                    }
                } else if (module == "audits" || module == "checklists")
                {
                    if(dataType == "openfields")
                    {
                        output = "openfields_properties_overview";
                    } else if (dataType == "valueregistration")
                    {
                        output = string.Concat("items_", "properties", "_overview");
                    } else if (dataType == "items" || (dataType == "tags"))
                    {
                        output = string.Concat(dataType.ToLower(), "_overview");
                    }
                    else if (dataType == "itemtags")
                    {
                        output = string.Concat("items_tags", "_overview");
                    } else
                    {
                        output = string.Concat("items_", dataType.ToLower(), "_overview");
                    }
                } else if (module == "assessments") 
                {
                    if (dataType == "instructions")
                    {
                        output = string.Concat("instruction", "_overview");
                    } else if (dataType == "tags") 
                    { 
                        output = string.Concat(dataType.ToLower(), "_overview");
                    } else
                    {
                        output = string.Concat("instruction_", dataType.ToLower(), "_overview");
                    }
                } else
                {
                    output = string.Concat(dataType.ToLower(), "_overview");
                }
               
            }   
            return output;
        }

        /// <summary>
        /// GetHoldingCompanies()
        /// </summary>
        /// <param name="holdingId">Holding Id where company needs to be retrieved.</param>
        /// <returns></returns>
        private async Task<List<int>> GetHoldingCompanies(int holdingId)
        {
            var output = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _dbmanager.GetDataReader("get_holding_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if(Convert.ToInt32(dr["holding_id"]) == holdingId) //only add specific data
                        {
                            output.Add(Convert.ToInt32(dr["company_id"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AutomatedExportManager.GetHoldingCompanies(): ", ex.Message));
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }
        #endregion

        #region - data warehouse manual import / export -
        private bool ValidateStoredProcedureName (string name)
        {
            //NOTE, list is setup the same as in DW project, DataManager.RetrieveStoredProcedures for ez checking if something is missing.
            List<string> storedProcedures = new List<string> { "export_data_dw_audit_items_actions_overview", "export_data_dw_audit_items_comments_overview", "export_data_dw_audit_items_overview", "export_data_dw_audit_items_properties_overview", "export_data_dw_audit_openfields_properties_overview", "export_data_dw_audit_overview",
            "export_data_dw_checklist_items_actions_overview", "export_data_dw_checklist_items_comments_overview", "export_data_dw_checklist_items_overview", "export_data_dw_checklist_items_properties_overview", "export_data_dw_checklist_openfields_properties_overview", "export_data_dw_checklist_overview",
            "export_data_dw_task_actions_overview", "export_data_dw_task_comments_overview", "export_data_dw_task_overview", "export_data_dw_task_properties_overview",
            "export_data_dw_action_overview",
            "export_data_dw_comment_overview",
            "export_data_dw_assessment_overview", "export_data_dw_assessment_instruction_overview", "export_data_dw_assessment_instruction_items_overview",
            "export_data_dw_area_overview",
            "export_data_dw_shift_overview",
            "export_data_dw_company_overview",
            "export_data_dw_tag_overview", "export_data_dw_tag_items_overview", "export_data_dw_action_tags_overview", "export_data_dw_comment_tags_overview", "export_data_dw_checklist_items_tags_overview", "export_data_dw_checklist_tags_overview", "export_data_dw_audit_items_tags_overview", "export_data_dw_audit_tags_overview", "export_data_dw_task_tags_overview", "export_data_dw_assessment_tags_overview", "export_data_dw_assessment_instruction_tags_overview", "export_data_dw_assessment_instruction_items_tags_overview",
            "export_data_dw_pictureproof_overview", "export_data_dw_checklist_items_pictureproof_overview", "export_data_dw_audit_items_pictureproof_overview", "export_data_dw_task_pictureproof_overview",
            "export_data_dw_task_linked_overview",
            "export_data_dw_task_linked_executed_overview",
            "export_data_dw_template_overview",
            "export_data_dw_checklist_stages_overview",
            "export_data_dw_task_times_overview",
            "export_data_dw_company_extendedinformation_overview",
            "export_data_dw_task_workinstructions_overview", "export_data_dw_audit_items_workinstruction_overview", "export_data_dw_checklist_items_workinstruction_overview",
            "export_data_dw_matrix_overview", "export_data_dw_matrix_skills_overview", "export_data_dw_matrix_groups_overview",
            "export_data_dw_skill_overview" ,
            "export_data_dw_group_overview", "export_data_dw_group_users_overview"};

            if (storedProcedures.Contains(name))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ExportToDatawarehouse(int holdingId, int companyId, string storedProcedureName, DateTime fromTime, DateTime toTime)
        {
            var output = false;
            if (!string.IsNullOrEmpty(storedProcedureName) && ValidateStoredProcedureName(name: storedProcedureName))
            {

                var sourceDt = await RetrieveDataTableSource(holdingId: holdingId, companyId: companyId, fromTime: fromTime, toTime: toTime, storedProcedureName: storedProcedureName);
                if (sourceDt != null && sourceDt.Rows != null && sourceDt.Rows.Count > 0)
                {
                    output = await SaveDataTable(holdingId: holdingId, companyId: companyId, storedProcedureName: storedProcedureName, sourceTable: sourceDt);
                }

            }
            return output;
        }

        /// <summary>
        /// RetrieveDataTableSource; Retrieve data from source database. 
        /// NOTE; This is copy of the functionality as written for the datawarehouse sync.
        /// </summary>
        /// <param name="holdingId">HoldingId, of the current holding</param>
        /// <param name="companyId">CompanyId, of the current company</param>
        /// <param name="storedProcedureName">StoredProcedure name to be used for retrieval of data at the source database.</param>
        /// <param name="fromTime">fromTime of time window to be retrieved. This is a full date. But reflects a smaller time window. (mostly 10 minutes difference with toTime)</param>
        /// <param name="toTime">toTime of time window to be retrieved. This is a full date. But reflects a smaller time window. (mostly 10 minutes difference with fromTime)</param>
        /// <returns>DataTable containing data for further processing.</returns>
        public async Task<DataTable> RetrieveDataTableSource(int holdingId, int companyId, string storedProcedureName, DateTime fromTime, DateTime toTime)
        {
            if (holdingId == 0 && companyId == 0) return null;
            if (string.IsNullOrEmpty(storedProcedureName)) return null;
            DataTable dt = new DataTable();

            try
            {

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_starttimestamp", new DateTime(year:fromTime.Year, month:fromTime.Month, day: fromTime.Day, hour: fromTime.Hour, minute:fromTime.Minute, second:fromTime.Second, kind: DateTimeKind.Unspecified)));
                parameters.Add(new NpgsqlParameter("@_endtimestamp", new DateTime(year: toTime.Year, month: toTime.Month, day: toTime.Day, hour: toTime.Hour, minute: toTime.Minute, second: toTime.Second, kind: DateTimeKind.Unspecified)));
                

                dt = await _dbmanager.GetDataTable(procedureNameOrQuery: storedProcedureName, parameters: parameters, commandType: CommandType.StoredProcedure, connectionKind: Data.Enumerations.ConnectionKind.Writer, dataTableName:"retrieve_table");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format("RetrieveDataTableSource: {0}", ex.Message));
            }
            finally
            {

            }

            return dt;
        }

        /// <summary>
        /// SaveDataTable(); Save a set of data to the DW database.
        /// NOTE; This is copy of the functionality as written for the datawarehouse sync.
        /// </summary>
        /// <param name="holdingId">HoldingId, of the current holding</param>
        /// <param name="companyId">CompanyId, of the current company</param>
        /// <param name="storedProcedureName">StoredProcedure name to be used for retrieval of data at the source database.</param>
        /// <param name="sourceTable">Source DataTable to be processed. Should contain all relevant data.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SaveDataTable(int holdingId, int companyId, string storedProcedureName, DataTable sourceTable)
        {
            int outputCounter = 0;

            if (holdingId == 0 && companyId == 0)
            {
                return false;
            }

            //warning, change to bulk insert construction if performance is an issue.
            if (sourceTable != null && sourceTable.Rows != null)
            {
                NpgsqlCommand cmd = null;

                var connectionDwString = _connectionHelper.GetConnectionStringWriter();
                //TODO, replace with full connection string when config of API has been changes on deployment. 
                //currently same user for connections can be used. 
                if(!string.IsNullOrEmpty(connectionDwString))
                {
                    connectionDwString = connectionDwString.Replace("Database=ezgo;", "Database=ez_dw;");
                }
                NpgsqlConnection connDW = new NpgsqlConnection(connectionDwString);

#pragma warning disable CS0168 // Variable is declared but never used
                try
                {

                    await connDW.OpenAsync();
                    //create dynamic sp string based on hold/user
                    string convertedStoredProcedure = string.Format("{0}_{1}_{2}", storedProcedureName.Replace("export_data_dw_", "import_data_"), holdingId, companyId);

                    //data is stored based on holding id (retrieved based on company id) therefor save to company agnostic table.
                    if (holdingId > 0)
                    {
                        convertedStoredProcedure = string.Format("{0}_{1}_0", storedProcedureName.Replace("export_data_dw_", "import_data_"), holdingId);
                    }

                    foreach (DataRow dr in sourceTable.Rows)
                    {
                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                        for (var i = 0; i < sourceTable.Columns.Count; i++)
                        {
                            var column = sourceTable.Columns[i];
                            if (dr[column.ColumnName.ToLower()] != DBNull.Value)
                            {
                                parameters.Add(new NpgsqlParameter(string.Format("@_{0}", column.ColumnName.ToLower()), dr[column.ColumnName.ToLower()]));
                            }
                        }

                        cmd = new NpgsqlCommand(cmdText: DataConnectorHelper.WrapFunctionCommand(convertedStoredProcedure, parameters) , connection: connDW);
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.AddRange(parameters.ToArray());

                        var result = await cmd.ExecuteNonQueryAsync();
                        outputCounter = outputCounter + 1;

                    }
                }
                catch (Exception ex)
                {
                    if(connDW != null)
                    {
                        await connDW.CloseAsync();
                        connDW.Close();
                        connDW = null;
                    }
                    if(cmd != null)
                    {
                        cmd.Dispose();
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
                    if (cmd != null) await cmd.DisposeAsync();
                }
#pragma warning restore CS0168 // Variable is declared but never used


            }

            return (outputCounter > 0);
        }
        #endregion

        #region - data collection exports-
        /// <summary>
        /// GetAutomatedDatawarehouseExport; Retrieve data based on the datawarehouse snapper functionality. 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="module"></param>
        /// <param name="from"></param>
        /// <returns>DataSet containing the data.</returns>
        public async Task<DataSet> GetAutomatedDatCollectionExport(int companyId, string module, DateTime? from = null)
        {
            var output = new DataSet();
            var dt = new DataTable(tableName: module.ToUpper());
            var sp = string.Format("get_user_data_{0}_by_companyid_date_modifiedat", module);

            var parametersOverview = new List<NpgsqlParameter>();

            parametersOverview.Add(new NpgsqlParameter("@_company_id", companyId));

            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersOverview.Add(new NpgsqlParameter("@_modified_at", from.Value));

            } else
            {
                parametersOverview.Add(new NpgsqlParameter("@_modified_at", DateTime.Now.AddDays(-1)));
            }

            dt = await _dbmanager.GetDataTable(procedureNameOrQuery: sp,
                                                    parameters: parametersOverview,
                                                    dataTableName: module.ToLower());

            //convert all names to lower to normalize data.
            foreach (DataColumn col in dt.Columns)
            {
                col.ColumnName = col.ColumnName.ToLower();
            }

            output.Tables.Add(dt); //add the table to the collection.
           
            return output;
        }
        #endregion

        #region - custom -

        public async Task<DataSet> GetAutomatedCustomExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null)
        {
            var output = new DataSet();
            var parametersCustomOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            //_starttimestamp
            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersCustomOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            //_endtimestamp
            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersCustomOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_task_overview_custom",
                                                    parameters: parametersCustomOverview,
                                                    dataTableName: "EXPORT"));


            return output;

        }
        public async Task<DataSet> GetAutomatedCustomBackwardsCompatibleExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null)
        {
            var output = new DataSet();
            var parametersCustomOverview = _dbmanager.GetBaseParameters(companyId: companyId);

            //_starttimestamp
            if (from.HasValue && from.Value != DateTime.MinValue)
            {
                parametersCustomOverview.Add(new NpgsqlParameter("@_starttimestamp", from.Value));
            }

            //_endtimestamp
            if (to.HasValue && to.Value != DateTime.MinValue)
            {
                parametersCustomOverview.Add(new NpgsqlParameter("@_endtimestamp", to.Value));
            }

            output.Tables.Add(await _dbmanager.GetDataTable(procedureNameOrQuery: "export_automated_data_task_overview_custom_bc",
                                                        parameters: parametersCustomOverview,
                                                        dataTableName: "EXPORT_BC"));

            return output;
        }

        #endregion

        #region - export logging -
        public async Task<bool> AddExportLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "")
        {
            if (_configHelper.GetValueAsBool("AppSettings:EnableDbLogging"))
            {
                try
                {
                    var source = _configHelper.GetValueAsString("AppSettings:ApplicationName");

                    var parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message.Length > 255 ? message.Substring(0,254) : message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventId.ToString()));
                    parameters.Add(new NpgsqlParameter("@_eventname", eventName));

                    if (string.IsNullOrEmpty(source))
                    {
                        parameters.Add(new NpgsqlParameter("@_source", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_source", source));
                    }
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    var output = await _dbmanager.ExecuteScalarAsync("add_log_exporter", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    _logger.LogError(exception: ex, message: string.Concat("AutomatedExportingManager.AddExportLogEvent(): ", ex.Message));
                }
                finally
                {

                }
            }
            return true;
        }
        #endregion

        #region - atoss related - 
        private async Task<DataTable> GetAutomatedExportForAtoss(int holdingId, int companyId, string storedProcedure, string tableName, bool checkValidIds, List<int> validHoldingIds, List<int> validCompanyIds)
        {
            var companies = new List<int>();

            //check if there is a holding, if so retrieve all underlying companies.
            if (holdingId > 0)
            {
                if(checkValidIds && !validHoldingIds.Contains(holdingId))
                {
                    await AddExportLogEvent(message: "Holding not in exporter settings.", description: string.Format("Run not started for ({0}).", holdingId));
                    return new DataTable("");
                }

                var holdingCompanyIds = await GetHoldingCompanies(holdingId: holdingId); //retrieve all companies of holding
                if(checkValidIds)
                {
                    //check if companies in holding is part of valid company ids. 
                    foreach (var cid in holdingCompanyIds)
                    {
                        if (validCompanyIds.Contains(cid))
                        {
                            companies.Add(cid);
                        } else
                        {
                            await AddExportLogEvent(message: "CompanyId not in exporter settings.", description: string.Format("Not added to valid companies ({0}).", cid));
                        }
                    }
                } else
                {
                    //add all companyholdings to set. 
                    companies.AddRange(holdingCompanyIds);
                }
            }
            else
            {
                if (checkValidIds && !validCompanyIds.Contains(companyId))
                {
                    await AddExportLogEvent(message: "CompanyId not in exporter settings.", description: string.Format("Run not started for ({0}).", companyId));
                    return new DataTable("");
                }
                companies.Add(companyId);
            }

            var dt = new DataTable(tableName: tableName.ToUpper());

            foreach (var cid in companies)
            {
                await AddExportLogEvent(message: "Retrieve data for company", description: string.Format("Data retrieved for ({1} - {0}).", cid, holdingId));

                var parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                parameters.Add(new NpgsqlParameter("@_companyid", cid));

                //if there is 1 company or no rows or new table, just retrieve table and overwrite
                if (companies.Count == 1)
                {
                    dt = await _dbmanager.GetDataTable(procedureNameOrQuery: storedProcedure,
                                                    parameters: parameters,
                                                    dataTableName: tableName.ToLower());
                }
                else
                {
                    //else retrieve table and merge with current table.
                    dt.Merge(await _dbmanager.GetDataTable(procedureNameOrQuery: storedProcedure,
                                                    parameters: parameters,
                                                    dataTableName: tableName.ToLower()));
                }
                
            }

            //convert all names to lower to normalize data.
            foreach (DataColumn col in dt.Columns)
            {
                col.ColumnName = col.ColumnName.ToLower();
            }

            return (dt);
        }

        //TODO possibly rename
        public async Task<DataTable> GetAutomatedUserExportForAtoss(int holdingId, int companyId, List<int> validHoldingIds, List<int> validCompanyIds, bool checkValidIds = true)
        {
            return await GetAutomatedExportForAtoss(holdingId: holdingId, companyId: companyId, storedProcedure: "export_data_atoss_users", tableName: "USER_EXPORT", checkValidIds: checkValidIds, validHoldingIds: validHoldingIds, validCompanyIds: validCompanyIds);
        }

        public async Task<DataTable> GetAutomatedScoreDataExportForAtoss(int holdingId, int companyId, List<int> validHoldingIds, List<int> validCompanyIds, bool checkValidIds = true)
        {
            return await GetAutomatedExportForAtoss(holdingId: holdingId, companyId: companyId, storedProcedure: "export_data_atoss_scores", tableName: "SCORE_EXPORT", checkValidIds: checkValidIds, validHoldingIds: validHoldingIds, validCompanyIds: validCompanyIds);
        }

        public async Task<DataTable> GetAutomatedMasterDataExportForAtoss(int holdingId, int companyId, List<int> validHoldingIds, List<int> validCompanyIds, bool checkValidIds = true)
        {
            return await GetAutomatedExportForAtoss(holdingId: holdingId, companyId: companyId, storedProcedure: "export_data_atoss_master", tableName: "MASTER_EXPORT", checkValidIds: checkValidIds, validHoldingIds: validHoldingIds, validCompanyIds: validCompanyIds);
        }
        #endregion

    }
}
