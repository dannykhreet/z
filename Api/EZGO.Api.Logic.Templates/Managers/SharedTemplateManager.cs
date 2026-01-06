using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Tags;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Data.Helpers;
using EZGO.Api.Utils.Json;
using EZGO.Api.Models.TemplateSharing;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Models.Skills;
using EZGO.Api.Utils.Media;
using EZGO.Api.Logic.Templates.Base;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// SharedTemplateManager handles all logic related to the sharing templates.
    /// Shared templates wwill be added to the shared_template table in the database.
    /// When a template is shared, the object will be stored in json format in a text field.
    /// When a shared template is requested, the stored json will be returned.
    /// </summary>
    public class SharedTemplateManager : BaseManager<SharedTemplateManager>, ISharedTemplateManager
    {
        private readonly IDatabaseAccessHelper _manager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IMediaUploader _mediauploader;

        public SharedTemplateManager(IDatabaseAccessHelper manager, IDataAuditing dataAuditing, IMediaUploader mediaUploader, ILogger<SharedTemplateManager> logger) : base(logger)
        {
            _manager = manager;
            _dataAuditing = dataAuditing;
            _mediauploader = mediaUploader;
        }

        /// <summary>
        /// Shares a checklist template with another company
        /// </summary>
        /// <param name="fromCompanyId">Company id of the company that shares the template</param>
        /// <param name="userId">User id of the user initializing the sharing of the template</param>
        /// <param name="checklistTemplate">Checklist id of the original checklist template that is being shared</param>
        /// <param name="toCompanyId">Company id of the company that the template is being shared to</param>
        /// <returns>Id of the shared template object</returns>
        public async Task<int> ShareChecklistTemplateAsync(int fromCompanyId, int userId, ChecklistTemplate checklistTemplate, int toCompanyId)
        {
            if (fromCompanyId == toCompanyId)
                return 0;

            ChecklistTemplate copiedChecklistTemplate = await PrepareShareableCopyOfChecklistTemplate(checklistTemplate, toCompanyId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_from_companyid", fromCompanyId),
                new NpgsqlParameter("@_name", copiedChecklistTemplate.Name),
                new NpgsqlParameter("@_objectjson", copiedChecklistTemplate.ToJsonFromObject()),
                new NpgsqlParameter("@_to_companyid", toCompanyId),
                new NpgsqlParameter("@_object_type", ObjectTypeEnum.ChecklistTemplate.ToString())
            };
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_shared_template", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.shared_template.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.shared_template.ToString(), objectId: id, userId: userId, companyId: fromCompanyId, description: "Shared checklist template.");
            }

            return id;
        }

        /// <summary>
        /// Shares an audit template with another company
        /// </summary>
        /// <param name="fromCompanyId">Company id of the company that shares the template</param>
        /// <param name="userId">User id of the user initializing the sharing of the template</param>
        /// <param name="auditTemplate">Audit id of the original audit template that is being shared</param>
        /// <param name="toCompanyId">Company id of the company that the template is being shared to</param>
        /// <returns>Id of the shared template object</returns>
        /// <returns></returns>
        public async Task<int> ShareAuditTemplateAsync(int fromCompanyId, int userId, AuditTemplate auditTemplate, int toCompanyId)
        {
            if (fromCompanyId == toCompanyId)
                return 0;

            AuditTemplate copiedAuditTemplate = await PrepareShareableCopyOfAuditTemplate(auditTemplate, toCompanyId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_from_companyid", fromCompanyId),
                new NpgsqlParameter("@_name", copiedAuditTemplate.Name),
                new NpgsqlParameter("@_objectjson", copiedAuditTemplate.ToJsonFromObject()),
                new NpgsqlParameter("@_to_companyid", toCompanyId),
                new NpgsqlParameter("@_object_type", ObjectTypeEnum.AuditTemplate.ToString())
            };
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_shared_template", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.shared_template.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.shared_template.ToString(), objectId: id, userId: userId, companyId: fromCompanyId, description: "Shared audit template.");
            }

            return id;
        }

        /// <summary>
        /// Shares an task template with another company
        /// </summary>
        /// <param name="fromCompanyId">Company id of the company that shares the template</param>
        /// <param name="userId">User id of the user initializing the sharing of the template</param>
        /// <param name="taskTemplate">Task id of the original task template that is being shared</param>
        /// <param name="toCompanyId">Company id of the company that the template is being shared to</param>
        /// <returns>Id of the shared template object</returns>
        public async Task<int> ShareTaskTemplateAsync(int fromCompanyId, int userId, TaskTemplate taskTemplate, int toCompanyId)
        {
            if (fromCompanyId == toCompanyId)
                return 0;

            TaskTemplate copiedTaskTemplate = await PrepareShareableCopyOfTaskTemplate(taskTemplate, toCompanyId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_from_companyid", fromCompanyId),
                new NpgsqlParameter("@_name", copiedTaskTemplate.Name),
                new NpgsqlParameter("@_objectjson", copiedTaskTemplate.ToJsonFromObject()),
                new NpgsqlParameter("@_to_companyid", toCompanyId),
                new NpgsqlParameter("@_object_type", ObjectTypeEnum.TaskTemplate.ToString())
            };
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_shared_template", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.shared_template.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.shared_template.ToString(), objectId: id, userId: userId, companyId: fromCompanyId, description: "Shared task template.");
            }

            return id;
        }

        /// <summary>
        /// Shares an work instructions template with another company
        /// </summary>
        /// <param name="fromCompanyId">Company id of the company that shares the template</param>
        /// <param name="userId">User id of the user initializing the sharing of the template</param>
        /// <param name="workInstructionTemplate">Work instructions id of the original work instructions template that is being shared</param>
        /// <param name="toCompanyId">Company id of the company that the template is being shared to</param>
        /// <returns>Id of the shared template object</returns>
        public async Task<int> ShareWorkInstructionTemplateAsync(int fromCompanyId, int userId, WorkInstructionTemplate workInstructionTemplate, int toCompanyId)
        {
            if (fromCompanyId == toCompanyId)
                return 0;

            WorkInstructionTemplate copiedWorkInstructionTemplate = await PrepareShareableCopyOfWorkInstructionTemplate(workInstructionTemplate, toCompanyId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_from_companyid", fromCompanyId),
                new NpgsqlParameter("@_name", copiedWorkInstructionTemplate.Name),
                new NpgsqlParameter("@_objectjson", copiedWorkInstructionTemplate.ToJsonFromObject()),
                new NpgsqlParameter("@_to_companyid", toCompanyId),
                new NpgsqlParameter("@_object_type", ObjectTypeEnum.WorkInstructionTemplate.ToString())
            };
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_shared_template", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.shared_template.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.shared_template.ToString(), objectId: id, userId: userId, companyId: fromCompanyId, description: "Shared workinstruction template.");
            }

            return id;
        }

        /// <summary>
        /// Returns a list of objects representing shared tempaltes of all types
        /// Each object will have Id, Name, Type, FromCompanyName, CreatedAt ModifiedAt
        /// The actual objects are not included.
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<SharedTemplate>> GetSharedTemplatesAsync(int companyId)
        {
            List<SharedTemplate> sharedChecklistTemplates = new();
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId)
            };

            using NpgsqlDataReader dr = await _manager.GetDataReader("get_shared_templates", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
            while (await dr.ReadAsync())
            {
                SharedTemplate sharedTemplate = new()
                {
                    Id = Convert.ToInt32(dr["id"]),
                    Name = Convert.ToString(dr["name"]),
                    FromCompanyName = Convert.ToString(dr["from_company_name"]),
                    CreatedAt = Convert.ToDateTime(dr["created_at"]),
                    ModifiedAt = Convert.ToDateTime(dr["modified_at"])
                };
                if (Enum.TryParse(Convert.ToString(dr["object_type"]), out ObjectTypeEnum type))
                    sharedTemplate.Type = type;
                sharedChecklistTemplates.Add(sharedTemplate);
            }

            return sharedChecklistTemplates;
        }

        /// <summary>
        /// Returns the shared template object json that was stored when the template was shared
        /// </summary>
        /// <param name="companyId">company id of the company requesting the shared template</param>
        /// <param name="sharedTemplateId">Id of the shared template in the shared_template table</param>
        /// <returns>Json representing the template when it was shared</returns>
        public async Task<TemplateJson> GetSharedTemplateAsync(int companyId, int sharedTemplateId)
        {
            TemplateJson sharedTemplateJson = new();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_id", sharedTemplateId)
            };

            using NpgsqlDataReader dr = await _manager.GetDataReader("get_shared_template", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
            while (await dr.ReadAsync())
            {
                sharedTemplateJson.Json = dr["object_json"].ToString();
                if (Enum.TryParse(Convert.ToString(dr["object_type"]), out ObjectTypeEnum type))
                    sharedTemplateJson.Type = type;
            }

            return sharedTemplateJson;
        }

        /// <summary>
        /// Counts the number of active incoming shared templates for a company
        /// </summary>
        /// <param name="companyId">Company id to get the count for</param>
        /// <returns>Number of active incoming shared templates</returns>
        public async Task<int> GetSharedTemplatesCountAsync(int companyId)
        {
            int count = 0;
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
            };

            using NpgsqlDataReader dr = await _manager.GetDataReader("get_shared_templates_count", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
            while (await dr.ReadAsync())
            {
                count = Convert.ToInt32(dr["count"]);
            }

            return count;
        }

        /// <summary>
        /// Sets the shared tempalte to inactive and add data auditing to indicate the template was accepted for the company that shared the template
        /// </summary>
        /// <param name="companyId">Company id of the company accpeting the template</param>
        /// <param name="userId">User accepting the tempalte</param>
        /// <param name="sharedTemplateId">Id of the shared tempalte</param>
        /// <returns>True if successful</returns>
        public async Task<bool> AcceptSharedTemplateAsync(int companyId, int userId, int sharedTemplateId)
        {
            var success = await SetSharedTemplateInactiveAsync(companyId, userId, sharedTemplateId);

            if (success)
            {
                await _dataAuditing.WriteDataAuditForSharedTemplate(sharedTemplateId: sharedTemplateId, statusDescription: "Shared template was accepted.");
            }

            //once we have a notification system, should send a notification to the sender to notify that the template was accepted and by what company.

            return (success);
        }

        /// <summary>
        /// Sets the shared template to inactive and add data auditing to indicate the template was declined for the company that shared the template.
        /// This also deletes any media from S3 that was copied in order to share this template.
        /// Note: If for any reason this shared template IsActive is set to true again in the database, the media that is linked to will not be available anymore, therefor it is not recommended to restore declined templates.
        /// </summary>
        /// <param name="companyId">Company id of the company accpeting the template</param>
        /// <param name="userId">User declining the tempalte</param>
        /// <param name="sharedTemplateId">Id of the shared tempalte</param>
        /// <returns>True if successful</returns>
        public async Task<bool> RejectSharedTemplateAsync(int companyId, int userId, int sharedTemplateId)
        {
            TemplateJson sharedTemplate = await GetSharedTemplateAsync(companyId, sharedTemplateId);

            switch (sharedTemplate.Type)
            {
                case ObjectTypeEnum.ChecklistTemplate:
                    ChecklistTemplate checklistTemplate = sharedTemplate.Json.ToObjectFromJson<ChecklistTemplate>();

                    if (!string.IsNullOrEmpty(checklistTemplate.Picture))
                    {
                        await _mediauploader.DeleteFileFromS3Async(keyName: checklistTemplate.Picture, mediaType: MediaTypeEnum.Image);
                    }

                    if(checklistTemplate.TaskTemplates != null)
                    {
                        foreach (var item in checklistTemplate.TaskTemplates)
                        {
                            if (!string.IsNullOrEmpty(item.Picture))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.Picture, mediaType: MediaTypeEnum.Image);
                            }
                            if (!string.IsNullOrEmpty(item.Video))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.Video, mediaType: MediaTypeEnum.Video);
                            }
                            if (!string.IsNullOrEmpty(item.VideoThumbnail))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.VideoThumbnail, mediaType: MediaTypeEnum.Image);
                            }
                        }
                    }
                   
                    break;
                case ObjectTypeEnum.AuditTemplate:
                    AuditTemplate auditTemplate = sharedTemplate.Json.ToObjectFromJson<AuditTemplate>();

                    if (!string.IsNullOrEmpty(auditTemplate.Picture))
                    {
                        await _mediauploader.DeleteFileFromS3Async(keyName: auditTemplate.Picture, mediaType: MediaTypeEnum.Image);
                    }

                    if(auditTemplate.TaskTemplates != null)
                    {
                        foreach (var item in auditTemplate.TaskTemplates)
                        {
                            if (!string.IsNullOrEmpty(item.Picture))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.Picture, mediaType: MediaTypeEnum.Image);
                            }
                            if (!string.IsNullOrEmpty(item.Video))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.Video, mediaType: MediaTypeEnum.Video);
                            }
                            if (!string.IsNullOrEmpty(item.VideoThumbnail))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.VideoThumbnail, mediaType: MediaTypeEnum.Image);
                            }
                        }
                    }
                  
                    break;
                case ObjectTypeEnum.TaskTemplate:
                    TaskTemplate taskTemplate = sharedTemplate.Json.ToObjectFromJson<TaskTemplate>();

                    if (!string.IsNullOrEmpty(taskTemplate.Picture))
                    {
                        await _mediauploader.DeleteFileFromS3Async(keyName: taskTemplate.Picture, mediaType: MediaTypeEnum.Image);
                    }

                    if(taskTemplate.Steps != null)
                    {
                        foreach (var item in taskTemplate.Steps)
                        {
                            if (!string.IsNullOrEmpty(item.Picture))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.Picture, mediaType: MediaTypeEnum.Image);
                            }
                            if (!string.IsNullOrEmpty(item.Video))
                            {
                                await _mediauploader.DeleteFileFromS3Async(keyName: item.Video, mediaType: MediaTypeEnum.Video);

                                if (!string.IsNullOrEmpty(item.VideoThumbnail))
                                {
                                    await _mediauploader.DeleteFileFromS3Async(keyName: item.VideoThumbnail, mediaType: MediaTypeEnum.Image);
                                }
                            }
                        }
                    }
                    
                    break;
                case ObjectTypeEnum.WorkInstructionTemplate:
                    WorkInstructionTemplate workinstruction = sharedTemplate.Json.ToObjectFromJson<WorkInstructionTemplate>();

                    if(workinstruction.Media != null)
                    {
                        if (workinstruction.Media.Count > 0)
                            await _mediauploader.DeleteFileFromS3Async(keyName: workinstruction.Media[0], mediaType: MediaTypeEnum.Image);
                        if (workinstruction.Media.Count == 2)
                            await _mediauploader.DeleteFileFromS3Async(keyName: workinstruction.Media[1], mediaType: MediaTypeEnum.Video);
                    }

                    if(workinstruction.InstructionItems != null)
                    {
                        foreach (var item in workinstruction.InstructionItems)
                        {
                            if (item.Media != null)
                            {
                                if (item.Media.Count > 0)
                                    await _mediauploader.DeleteFileFromS3Async(keyName: item.Media[0], mediaType: MediaTypeEnum.Image);
                                if (item.Media.Count == 2)
                                    await _mediauploader.DeleteFileFromS3Async(keyName: item.Media[1], mediaType: MediaTypeEnum.Video);
                            }
                        }
                    }
                    break;
                case ObjectTypeEnum.AssessmentTemplate:
                    AssessmentTemplate assessmentTemplate = sharedTemplate.Json.ToObjectFromJson<AssessmentTemplate>();
                    if(assessmentTemplate.Media != null)
                    {
                        if (assessmentTemplate.Media.Count > 0)
                            await _mediauploader.DeleteFileFromS3Async(keyName: assessmentTemplate.Media[0], mediaType: MediaTypeEnum.Image);
                        if (assessmentTemplate.Media.Count == 2)
                            await _mediauploader.DeleteFileFromS3Async(keyName: assessmentTemplate.Media[1], mediaType: MediaTypeEnum.Video);
                    }
                    break;
            }

            var success = await SetSharedTemplateInactiveAsync(companyId, userId, sharedTemplateId);

            if (success)
            {
                await _dataAuditing.WriteDataAuditForSharedTemplate(sharedTemplateId: sharedTemplateId, statusDescription: "Shared template was declined.");
            }

            //once we have a notification system, should send a notification to the sender to notify that the template was declined and by what company.

            return (success);
        }

        #region - private -
        /// <summary>
        /// Sets the shared template active state to false
        /// </summary>
        /// <param name="companyId">Company id of the company to which the tempalte was shared</param>
        /// <param name="userId">User id of the user taking this action</param>
        /// <param name="sharedTemplateId">Id of the shared template</param>
        /// <returns>True if successful</returns>
        private async Task<bool> SetSharedTemplateInactiveAsync(int companyId, int userId, int sharedTemplateId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.shared_template.ToString(), sharedTemplateId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_id", sharedTemplateId),
                new NpgsqlParameter("@_active", false)
            };
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_shared_template_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.shared_template.ToString(), sharedTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.shared_template.ToString(), objectId: sharedTemplateId, userId: userId, companyId: companyId, description: "Changed shared template active state.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// Strip checklist template of all company specific data so that it can be shared with another company.
        /// All properties that are specific to the company will be unset. (Area and related properties, properties based on derived items)
        /// Relations with company specific objects will be removed (Tags that are not shared between companies)
        /// Any other references to other templates will be removed (linked workinstructions)
        /// Any media will be copied for the company what is shared to.
        /// </summary>
        /// <param name="checklistTemplate">ChecklistTemplate to be stripped</param>
        /// <param name="toCompanyId">CompanyToShareTo</param>
        /// <returns>Checklist template stripped of company specific data</returns>
        private async Task<ChecklistTemplate> PrepareShareableCopyOfChecklistTemplate(ChecklistTemplate checklistTemplate, int toCompanyId)
        {
            //unset all company specific values
            checklistTemplate.Id = 0;
            checklistTemplate.AreaId = 0;
            checklistTemplate.CompanyId = toCompanyId;
            checklistTemplate.HasIncompleteChecklists = null;
            checklistTemplate.AreaPath = null;
            checklistTemplate.AreaPathIds = null;
            checklistTemplate.HasDerivedItems = null;
            checklistTemplate.ModifiedAt = null;
            checklistTemplate.WorkInstructions = null;
            checklistTemplate.WorkInstructionRelations = null;
            if (checklistTemplate.Tags != null) checklistTemplate.Tags = CleanTagsForShareableCopy(checklistTemplate.Tags, true);
            if (!string.IsNullOrEmpty(checklistTemplate.Picture))
                checklistTemplate.Picture = await _mediauploader.CopyFileAsync(sourceFileKey: checklistTemplate.Picture, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Checklists, companyId: toCompanyId, includeBaseUrlOnReturn: false);

            if (checklistTemplate.TaskTemplates != null)
                foreach (TaskTemplate item in checklistTemplate.TaskTemplates)
                {
                    await PrepareShareableCopyOfTaskTemplate(item, toCompanyId);
                }

            checklistTemplate.OpenFieldsProperties?.ForEach(openField =>
            {
                openField.Id = 0;
                openField.ChecklistTemplateId = 0;
            });
            checklistTemplate.Properties?.ForEach(prop =>
            {
                prop.Id = 0;
                prop.ChecklistTemplateId = 0;
            });

            if (checklistTemplate.HasStages)
            {
                checklistTemplate.StageTemplates?.ForEach(stage =>
                {
                    stage.Id = stage.Index;
                    stage.CompanyId = toCompanyId;
                    stage.ChecklistTemplateId = 0;
                    stage.TaskTemplateIds = null;
                });
            }

            return checklistTemplate;
        }

        /// <summary>
        /// Strip audit template of all company specific data so that it can be shared with another company.
        /// All properties that are specific to the company will be unset. (Area and related properties, properties based on derived items)
        /// Relations with company specific objects will be removed (Tags that are not shared between companies)
        /// Any other references to other templates will be removed (linked workinstructions)
        /// Any media will be copied for the company what is shared to.
        /// </summary>
        /// <param name="auditTemplate">AuditTemplate to be stripped</param>
        /// <param name="toCompanyId">CompanyToShareTo</param>
        /// <returns>Audit template stripped of company specific data</returns>
        private async Task<AuditTemplate> PrepareShareableCopyOfAuditTemplate(AuditTemplate auditTemplate, int toCompanyId)
        {
            //unset all company specific values
            auditTemplate.Id = 0;
            auditTemplate.AreaId = 0;
            auditTemplate.CompanyId = toCompanyId;
            auditTemplate.HasIncompleteAudits = null;
            auditTemplate.AreaPath = null;
            auditTemplate.AreaPathIds = null;
            auditTemplate.HasDerivedItems = null;
            auditTemplate.ModifiedAt = null;
            auditTemplate.WorkInstructions = null;
            auditTemplate.WorkInstructionRelations = null;
            if (auditTemplate.Tags != null) auditTemplate.Tags = CleanTagsForShareableCopy(auditTemplate.Tags, true);
            if (!string.IsNullOrEmpty(auditTemplate.Picture))
                auditTemplate.Picture = await _mediauploader.CopyFileAsync(sourceFileKey: auditTemplate.Picture, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Audits, companyId: toCompanyId, includeBaseUrlOnReturn: false);

            if (auditTemplate.TaskTemplates != null)
                foreach (TaskTemplate item in auditTemplate.TaskTemplates)
                {
                    await PrepareShareableCopyOfTaskTemplate(item, toCompanyId);
                }

            auditTemplate.OpenFieldsProperties?.ForEach(openField =>
            {
                openField.Id = 0;
                openField.AuditTemplateId = 0;
            });
            auditTemplate.Properties?.ForEach(prop =>
            {
                prop.Id = 0;
                prop.AuditTemplateId = 0;
            });

            return auditTemplate;
        }

        /// <summary>
        /// Strip task template of all company specific data so that it can be shared with another company.
        /// All properties that are specific to the company will be unset. (Area and related properties, properties based on derived items)
        /// Relations with company specific objects will be removed (Tags that are not shared between companies)
        /// Any other references to other templates will be removed (linked workinstructions)
        /// Any media will be copied for the company what is shared to.
        /// </summary>
        /// <param name="taskTemplate">TaskTemplate to be stripped</param>
        /// <param name="toCompanyId">CompanyToShareTo</param>
        /// <returns>Task template stripped of company specific data</returns>
        private async Task<TaskTemplate> PrepareShareableCopyOfTaskTemplate(TaskTemplate taskTemplate, int toCompanyId)
        {
            //unset all company specific values
            taskTemplate.Id = taskTemplate.Type.Equals("task") ? 0 : taskTemplate.Index ?? 0;
            taskTemplate.ChecklistTemplateId = null;
            taskTemplate.AuditTemplateId = null;
            taskTemplate.AreaId = 0;
            taskTemplate.CompanyId = toCompanyId;
            taskTemplate.AreaPath = null;
            taskTemplate.AreaPathIds = null;
            taskTemplate.HasDerivedItems = null;
            taskTemplate.ModifiedAt = null;
            taskTemplate.WorkInstructions = null;
            taskTemplate.WorkInstructionRelations = null;
            taskTemplate.Actions = null;
            taskTemplate.Recurrency = null;
            taskTemplate.RecurrencyType = null;

            //remove linked template
            taskTemplate.DeepLinkTo = null;
            taskTemplate.DeepLinkId = null;
            taskTemplate.DeepLinkCompletionIsRequired = null;
            
            if (taskTemplate.Tags != null) taskTemplate.Tags = CleanTagsForShareableCopy(taskTemplate.Tags, true);
            if (!string.IsNullOrEmpty(taskTemplate.Picture))
                taskTemplate.Picture = await _mediauploader.CopyFileAsync(sourceFileKey: taskTemplate.Picture, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Tasks, companyId: toCompanyId, includeBaseUrlOnReturn: false);
            if (!string.IsNullOrEmpty(taskTemplate.Video))
                taskTemplate.Video = await _mediauploader.CopyFileAsync(sourceFileKey: taskTemplate.Video, mediaType: MediaTypeEnum.Video, mediaStorageType: MediaStorageTypeEnum.Tasks, companyId: toCompanyId, includeBaseUrlOnReturn: true);
            if (!string.IsNullOrEmpty(taskTemplate.VideoThumbnail))
                taskTemplate.VideoThumbnail = await _mediauploader.CopyFileAsync(sourceFileKey: taskTemplate.VideoThumbnail, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Tasks, companyId: toCompanyId, includeBaseUrlOnReturn: false);
            if (!string.IsNullOrEmpty(taskTemplate.DescriptionFile))
                taskTemplate.DescriptionFile = await _mediauploader.CopyFileAsync(sourceFileKey: taskTemplate.DescriptionFile, mediaType: MediaTypeEnum.Docs, mediaStorageType: MediaStorageTypeEnum.Tasks, companyId: toCompanyId, includeBaseUrlOnReturn: false);
            
            if (taskTemplate.Attachments != null)
                foreach (Attachment attachment in taskTemplate.Attachments)
                {
                    if (attachment.AttachmentType.ToLower().Equals("pdf"))
                    {
                        attachment.Uri = await _mediauploader.CopyFileAsync(sourceFileKey: attachment.Uri, mediaType: MediaTypeEnum.Docs, mediaStorageType: MediaStorageTypeEnum.Tasks, companyId: toCompanyId, includeBaseUrlOnReturn: false);
                        var uriParts = attachment.Uri.Split('/');
                        var filename = uriParts.Last();
                        attachment.FileName = filename;
                    }
                }

            taskTemplate.Properties?.ForEach(prop =>
            {
                prop.Id = 0;
                prop.TaskTemplateId = 0;
            });
            taskTemplate.Steps?.ForEach(step =>
            {
                step.Id = 0;
                step.TaskTemplateId = 0;
            });

            if (taskTemplate.Steps != null)
                foreach (Step itemStep in taskTemplate.Steps)
                {
                    itemStep.Id = itemStep.Index;
                    itemStep.TaskTemplateId = 0;

                    if (!string.IsNullOrEmpty(itemStep.Picture))
                        itemStep.Picture = await _mediauploader.CopyFileAsync(sourceFileKey: itemStep.Picture, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.TaskSteps, companyId: toCompanyId, includeBaseUrlOnReturn: false);

                    if (!string.IsNullOrEmpty(itemStep.Video))
                        itemStep.Video = await _mediauploader.CopyFileAsync(sourceFileKey: itemStep.Video, mediaType: MediaTypeEnum.Video, mediaStorageType: MediaStorageTypeEnum.TaskSteps, companyId: toCompanyId, includeBaseUrlOnReturn: true);

                    if (!string.IsNullOrEmpty(itemStep.VideoThumbnail))
                        itemStep.VideoThumbnail = await _mediauploader.CopyFileAsync(sourceFileKey: itemStep.VideoThumbnail, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.TaskSteps, companyId: toCompanyId, includeBaseUrlOnReturn: false);
                }

            return taskTemplate;
        }

        /// <summary>
        /// Strip work instruction template of all company specific data so that it can be shared with another company.
        /// All properties that are specific to the company will be unset. (Area and related properties, properties based on derived items)
        /// Relations with company specific objects will be removed (Tags that are not shared between companies)
        /// Any other references to other templates will be removed (linked workinstructions)
        /// Any media will be copied for the company what is shared to.
        /// </summary>
        /// <param name="workInstructionTemplate">WorkInstructionTemplate to be stripped</param>
        /// <param name="toCompanyId">CompanyToShareTo</param>
        /// <returns>Work instruction template stripped of company specific data</returns>
        private async Task<WorkInstructionTemplate> PrepareShareableCopyOfWorkInstructionTemplate(WorkInstructionTemplate workInstructionTemplate, int toCompanyId)
        {
            //unset all company specific values
            workInstructionTemplate.Id = 0;
            workInstructionTemplate.AreaId = 0;
            workInstructionTemplate.CompanyId = toCompanyId;
            workInstructionTemplate.AreaPath = null;
            workInstructionTemplate.AreaPathIds = null;
            workInstructionTemplate.CreatedAt = null;
            workInstructionTemplate.ModifiedAt = null;
            workInstructionTemplate.CreatedBy = null;
            workInstructionTemplate.ModifiedBy = null;
            workInstructionTemplate.CreatedById = null;
            workInstructionTemplate.ModifiedById = null;
            workInstructionTemplate.ParentRelations = null;
            if (!string.IsNullOrEmpty(workInstructionTemplate.Picture))
            {
                //first item in media is the picture
                string copyOfMediaPicture = await _mediauploader.CopyFileAsync(sourceFileKey: workInstructionTemplate.Media[0], mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.WorkInstruction, companyId: toCompanyId, includeBaseUrlOnReturn: false);
                workInstructionTemplate.Media[0] = copyOfMediaPicture;
                workInstructionTemplate.Picture = copyOfMediaPicture;
            }

            if (workInstructionTemplate.Tags != null) workInstructionTemplate.Tags = CleanTagsForShareableCopy(workInstructionTemplate.Tags, true);
            if (workInstructionTemplate.InstructionItems != null)
                foreach (var item in workInstructionTemplate.InstructionItems)
                {
                    item.Id = item.Index;
                    item.CompanyId = toCompanyId;
                    item.InstructionTemplateId = 0;
                    item.AssessmentTemplateId = 0;
                    if (item.Tags != null) item.Tags = CleanTagsForShareableCopy(item.Tags, true);

                    if (!string.IsNullOrEmpty(item.Picture))
                    {
                        //first item in media is the picture
                        string copyOfMediaPicture = await _mediauploader.CopyFileAsync(sourceFileKey: item.Media[0], mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.WorkInstruction, companyId: toCompanyId, includeBaseUrlOnReturn: false);
                        item.Media[0] = copyOfMediaPicture;
                        item.Picture = copyOfMediaPicture;
                    }
                    if (!string.IsNullOrEmpty(item.VideoThumbnail))
                    {
                        //instructionItemTemplate.VideoThumbnail is Media[0];
                        //instructionItemTemplate.Video is Media[1];
                        string copyOfVideoThumbnail = await _mediauploader.CopyFileAsync(sourceFileKey: item.Media[0], mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.WorkInstructionItem, companyId: toCompanyId, includeBaseUrlOnReturn: false);
                        item.Media[0] = copyOfVideoThumbnail;
                        item.VideoThumbnail = copyOfVideoThumbnail;
                    }
                    if (!string.IsNullOrEmpty(item.Video))
                    {
                        //instructionItemTemplate.VideoThumbnail is Media[0];
                        //instructionItemTemplate.Video is Media[1];
                        string copyOfVideo = await _mediauploader.CopyFileAsync(sourceFileKey: item.Media[1], mediaType: MediaTypeEnum.Video, mediaStorageType: MediaStorageTypeEnum.WorkInstructionItem, companyId: toCompanyId, includeBaseUrlOnReturn: true);
                        item.Media[1] = copyOfVideo;
                        item.Video = copyOfVideo;
                    }
                }

            return workInstructionTemplate;
        }

        /// <summary>
        /// Removes all company specific tags.
        /// Keeps all system tags and optionally keeps holding tags.
        /// </summary>
        /// <param name="tags">List of tags to clean</param>
        /// <param name="keepHoldingTags">Determines if holding tags will be kept or not</param>
        /// <returns>List of tags that was cleaned</returns>
        private List<Tag> CleanTagsForShareableCopy(List<Models.Tags.Tag> tags, bool keepHoldingTags)
        {
            //only keep system tags and option to keep holding tags for sharing templates within a holding
            tags = tags.Where(t => (t.IsHoldingTag == true && keepHoldingTags) || t.IsSystemTag == true).ToList();

            return tags;
        }
        #endregion
    }
}
