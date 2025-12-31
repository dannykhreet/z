using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IMatrixManager
    {
        Task<List<SkillsMatrix>> GetMatricesAsync(int companyId, int? userId = null, MatrixFilters? filters = null, string include = null);
        Task<SkillsMatrix> GetMatrixAsync(int companyId, int userId, int matrixId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<SkillsMatrixBehaviourItem>> GetMatrixOperationalBehaviour(int companyId, int matrixId);
        Task<List<SkillsMatrixBehaviourItem>> GetMatrixTotals(int companyId, int matrixId);
        Task<int> AddMatrixAsync(int companyId, int userId, SkillsMatrix matrix);
        Task<bool> ChangeMatrixAsync(int companyId, int userId, int matrixId, SkillsMatrix matrix);
        Task<bool> SetMatrixActiveAsync(int companyId, int userId, int matrixId, bool isActive = true);
        Task<int> AddMatrixUserGroupRelationAsync(int companyId, int userId, int matrixId, MatrixRelationUserGroup matrixRelationUserGroup);
        Task<bool> ChangeMatrixUserGroupRelationAsync(int companyId, int userId, int matrixId, int matrixRelationUserGroupId, MatrixRelationUserGroup matrixRelationUserGroup);
        Task<int> AddMatrixUserSkillRelationAsync(int companyId, int userId, int matrixId, MatrixRelationUserSkill matrixRelationUserSkill);
        Task<bool> ChangeMatrixUserSkillRelationAsync(int companyId, int userId, int matrixId, int matrixRelationUserSkillId, MatrixRelationUserSkill matrixRelationUserSkill);
        
        Task<bool> RemoveMatrixUserSkillRelationAsync(int companyId, int userId, int matrixId, int matrixRelationUserSkillId, MatrixRelationUserSkill matrixRelationUserSkill);
        Task<int> AddMatrixUserSkillAsync(int companyId, int userId, int matrixId, SkillsMatrixItem matrixUserSkill);
        Task<int> ChangeMatrixUserSkillAsync(int companyId, int userId, int matrixId, SkillsMatrixItem matrixUserSkill);

        Task<int> AddMatrixUserGroupAsync(int companyId, int userId, int matrixId, SkillsMatrixUserGroup matrixUserGroup);
        Task<int> ChangeMatrixUserGroupAsync(int companyId, int userId, int matrixId, SkillsMatrixUserGroup matrixUserGroup);

        Task<bool> RemoveMatrixUserGroupAsync(int companyId, int userId, int matrixId, MatrixRelationUserGroup matrixRelationUserGroup);
        Task<List<SkillsMatrixUserGroup>> GetMatrixUserGroupsAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<SkillsMatrixItem>> GetMatrixUserSkillsAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<SkillsMatrixUser>> GetMatrixUsersAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<SkillsMatrixItemValue>> GetMatrixUserSkillValuesAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<SkillsMatrixUserGroup> GetMatrixUserGroupAsync(int companyId, int userId, int matrixId, int matrixUserGroupId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<SkillsMatrixItem> GetMatrixUserSkillAsync(int companyId, int userId, int matrixId, int matrixUserSkillId, SkillTypeEnum skillTypeEnum, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<SkillsMatrixUser> GetMatrixUserByUserProfileAsync(int companyId, int userId, int matrixId, int userProfileId, ConnectionKind connectionKind = ConnectionKind.Reader);

        Task<bool> SaveMatrixUserSkillValue(int companyId, int userId, int matrixId, SkillsMatrixItemValue matrixItemValue);

        Task<List<AssessmentScoreItem>> GetMatrixAssessmentScoreItemsAsync(int companyId, int userId, int matrixId);
        List<Exception> GetPossibleExceptions();

        #region Legend Configuration
        Task<SkillMatrixLegendConfiguration> GetLegendConfigurationAsync(int companyId, int userId);
        Task<SkillMatrixLegendConfiguration> SaveLegendConfigurationAsync(SkillMatrixLegendConfiguration configuration, int userId);
        Task<SkillMatrixLegendItem> UpdateLegendItemAsync(int companyId, SkillMatrixLegendItem item);
        #endregion

    }
}
