using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Security.Interfaces
{
    public interface IObjectRights
    {
        public Task<bool> CheckObjectRights(int objectId, ObjectTypeEnum objectType, int companyId, int userId, string referrer);
        public Task<bool> CheckObjectCompanyRights(int companyId, int companyIdToCheck);
        public Task<bool> CheckObjectRights(int companyId, int userId, int[] actionIds = null, int[] actionCommentIds = null,
                                            int[] assessmentIds = null, int[] assessmentSkillInstructionIds = null, int[] assessmentSkillInstructionItemIds = null, int[] assessmentTemplateIds = null, int[] assessmentSkillInstructionTemplateIds = null, int[] auditIds = null, int[] auditTemplateIds = null,
                                            int[] checklistIds = null, int[] checklistTemplateIds = null, int[] taskIds = null, int[] taskTemplateIds = null,
                                            int[] workInstructionTemplateIds = null, int[] workInstructionTemplateItemIds = null, int[] factoryFeedIds = null, int[] feedMessageItemIds = null, int[] matrixIds = null,
                                            int[] userProfileIds = null, int[] areaIds = null, int[] userIds = null, int[] assessmenttemplateskillinstructionIds = null, int[] shiftIds = null, int[] stepIds = null);
        public Task<bool> CheckObjectRights(int companyId, int userId, ActionComment objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, ActionsAction objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, Area objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, Assessment objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, AssessmentTemplate objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, Audit objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, AuditTemplate objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, Checklist objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, ChecklistTemplate objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, FactoryFeed objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, FeedMessageItem objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, Shift objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, SkillsMatrix objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, TasksTask objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, TaskTemplate objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, UserProfile objectToCheck, string referrer);
        public Task<bool> CheckObjectRights(int companyId, int userId, WorkInstructionTemplate objectToCheck, string referrer);
        public Task<bool> CheckAreaRightsForWorkinstruction(int companyId, int userId, int workInstructionId);

    }
}
