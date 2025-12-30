using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IPropertyValueManager
    {
        Task<List<Property>> GetPropertiesAsync(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null);
        Task<List<PropertyGroup>> GetPropertyGroupsAsync(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null);
        Task<Property> GetPropertyAsync(int companyId, int propertyId, int? userId = null, string include = null);
        Task<List<PropertyAuditTemplate>> GetPropertiesAuditTemplatesAsync(int companyId, List<int> auditTemplateIds = null, int? userId = null, PropertyFilters? filters = null, string include = null);
        Task<List<PropertyAuditTemplate>> GetPropertiesAuditTemplateAsync(int companyId, int auditTemplateId, int? userId = null, PropertyFilters? filters = null, string include = null);
        Task<List<PropertyAction>> GetPropertiesActionsAsync(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null);
        Task<List<PropertyChecklistTemplate>> GetPropertiesChecklistTemplatesAsync(int companyId, List<int> checklistTemplateIds = null, int ? userId = null, PropertyFilters? filters = null, string include = null);
        Task<List<PropertyChecklistTemplate>> GetPropertiesChecklistTemplateAsync(int companyId, int checklistTemplateId, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyTaskTemplate>> GetPropertiesTaskTemplatesAsync(int companyId, int? userId = null, List<int> taskTemplateIds = null, PropertyFilters? filters = null, string include = null, bool? isActive = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyTaskTemplate>> GetPropertiesTaskTemplateAsync(int companyId, int taskTemplateId, int? userId = null, PropertyFilters? filters = null, string include = null, bool? isActive = null);
        Task<List<PropertyUserValue>> GetPropertyUserValuesWithTasks(int companyId, int? userId = null, List<long> taskIds = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyUserValue>> GetPropertyUserValuesByTaskId(int companyId, int taskId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyUserValue>> GetPropertyUserValuesWithActions(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null);
        Task<List<PropertyUserValue>> GetPropertyUserValuesWithChecklists(int companyId, List<int> checklistIds = null, int ? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyUserValue>> GetPropertyUserValuesByChecklistId(int companyId, int checklistId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyUserValue>> GetPropertyUserValuesWithAudits(int companyId, List<int> auditIds = null, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> AddPropertyAsync(int companyId, Property property, int userId);
        Task<int> AddPropertyValueAsync(int companyId, PropertyValue propertyValue, int userId);
        Task<int> AddPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int userId);
        Task<int> AddPropertyUserValueAsync(int companyId, PropertyDTO property, int userId);
        Task<int> AddChecklistPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int userId);
        Task<int> AddChecklistPropertyUserValueAsync(int companyId, PropertyDTO property, int userId);
        Task<int> AddAuditPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int userId);
        Task<int> AddAuditPropertyUserValueAsync(int companyId, PropertyDTO property, int userId);
        Task<int> AddChangeAuditPropertyUserValuesAsync(int companyId, List<PropertyUserValue> propertyValues, int userId);
        Task<int> AddChangeChecklistPropertyUserValuesAsync(int companyId, List<PropertyUserValue> propertyValues, int userId);

        Task<int> ChangePropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int propertyUserValueId, int userId);
        Task<int> ChangePropertyUserValueAsync(int companyId, PropertyDTO property, int propertyUserValueId, int userId);
        Task<int> ChangeChecklistPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int propertyUserValueId, int userId);
        Task<int> ChangeAuditPropertyUserValueAsync(int companyId, PropertyUserValue propertyValue, int propertyUserValueId, int userId);

        Task<bool> ChangePropertyAsync(int companyId, int propertyId, Property property, int userId);
        Task<bool> ChangePropertyValueAsync(int companyId, int propertyValueId, PropertyUserValue propertyValue, int userId);
        Task<bool> SetPropertyActiveAsync(int companyId, int userId, int actionId, bool isActive = true);
        Task<List<PropertyValue>> GetPropertyValues(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyValueKind>> GetPropertyValueKinds(int companyId, int? userId = null, PropertyFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyTaskTemplate>> GetTaskTemplateProperties(int companyId, List<int> taskTemplateIds = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<PropertyTaskTemplate>> GetTaskTemplateProperties(int companyId, int taskTemplateId);
        Task<int> AddTaskTemplatePropertyAsync(int companyId, int userId, PropertyTaskTemplate templateProperty);
        Task<int> ChangeTaskTemplatePropertyAsync(int companyId, int userId, int taskTemplatePropertyId, PropertyTaskTemplate templateProperty);
        Task<int> RemoveTaskTemplatePropertyAsync(int companyId, int userId, int taskTemplatePropertyId);

        #region - add/change audit template properties -
        Task<int> AddAuditTemplatePropertyAsync(int companyId, int userId, PropertyAuditTemplate templateProperty);
        Task<int> ChangeAuditTemplatePropertyAsync(int companyId, int userId, int auditTemplatePropertyId, PropertyAuditTemplate templateProperty);
        Task<int> RemoveAuditTemplatePropertyAsync(int companyId, int userId, int auditTemplatePropertyId);
        #endregion

        #region - add/change checklist template properties -
        Task<int> AddChecklistTemplatePropertyAsync(int companyId, int userId, PropertyChecklistTemplate templateProperty);
        Task<int> ChangeChecklistTemplatePropertyAsync(int companyId, int userId, int checklistTemplatePropertyId, PropertyChecklistTemplate templateProperty);
        Task<int> RemoveChecklistTemplatePropertyAsync(int companyId, int userId, int checklistTemplatePropertyId);
        #endregion
        List<Exception> GetPossibleExceptions();
    }
}
