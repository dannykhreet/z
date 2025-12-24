using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IChecklistManager, Interface for use with the ChecklistManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IChecklistManager
    {
        #region - properties -
        string Culture { get; set; }
        #endregion
        #region - checklists -
        Task<List<Checklist>> GetChecklistsAsync(int companyId, int? userId = null, ChecklistFilters? filters = null, string include = null, bool useStatic = false);
        Task<Checklist> GetChecklistAsync(int companyId, int checklistId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false);
        Task<int> AddChecklistAsync(int companyId, int userId, Checklist checklist);
        Task<bool> ChangeChecklistAsync(int companyId, int userId, int checklistId, Checklist checklist);
        Task<bool> SetChecklistActiveAsync(int companyId, int userId, int checklistId, bool isActive = true);
        Task<bool> SetChecklistCompletedAsync(int companyId, int userId, int checklistId, bool isCompleted = true);
        Task<bool> ChecklistSigningAsync(int companyId, int userId, int checklistId, ChecklistRelationSigning signing);
        Task<ChecklistRelationStatus> CreateChecklistAsync(int companyId, int userId, ChecklistRelationStatus checklistRelation);
        Task<ChecklistRelationStatus> SetChecklistTaskStatusAsync(int companyId, int userId, ChecklistRelationStatus checklistRelation);
        #endregion
        #region - templates -
        Task<List<ChecklistTemplate>> GetChecklistTemplatesAsync(int companyId, int? userId = null, ChecklistFilters? filters = null, string include = null);
        Task<ChecklistTemplateCountStatistics> GetChecklistTemplateCountsAsync(int companyId, int? userId = null, ChecklistFilters? filters = null, string include = null);
        Task<ChecklistTemplate> GetChecklistTemplateAsync(int companyId, int checklistTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<Dictionary<int, string>> GetChecklistTemplateNamesAsync(int companyId, List<int> checklistTemplateIds);
        Task<int> AddChecklistTemplateAsync(int companyId, int userId, ChecklistTemplate checklistTemplate);
        Task<bool> ChangeChecklistTemplateAsync(int companyId, int userId, int checklistTemplateId, ChecklistTemplate checklistTemplate);
        Task<bool> SetChecklistTemplateActiveAsync(int companyId, int userId, int checklistTemplateId, bool isActive = true);

        Task<List<StageTemplate>> GetStageTemplatesByChecklistTemplateIdAsync(int companyId, int checklistTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<Stage>> GetStagesByChecklistIdAsync(int companyId, int checklistId, string include = null,ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TaskTemplate>> GetTaskTemplatesWithChecklistTemplate(int companyId, int checklistTemplateId, string include = "", ConnectionKind connectionKind = ConnectionKind.Reader);


        Task<List<int>> GetConnectedTaskTemplateIds(int companyId, int checklistTemplateId);
        //Task<int> ShareChecklistTemplateAsync(int fromCompanyId, int userId, ChecklistTemplate checklistTemplate, int toCompanyId);
        #endregion
        List<Exception> GetPossibleExceptions();
    }
}
