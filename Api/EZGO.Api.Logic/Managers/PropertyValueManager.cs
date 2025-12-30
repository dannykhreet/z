using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.PropertyValue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EZGO.Api.Models.Enumerations;
using System.Linq;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Utils;
using Microsoft.Extensions.Logging;
using EZGO.Api.Logic.Base;
using Npgsql;
using System.Data;
using EZGO.Api.Interfaces.Settings;
using System.Diagnostics;
using EZGO.Api.Settings;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EEZGO.Api.Utils.Data;

//TODO: regroup methods and create correct regions.
//TODO refactor, split up in seperate entities based on audit, checklist, task etc.


namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// PropertyValueManager; Property Value Manager, contains functionalities for adding/changing properties to the database, adding/changing/retrieving property values to the database
    /// and adding/changing/retrieving audit, audititems, audittemplates, tasks, tasktemplates, checklist, checklisttemplates - property related items.
    /// This manager is mostly used in the Task, Checklist and Audit manager. 
    /// NOTE! not all of the functionalities are currently in use or in production. 
    /// NOTE! due to the size of this manager, this will be split up in the near future.
    /// </summary>
    public class PropertyValueManager : BaseManager<PropertyValueManager>, IPropertyValueManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;

        private List<Property> _properties;
        private List<PropertyValueKind> _propertyvaluekinds;
        private List<PropertyValue> _propertyvalues;
        #endregion

        #region - constructor(s) -
        public PropertyValueManager(IDatabaseAccessHelper manager, IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, ILogger<PropertyValueManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public property methods -
        /// <summary>
        /// GetPropertiesAsync; Get a list of properties for use with a company.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">PropertyFilters, containing a set of filters that will be used for filtering.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>A list of properties.</returns>
        public async Task<List<Property>> GetPropertiesAsync(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<Property>();

            NpgsqlDataReader dr = null;

            try
            {
                if (this._properties != null && this._properties.Count > 0)
                {
                    output = this._properties;
                }
                else
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                    using (dr = await _manager.GetDataReader("get_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                    {
                        while (await dr.ReadAsync())
                        {
                            var property = CreateOrFillPropertyFromReader(dr);
                            output.Add(property);
                        }
                    }

                    this._properties = output;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertiesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower())) output = await AppendPropertyValuesToProperties(companyId: companyId, properties: output);

            if(filters.HasValue && filters.Value.PropertyGroupIds!=null && filters.Value.PropertyGroupIds.Length > 0)
            {
                output = output.Where(x => filters.Value.PropertyGroupIds.ToList().Contains(x.PropertyGroupId)).ToList();
            }

            return output;
        }

        /// <summary>
        /// GetPropertyAsync; Get a specific property based on the property id
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyId">PropertyId (DB: properties.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>A property object.</returns>
        public async Task<Property> GetPropertyAsync(int companyId, int propertyId, int? userId = null, string include = null)
        {
            Property output = null;

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_propertyid", propertyId));

                using (dr = await _manager.GetDataReader("get_property", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillPropertyFromReader(dr);
                        output = property;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertiesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;

        }

        /// <summary>
        /// GetTaskTemplateProperties; Retrieve all task template properties (properties linked to a task template)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>A list of PropertyTaskTemplate items.</returns>
        public async Task<List<PropertyTaskTemplate>> GetTaskTemplateProperties(int companyId, List<int> taskTemplateIds = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyTaskTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (taskTemplateIds != null) 
                    parameters.Add(new NpgsqlParameter("@_tasktemplateids", taskTemplateIds));

                using (dr = await _manager.GetDataReader("get_tasktemplates_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillTaskTemplatePropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetTaskTemplateProperties(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetChecklistTemplateProperties; Retrieve all checklist template properties (properties linked to a checklist template)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of PropertyChecklistTemplate items.</returns>
        private async Task<List<PropertyChecklistTemplate>> GetChecklistTemplateProperties(int companyId, List<int> checklistTemplateIds)
        {
            var output = new List<PropertyChecklistTemplate>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (checklistTemplateIds != null)
                {
                    parameters.Add(new NpgsqlParameter("@_checklisttemplateids", checklistTemplateIds));
                }

                using (dr = await _manager.GetDataReader("get_checklisttemplate_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillChecklistTemplatePropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetChecklistTemplateProperties(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetChecklistTemplateProperties; Retrieve all checklist template properties (properties linked to a checklist template) linked to a specific checklistTemplateId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId (DB: checklists_checklisttemplate.id)</param>
        /// <returns>List of PropertyChecklistTemplate items.</returns>
        private async Task<List<PropertyChecklistTemplate>> GetChecklistTemplateProperties(int companyId, int checklistTemplateId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyChecklistTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_templateid", checklistTemplateId));

                using (dr = await _manager.GetDataReader("get_checklisttemplate_properties_by_template", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillChecklistTemplatePropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetChecklistTemplateProperties(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetAuditTemplateProperties;  Retrieve all audit template properties (properties linked to a audit template) 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of PropertyAuditTemplate items</returns>
        private async Task<List<PropertyAuditTemplate>> GetAuditTemplateProperties(int companyId, List<int> auditTemplateIds = null)
        {
            var output = new List<PropertyAuditTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if(auditTemplateIds != null)
                {
                    parameters.Add(new NpgsqlParameter("@_audittemplateids", auditTemplateIds));
                }

                using (dr = await _manager.GetDataReader("get_audittemplate_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillAuditTemplatePropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetAuditTemplateProperties(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetAuditTemplateProperties; Retrieve all audit template properties (properties linked to a audit template) linked to audit template id;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <returns>List of PropertyAuditTemplate items</returns>
        private async Task<List<PropertyAuditTemplate>> GetAuditTemplateProperties(int companyId, int auditTemplateId)
        {
            var output = new List<PropertyAuditTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_templateid", auditTemplateId));

                using (dr = await _manager.GetDataReader("get_audittemplate_properties_by_template", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillAuditTemplatePropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetAuditTemplateProperties(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetTaskTemplateProperties; Retrieve all task template properties (properties linked to a task template) linked to tasktemplateid.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplateId">TaskTemplateId (DB: tasks_task.id)</param>
        /// <returns>list of PropertyTaskTemplate items</returns>
        public async Task<List<PropertyTaskTemplate>> GetTaskTemplateProperties(int companyId, int taskTemplateId)
        {
            var output = new List<PropertyTaskTemplate>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_tasktemplateid", taskTemplateId));

                using (dr = await _manager.GetDataReader("get_tasktemplates_properties_by_tasktemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillTaskTemplatePropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetTaskTemplateProperties(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }
        #endregion

        #region - public property group methods -
        /// <summary>
        /// GetPropertyGroupsAsync; Get a list of property groups. Depending on settings, also underlying properties are included.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters which can be used for filtering the returning data set</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. </param>
        /// <returns></returns>
        public async Task<List<PropertyGroup>> GetPropertyGroupsAsync(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<PropertyGroup>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                using (dr = await _manager.GetDataReader("get_propertygroups", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var propertygroup = CreateOrFillPropertyGroupFromReader(dr);
                        output.Add(propertygroup);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyGroupsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower())) output = await AppendTemplatePropertiesToGroups(companyId: companyId, propertyGroups: output, include: include);
            
            return output;
        }
        #endregion

        #region - public properties audittemplates, tasktemplates and checklisttempaltes -
        /// <summary>
        /// GetPropertiesAuditTemplatesAsync; Get a list of PropertyAuditTemplate items (DB: audits_audittemplate_properties), containing basic property information if included. 
        /// Currently only open field properties are implemented for AuditTemplates.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns></returns>
        public async Task<List<PropertyAuditTemplate>> GetPropertiesAuditTemplatesAsync(int companyId, List<int> auditTemplateIds = null, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<PropertyAuditTemplate>();
            var templateproperties = await GetAuditTemplateProperties(companyId: companyId, auditTemplateIds: auditTemplateIds);

            if (templateproperties != null && templateproperties.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFieldsPropertyDetails.ToString().ToLower()))
                {
                    var properties = await GetPropertiesBasedOnTemplate("get_properties_with_audittemplates", ids: auditTemplateIds, companyId: companyId, include: include);

                    foreach (var templateprop in templateproperties)
                    {
                        templateprop.Property = properties.Where(x => x.Id == templateprop.PropertyId).FirstOrDefault();
                        if (!templateprop.FieldType.HasValue && templateprop.Property != null)
                        {
                            templateprop.FieldType = templateprop.Property.FieldType;
                        }
                        if (!templateprop.ValueType.HasValue && templateprop.Property != null)
                        {
                            templateprop.ValueType = templateprop.Property.ValueType;
                        }
                    }
                }

                output = templateproperties;
            }

            return output;
        }

        /// <summary>
        /// GetPropertiesAuditTemplatesAsync; Get a list of PropertyAuditTemplate items (DB: audits_audittemplate_properties), containing basic property information if included based on a specific audit template. 
        /// Currently only open field properties are implemented for AuditTemplates.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>List of PropertyAuditTemplate items.</returns>
        public async Task<List<PropertyAuditTemplate>> GetPropertiesAuditTemplateAsync(int companyId, int auditTemplateId, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<PropertyAuditTemplate>();
            var templateproperties = await GetAuditTemplateProperties(companyId: companyId, auditTemplateId: auditTemplateId);

            if (templateproperties != null && templateproperties.Count > 0)
            {

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFieldsPropertyDetails.ToString().ToLower()))
                {
                    var properties = await GetPropertiesBasedOnTemplate("get_properties_with_audittemplates", ids: new List<int>() { auditTemplateId }, companyId: companyId, include: include);

                    foreach (var templateprop in templateproperties)
                    {
                        templateprop.Property = properties.Where(x => x.Id == templateprop.PropertyId).FirstOrDefault();
                        if (!templateprop.FieldType.HasValue && templateprop.Property != null)
                        {
                            templateprop.FieldType = templateprop.Property.FieldType;
                        }
                        if (!templateprop.ValueType.HasValue && templateprop.Property != null)
                        {
                            templateprop.ValueType = templateprop.Property.ValueType;
                        }
                    }
                }

                output = templateproperties;
            }

            return output;
        }

        /// <summary>
        /// GetPropertiesChecklistTemplatesAsync; Get a list of PropertyChecklistTemplate items (DB: checklists_checklisttemplate_properties), containing basic property information if included. 
        /// Currently only open field properties are implemented for ChecklistTemplates.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>List of PropertyChecklistTemplate items.</returns>
        public async Task<List<PropertyChecklistTemplate>> GetPropertiesChecklistTemplatesAsync(int companyId, List<int> checklistTemplateIds = null, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<PropertyChecklistTemplate>();
            var templateproperties = await GetChecklistTemplateProperties(companyId: companyId, checklistTemplateIds: checklistTemplateIds);

            if (templateproperties != null && templateproperties.Count > 0)
            {

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFieldsPropertyDetails.ToString().ToLower()))
                {
                    var properties = await GetPropertiesBasedOnTemplate("get_properties_with_checklisttemplates", companyId: companyId, ids: checklistTemplateIds, include: include);

                    foreach (var templateprop in templateproperties)
                    {
                        templateprop.Property = properties.Where(x => x.Id == templateprop.PropertyId).FirstOrDefault();
                        if (!templateprop.FieldType.HasValue && templateprop.Property != null)
                        {
                            templateprop.FieldType = templateprop.Property.FieldType;
                        }
                        if (!templateprop.ValueType.HasValue && templateprop.Property != null)
                        {
                            templateprop.ValueType = templateprop.Property.ValueType;
                        }
                    }
                }

                output = templateproperties;
            }

            return output;
        }

        /// <summary>
        /// GetPropertiesChecklistTemplatesAsync; Get a list of PropertyChecklistTemplate items (DB: checklists_checklisttemplate_properties), containing basic property information if included based checklisttemplate. 
        /// Currently only open field properties are implemented for ChecklistTemplates. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId (DB: checklist_checklisttemplate.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>List of PropertyChecklistTemplate items.</returns>
        public async Task<List<PropertyChecklistTemplate>> GetPropertiesChecklistTemplateAsync(int companyId, int checklistTemplateId, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyChecklistTemplate>();
            var templateproperties = await GetChecklistTemplateProperties(companyId: companyId, checklistTemplateId: checklistTemplateId, connectionKind: connectionKind);

            if (templateproperties != null && templateproperties.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFieldsPropertyDetails.ToString().ToLower()))
                {
                    var properties = await GetPropertiesBasedOnTemplate("get_properties_with_checklisttemplates", companyId: companyId, ids: new List<int>() { checklistTemplateId }, include: include, connectionKind: connectionKind);

                    foreach (var templateprop in templateproperties)
                    {
                        templateprop.Property = properties.Where(x => x.Id == templateprop.PropertyId).FirstOrDefault();
                        if (!templateprop.FieldType.HasValue && templateprop.Property != null)
                        {
                            templateprop.FieldType = templateprop.Property.FieldType;
                        }
                        if (!templateprop.ValueType.HasValue && templateprop.Property != null)
                        {
                            templateprop.ValueType = templateprop.Property.ValueType;
                        }
                    }
                }

                output = templateproperties;
            }

            return output;
        }


        /// <summary>
        /// GetPropertiesTaskTemplatesAsync; Get a list of task template properties (DB: tasks_tasktemplate_properties). 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>A list of PropertyTaskTemplate items</returns>
        public async Task<List<PropertyTaskTemplate>> GetPropertiesTaskTemplatesAsync(int companyId, int? userId = null, List<int> taskTemplateIds = null, PropertyFilters? filters = null, string include = null, bool? isActive = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyTaskTemplate>();
            var templateproperties = await GetTaskTemplateProperties(companyId: companyId, taskTemplateIds: taskTemplateIds, connectionKind: connectionKind);
            if (templateproperties != null && isActive.HasValue) templateproperties = templateproperties.Where(x => x.IsActive == isActive).ToList();

            if(templateproperties != null && templateproperties.Count > 0)
            {
                //temp override
                include = "properties,propertydetails,propertyvalues";

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))
                {
                    var properties = await GetPropertiesBasedOnTemplate("get_properties_with_tasktemplates", ids: taskTemplateIds, companyId: companyId, include: include, connectionKind: connectionKind);

                    foreach (var templateprop in templateproperties)
                    {
                        templateprop.Property = properties.Where(x => x.Id == templateprop.PropertyId).FirstOrDefault();
                        if(!templateprop.FieldType.HasValue && templateprop.Property != null)
                        {
                            templateprop.FieldType = templateprop.Property.FieldType;
                        }
                        if (!templateprop.ValueType.HasValue && templateprop.Property != null)
                        {
                            templateprop.ValueType = templateprop.Property.ValueType;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()))
                {
                    var propertyvalues = await GetPropertyValues(companyId: companyId, connectionKind: connectionKind);
                    foreach (var templateprop in templateproperties)
                    {
                        if (templateprop.PropertyValueId.HasValue)
                        {
                            templateprop.PropertyValue = propertyvalues.Where(x => x.Id == templateprop.PropertyValueId).FirstOrDefault();
                        }
                    }
                }

                output = templateproperties;
            }


            return output;
        }

        /// <summary>
        /// GetPropertiesTaskTemplateAsync; Get a list of task template properties (DB: tasks_tasktemplate_properties) based on a specific task template.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplateId">TaskTemplateId (DB: tasks_tasktemplate.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum.</param>
        /// <param name="isActive">Filter is active items directly (used for CMS)</param>
        /// <returns>List of PropertyTaskTemplate items</returns>
        public async Task<List<PropertyTaskTemplate>> GetPropertiesTaskTemplateAsync(int companyId, int taskTemplateId, int? userId = null, PropertyFilters? filters = null, string include = null, bool? isActive = null)
        {
            var output = new List<PropertyTaskTemplate>();
            var templateproperties = await GetTaskTemplateProperties(companyId: companyId, taskTemplateId: taskTemplateId);
            if (templateproperties != null && isActive.HasValue) templateproperties = templateproperties.Where(x => x.IsActive == isActive).ToList();

            if(templateproperties != null && templateproperties.Count > 0)
            {
                //temp override
                include = "properties,propertydetails,propertyvalues";

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))
                {
                    var properties = await GetPropertiesBasedOnTemplate("get_properties_with_tasktemplates", ids: new List<int>() { taskTemplateId }, companyId: companyId, include: include); //TODO make better filter

                    foreach (var templateprop in templateproperties)
                    {
                        templateprop.Property = properties.Where(x => x.Id == templateprop.PropertyId).FirstOrDefault();
                        if (!templateprop.FieldType.HasValue && templateprop.Property != null)
                        {
                            templateprop.FieldType = templateprop.Property.FieldType;
                        }
                        if (!templateprop.ValueType.HasValue && templateprop.Property != null)
                        {
                            templateprop.ValueType = templateprop.Property.ValueType;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()))
                {
                    var propertyvalues = await GetPropertyValues(companyId: companyId);
                    foreach (var templateprop in templateproperties)
                    {
                        if (templateprop.PropertyValueId.HasValue)
                        {
                            templateprop.PropertyValue = propertyvalues.Where(x => x.Id == templateprop.PropertyValueId).FirstOrDefault();
                        }
                    }
                }

                output = templateproperties;
            }

            return output;
        }

        #endregion

        #region - public properties actions -

        /// <summary>
        /// GetPropertiesActionsAsync; NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters"></param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. </param>
        /// <returns></returns>
        public async Task<List<PropertyAction>> GetPropertiesActionsAsync(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<PropertyAction>();
            //TODO implement : var properties = await GetPropertiesBasedOnTemplate("get_properties_with_actions", companyId: companyId, include: include);
            await Task.CompletedTask;
            //TODO implement;

            return output;
        }
        #endregion

        #region - public propertyuservalues with tasks, actions, checklists and audits-
        /// <summary>
        /// GetPropertyUserValuesWithTasks; Get property user values (user inputted items, DB: tasks_properties);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of PropertyUserValue items.</returns>
        public async Task<List<PropertyUserValue>> GetPropertyUserValuesWithTasks(int companyId, int? userId = null, List<long> taskIds = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyUserValue>();

            //NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId)
                };
                if (taskIds != null)
                {
                    List<int> taskIdsInts = taskIds.ConvertAll(taskId => Convert.ToInt32(taskId)); //convert longs to ints since the db uses int4 for task_id, remove this conversion once task_id is int8 in db
                    parameters.Add(new NpgsqlParameter("@_task_ids", taskIdsInts));
                }

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_tasks_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind);
                while (await dr.ReadAsync())
                {
                    var objectPropertyValueItem = CreateOrFillPropertyUserValueItemFromReader(dr);
                    output.Add(objectPropertyValueItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyValuesWithTasks(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }
        
        /// <summary>
        /// GetPropertyUserValuesWithTasks; Get property user values (user inputted items, DB: tasks_properties);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of PropertyUserValue items.</returns>
        public async Task<List<PropertyUserValue>> GetPropertyUserValuesByTaskId(int companyId, int taskId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyUserValue>();

            //NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_taskid", taskId)
                };

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_task_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind);
                while (await dr.ReadAsync())
                {
                    var objectPropertyValueItem = CreateOrFillPropertyUserValueItemFromReader(dr);
                    output.Add(objectPropertyValueItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyValuesWithTasks(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        /// <summary>
        /// GetPropertyUserValuesWithActions; Get property user values (user inputted items, DB: tasks_properties)
        /// NOTE NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of PropertyUserValue items</returns>
        public async Task<List<PropertyUserValue>> GetPropertyUserValuesWithActions(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null)
        {
            var output = new List<PropertyUserValue>();

            var properties = await GetPropertiesActionsAsync(companyId: companyId);

            return output;
        }

        /// <summary>
        /// GetPropertyUserValuesWithChecklists; Get property user values with checklists (user inputted items, DB: checklists_properties)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of PropertyUserValue items</returns>
        public async Task<List<PropertyUserValue>> GetPropertyUserValuesWithChecklists(int companyId, List<int> checklistIds = null, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyUserValue>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (checklistIds != null)
                {
                    parameters.Add(new NpgsqlParameter("@_checklistids", checklistIds));
                }

                using (dr = await _manager.GetDataReader("get_checklists_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var objectPropertyValueItem = CreateOrFillPropertyUserValueItemFromReader(dr);
                        output.Add(objectPropertyValueItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyUserValuesWithChecklists(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetPropertyUserValuesWithChecklists; Get property user values with checklists (user inputted items, DB: checklists_properties)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of PropertyUserValue items</returns>
        public async Task<List<PropertyUserValue>> GetPropertyUserValuesByChecklistId(int companyId, int checklistId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyUserValue>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_checklistid", checklistId));

                using (dr = await _manager.GetDataReader("get_checklist_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var objectPropertyValueItem = CreateOrFillPropertyUserValueItemFromReader(dr);
                        output.Add(objectPropertyValueItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyUserValuesByChecklistId(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }
        /// <summary>
        /// GetPropertyUserValuesWithAudits; Get property user values with audits (user inputted items, DB: audits_properties)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of PropertyUserValue items</returns>
        public async Task<List<PropertyUserValue>> GetPropertyUserValuesWithAudits(int companyId, List<int> auditIds = null, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyUserValue>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (auditIds != null)
                {
                    parameters.Add(new NpgsqlParameter("@_auditids", auditIds));
                }

                using (dr = await _manager.GetDataReader("get_audits_properties", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var objectPropertyValueItem = CreateOrFillPropertyUserValueItemFromReader(dr);
                        output.Add(objectPropertyValueItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyUserValuesWithAudits(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        #endregion

        #region - public propertyvalues -
        /// <summary>
        /// GetPropertyValues; Get a list of property values (DB: propertyvalue)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>List of PropertyValue items for use with properties.</returns>
        public async Task<List<PropertyValue>> GetPropertyValues(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyValue>();

            //check if propertyvalues are already retrieved if so return immediately
            if (this._propertyvalues != null && this._propertyvalues.Count > 0) return this._propertyvalues;

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                using (dr = await _manager.GetDataReader("get_propertyvalues", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var propertyvalue = CreateOrFillPropertyValueFromReader(dr);
                        output.Add(propertyvalue);
                    }
                }

                this._propertyvalues = output; //set local storage
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyValues(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetPropertyValueKinds; Get a list of property value kinds (DB: propertyvaluekind)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">Filters that are used for filtering information.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>List of PropertyValueKind for use with properties</returns>
        public async Task<List<PropertyValueKind>> GetPropertyValueKinds(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<PropertyValueKind>();

            NpgsqlDataReader dr = null;

            try
            {
                if (this._propertyvaluekinds != null && this._propertyvaluekinds.Count > 0)
                {
                    //check if items are already retrieved, if so use this set as output.
                    output = this._propertyvaluekinds;
                }
                else
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                    using (dr = await _manager.GetDataReader("get_propertyvaluekinds", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                    {
                        while (await dr.ReadAsync())
                        {
                            var propertyvaluekind = CreateOrFillPropertyValueKindFromReader(dr);
                            output.Add(propertyvaluekind);
                        }
                    }

                    this._propertyvaluekinds = output; //set local variable with collection
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertyValueKinds(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower())) output = await AppendPropertyValuesToPropertyKinds(companyId: companyId, propertyvaluekinds: output, connectionKind: connectionKind);

            return output;
        }

        #endregion

        #region - public add/change/remove tasktemplateproperty -

        /// <summary>
        /// AddTaskTemplatePropertyAsync; Add a tasktemplateproperty (DB: tasks_tasktemplate_properties)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty"></param>
        /// <returns></returns>
        public async Task<int> AddTaskTemplatePropertyAsync(int companyId, int userId, PropertyTaskTemplate templateProperty)
        {
           
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromTaskTemplateProperty(propertyTaskTemplate: templateProperty, companyId: companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_tasktemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), possibleId);
            await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: string.Concat("Added tasktemplate property."));


            return possibleId;
        }

        /// <summary>
        /// ChangeTaskTemplatePropertyAsync; Change a tasktemplateproperty based on its id.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty"></param>
        /// <returns></returns>
        public async Task<int> ChangeTaskTemplatePropertyAsync(int companyId, int userId, int taskTemplatePropertyId, PropertyTaskTemplate templateProperty)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), taskTemplatePropertyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromTaskTemplateProperty(propertyTaskTemplate: templateProperty, companyId: companyId, propertyTemplateId: taskTemplatePropertyId));

            var rownr = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_tasktemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(rownr > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), taskTemplatePropertyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), objectId: taskTemplatePropertyId, userId: userId, companyId: companyId, description: string.Concat("Changed tasktemplate property."));
            }

            return rownr;
        }

        /// <summary>
        /// RemoveTaskTemplatePropertyAsync; Remove property from tasktemplate;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplatePropertyId"></param>
        /// <returns></returns>
        public async Task<int> RemoveTaskTemplatePropertyAsync(int companyId, int userId, int taskTemplatePropertyId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), taskTemplatePropertyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_tasktemplatepropertyid", taskTemplatePropertyId));

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_tasktemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), taskTemplatePropertyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tasks_tasktemplate_properties.ToString(), objectId: taskTemplatePropertyId, userId: userId, companyId: companyId, description: string.Concat("Removed or set is active state tasktemplate property."));
            }

            return rowCount;
        }

        #endregion

        #region - add/change/remove audittemplateproperty -

        /// <summary>
        /// AddTaskTemplatePropertyAsync; Add a tasktemplateproperty (DB: tasks_tasktemplate_properties)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty"></param>
        /// <returns></returns>
        public async Task<int> AddAuditTemplatePropertyAsync(int companyId, int userId, PropertyAuditTemplate templateProperty)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAuditTemplateProperty(propertyAuditTemplate: templateProperty, companyId: companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_audittemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), possibleId);
            await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: string.Concat("Added audittemplate property."));


            return possibleId;
        }

        /// <summary>
        /// ChangeTaskTemplatePropertyAsync; Change a tasktemplateproperty based on its id.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty"></param>
        /// <returns></returns>
        public async Task<int> ChangeAuditTemplatePropertyAsync(int companyId, int userId, int auditTemplatePropertyId, PropertyAuditTemplate templateProperty)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), auditTemplatePropertyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAuditTemplateProperty(propertyAuditTemplate: templateProperty, companyId: companyId, propertyTemplateId: auditTemplatePropertyId));

            var rownr = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_audittemplate_property", parameters: parameters, commandType: CommandType.StoredProcedure));

            if (rownr > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), auditTemplatePropertyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), objectId: auditTemplatePropertyId, userId: userId, companyId: companyId, description: string.Concat("Changed audittemplate property."));
            }

            return rownr;
        }

        /// <summary>
        /// RemoveTaskTemplatePropertyAsync; Remove property from tasktemplate;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplatePropertyId"></param>
        /// <returns></returns>
        public async Task<int> RemoveAuditTemplatePropertyAsync(int companyId, int userId, int auditTemplatePropertyId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), auditTemplatePropertyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_audittemplatepropertyid", auditTemplatePropertyId));

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_audittemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(rowCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), auditTemplatePropertyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate_properties.ToString(), objectId: auditTemplatePropertyId, userId: userId, companyId: companyId, description: string.Concat("Removed or set is active state audittemplate property."));

            }
            return rowCount;
        }

        #endregion

        #region - add/change/remove checklisttemplateproperty -

        /// <summary>
        /// AddTaskTemplatePropertyAsync; Add a tasktemplateproperty (DB: tasks_tasktemplate_properties)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty"></param>
        /// <returns></returns>
        public async Task<int> AddChecklistTemplatePropertyAsync(int companyId, int userId, PropertyChecklistTemplate templateProperty)
        {

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromChecklistTemplateProperty(propertyChecklistTemplate: templateProperty, companyId: companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklisttemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), possibleId);
            await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: string.Concat("Added checklisttemplate property."));

            return possibleId;
        }

        /// <summary>
        /// ChangeTaskTemplatePropertyAsync; Change a tasktemplateproperty based on its id.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty"></param>
        /// <returns></returns>
        public async Task<int> ChangeChecklistTemplatePropertyAsync(int companyId, int userId, int checklistTemplatePropertyId, PropertyChecklistTemplate templateProperty)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), checklistTemplatePropertyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromChecklistTemplateProperty(propertyChecklistTemplate: templateProperty, companyId: companyId, propertyTemplateId: checklistTemplatePropertyId));

            var rownr = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_checklisttemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rownr > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), checklistTemplatePropertyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), objectId: checklistTemplatePropertyId, userId: userId, companyId: companyId, description: string.Concat("Changed checklisttemplate property."));
            }

            return rownr;
        }

        /// <summary>
        /// RemoveTaskTemplatePropertyAsync; Remove property from tasktemplate;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplatePropertyId"></param>
        /// <returns></returns>
        public async Task<int> RemoveChecklistTemplatePropertyAsync(int companyId, int userId, int checklistTemplatePropertyId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), checklistTemplatePropertyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_checklisttemplatepropertyid", checklistTemplatePropertyId));

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_checklisttemplate_property", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), checklistTemplatePropertyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_properties.ToString(), objectId: checklistTemplatePropertyId, userId: userId, companyId: companyId, description: string.Concat("Removed or set is active state checklisttemplate property."));

            }

            return rowCount;
        }

        #endregion

        #region - public add/change properties/propertyvalues -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="property"></param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddPropertyAsync(int companyId, Property property, int userId)
        {
            await Task.CompletedTask;
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddPropertyValueAsync(int companyId, PropertyValue propertyValue, int userId)
        {
            await Task.CompletedTask;
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyId"></param>
        /// <param name="property"></param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns></returns>
        public async Task<bool> ChangePropertyAsync(int companyId, int propertyId, Property property, int userId)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValueId"></param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns></returns>
        public async Task<bool> ChangePropertyValueAsync(int companyId, int propertyValueId, PropertyUserValue propertyValue, int userId)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public async Task<bool> SetPropertyActiveAsync(int companyId, int userId, int actionId, bool isActive = true)
        {
            await Task.CompletedTask;
            return false;
        }

        #endregion

        #region - public add/change property user values (audits, checklist, task properties) -
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>task property id. If propertyValue.TemplatePropertyId does not exist, returns 0</returns>
        public async Task<int> AddPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int userId)
        {
            int possibleId = await AddPropertyUserValueAsync(companyId: companyId, storedProcedure: "add_task_property", propertyUserValue: propertyValue, userId: userId);
            if (possibleId == 0)
            {
                _logger.LogWarning(message: "PropertyValueManager.AddPropertyUserValueAsync: TemplatePropertyId " + propertyValue.TemplatePropertyId + " was not found in tasks_tasktemplate_properties database.");
            }

            return possibleId;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="property"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>task property id. If property.TemplatePropertyId does not exist, returns 0</returns>
        public async Task<int> AddPropertyUserValueAsync(int companyId, PropertyDTO property, int userId)
        {
            int possibleId = await AddPropertyUserValueAsync(companyId: companyId, storedProcedure: "add_task_property", property: property, userId: userId);
            if (possibleId == 0)
            {
                _logger.LogWarning(message: "PropertyValueManager.AddPropertyUserValueAsync: TemplatePropertyId " + property.PropertyTemplate.Id + " was not found in tasks_tasktemplate_properties database.");
            }

            return possibleId;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddChecklistPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int userId)
        {
            return await AddPropertyUserValueAsync(companyId: companyId, storedProcedure: "add_checklist_property", propertyUserValue: propertyValue, userId: userId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddChecklistPropertyUserValueAsync(int companyId, PropertyDTO property, int userId)
        {
            return await AddPropertyUserValueAsync(companyId: companyId, storedProcedure: "add_checklist_property", property: property, userId: userId);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddAuditPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int userId)
        {
            return await AddPropertyUserValueAsync(companyId: companyId, storedProcedure: "add_audit_property", propertyUserValue: propertyValue, userId: userId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddAuditPropertyUserValueAsync(int companyId, PropertyDTO property, int userId)
        {
            return await AddPropertyUserValueAsync(companyId: companyId, storedProcedure: "add_audit_property", property: property, userId: userId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> ChangePropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int propertyUserValueId, int userId)
        {
            return await ChangePropertyUserValueAsync(companyId: companyId, storedProcedure: "change_task_property", propertyUserValue: propertyValue, propertyUserValueId: propertyUserValueId, userId: userId);

        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="property"></param>
        /// <param name="propertyUserValueId"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> ChangePropertyUserValueAsync(int companyId, PropertyDTO property, int propertyUserValueId, int userId)
        {
            return await ChangePropertyUserValueAsync(companyId: companyId, storedProcedure: "change_task_property", property: property, propertyUserValueId: propertyUserValueId, userId: userId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> ChangeChecklistPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int propertyUserValueId, int userId)
        {
            return await ChangePropertyUserValueAsync(companyId: companyId, storedProcedure: "change_checklist_property", propertyUserValue: propertyValue, propertyUserValueId: propertyUserValueId, userId: userId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValue"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> ChangeAuditPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int propertyUserValueId, int userId)
        {
            return await ChangePropertyUserValueAsync(companyId: companyId, storedProcedure: "change_audit_property", propertyUserValue: propertyValue, propertyUserValueId: propertyUserValueId, userId: userId);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValues"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddChangeChecklistPropertyUserValuesAsync(int companyId, List<PropertyUserValue> propertyValues, int userId)
        {
            if(propertyValues != null && propertyValues.Count > 0)
            {
                int rows = 0;
                foreach (var propertyUserValue in propertyValues)
                {
                    if(propertyUserValue.Id > 0)
                    {
                        propertyUserValue.CompanyId = companyId;
                        rows = rows + await ChangeChecklistPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, propertyUserValueId: propertyUserValue.Id, userId: userId);

                    } else
                    {
                        propertyUserValue.CompanyId = companyId;
                        var result = await AddChecklistPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, userId: userId);
                        if (result > 0)
                        {
                            rows = rows + 1;
                        }
                    }
                }
                return rows;
            }
            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyValues"></param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        public async Task<int> AddChangeAuditPropertyUserValuesAsync(int companyId, List<PropertyUserValue> propertyValues, int userId)
        {
            if (propertyValues != null && propertyValues.Count > 0)
            {
                int rows = 0;
                foreach (var propertyUserValue in propertyValues)
                {
                    if (propertyUserValue.Id > 0)
                    {
                        propertyUserValue.CompanyId = companyId;
                        rows = rows + await ChangeAuditPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, propertyUserValueId: propertyUserValue.Id, userId: userId);
                    }
                    else
                    {
                        propertyUserValue.CompanyId = companyId;
                        var result = await AddAuditPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, userId: userId);
                        if(result > 0)
                        {
                            rows = rows + 1;
                        }
                    }
                }
                return rows;
            }
            return 0;
        }
        #endregion

        #region - private propertvalues -
        /// <summary>
        /// AppendPropertyValuesToProperties; Append property values to a list of properties.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="properties">List of properties.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>A list of properties with appended data.</returns>
        private async Task<List<Property>> AppendPropertyValuesToProperties(int companyId, List<Property> properties, ConnectionKind connectionKind = ConnectionKind.Reader)
        {

            if (properties != null && properties.Count > 0)
            {
                var propertyvaluekinds = await GetPropertyValueKinds(companyId: companyId, include: "propertyvalues", connectionKind: connectionKind);

                foreach (var property in properties)
                {
                    var propertyvaluekind = propertyvaluekinds.Where(x => x.Id == property.PropertyValueKindId).FirstOrDefault();
                    if (propertyvaluekind != null && propertyvaluekind.Id > 0)
                    {
                        property.PropertyValueKind = propertyvaluekind;
                        var propertyvalue = propertyvaluekind.PropertyValues.Where(y => y.Id == property.PropertyValueId).FirstOrDefault();
                        if (propertyvalue != null)
                        {
                            property.PropertyValue = propertyvalue;
                        }
                    }

                }
            }
            return properties;
        }

        /// <summary>
        /// AppendPropertyValuesToPropertyKinds; Append property values to property kind objects. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyvaluekinds">List of property value kinds.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>List of PropertyValueKind with appended data.</returns>
        private async Task<List<PropertyValueKind>> AppendPropertyValuesToPropertyKinds(int companyId, List<PropertyValueKind> propertyvaluekinds, ConnectionKind connectionKind = ConnectionKind.Reader)
        {

            if (propertyvaluekinds != null && propertyvaluekinds.Count > 0)
            {
                var propertyvalues = await GetPropertyValues(companyId: companyId, connectionKind: connectionKind);

                foreach (var propertyvaluekind in propertyvaluekinds)
                {
                    var currentPropertyValues = propertyvalues.Where(x => x.PropertyValueKindId == propertyvaluekind.Id).ToList();
                    if (propertyvaluekind != null && propertyvaluekind.Id > 0)
                    {
                        propertyvaluekind.PropertyValues = currentPropertyValues;
                    }
                }
            }

            return propertyvaluekinds;
        }
        #endregion

        #region - private properties -
        /// <summary>
        /// AppendTemplatePropertiesToGroups; Append properties to Task items. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyGroups">List of properties</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>List of groups with properties appended.</returns>
        private async Task<List<PropertyGroup>> AppendTemplatePropertiesToGroups(int companyId, List<PropertyGroup> propertyGroups, string include = null)
        {
            var properties = await GetPropertiesAsync(companyId: companyId, include: include);
            if (properties != null && properties.Count > 0)
            {
                foreach (var propertygroup in propertyGroups)
                {
                    var propertyGroupProperties = properties.Where(x => x.PropertyGroupId == propertygroup.Id).ToList();
                    if (propertyGroupProperties != null && propertyGroupProperties.Count > 0)
                    {
                        propertygroup.Properties = propertyGroupProperties;
                    }
                }
            }

            return propertyGroups;
        }

        #endregion

        #region - private property values / property value kinds -
        /// <summary>
        /// CreateOrFillPropertyValueFromReader; creates and fills a propertyvalue object from a DataReader.
        /// NOTE! intended for use with the propertyvalue stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="propertyvalue">Propertyvalue object, if not supplied it will be created.</param>
        /// <returns>Propertyvalue filled with data based on reader.</returns>
        private PropertyValue CreateOrFillPropertyValueFromReader(NpgsqlDataReader dr, PropertyValue propertyvalue = null)
        {
            if (propertyvalue == null) propertyvalue = new PropertyValue();

            propertyvalue.Id = Convert.ToInt32(dr["id"]);
            propertyvalue.PropertyValueKindId = Convert.ToInt32(dr["propertyvaluekind_id"]);
            propertyvalue.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                propertyvalue.Description = dr["description"].ToString();
            }

            if (dr["value_symbol"] != DBNull.Value && !string.IsNullOrEmpty(dr["value_symbol"].ToString()))
            {
                propertyvalue.ValueSymbol = dr["value_symbol"].ToString();
            }

            if (dr["value_abbreviation"] != DBNull.Value && !string.IsNullOrEmpty(dr["value_abbreviation"].ToString()))
            {
                propertyvalue.ValueAbbreviation = dr["value_abbreviation"].ToString();
            }

            if (dr["default_value_type"] != DBNull.Value && !string.IsNullOrEmpty(dr["default_value_type"].ToString()))
            {
                propertyvalue.DefaultValueType = (PropertyValueTypeEnum)Convert.ToInt32(dr["default_value_type"]);
            }

            if (dr["resource_key_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["resource_key_name"].ToString()))
            {
                propertyvalue.ResourceKeyName = dr["resource_key_name"].ToString();
            }

            if (dr["created_at"] != DBNull.Value)
            {
                propertyvalue.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }

            if (dr["modified_at"] != DBNull.Value)
            {
                propertyvalue.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            return propertyvalue;
        }

        /// <summary>
        /// GetNpgsqlParametersFromPropertyValue; Creates a list of NpgsqlParameters, and fills it based on the supplied feed item object.
        /// NOTE! intended for use with the property value item stored procedures within the database.
        /// NOTE! not yet implemented.
        /// </summary>
        /// <param name="property">property object</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyvalueId">PropertyvalueId (DB: propertvalues.id)</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromPropertyValue(PropertyValue property, int companyId, int propertyvalueId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyvalueId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyvalueId));


            return parameters;
        }

        /// <summary>
        /// CreateOrFillPropertyValueKindFromReader; creates and fills a propertyvaluekind object from a DataReader.
        /// NOTE! intended for use with the propertyvaluekind stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="propertyvaluekind">PropertyValueKind object, if not supplied it will be created.</param>
        /// <returns>PropertyValueKind filled with data from the datareader.</returns>
        private PropertyValueKind CreateOrFillPropertyValueKindFromReader(NpgsqlDataReader dr, PropertyValueKind propertyvaluekind = null)
        {
            if (propertyvaluekind == null) propertyvaluekind = new PropertyValueKind();

            propertyvaluekind.Id = Convert.ToInt32(dr["id"]);
            propertyvaluekind.Name = dr["name"].ToString();

            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                propertyvaluekind.Description = dr["description"].ToString();
            }
            if (dr["resource_key_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["resource_key_name"].ToString()))
            {
                propertyvaluekind.ResourceKeyName = dr["resource_key_name"].ToString();
            }
            if (dr["created_at"] != DBNull.Value)
            {
                propertyvaluekind.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                propertyvaluekind.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            return propertyvaluekind;
        }

        /// <summary>
        /// GetNpgsqlParametersFromPropertyValueKind;
        /// NOTE NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="propertyvaluekind">propertyvalue kind item</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyvalueId">PropertyvAlueId (DB: propertyvalue.id)</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromPropertyValueKind(PropertyValueKind propertyvaluekind, int companyId, int propertyvalueId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyvalueId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyvalueId));


            return parameters;
        }
        #endregion

        #region - private property groups -

        /// <summary>
        /// CreateOrFillPropertyGroupFromReader; creates and fills a propertyGroup object from a DataReader.
        /// NOTE! intended for use with the propertyGroup stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="propertyGroup">PropertyGroup object, will be created if not supplied.</param>
        /// <returns>PropertyGroup object containing data from the data reader.</returns>
        private PropertyGroup CreateOrFillPropertyGroupFromReader(NpgsqlDataReader dr, PropertyGroup propertyGroup = null)
        {
            if (propertyGroup == null) propertyGroup = new PropertyGroup();

            propertyGroup.Id = Convert.ToInt32(dr["id"]);
            propertyGroup.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                propertyGroup.Description = dr["description"].ToString();
            }
            if (dr["created_at"] != DBNull.Value)
            {
                propertyGroup.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                propertyGroup.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            return propertyGroup;
        }

        /// <summary>
        /// GetNpgsqlParametersFromPropertyGroup; Creates a list of NpgsqlParameters, and fills it based on the supplied propertyGroup object.
        /// NOTE! intended for use with the property group stored procedures within the database.
        /// NOTE! not yet implemented.
        /// </summary>
        /// <param name="propertyGroup">PropertyGroup object</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyGroupId">PRopertyGroupId (DB: propertygroup.id)</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromPropertyGroup(PropertyGroup propertyGroup, int companyId, int propertyGroupId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyGroupId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyGroupId));


            return parameters;
        }

        #endregion

        #region - private methods property user values -
        /// <summary>
        /// AddPropertyUserValueAsync; Add a property to a checklist/audit/task; Depending on the SP this will be a audit_property, checklist_property or task_property.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="storedProcedure">Stored Procedure to be executed.</param>
        /// <param name="propertyUserValue">PropertyUserValue, input of the user. </param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>Possible id</returns>
        private async Task<int> AddPropertyUserValueAsync(int companyId, string storedProcedure, PropertyUserValue propertyUserValue, int userId)
        {
            var type = "task";
            var table = Models.Enumerations.TableNames.tasks_properties.ToString();
            if (storedProcedure.Contains("audit"))
            {
                type = "audit";
                table = Models.Enumerations.TableNames.audits_properties.ToString();
            }
            if(storedProcedure.Contains("checklist"))
            {
                type = "checklist";
                table = Models.Enumerations.TableNames.checklists_properties.ToString();
            }

            //make sure correct user id is used, the general one, not the one in propertyUserValue
            propertyUserValue.UserId = userId;

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromPropertyUserValueItem(propertyUserValue: propertyUserValue, companyId: companyId, propertyUserValue.Id));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(table, possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, table, objectId: possibleId, userId: userId, companyId: companyId, description: string.Concat("Added ",type," property user value."));

            }

            return possibleId;

        }
        /// <summary>
        /// AddPropertyUserValueAsync; Add a property to a checklist/audit/task; Depending on the SP this will be a audit_property, checklist_property or task_property.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="storedProcedure">Stored Procedure to be executed.</param>
        /// <param name="property">PropertyDTO, input of the user. </param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>Possible id</returns>
        private async Task<int> AddPropertyUserValueAsync(int companyId, string storedProcedure, PropertyDTO property, int userId)
        {
            var type = "task";
            var table = Models.Enumerations.TableNames.tasks_properties.ToString();
            if (storedProcedure.Contains("audit"))
            {
                type = "audit";
                table = Models.Enumerations.TableNames.audits_properties.ToString();
            }
            if (storedProcedure.Contains("checklist"))
            {
                type = "checklist";
                table = Models.Enumerations.TableNames.checklists_properties.ToString();
            }

            List<NpgsqlParameter> parameters = GetNpgsqlParametersFromPropertyDTOItem(property: property, companyId: companyId, userId: userId, propertyUserValueId: property.UserValue.Id);

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(table, possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, table, objectId: possibleId, userId: userId, companyId: companyId, description: string.Concat("Added ", type, " property user value."));

            }

            return possibleId;
        }


        /// <summary>
        /// ChangePropertyUserValueAsync; Change property user value; Depending on the SP this will be a audit_property, checklist_property or task_property.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="storedProcedure">Stored Procedure to be executed.</param>
        /// <param name="propertyUserValue">PropertyUserValue, input of the user. </param>
        /// <param name="propertyUserValueId">PropertyValueUserValueId of the object that need to be updated.</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        private async Task<int> ChangePropertyUserValueAsync(int companyId, string storedProcedure, PropertyUserValue propertyUserValue, int propertyUserValueId, int userId)
        {
            var type = "task";
            var table = Models.Enumerations.TableNames.tasks_properties.ToString();
            if (storedProcedure.Contains("audit"))
            {
                type = "audit";
                table = Models.Enumerations.TableNames.audits_properties.ToString();
            }
            if (storedProcedure.Contains("checklist"))
            {
                type = "checklist";
                table = Models.Enumerations.TableNames.checklists_properties.ToString();
            }

            var original = await _manager.GetDataRowAsJson(table, propertyUserValueId);

            //make sure correct user id is used, the general one, not the one in propertyUserValue
            propertyUserValue.UserId = userId;

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromPropertyUserValueItem(propertyUserValue: propertyUserValue, companyId: companyId, propertyUserValueId: propertyUserValueId));

            var rowsNr = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowsNr > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(table, propertyUserValueId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, table, objectId: propertyUserValueId, userId: userId, companyId: companyId, description: string.Concat("Changed ", type, " property user value."));

            }

            return rowsNr;

        }
        /// <summary>
        /// ChangePropertyUserValueAsync; Change property user value; Depending on the SP this will be a audit_property, checklist_property or task_property.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="storedProcedure">Stored Procedure to be executed.</param>
        /// <param name="property">PropertyDTO, input of the user. </param>
        /// <param name="propertyUserValueId">PropertyValueUserValueId of the object that need to be updated.</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns></returns>
        private async Task<int> ChangePropertyUserValueAsync(int companyId, string storedProcedure, PropertyDTO property, int propertyUserValueId, int userId)
        {
            var type = "task";
            var table = Models.Enumerations.TableNames.tasks_properties.ToString();
            if (storedProcedure.Contains("audit"))
            {
                type = "audit";
                table = Models.Enumerations.TableNames.audits_properties.ToString();
            }
            if (storedProcedure.Contains("checklist"))
            {
                type = "checklist";
                table = Models.Enumerations.TableNames.checklists_properties.ToString();
            }

            var original = await _manager.GetDataRowAsJson(table, propertyUserValueId);

            List<NpgsqlParameter> parameters = GetNpgsqlParametersFromPropertyDTOItem(property: property, companyId: companyId, propertyUserValueId: propertyUserValueId, userId: userId);

            var rowsNr = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowsNr > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(table, propertyUserValueId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, table, objectId: propertyUserValueId, userId: userId, companyId: companyId, description: string.Concat("Changed ", type, " property user value."));

            }

            return propertyUserValueId;
        }

        #endregion

        #region - private methods properties / propertyvalue fillers -
        /// <summary>
        /// CreateOrFillPropertyFromReader; creates and fills a property object from a DataReader.
        /// NOTE! intended for use with the property stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="property">Property object, if not supplied it will be created.</param>
        /// <returns>Property containing relevant data of the reader.</returns>
        private Property CreateOrFillPropertyFromReader(NpgsqlDataReader dr, Property property = null)
        {
            if (property == null) property = new Property();

            property.Id = Convert.ToInt32(dr["id"]);
            if (dr["propertygroup_id"] != DBNull.Value)
            {
                property.PropertyGroupId = Convert.ToInt32(dr["propertygroup_id"]);
            }
            if (dr["propertyvaluekind_id"] != DBNull.Value)
            {
                property.PropertyValueKindId = Convert.ToInt32(dr["propertyvaluekind_id"]);
            }
            if (dr["propertyvalue_id"] != DBNull.Value)
            {
                property.PropertyValueId = Convert.ToInt32(dr["propertyvalue_id"]);
            }

            property.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                property.Description = dr["description"].ToString();
            }
            if (dr["shortname"] != DBNull.Value && !string.IsNullOrEmpty(dr["shortname"].ToString()))
            {
                property.ShortName = dr["shortname"].ToString();
            }
            property.FieldType = (PropertyFieldTypeEnum)Convert.ToInt32(dr["field_type"]);
            property.Type = (PropertyTypeEnum)Convert.ToInt32(dr["type"]);
            property.ValueType = (PropertyValueTypeEnum)Convert.ToInt32(dr["value_type"]);

            if (dr["display_value_type"] != DBNull.Value && !string.IsNullOrEmpty(dr["display_value_type"].ToString()))
            {
                property.DisplayValueType = (PropertyValueTypeEnum)Convert.ToInt32(dr["display_value_type"]);
            }
            if (dr["display_type"] != DBNull.Value && !string.IsNullOrEmpty(dr["display_type"].ToString()))
            {
                property.DisplayType = (PropertyDisplayTypeEnum)Convert.ToInt32(dr["display_type"]);
            }

            if (dr["resource_key_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["resource_key_name"].ToString()))
            {
                property.ResourceKeyName = dr["resource_key_name"].ToString();
            }

            if (dr["is_customer_specific"] != DBNull.Value && !string.IsNullOrEmpty(dr["is_customer_specific"].ToString()))
            {
                property.IsCustomerSpecific = Convert.ToBoolean(dr["is_customer_specific"]);
            }

            if (dr["is_system"] != DBNull.Value && !string.IsNullOrEmpty(dr["is_system"].ToString()))
            {
                property.IsSystem = Convert.ToBoolean(dr["is_system"]);
            }

            if (dr["created_at"] != DBNull.Value)
            {
                property.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                property.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }
            if (dr.HasColumn("template_id") && dr["template_id"] != DBNull.Value)
            {
                property.TemplateId = Convert.ToInt32(dr["template_id"]);
            }

            return property;
        }

        /// <summary>
        /// CreateOrFillTaskTemplatePropertyFromReader; Creates a list of NpgsqlParameters, and fills it based on the supplied tasktemplateproperty object.
        /// NOTE! intended for use with the tasktemplate property stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="tasktemplateproperty">ProeprtytaskTemplate object, if not supplied it will be created.</param>
        /// <returns>PropertyTaskTemplate object filled with data.</returns>
        private PropertyTaskTemplate CreateOrFillTaskTemplatePropertyFromReader(NpgsqlDataReader dr, PropertyTaskTemplate tasktemplateproperty = null)
        {
            if (tasktemplateproperty == null) tasktemplateproperty = new PropertyTaskTemplate();

            tasktemplateproperty.Id = Convert.ToInt32(dr["id"]);

            tasktemplateproperty.TaskTemplateId = Convert.ToInt32(dr["tasktemplate_id"]);
            tasktemplateproperty.PropertyId = Convert.ToInt32(dr["property_id"]);

            if (dr.HasColumn("propertygroup_id") && dr["propertygroup_id"] != DBNull.Value)
            {
                tasktemplateproperty.PropertyGroupId = Convert.ToInt32(dr["propertygroup_id"]);
            }

            if (dr["propertyvalue_id"] != DBNull.Value)
            {
                tasktemplateproperty.PropertyValueId = Convert.ToInt32(dr["propertyvalue_id"]);
            }

            if (dr["primary_int_value"] != DBNull.Value)
            {
                tasktemplateproperty.PrimaryIntValue = Convert.ToInt32(dr["primary_int_value"]);
            }

            if (dr["primary_decimal_value"] != DBNull.Value)
            {

                tasktemplateproperty.PrimaryDecimalValue = Convert.ToDecimal(dr["primary_decimal_value"]);
            }

            if (dr["primary_string_value"] != DBNull.Value)
            {
                tasktemplateproperty.PrimaryStringValue = dr["primary_string_value"].ToString();
            }

            if (dr["primary_datetime_value"] != DBNull.Value)
            {
                tasktemplateproperty.PrimaryDateTimeValue = Convert.ToDateTime(dr["primary_datetime_value"]);
            }

            if (dr["primary_time_value"] != DBNull.Value)
            {
                tasktemplateproperty.PrimaryTimeValue = dr["primary_time_value"].ToString();
            }

            if (dr["secondary_int_value"] != DBNull.Value)
            {
                tasktemplateproperty.SecondaryIntValue = Convert.ToInt32(dr["secondary_int_value"]);
            }

            if (dr["secondary_decimal_value"] != DBNull.Value)
            {
                tasktemplateproperty.SecondaryDecimalValue = Convert.ToDecimal(dr["secondary_decimal_value"]);
            }

            if (dr["secondary_string_value"] != DBNull.Value)
            {
                tasktemplateproperty.SecondaryStringValue = dr["secondary_string_value"].ToString();
            }

            if (dr["secondary_datetime_value"] != DBNull.Value)
            {
                tasktemplateproperty.SecondaryDateTimeValue = Convert.ToDateTime(dr["secondary_datetime_value"]);
            }

            if (dr["secondary_time_value"] != DBNull.Value)
            {
                tasktemplateproperty.SecondaryTimeValue = dr["secondary_time_value"].ToString();
            }

            if (dr["bool_value"] != DBNull.Value)
            {
                tasktemplateproperty.BoolValue = Convert.ToBoolean(dr["bool_value"]);
            }

            if (dr["custom_value_type_display"] != DBNull.Value)
            {
                tasktemplateproperty.PropertyValueDisplay = dr["custom_value_type_display"].ToString();
            }

            if (dr["custom_title_display"] != DBNull.Value)
            {
                tasktemplateproperty.TitleDisplay = dr["custom_title_display"].ToString();
            }

            if (dr["custom_display_type"] != DBNull.Value)
            {
                tasktemplateproperty.CustomDisplayType = dr["custom_display_type"].ToString();
            }

            if (dr["display_type"] != DBNull.Value)
            {
                tasktemplateproperty.DisplayType = (PropertyDisplayTypeEnum)Convert.ToInt32(dr["display_type"]);
            }

            if (dr["is_required"] != DBNull.Value)
            {
                tasktemplateproperty.IsRequired = Convert.ToBoolean(dr["is_required"]);
            }

            if (dr["index"] != DBNull.Value)
            {
                tasktemplateproperty.Index = Convert.ToInt32(dr["index"]);
            }

            if (dr.HasColumn("is_active") && dr["is_active"] != DBNull.Value)
            {
                tasktemplateproperty.IsActive = Convert.ToBoolean(dr["is_active"]);
            }

            if (dr.HasColumn("value_type") && dr["value_type"] != DBNull.Value)
            {
                tasktemplateproperty.ValueType = (PropertyValueTypeEnum)Convert.ToInt32(dr["value_type"]);
            }

            if (dr.HasColumn("field_type") && dr["field_type"] != DBNull.Value)
            {
                tasktemplateproperty.FieldType = (PropertyFieldTypeEnum)Convert.ToInt32(dr["field_type"]);
            }

            return tasktemplateproperty;
        }

        /// <summary>
        /// CreateOrFillPropertyUserValueItemFromReader; Creates a list of NpgsqlParameters, and fills it based on the supplied property user value item object.
        /// NOTE! intended for use with the property user value stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="propertyValueItem">PropertyUserValue item, if not supplied it will be created.</param>
        /// <returns>Fileld PropertyUserValue;</returns>
        private PropertyUserValue CreateOrFillPropertyUserValueItemFromReader(NpgsqlDataReader dr, PropertyUserValue propertyValueItem = null)
        {
            if (propertyValueItem == null) propertyValueItem = new PropertyUserValue();

            propertyValueItem.Id = Convert.ToInt32(dr["id"]);
            propertyValueItem.TemplatePropertyId = Convert.ToInt32(dr["template_property_id"]);
            propertyValueItem.PropertyId = Convert.ToInt32(dr["property_id"]);
            if (dr.HasColumn("propertygroup_id"))
            {
                propertyValueItem.PropertyGroupId = Convert.ToInt32(dr["propertygroup_id"]);
            }

            if (dr.HasColumn("task_id"))
            {
                propertyValueItem.TaskId = Convert.ToInt32(dr["task_id"]);
            }
            if (dr.HasColumn("checklist_id"))
            {
                propertyValueItem.ChecklistId = Convert.ToInt32(dr["checklist_id"]);
            }
            if (dr.HasColumn("audit_id"))
            {
                propertyValueItem.AuditId = Convert.ToInt32(dr["audit_id"]);
            }

            propertyValueItem.CompanyId = Convert.ToInt32(dr["company_id"]);
            propertyValueItem.UserId = Convert.ToInt32(dr["user_id"]);

            if (dr["value_bool"] != DBNull.Value)
            {
                propertyValueItem.UserValueBool = Convert.ToBoolean(dr["value_bool"]);
            }

            if (dr["value_int"] != DBNull.Value)
            {
                propertyValueItem.UserValueInt = Convert.ToInt32(dr["value_int"]);
            }

            if (dr["value_string"] != DBNull.Value)
            {
                propertyValueItem.UserValueString = dr["value_string"].ToString();
            }

            if (dr["value_decimal"] != DBNull.Value)
            {
                propertyValueItem.UserValueDecimal = Convert.ToDecimal(dr["value_decimal"]);
            }

            if (dr["value_time"] != DBNull.Value)
            {
                propertyValueItem.UserValueTime = dr["value_time"].ToString(); //TODO check
            }

            if (dr["value_date"] != DBNull.Value)
            {
                propertyValueItem.UserValueDate = Convert.ToDateTime(dr["value_date"]); //TODO check
            }

            if (dr["created_at"] != DBNull.Value)
            {
                propertyValueItem.CreatedAt = Convert.ToDateTime(dr["created_at"]); //TODO check
            }

            if (dr["modified_at"] != DBNull.Value)
            {
                propertyValueItem.ModifiedAt = Convert.ToDateTime(dr["modified_at"]); //TODO check
            }

            if (dr.HasColumn("registered_at") && dr["registered_at"] != DBNull.Value)
            {
                propertyValueItem.RegisteredAt = Convert.ToDateTime(dr["registered_at"]);
            }


            return propertyValueItem;
        }

        /// <summary>
        /// GetNpgsqlParametersFromPropertyUserValueItem; Creates a list of NpgsqlParameters, and fills it based on the supplied PropertyUserValue object.
        /// NOTE! intended for use with the property user value stored procedures within the database.
        /// </summary>
        /// <param name="propertyUserValue">PropertyUserValue object where parameters need to be generated with.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyUserValueId">Id of the object that needs to be generated.</param>
        /// <returns>List of NpgsqlParameter items.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromPropertyUserValueItem(PropertyUserValue propertyUserValue, int companyId, int propertyUserValueId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyUserValueId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyUserValueId));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if (propertyUserValue.TaskId.HasValue && propertyUserValue.TaskId.Value > 0)
            {
                parameters.Add(new NpgsqlParameter("@_taskid", propertyUserValue.TaskId.Value));
            }

            if (propertyUserValue.ChecklistId.HasValue && propertyUserValue.ChecklistId.Value > 0)
            {
                parameters.Add(new NpgsqlParameter("@_checklistid", propertyUserValue.ChecklistId.Value));
            }

            if (propertyUserValue.AuditId.HasValue && propertyUserValue.AuditId.Value > 0)
            {
                parameters.Add(new NpgsqlParameter("@_auditid", propertyUserValue.AuditId.Value));
            }

            parameters.Add(new NpgsqlParameter("@_propertyid", propertyUserValue.PropertyId));
            parameters.Add(new NpgsqlParameter("@_templatepropertyid", propertyUserValue.TemplatePropertyId));
            parameters.Add(new NpgsqlParameter("@_userid", propertyUserValue.UserId));

            if (propertyUserValue.UserValueInt.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_value_int", propertyUserValue.UserValueInt.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value_int", DBNull.Value));
            }

            if (propertyUserValue.UserValueDecimal.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_value_decimal", propertyUserValue.UserValueDecimal));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value_decimal", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyUserValue.UserValueString))
            {
                parameters.Add(new NpgsqlParameter("@_value_string", propertyUserValue.UserValueString));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value_string", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyUserValue.UserValueTime))
            {
                var timevalue = new NpgsqlParameter("@_value_time", Convert.ToDateTime(propertyUserValue.UserValueTime).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value_time", DBNull.Value));
            }

            if (propertyUserValue.UserValueDate.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_value_date", new DateTime(propertyUserValue.UserValueDate.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value_date", DBNull.Value));
            }

            if (propertyUserValue.UserValueBool.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_value_bool", propertyUserValue.UserValueBool.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value_bool", DBNull.Value));
            }

            if (propertyUserValue.RegisteredAt.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_registered_at", new DateTime(propertyUserValue.RegisteredAt.Value.Ticks)));
            }

            return parameters;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromPropertyDTOItem(PropertyDTO property, int companyId, int userId, int? propertyUserValueId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyUserValueId != null && propertyUserValueId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_id", propertyUserValueId));
            }
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            if (property.UserValue.TaskId != null && property.UserValue.TaskId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_taskid", property.UserValue.TaskId));
            }
            if (property.UserValue.ChecklistId != null && property.UserValue.ChecklistId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_checklistid", property.UserValue.ChecklistId));
            }
            if (property.UserValue.AuditId != null && property.UserValue.AuditId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_auditid", property.UserValue.AuditId));
            }

            parameters.Add(new NpgsqlParameter("@_propertyid", property.PropertyTemplate.PropertyId));
            parameters.Add(new NpgsqlParameter("@_templatepropertyid", property.PropertyTemplate.Id));

            parameters.Add(new NpgsqlParameter("@_value_int", DBNull.Value));
            parameters.Add(new NpgsqlParameter("@_value_decimal", DBNull.Value));
            parameters.Add(new NpgsqlParameter("@_value_string", DBNull.Value));
            parameters.Add(new NpgsqlParameter("@_value_time", DBNull.Value));
            parameters.Add(new NpgsqlParameter("@_value_date", DBNull.Value));
            parameters.Add(new NpgsqlParameter("@_value_bool", DBNull.Value));


            switch (property.PropertyTemplate.ValueType)
            {
                case PropertyValueTypeEnum.Integer:
                    parameters.RemoveAll(p => p.ParameterName == "@_value_int");                    
                    parameters.Add(new NpgsqlParameter("@_value_int", Convert.ToInt32(property.UserValue.UserValue)));
                    break;
                case PropertyValueTypeEnum.Decimal:
                    parameters.RemoveAll(p => p.ParameterName == "@_value_decimal");
                    parameters.Add(new NpgsqlParameter("@_value_decimal", Convert.ToDecimal(property.UserValue.UserValue)));
                    break;
                case PropertyValueTypeEnum.String:
                    parameters.RemoveAll(p => p.ParameterName == "@_value_string");
                    parameters.Add(new NpgsqlParameter("@_value_string", property.UserValue.UserValue));
                    break;
                case PropertyValueTypeEnum.Date:
                case PropertyValueTypeEnum.DateTime:
                    parameters.RemoveAll(p => p.ParameterName == "@_value_date");
                    parameters.Add(new NpgsqlParameter("@_value_date", Convert.ToDateTime(property.UserValue.UserValue)));
                    break;
                case PropertyValueTypeEnum.Time:
                    parameters.RemoveAll(p => p.ParameterName == "@_value_time");
                    var timevalue = new NpgsqlParameter("@_value_time", Convert.ToDateTime(property.UserValue.UserValue).TimeOfDay);
                    timevalue.DbType = System.Data.DbType.Time;
                    parameters.Add(timevalue);
                    break;
                case PropertyValueTypeEnum.Boolean:
                    parameters.RemoveAll(p => p.ParameterName == "@_value_bool");
                    parameters.Add(new NpgsqlParameter("@_value_bool", Convert.ToBoolean(property.UserValue.UserValue)));
                    break;
            }

            if (property.UserValue.RegisteredAt.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_registered_at", new DateTime(property.UserValue.RegisteredAt.Value.Ticks)));
            }

            return parameters;
        }

        /// <summary>
        /// GetNpgsqlParametersFromTaskTemplateProperty; Creates a list of NpgsqlParameters, and fills it based on the supplied PropertyTaskTemplate object.
        /// NOTE! intended for use with the property task template stored procedures within the database.
        /// </summary>
        /// <param name="propertyTaskTemplate">PropertyTaskTemplate object where parameters need to be generated with.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyTemplateId">Id of PropertyTaskTemplate item.</param>
        /// <returns>List of NpgsqlParameter items</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromTaskTemplateProperty(PropertyTaskTemplate propertyTaskTemplate, int companyId, int propertyTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyTemplateId));

            parameters.Add(new NpgsqlParameter("@_propertyid", propertyTaskTemplate.PropertyId));
            parameters.Add(new NpgsqlParameter("@_taskttemplateid", propertyTaskTemplate.TaskTemplateId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if (propertyTaskTemplate.PropertyValueId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_propertyvalueid", propertyTaskTemplate.PropertyValueId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_propertyvalueid", DBNull.Value));
            }

            if (propertyTaskTemplate.PrimaryIntValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_int_value", propertyTaskTemplate.PrimaryIntValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_int_value", DBNull.Value));
            }

            if (propertyTaskTemplate.SecondaryIntValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_int_value", propertyTaskTemplate.SecondaryIntValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_int_value", DBNull.Value));
            }

            if (propertyTaskTemplate.PrimaryDecimalValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_decimal_value", propertyTaskTemplate.PrimaryDecimalValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_decimal_value", DBNull.Value));
            }

            if (propertyTaskTemplate.SecondaryDecimalValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_decimal_value", propertyTaskTemplate.SecondaryDecimalValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_decimal_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.PrimaryStringValue))
            {
                parameters.Add(new NpgsqlParameter("@_primary_string_value", propertyTaskTemplate.PrimaryStringValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_string_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.SecondaryStringValue))
            {
                parameters.Add(new NpgsqlParameter("@_secondary_string_value", propertyTaskTemplate.SecondaryStringValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_string_value", DBNull.Value));
            }

            if (propertyTaskTemplate.PrimaryDateTimeValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_datetime_value", new DateTime(propertyTaskTemplate.PrimaryDateTimeValue.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_datetime_value", DBNull.Value));
            }

            if (propertyTaskTemplate.SecondaryDateTimeValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_datetime_value", new DateTime(propertyTaskTemplate.SecondaryDateTimeValue.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_datetime_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.PrimaryTimeValue))
            {
                var timevalue = new NpgsqlParameter("@_primary_time_value", Convert.ToDateTime(propertyTaskTemplate.PrimaryTimeValue).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_time_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.SecondaryTimeValue))
            {
                var timevalue = new NpgsqlParameter("@_secondary_time_value", Convert.ToDateTime(propertyTaskTemplate.SecondaryTimeValue).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_time_value", DBNull.Value));
            }

            if (propertyTaskTemplate.BoolValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_bool_value", propertyTaskTemplate.BoolValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_bool_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.PropertyValueDisplay))
            {
                parameters.Add(new NpgsqlParameter("@_custom_value_type_display", propertyTaskTemplate.PropertyValueDisplay));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_value_type_display", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.TitleDisplay))
            {
                parameters.Add(new NpgsqlParameter("@_custom_title_display", propertyTaskTemplate.TitleDisplay));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_title_display", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyTaskTemplate.CustomDisplayType))
            {
                parameters.Add(new NpgsqlParameter("@_custom_display_type", propertyTaskTemplate.CustomDisplayType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_display_type", DBNull.Value));
            }

            if (propertyTaskTemplate.DisplayType.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_display_type", propertyTaskTemplate.DisplayType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_display_type", DBNull.Value));
            }

            if (propertyTaskTemplate.IsRequired.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_is_required", propertyTaskTemplate.IsRequired));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_is_required", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_index", propertyTaskTemplate.Index));


            if(_configurationHelper.GetValueAsInteger("AppSettings:PropertyStructureVersion") == 2)
            {
                if (propertyTaskTemplate.ValueType.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_value_type", Convert.ToInt32(propertyTaskTemplate.ValueType)));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_value_type", DBNull.Value));
                }

                if (propertyTaskTemplate.FieldType.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_field_type", Convert.ToInt32(propertyTaskTemplate.FieldType)));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_field_type", DBNull.Value));
                }
            }

            return parameters;
        }

        /// <summary>
        /// GetPropertiesBasedOnTemplate; Get properties based on template. Which template type is based on the StoredProcedure that will be executed.
        /// </summary>
        /// <param name="sp">Stored procedure that will be executed.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of properties.</returns>
        private async Task<List<Property>> GetPropertiesBasedOnTemplate(string sp, int companyId, List<int> ids = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<Property>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (ids != null)
                {
                    if (sp == "get_properties_with_checklisttemplates")
                    { 
                        parameters.Add(new NpgsqlParameter("_checklisttemplateids", ids)); 
                    }
                    else if (sp == "get_properties_with_audittemplates")
                    {
                        parameters.Add(new NpgsqlParameter("_audittemplateids", ids));
                    }
                    else if (sp == "get_properties_with_tasktemplates")
                    {
                        parameters.Add(new NpgsqlParameter("_tasktemplateids", ids));
                    }
                }

                using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var property = CreateOrFillPropertyFromReader(dr);
                        output.Add(property);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("PropertyValueManager.GetPropertiesBasedOnTemplate(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower())) output = await AppendPropertyValuesToProperties(companyId: companyId, properties: output, connectionKind: connectionKind);


            return output;
        }



        #endregion

        #region - property/property value related checklists -
        /// <summary>
        /// CreateOrFillChecklistTemplatePropertyFromReader; creates and fills a checklist template property object from a DataReader.
        /// NOTE! intended for use with the checklist template property stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="checklisttemplateproperty">PropertyChecklistTemplate object, if not supplied it will be created.</param>
        /// <returns>PropertyChecklistTemplate filled with data from the data reader.</returns>
        private PropertyChecklistTemplate CreateOrFillChecklistTemplatePropertyFromReader(NpgsqlDataReader dr, PropertyChecklistTemplate checklisttemplateproperty = null)
        {
            if (checklisttemplateproperty == null) checklisttemplateproperty = new PropertyChecklistTemplate();

            checklisttemplateproperty.Id = Convert.ToInt32(dr["id"]);

            checklisttemplateproperty.ChecklistTemplateId = Convert.ToInt32(dr["checklisttemplate_id"]);
            checklisttemplateproperty.PropertyId = Convert.ToInt32(dr["property_id"]);

            if (dr["propertyvalue_id"] != DBNull.Value)
            {
                checklisttemplateproperty.PropertyValueId = Convert.ToInt32(dr["propertyvalue_id"]);
            }

            if (dr.HasColumn("propertygroup_id") && dr["propertygroup_id"] != DBNull.Value)
            {
                checklisttemplateproperty.PropertyGroupId = Convert.ToInt32(dr["propertygroup_id"]);
            }

            if (dr["primary_int_value"] != DBNull.Value)
            {
                checklisttemplateproperty.PrimaryIntValue = Convert.ToInt32(dr["primary_int_value"]);
            }

            if (dr["primary_decimal_value"] != DBNull.Value)
            {

                checklisttemplateproperty.PrimaryDecimalValue = Convert.ToDecimal(dr["primary_decimal_value"]);
            }

            if (dr["primary_string_value"] != DBNull.Value)
            {
                checklisttemplateproperty.PrimaryStringValue = dr["primary_string_value"].ToString();
            }

            if (dr["primary_datetime_value"] != DBNull.Value)
            {
                checklisttemplateproperty.PrimaryDateTimeValue = Convert.ToDateTime(dr["primary_datetime_value"]);
            }

            if (dr["primary_time_value"] != DBNull.Value)
            {
                checklisttemplateproperty.PrimaryTimeValue = dr["primary_time_value"].ToString();
            }

            if (dr["secondary_int_value"] != DBNull.Value)
            {
                checklisttemplateproperty.SecondaryIntValue = Convert.ToInt32(dr["secondary_int_value"]);
            }

            if (dr["secondary_decimal_value"] != DBNull.Value)
            {
                checklisttemplateproperty.SecondaryDecimalValue = Convert.ToDecimal(dr["secondary_decimal_value"]);
            }

            if (dr["secondary_string_value"] != DBNull.Value)
            {
                checklisttemplateproperty.SecondaryStringValue = dr["secondary_string_value"].ToString();
            }

            if (dr["secondary_datetime_value"] != DBNull.Value)
            {
                checklisttemplateproperty.SecondaryDateTimeValue = Convert.ToDateTime(dr["secondary_datetime_value"]);
            }

            if (dr["secondary_time_value"] != DBNull.Value)
            {
                checklisttemplateproperty.SecondaryTimeValue = dr["secondary_time_value"].ToString();
            }

            if (dr["bool_value"] != DBNull.Value)
            {
                checklisttemplateproperty.BoolValue = Convert.ToBoolean(dr["bool_value"]);
            }

            if (dr["custom_value_type_display"] != DBNull.Value)
            {
                checklisttemplateproperty.PropertyValueDisplay = dr["custom_value_type_display"].ToString();
            }

            if (dr["custom_title_display"] != DBNull.Value)
            {
                checklisttemplateproperty.TitleDisplay = dr["custom_title_display"].ToString();
            }

            if (dr.HasColumn("custom_display_type") && dr["custom_display_type"] != DBNull.Value)
            {
                checklisttemplateproperty.CustomDisplayType = dr["custom_display_type"].ToString();
            }

            if (dr.HasColumn("display_type") && dr["display_type"] != DBNull.Value)
            {
                checklisttemplateproperty.DisplayType = (PropertyDisplayTypeEnum)Convert.ToInt32(dr["display_type"]);
            }

            if (dr["is_required"] != DBNull.Value)
            {
                checklisttemplateproperty.IsRequired = Convert.ToBoolean(dr["is_required"]);
            }

            if (dr["index"] != DBNull.Value)
            {
                checklisttemplateproperty.Index = Convert.ToInt32(dr["index"]);
            }

            if (dr.HasColumn("value_type") && dr["value_type"] != DBNull.Value)
            {
                checklisttemplateproperty.ValueType = (PropertyValueTypeEnum)Convert.ToInt32(dr["value_type"]);
            }

            if (dr.HasColumn("field_type") && dr["field_type"] != DBNull.Value)
            {
                checklisttemplateproperty.FieldType = (PropertyFieldTypeEnum)Convert.ToInt32(dr["field_type"]);
            }

            return checklisttemplateproperty;
        }

        /// <summary>
        /// GetNpgsqlParametersFromChecklistTemplateProperty; Creates a list of NpgsqlParameters, and fills it based on the supplied PropertyChecklistTemplate object.
        /// NOTE! intended for use with the property checklist template stored procedures within the database.
        /// </summary>
        /// <param name="propertyChecklistTemplate">PropertyChecklistTemplate object containing the data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyTemplateId">PropertyTemplateId (DB: checklists_checklisttemplate_properties.id)</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromChecklistTemplateProperty(PropertyChecklistTemplate propertyChecklistTemplate, int companyId, int propertyTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyTemplateId));

            parameters.Add(new NpgsqlParameter("@_propertyid", propertyChecklistTemplate.PropertyId));
            parameters.Add(new NpgsqlParameter("@_templateid", propertyChecklistTemplate.ChecklistTemplateId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if (propertyChecklistTemplate.PropertyValueId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_propertyvalueid", propertyChecklistTemplate.PropertyValueId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_propertyvalueid", DBNull.Value));
            }

            if (propertyChecklistTemplate.PrimaryIntValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_int_value", propertyChecklistTemplate.PrimaryIntValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_int_value", DBNull.Value));
            }

            if (propertyChecklistTemplate.SecondaryIntValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_int_value", propertyChecklistTemplate.SecondaryIntValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_int_value", DBNull.Value));
            }

            if (propertyChecklistTemplate.PrimaryDecimalValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_decimal_value", propertyChecklistTemplate.PrimaryDecimalValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_decimal_value", DBNull.Value));
            }

            if (propertyChecklistTemplate.SecondaryDecimalValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_decimal_value", propertyChecklistTemplate.SecondaryDecimalValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_decimal_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.PrimaryStringValue))
            {
                parameters.Add(new NpgsqlParameter("@_primary_string_value", propertyChecklistTemplate.PrimaryStringValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_string_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.SecondaryStringValue))
            {
                parameters.Add(new NpgsqlParameter("@_secondary_string_value", propertyChecklistTemplate.SecondaryStringValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_string_value", DBNull.Value));
            }

            if (propertyChecklistTemplate.PrimaryDateTimeValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_datetime_value", new DateTime(propertyChecklistTemplate.PrimaryDateTimeValue.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_datetime_value", DBNull.Value));
            }

            if (propertyChecklistTemplate.SecondaryDateTimeValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_datetime_value", new DateTime(propertyChecklistTemplate.SecondaryDateTimeValue.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_datetime_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.PrimaryTimeValue))
            {
                var timevalue = new NpgsqlParameter("@_primary_time_value", Convert.ToDateTime(propertyChecklistTemplate.PrimaryTimeValue).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_time_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.SecondaryTimeValue))
            {
                var timevalue = new NpgsqlParameter("@_secondary_time_value", Convert.ToDateTime(propertyChecklistTemplate.SecondaryTimeValue).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_time_value", DBNull.Value));
            }

            if (propertyChecklistTemplate.BoolValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_bool_value", propertyChecklistTemplate.BoolValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_bool_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.PropertyValueDisplay))
            {
                parameters.Add(new NpgsqlParameter("@_custom_value_type_display", propertyChecklistTemplate.PropertyValueDisplay));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_value_type_display", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.TitleDisplay))
            {
                parameters.Add(new NpgsqlParameter("@_custom_title_display", propertyChecklistTemplate.TitleDisplay));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_title_display", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyChecklistTemplate.CustomDisplayType))
            {
                parameters.Add(new NpgsqlParameter("@_custom_display_type", propertyChecklistTemplate.CustomDisplayType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_display_type", DBNull.Value));
            }

            if (propertyChecklistTemplate.DisplayType.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_display_type", propertyChecklistTemplate.DisplayType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_display_type", DBNull.Value));
            }

            if (propertyChecklistTemplate.IsRequired.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_is_required", propertyChecklistTemplate.IsRequired));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_is_required", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_index", propertyChecklistTemplate.Index));

            if (_configurationHelper.GetValueAsInteger("AppSettings:PropertyStructureVersion") == 2)
            {
                if (propertyChecklistTemplate.ValueType.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_value_type", Convert.ToInt32(propertyChecklistTemplate.ValueType)));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_value_type", DBNull.Value));
                }

                if (propertyChecklistTemplate.FieldType.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_field_type", Convert.ToInt32(propertyChecklistTemplate.FieldType)));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_field_type", DBNull.Value));
                }
            }

            return parameters;
        }


        #endregion

        #region - property/property value related audits -
        /// <summary>
        /// CreateOrFillAuditTemplatePropertyFromReader; creates and fills a audit template property object from a DataReader.
        /// NOTE! intended for use with the audit template property stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audittemplateproperty">PropertyAuditTemplate object, if not supplied it will be created.</param>
        /// <returns>PropertyAuditTemplate filled with data from the data reader.</returns>
        private PropertyAuditTemplate CreateOrFillAuditTemplatePropertyFromReader(NpgsqlDataReader dr, PropertyAuditTemplate audittemplateproperty = null)
        {
            if (audittemplateproperty == null) audittemplateproperty = new PropertyAuditTemplate();

            audittemplateproperty.Id = Convert.ToInt32(dr["id"]);

            audittemplateproperty.AuditTemplateId = Convert.ToInt32(dr["audittemplate_id"]);
            audittemplateproperty.PropertyId = Convert.ToInt32(dr["property_id"]);

            if (dr["propertyvalue_id"] != DBNull.Value)
            {
                audittemplateproperty.PropertyValueId = Convert.ToInt32(dr["propertyvalue_id"]);
            }

            if (dr.HasColumn("propertygroup_id") && dr["propertygroup_id"] != DBNull.Value)
            {
                audittemplateproperty.PropertyGroupId = Convert.ToInt32(dr["propertygroup_id"]);
            }

            if (dr["primary_int_value"] != DBNull.Value)
            {
                audittemplateproperty.PrimaryIntValue = Convert.ToInt32(dr["primary_int_value"]);
            }

            if (dr["primary_decimal_value"] != DBNull.Value)
            {

                audittemplateproperty.PrimaryDecimalValue = Convert.ToDecimal(dr["primary_decimal_value"]);
            }

            if (dr["primary_string_value"] != DBNull.Value)
            {
                audittemplateproperty.PrimaryStringValue = dr["primary_string_value"].ToString();
            }

            if (dr["primary_datetime_value"] != DBNull.Value)
            {
                audittemplateproperty.PrimaryDateTimeValue = Convert.ToDateTime(dr["primary_datetime_value"]);
            }

            if (dr["primary_time_value"] != DBNull.Value)
            {
                audittemplateproperty.PrimaryTimeValue = dr["primary_time_value"].ToString();
            }

            if (dr["secondary_int_value"] != DBNull.Value)
            {
                audittemplateproperty.SecondaryIntValue = Convert.ToInt32(dr["secondary_int_value"]);
            }

            if (dr["secondary_decimal_value"] != DBNull.Value)
            {
                audittemplateproperty.SecondaryDecimalValue = Convert.ToDecimal(dr["secondary_decimal_value"]);
            }

            if (dr["secondary_string_value"] != DBNull.Value)
            {
                audittemplateproperty.SecondaryStringValue = dr["secondary_string_value"].ToString();
            }

            if (dr["secondary_datetime_value"] != DBNull.Value)
            {
                audittemplateproperty.SecondaryDateTimeValue = Convert.ToDateTime(dr["secondary_datetime_value"]);
            }

            if (dr["secondary_time_value"] != DBNull.Value)
            {
                audittemplateproperty.SecondaryTimeValue = dr["secondary_time_value"].ToString();
            }

            if (dr["bool_value"] != DBNull.Value)
            {
                audittemplateproperty.BoolValue = Convert.ToBoolean(dr["bool_value"]);
            }

            if (dr["custom_value_type_display"] != DBNull.Value)
            {
                audittemplateproperty.PropertyValueDisplay = dr["custom_value_type_display"].ToString();
            }

            if (dr["custom_title_display"] != DBNull.Value)
            {
                audittemplateproperty.TitleDisplay = dr["custom_title_display"].ToString();
            }

            if (dr.HasColumn("custom_display_type") && dr["custom_display_type"] != DBNull.Value)
            {
                audittemplateproperty.CustomDisplayType = dr["custom_display_type"].ToString();
            }

            if (dr.HasColumn("display_type") && dr["display_type"] != DBNull.Value)
            {
                audittemplateproperty.DisplayType = (PropertyDisplayTypeEnum)Convert.ToInt32(dr["display_type"]);
            }

            if (dr["is_required"] != DBNull.Value)
            {
                audittemplateproperty.IsRequired = Convert.ToBoolean(dr["is_required"]);
            }

            if (dr["index"] != DBNull.Value)
            {
                audittemplateproperty.Index = Convert.ToInt32(dr["index"]);
            }

            if (dr.HasColumn("value_type") && dr["value_type"] != DBNull.Value)
            {
                audittemplateproperty.ValueType = (PropertyValueTypeEnum)Convert.ToInt32(dr["value_type"]);
            }

            if (dr.HasColumn("field_type") && dr["field_type"] != DBNull.Value)
            {
                audittemplateproperty.FieldType = (PropertyFieldTypeEnum)Convert.ToInt32(dr["field_type"]);
            }

            return audittemplateproperty;
        }

        /// <summary>
        /// GetNpgsqlParametersFromAuditTemplateProperty; Creates a list of NpgsqlParameters, and fills it based on the supplied PropertyAuditTemplate object.
        /// NOTE! intended for use with the property audit template stored procedures within the database.
        /// </summary>
        /// <param name="propertyAuditTemplate">PropertyAuditTemplate object containing the data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="propertyTemplateId">PropertyTemplateId (DB: audits_audittemplate_properties.id)</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromAuditTemplateProperty(PropertyAuditTemplate propertyAuditTemplate, int companyId, int propertyTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (propertyTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", propertyTemplateId));

            parameters.Add(new NpgsqlParameter("@_propertyid", propertyAuditTemplate.PropertyId));
            parameters.Add(new NpgsqlParameter("@_templateid", propertyAuditTemplate.AuditTemplateId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if (propertyAuditTemplate.PropertyValueId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_propertyvalueid", propertyAuditTemplate.PropertyValueId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_propertyvalueid", DBNull.Value));
            }

            if (propertyAuditTemplate.PrimaryIntValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_int_value", propertyAuditTemplate.PrimaryIntValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_int_value", DBNull.Value));
            }

            if (propertyAuditTemplate.SecondaryIntValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_int_value", propertyAuditTemplate.SecondaryIntValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_int_value", DBNull.Value));
            }

            if (propertyAuditTemplate.PrimaryDecimalValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_decimal_value", propertyAuditTemplate.PrimaryDecimalValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_decimal_value", DBNull.Value));
            }

            if (propertyAuditTemplate.SecondaryDecimalValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_decimal_value", propertyAuditTemplate.SecondaryDecimalValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_decimal_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.PrimaryStringValue))
            {
                parameters.Add(new NpgsqlParameter("@_primary_string_value", propertyAuditTemplate.PrimaryStringValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_string_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.SecondaryStringValue))
            {
                parameters.Add(new NpgsqlParameter("@_secondary_string_value", propertyAuditTemplate.SecondaryStringValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_string_value", DBNull.Value));
            }

            if (propertyAuditTemplate.PrimaryDateTimeValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_primary_datetime_value", new DateTime(propertyAuditTemplate.PrimaryDateTimeValue.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_datetime_value", DBNull.Value));
            }

            if (propertyAuditTemplate.SecondaryDateTimeValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_secondary_datetime_value", new DateTime(propertyAuditTemplate.SecondaryDateTimeValue.Value.Ticks)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_datetime_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.PrimaryTimeValue))
            {
                var timevalue = new NpgsqlParameter("@_primary_time_value", Convert.ToDateTime(propertyAuditTemplate.PrimaryTimeValue).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_primary_time_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.SecondaryTimeValue))
            {
                var timevalue = new NpgsqlParameter("@_secondary_time_value", Convert.ToDateTime(propertyAuditTemplate.SecondaryTimeValue).TimeOfDay);
                timevalue.DbType = System.Data.DbType.Time;
                parameters.Add(timevalue);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_secondary_time_value", DBNull.Value));
            }

            if (propertyAuditTemplate.BoolValue.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_bool_value", propertyAuditTemplate.BoolValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_bool_value", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.PropertyValueDisplay))
            {
                parameters.Add(new NpgsqlParameter("@_custom_value_type_display", propertyAuditTemplate.PropertyValueDisplay));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_value_type_display", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.TitleDisplay))
            {
                parameters.Add(new NpgsqlParameter("@_custom_title_display", propertyAuditTemplate.TitleDisplay));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_title_display", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(propertyAuditTemplate.CustomDisplayType))
            {
                parameters.Add(new NpgsqlParameter("@_custom_display_type", propertyAuditTemplate.CustomDisplayType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_custom_display_type", DBNull.Value));
            }

            if (propertyAuditTemplate.DisplayType.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_display_type", propertyAuditTemplate.DisplayType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_display_type", DBNull.Value));
            }

            if (propertyAuditTemplate.IsRequired.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_is_required", propertyAuditTemplate.IsRequired));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_is_required", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_index", propertyAuditTemplate.Index));

            if (_configurationHelper.GetValueAsInteger("AppSettings:PropertyStructureVersion") == 2)
            {
                if (propertyAuditTemplate.ValueType.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_value_type", Convert.ToInt32(propertyAuditTemplate.ValueType)));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_value_type", DBNull.Value));
                }

                if (propertyAuditTemplate.FieldType.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_field_type", Convert.ToInt32(propertyAuditTemplate.FieldType)));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_field_type", DBNull.Value));
                }
            }

            return parameters;
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
