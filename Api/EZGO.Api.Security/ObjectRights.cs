using EZGO.Api.Interfaces.Data;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Security.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Security
{
    /// <summary>
    /// ObjectRights; 
    /// ObjectRights -> single implementation, used for checking a incoming objects ID (ussually through query or route) if user / company has access to this Id. This will be used for viewing, updates and changes.
    /// ObjectRights -> multiple implementation, used for checking incoming objects, all Ids within those objects (e.g. for audit its audit id, audit template id, task ids etc) if user / company has access to all these Id.  This will be used for updates and changes.
    /// </summary>
    public class ObjectRights : IObjectRights
    {
        //TODO add relation data
        //TODO add null checks
        private readonly IDatabaseAccessHelper _manager;
        private ILogger _logger;
        public ObjectRights(IDatabaseAccessHelper manager, ILogger<ObjectRights> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        public async Task<bool> CheckObjectRights(int objectId, ObjectTypeEnum objectType, int companyId, int userId, string referrer)
        {
            var sp = string.Concat("check_object_rights_", objectType.ToString().ToLower());

            //"check_object_rights_action"("_id" int4, "_companyid" int4, "_userid" int4)
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", objectId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            var ok = Convert.ToBoolean(await _manager.ExecuteScalarAsync(sp, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (!ok) _logger.LogWarning(message: "Warning, no rights to ObjectId [{0}, {1}] for company [{2}]|user [{3}]|ref [{4}].", objectId, objectType.ToString(), companyId, userId, referrer);

            return ok;
        }

        public async Task<bool> CheckObjectCompanyRights(int companyId, int companyIdToCheck)
        {
            await Task.CompletedTask;
            return (companyId == companyIdToCheck);
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, int[] actionIds = null, int[] actionCommentIds = null, int[] assessmentIds = null, int[] assessmentSkillInstructionIds = null, int[] assessmentSkillInstructionItemIds = null, int[] assessmentTemplateIds = null, int[] assessmentSkillInstructionTemplateIds = null, int[] auditIds = null, int[] auditTemplateIds = null, int[] checklistIds = null, int[] checklistTemplateIds = null, int[] taskIds = null, int[] taskTemplateIds = null, int[] workInstructionTemplateIds = null, int[] workInstructionTemplateItemIds = null, int[] factoryFeedIds = null, int[] feedMessageItemIds = null, int[] matrixIds = null, int[] userProfileIds = null, int[] areaIds = null, int[] userIds = null, int[] assessmenttemplateskillinstructionIds = null, int[] shiftIds = null, int[] stepIds = null)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));


            if (actionIds != null && actionIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_actionids", Cleaner(actionIds))); }
            if (actionCommentIds != null && actionCommentIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_actioncommentids", Cleaner(actionCommentIds))); }
            if (assessmentIds != null && assessmentIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_assessmentids", Cleaner(assessmentIds))); }
            if (assessmentSkillInstructionIds != null && assessmentSkillInstructionIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_assessmentskillinstructionids", Cleaner(assessmentSkillInstructionIds))); }
            if (assessmentSkillInstructionItemIds != null && assessmentSkillInstructionItemIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_assessmentskillinstructionitemids", Cleaner(assessmentSkillInstructionItemIds))); }
            if (assessmentTemplateIds != null && assessmentTemplateIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_assessmenttemplateids", Cleaner(assessmentTemplateIds))); }
            if (assessmentSkillInstructionTemplateIds != null && assessmentSkillInstructionTemplateIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_assessmentskillinstructiontemplateids", Cleaner(assessmentSkillInstructionTemplateIds))); }
            if (auditIds != null && auditIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_auditids", Cleaner(auditIds))); }
            if (auditTemplateIds != null && auditTemplateIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_audittemplateids", Cleaner(auditTemplateIds))); }
            if (checklistIds != null && checklistIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_checklistids", Cleaner(checklistIds))); }
            if (checklistTemplateIds != null && checklistTemplateIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_checklisttemplateids", Cleaner(checklistTemplateIds))); }
            if (taskTemplateIds != null && taskTemplateIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_tasktemplateids", Cleaner(taskTemplateIds))); }
            if (workInstructionTemplateIds != null && workInstructionTemplateIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_workinstructiontemplateids", Cleaner(workInstructionTemplateIds))); }
            if (workInstructionTemplateItemIds != null && workInstructionTemplateItemIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_workinstructiontemplateitemids", Cleaner(workInstructionTemplateItemIds))); }
            if (factoryFeedIds != null && factoryFeedIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_factoryfeedids", Cleaner(factoryFeedIds))); }
            if (feedMessageItemIds != null && feedMessageItemIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_feedmessageitemids", Cleaner(feedMessageItemIds))); }
            if (matrixIds != null && matrixIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_matrixids", Cleaner(matrixIds))); }
            if (userProfileIds != null && userProfileIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_userprofileids", Cleaner(userProfileIds))); }
            if (areaIds != null && areaIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_areaids", Cleaner(areaIds))); }
            if (userIds != null && userIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_userids", Cleaner(userIds))); }
            if (assessmenttemplateskillinstructionIds != null && assessmenttemplateskillinstructionIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_assessmenttemplateskillinstructionids", Cleaner(assessmenttemplateskillinstructionIds))); }
            if (shiftIds != null && shiftIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_shiftids", Cleaner(shiftIds))); }
            if (stepIds != null && stepIds.Count() > 0) { parameters.Add(new NpgsqlParameter("@_stepids", Cleaner(stepIds))); }

            var ok = Convert.ToBoolean(await _manager.ExecuteScalarAsync("check_object_rights", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (!ok) _logger.LogWarning(message: "Warning, no rights to Objects for company [{0}] | user [{1}].", companyId, userId);

            return ok;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, ActionComment objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.ActionComment, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var actionIds = new[] { objectToCheck.ActionId };
                var actionCommentIds = new[] { objectToCheck.Id };
                var userIds = new[] { objectToCheck.UserId };
                succes = await CheckObjectRights(companyId: companyId, userId: userId, actionIds: actionIds, actionCommentIds: actionCommentIds, userIds: userIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, ActionsAction objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Action, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                // prep
                var userIdsList = objectToCheck.AssignedUsers?.Select(x => x.Id).ToList();
                userIdsList.Add(objectToCheck.CreatedById);

                //array creation
                var actionIds = new[] { objectToCheck.Id };
                var actionCommentIds = objectToCheck.Comments?.Select(x => x.Id).ToArray();
                var areaIds = objectToCheck.AssignedAreas?.Select(x => x.Id).ToArray();
                var userIds = userIdsList.ToArray();
                var taskIds = new[] { objectToCheck.TaskId.Value };
                var taskTemplateIds = new[] { objectToCheck.TaskTemplateId.Value };

                succes = await CheckObjectRights(companyId: companyId, userId: userId, actionIds: actionIds, actionCommentIds: actionCommentIds, areaIds: areaIds, taskIds: taskIds, taskTemplateIds: taskTemplateIds, userIds: userIds);
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, Area objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Area, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                //array creation
                var areaIds = new[] { objectToCheck.Id, objectToCheck.ParentId.Value };

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, Assessment objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Assessment, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                //prep
                var workInstructionTemplateCollectionIds = objectToCheck.SkillInstructions?.Select(x => x.WorkInstructionTemplateId).ToList();
                var workInstructionTemplateItemCollectionIds = new List<int>();
                var userCollectionIds = objectToCheck.Signatures?.Select(x => x.SignedById.Value).ToList();
                userCollectionIds.AddRange(new[] { objectToCheck.AssessorId.Value, objectToCheck.CompletedForId.Value, objectToCheck.ModifiedById.Value, objectToCheck.CreatedById.Value });

                foreach(var item in objectToCheck.SkillInstructions)
                {
                    userCollectionIds.AddRange(new[] { item.CompletedForId.Value, item.ModifiedById.Value, item.CreatedById.Value });
                    userCollectionIds.AddRange(item.InstructionItems.Select(x => x.CompletedForId.Value));
                    workInstructionTemplateCollectionIds.AddRange(item.InstructionItems.Select(x => x.WorkInstructionTemplateId.Value).ToArray());
                    workInstructionTemplateItemCollectionIds.AddRange(item.InstructionItems.Select(x => x.WorkInstructionTemplateItemId.Value).ToArray());
                    
                }

                //array creation
                var assessmentIds = new[] { objectToCheck.Id };
                var areaIds = new[] { objectToCheck.AreaId.Value };
                var userIds = userCollectionIds.ToArray();
                var assessmentTemplateIds = new[] { objectToCheck.TemplateId };
                var assessmentTemplateSkillInstructionIds = objectToCheck.SkillInstructions?.Select(x => x.AssessmentTemplateSkillInstructionId).ToList().ToArray();
                var workInstructionTemplateIds = workInstructionTemplateCollectionIds.ToArray();
                var workInstructionTemplateItemIds = workInstructionTemplateItemCollectionIds.ToArray();


                succes = await CheckObjectRights(companyId: companyId, userId: userId, assessmentIds: assessmentIds, areaIds: areaIds, userIds: userIds, assessmentTemplateIds: assessmentTemplateIds, assessmenttemplateskillinstructionIds: assessmentTemplateSkillInstructionIds, workInstructionTemplateIds: workInstructionTemplateIds, workInstructionTemplateItemIds: workInstructionTemplateItemIds);
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, AssessmentTemplate objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.AssessmentTemplate, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                //prep
                var workInstructionTemplateCollectionIds = objectToCheck.SkillInstructions?.Select(x => x.WorkInstructionTemplateId.Value).ToList();
                var workInstructionTemplateItemCollectionIds = new List<int>();
                var userCollectionIds = new List<int>();
                userCollectionIds.AddRange(new[] { objectToCheck.ModifiedById.Value, objectToCheck.CreatedById.Value });

                foreach (var item in objectToCheck.SkillInstructions)
                {
                    userCollectionIds.AddRange(new[] { item.ModifiedById.Value, item.CreatedById.Value });
                }

                //array creation
                var assessmentTemplateIds = new[] { objectToCheck.Id };
                var areaIds = new[] { objectToCheck.AreaId.Value };
                var userIds = userCollectionIds.ToArray();
                var workInstructionTemplateIds = workInstructionTemplateCollectionIds.ToArray();
                var workInstructionTemplateItemIds = workInstructionTemplateItemCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId,  areaIds: areaIds, userIds: userIds, assessmentTemplateIds: assessmentTemplateIds, workInstructionTemplateIds: workInstructionTemplateIds, workInstructionTemplateItemIds: workInstructionTemplateItemIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, Audit objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Audit, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var stepCollectionIds = new List<int>();
                var taskCollectionIds = new List<int>();
                var taskTemplateCollectionIds = new List<int>();
                var auditTemplateCollectionIds = new List<int>();
                var auditCollectionIds = new List<int>();

                auditTemplateCollectionIds.Add(objectToCheck.TemplateId);
                auditCollectionIds.Add(objectToCheck.Id);
                foreach (var item in objectToCheck.Tasks)
                {
                    taskCollectionIds.Add((int)item.Id);
                    taskTemplateCollectionIds.Add(item.TemplateId);
                    stepCollectionIds.AddRange(item.Steps.Select(x => x.Id));
                    taskTemplateCollectionIds.AddRange(item.Steps.Select(x => x.TaskTemplateId));
                    auditCollectionIds.Add(item.AuditId.Value);
                }


                var areaIds = new[] { objectToCheck.AreaId };
                var auditTemplateIds = auditTemplateCollectionIds.ToArray();
                var taskTemplateIds = taskTemplateCollectionIds.ToArray();
                var stepIds = stepCollectionIds.ToArray();
                var taskIds = taskCollectionIds.ToArray();
                var auditIds = auditCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds, auditTemplateIds: auditTemplateIds, taskTemplateIds: taskTemplateIds, taskIds: taskIds, auditIds: auditIds, stepIds: stepIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, AuditTemplate objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.AuditTemplate, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var workInstructionTemplateCollectionIds = objectToCheck.WorkInstructionRelations.Select(x => x.WorkInstructionTemplateId).ToList();
                var stepCollectionIds = new List<int>();
                var taskTemplateCollectionIds = objectToCheck.TaskTemplates.Select(x => x.Id).ToList();
                var auditTemplateCollectionIds = new List<int>();

                auditTemplateCollectionIds.Add(objectToCheck.Id);
                foreach (var item in objectToCheck.TaskTemplates)
                {
                    workInstructionTemplateCollectionIds.AddRange(item.WorkInstructionRelations.Select(x => x.WorkInstructionTemplateId));
                    stepCollectionIds.AddRange(item.Steps.Select(x => x.Id));
                    taskTemplateCollectionIds.AddRange(item.Steps.Select(x => x.TaskTemplateId));
                    auditTemplateCollectionIds.Add(item.AuditTemplateId.Value);
                }


                var areaIds = new[] { objectToCheck.AreaId };
                var auditTemplateIds = auditTemplateCollectionIds.ToArray();
                var taskTemplateIds = taskTemplateCollectionIds.ToArray();
                var workInstructionTemplateIds = workInstructionTemplateCollectionIds.ToArray();
                var stepIds = stepCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds, auditTemplateIds: auditTemplateIds, taskTemplateIds: taskTemplateIds, workInstructionTemplateIds: workInstructionTemplateIds, stepIds: stepIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, Checklist objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Checklist, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var stepCollectionIds = new List<int>();
                var taskCollectionIds = new List<int>();
                var taskTemplateCollectionIds = new List<int>();
                var checklistTemplateCollectionIds = new List<int>();
                var checklistCollectionIds = new List<int>();

                checklistTemplateCollectionIds.Add(objectToCheck.TemplateId);
                checklistCollectionIds.Add(objectToCheck.Id);
                foreach (var item in objectToCheck.Tasks)
                {
                    taskCollectionIds.Add((int)item.Id);
                    taskTemplateCollectionIds.Add(item.TemplateId);
                    stepCollectionIds.AddRange(item.Steps.Select(x => x.Id));
                    taskTemplateCollectionIds.AddRange(item.Steps.Select(x => x.TaskTemplateId));
                    checklistCollectionIds.Add(item.ChecklistId.Value);
                }


                var areaIds = new[] { objectToCheck.AreaId };
                var checklistTemplateIds = checklistTemplateCollectionIds.ToArray();
                var taskTemplateIds = taskTemplateCollectionIds.ToArray();
                var stepIds = stepCollectionIds.ToArray();
                var taskIds = taskCollectionIds.ToArray();
                var checklistIds = checklistCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds, checklistTemplateIds: checklistTemplateIds, taskTemplateIds: taskTemplateIds, taskIds: taskIds, checklistIds: checklistIds, stepIds: stepIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, ChecklistTemplate objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.ChecklistTemplate, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var workInstructionTemplateCollectionIds = objectToCheck.WorkInstructionRelations.Select(x => x.WorkInstructionTemplateId).ToList();
                var stepCollectionIds = new List<int>();
                var taskTemplateCollectionIds = objectToCheck.TaskTemplates.Select(x => x.Id).ToList();
                var checklistTemplateCollectionIds = new List<int>();

                checklistTemplateCollectionIds.Add(objectToCheck.Id);
                foreach (var item in objectToCheck.TaskTemplates)
                {
                    workInstructionTemplateCollectionIds.AddRange(item.WorkInstructionRelations.Select(x => x.WorkInstructionTemplateId));
                    stepCollectionIds.AddRange(item.Steps.Select(x => x.Id));
                    taskTemplateCollectionIds.AddRange(item.Steps.Select(x => x.TaskTemplateId));
                    checklistTemplateCollectionIds.Add(item.ChecklistTemplateId.Value);
                }


                var areaIds = new[] { objectToCheck.AreaId };
                var checklistTemplateIds = checklistTemplateCollectionIds.ToArray();
                var taskTemplateIds = taskTemplateCollectionIds.ToArray();
                var workInstructionTemplateIds = workInstructionTemplateCollectionIds.ToArray();
                var stepIds = stepCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds, checklistTemplateIds: checklistTemplateIds, taskTemplateIds: taskTemplateIds, workInstructionTemplateIds: workInstructionTemplateIds, stepIds: stepIds);

                //TODO add properties
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, FactoryFeed objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Factoryfeed, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var factoryFeedIds = new[] { objectToCheck.Id };
                
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, FeedMessageItem objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.FactoryfeedMessage, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var feedMessageItemIds = new[] { objectToCheck.Id };
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, Shift objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Shift, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var shiftIds = new[] { objectToCheck.Id };
                var areaIds = new[] { objectToCheck.AreaId.Value };

                succes = await CheckObjectRights(companyId: companyId, userId: userId, shiftIds: shiftIds, areaIds: areaIds);
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, SkillsMatrix objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Matrix, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, TasksTask objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.Task, objectId: (int)objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var taskTemplateCollectionIds = new List<int>();
                taskTemplateCollectionIds.Add(objectToCheck.TemplateId);
                taskTemplateCollectionIds.AddRange(objectToCheck.Actions.Select(x => x.TaskTemplateId.Value));
                var userIds = new[] { objectToCheck.Signature.SignedById.Value, objectToCheck.TimeRealizedById.Value };
                var actionIds = objectToCheck.Actions.Select(x => x.Id).ToArray();
                var taskIds = objectToCheck.Actions.Select(x => x.TaskId.Value).ToArray();
                var taskTemplateIds = taskTemplateCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, taskIds: taskIds, taskTemplateIds: taskTemplateIds, userIds: userIds, actionIds: actionIds);
            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, TaskTemplate objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.TaskTemplate, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var workInstructionTemplateCollectionIds = objectToCheck.WorkInstructionRelations.Select(x => x.WorkInstructionTemplateId).ToList();
                var taskTemplateCollectionIds = new List<int>();
                var shiftCollectionIds = new List<int>();
                var areaCollectionIds = new List<int>();
                var auditTemplateCollectionIds = new List<int>();
                var checklistTemplateCollectionIds = new List<int>();
                var stepCollectionIds = objectToCheck.Steps.Select(x => x.Id);

                taskTemplateCollectionIds.Add(objectToCheck.Id);
                taskTemplateCollectionIds.AddRange(objectToCheck.WorkInstructionRelations.Select(x => x.TaskTemplateId));
                taskTemplateCollectionIds.AddRange(objectToCheck.Steps.Select(x => x.TaskTemplateId));
                areaCollectionIds.Add(objectToCheck.AreaId.Value);
                if(objectToCheck.DeepLinkTo == "audit")
                {
                    auditTemplateCollectionIds.Add(objectToCheck.DeepLinkId.Value);

                }
                if (objectToCheck.DeepLinkTo == "checklist")
                {
                    checklistTemplateCollectionIds.Add(objectToCheck.DeepLinkId.Value);
                }
             

                if (objectToCheck.Recurrency != null)
                {
                    taskTemplateCollectionIds.Add(objectToCheck.Recurrency.TemplateId);
                    shiftCollectionIds.AddRange(objectToCheck.Recurrency.Shifts);
                    shiftCollectionIds.Add(objectToCheck.Recurrency.ShiftId.Value);
                    areaCollectionIds.Add(objectToCheck.Recurrency.AreaId);
                    
                }


                var areaIds = areaCollectionIds.ToArray();
                var taskTemplateIds = taskTemplateCollectionIds.ToArray();
                var shiftIds = shiftCollectionIds.ToArray();
                var auditTemplateIds = auditTemplateCollectionIds.ToArray();
                var checklistTemplateIds = checklistTemplateCollectionIds.ToArray();
                var stepIds = stepCollectionIds.ToArray();
                var workInstructionTemplateIds = workInstructionTemplateCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds, checklistTemplateIds: checklistTemplateIds, taskTemplateIds: taskTemplateIds, workInstructionTemplateIds: workInstructionTemplateIds, stepIds: stepIds, shiftIds: shiftIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, UserProfile objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.ProfileUsers, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                //prep
                var areaIdsCollection = new List<int>();
                areaIdsCollection.AddRange(objectToCheck.AllowedAreas.Select(x => x.Id));
                areaIdsCollection.AddRange(objectToCheck.AllowedAreas.Select(x => x.ParentId.Value));
                areaIdsCollection.AddRange(objectToCheck.DisplayAreas.Select(x => x.Id));
                areaIdsCollection.AddRange(objectToCheck.DisplayAreas.Select(x => x.ParentId.Value));

                //create arrays
                var userIds = new int[] { objectToCheck.Id, objectToCheck.SuccessorId.Value };
                var areaIds = areaIdsCollection.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, userIds: userIds, areaIds : areaIds);

            }
            return succes;
        }

        public async Task<bool> CheckObjectRights(int companyId, int userId, WorkInstructionTemplate objectToCheck, string referrer)
        {
            var succes = true;
            if (objectToCheck.Id > 0) { succes = await CheckObjectRights(companyId: companyId, userId: userId, objectType: ObjectTypeEnum.WorkInstructionTemplate, objectId: objectToCheck.Id, referrer: referrer); }
            if (succes)
            {
                var workinstructionTemplateCollectionIds = new List<int>();
                var workinstructionTemplateItemCollectionIds = new List<int>();
                var userCollectionIds = new List<int>();

                workinstructionTemplateCollectionIds.Add(objectToCheck.Id);
                userCollectionIds.Add(objectToCheck.ModifiedById.Value);
                userCollectionIds.Add(objectToCheck.CreatedById.Value);

                foreach (var item in objectToCheck.InstructionItems)
                {
                    workinstructionTemplateCollectionIds.Add(item.InstructionTemplateId);
                    workinstructionTemplateItemCollectionIds.Add(item.Id);
                    userCollectionIds.Add(item.ModifiedById.Value);
                    userCollectionIds.Add(item.CreatedById.Value);
                }

                var areaIds = new[] { objectToCheck.AreaId.Value };
                var workInstructionTemplateIds = workinstructionTemplateCollectionIds.ToArray();
                var workInstructionTemplateItemIds = workinstructionTemplateItemCollectionIds.ToArray();
                var userIds = userCollectionIds.ToArray();

                succes = await CheckObjectRights(companyId: companyId, userId: userId, areaIds: areaIds, userIds: userIds, workInstructionTemplateIds: workInstructionTemplateIds, workInstructionTemplateItemIds: workInstructionTemplateItemIds);

            }
            return succes;
        }

        /// <summary>
        /// Check if the user has enough rights to retrieve work instruction based on the allowed areas of the user an the settings of the work instruction.
        /// Not a replacement for CheckObjectRights.
        /// </summary>
        /// <param name="userId">User id of user to check</param>
        /// <param name="workInstructionId">workinstruction id of work instruction to check</param>
        /// <returns>True if user has right to retrieve work instruction</returns>
        public async Task<bool> CheckAreaRightsForWorkinstruction(int companyId, int userId, int workInstructionId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_userid", userId),
                new NpgsqlParameter("@_id", workInstructionId)
            };

            var ok = Convert.ToBoolean(await _manager.ExecuteScalarAsync("check_area_rights_workinstruction", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (!ok) _logger.LogWarning(message: "Warning, no rights to Objects for company [{0}] | user [{1}].", companyId, userId);

            return ok;
        }

        /// <summary>
        /// Cleaner; Remove 0 values and distinct values.
        /// </summary>
        /// <param name="idArray"></param>
        /// <returns></returns>
        private int[] Cleaner(int[] idArray)
        {
            idArray = idArray.ToList().Where(x => x != 0).Distinct().ToArray();
            return idArray;
        }
    }
}
