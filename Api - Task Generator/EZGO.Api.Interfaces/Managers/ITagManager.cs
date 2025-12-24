using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ITagManager
    {
        string Culture { get; set; }
        Task<Tag> GetTagAsync(int companyId, int tagId);
        Task<List<Tag>> GetTagsAsync(int companyId, Features features, TagsFilters? filters = null, string include = null);
        Task<Dictionary<int, string>> GetTagNamesAsync(int companyId, List<int> tagIds);
        Task<List<Tag>> GetTagsByGroupAsync(int companyId, string groupGuid);
        Task<List<TagGroup>> GetTagGroupsAsync(int companyId, bool returnAllGroups = false, string include = null, Features features = null);
        Task<int> SetTagGroupsAsync(int companyId, List<TagGroup> tagGroups);
        Task<int> AddTagAsync(int companyId, int userid, Tag tag);
        Task<bool> SetTagActiveAsync(int companyId, int userId, int tagId, bool isActive = true);
        Task<bool> ChangeTagAsync(int companyId, int userId, int tagId, Tag tag);
        Task<List<Tag>> GetTagsWithObjectAsync(int companyId, ObjectTypeEnum objectType, int id, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<bool> UpdateTagsOnObjectAsync(ObjectTypeEnum objectType, int id, List<Tag> tags, int companyId, int userId);
        Task<List<TagRelation>> GetTagRelationsByObjectTypeAsync(int companyId, ObjectTypeEnum objectType, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TagRelation>> GetTagsOnAssessmentTemplateSkillInstructionsAsync(int companyId, int? assessmentTemplateId = null);
        Task<List<TagRelation>> GetTagsOnAssessmentSkillInstructions(int companyId, int? assessmentId = null);
        Task<int> GetTagsCountCompany(int companyId);
        Task<int> GetTagsCountHolding(int companyId);
        List<Exception> GetPossibleExceptions();
    }
}
