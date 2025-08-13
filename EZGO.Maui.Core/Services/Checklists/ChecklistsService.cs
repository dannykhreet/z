using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Local;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using NodaTime;
using System.Diagnostics;
using System.Net;
using itemsQueue = EZGO.Maui.Classes.QueueManager<EZGO.Maui.Core.Models.Checklists.SignedChecklistModel>;


namespace EZGO.Maui.Core.Services.Checklists
{
    public class ChecklistsService : IChecklistService
    {
        private readonly string _debugCat = "[ChecklistsService]";

        private readonly IApiRequestHandler _apiRequestHandler;

        private readonly IFileService _fileService;

        private readonly IActionsService _actionService;
        private readonly ISignatureService _signatureService;
        private readonly ITasksService _taskService;
        private readonly IPropertyService _propertyService;
        private readonly ITaskCommentService _commentService;
        private readonly IInstructionsService _instructionsService;
        private readonly IMessageService _messageService;
        private readonly IMediaService _mediaService;
        private readonly IRoleFunctionsWrapper _roleFunctionsWrapper;

        private const string signedChecklistsFilename = "signedchecklists.json";
        private const string localChecklistTemplatesFilename = "localchecklisttemplates.json";

        public ChecklistsService(
            IApiRequestHandler apiRequestHandler,
            IActionsService actionsService,
            ISignatureService signatureService,
            ITasksService tasksService,
            IPropertyService propertyService,
            ITaskCommentService taskCommentService,
            IInstructionsService instructionsService,
            IMessageService messageService,
            IRoleFunctionsWrapper roleFunctionsWrapper,
            IMediaService mediaService,
            IFileService fileService)
        {
            //_fileService = DependencyService.Get<IFileService>();
            _fileService = fileService;
            _apiRequestHandler = apiRequestHandler;
            _actionService = actionsService;
            _signatureService = signatureService;
            _taskService = tasksService;
            _propertyService = propertyService;
            _commentService = taskCommentService;
            _instructionsService = instructionsService;
            _messageService = messageService;
            _mediaService = mediaService;
            _roleFunctionsWrapper = roleFunctionsWrapper;
        }

        public async Task<List<ChecklistModel>> GetReportChecklistsAsync(DateTime? startTimeStamp, DateTime? endTimeStamp, bool refresh = false, int limit = 0, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false)
        {
            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();

            var includes = $"include=signatures,tasks,propertyuservalues,properties,openfields,userinformation";
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes += ",pictureproof";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                includes += ",tags";

            string uri = $"checklists?areaid={Settings.ReportWorkAreaId}&filterareatype=1&iscomplete=true&limit={limit}&{includes}&allowedonly={allowed}&offset={offset}";

            if (timeStamp.HasValue)
                uri += $"&timestamp={timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null)}";

            if (startTimeStamp.HasValue)
                uri += $"&starttimestamp={startTimeStamp.Value.ToString(Constants.ApiDateTimeFormat)}";

            if (startTimeStamp.HasValue)
                uri += $"&endtimestamp={endTimeStamp.Value.ToString(Constants.ApiDateTimeFormat)}";


            List<ChecklistModel> result = await _apiRequestHandler.HandleListRequest<ChecklistModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            List<TasksTaskModel> tasks = new List<TasksTaskModel>();

            foreach (ChecklistModel checklist in result)
            {
                if (!checklist.Tasks.IsNullOrEmpty())
                {
                    tasks.AddRange(checklist.Tasks);
                }
            }

            tasks.ForEach(x =>
            {
                x.CreatePropertyList();
                x.SetPictureProofMediaItems();
            });

            await _taskService?.LoadActionCountForTasksAsync(tasks, refresh);

            return result;
        }

        public async Task<List<ChecklistModel>> GetIncompleteChecklistsAsync(bool refresh = false, bool isFromSyncService = false, int checklistTemplateId = 0, long taskId = 0, bool refreshActions = true)
        {
            if (!await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                return new List<ChecklistModel>();

            if (!CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                return new List<ChecklistModel>();

            var result = await GetChecklistsAsync(isComplete: false, limit: 0, timeStamp: null, refresh: refresh, isFromSyncService: isFromSyncService, useAreaId: false, taskId: taskId, templateId: checklistTemplateId).ConfigureAwait(false);

            if (checklistTemplateId != 0)
                result = result.Where(x => x.TemplateId == checklistTemplateId).ToList();

            result.ForEach(r => r.SetPercentageFinished());
            return result;
        }

        public async Task<List<ChecklistModel>> GetIncompleteDeeplinkChecklistsAsync(bool refresh = false, bool isFromSyncService = false, long taskId = 0)
        {
            if (taskId == 0)
                return new List<ChecklistModel>();

            var incompleteChecklists = await GetIncompleteChecklistsAsync(refresh, isFromSyncService, taskId: taskId, refreshActions: false).ConfigureAwait(false);
            return incompleteChecklists ?? new List<ChecklistModel>();
        }

        public async Task<ChecklistModel> GetIncompleteChecklistAsync(bool refresh = false, bool isFromSyncService = false, int checklistId = 0)
        {
            if (checklistId == 0)
                return new ChecklistModel();

            var incompleteChecklists = await GetChecklistAsync(checklistId, refresh, isFromSyncService).ConfigureAwait(false);
            var result = incompleteChecklists;
            return result ?? new ChecklistModel();
        }

        public async Task<List<ChecklistModel>> GetChecklistsAsync(bool isComplete = false, bool useAreaId = true, bool refresh = false, int limit = 100, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false, long taskId = 0, int templateId = 0, bool showLocalChecklists = true, bool refreshActions = true)
        {
            DebugService.Start(_debugCat);

            int workAreaId = useAreaId ? Settings.AreaSettings.WorkAreaId : 0;
            var parameters = new List<string>
                {
                    $"limit={limit}",
                    $"iscompleted={isComplete.ToString().ToLower()}"
                };

            if (workAreaId != 0)
            {
                parameters.Add($"areaid={workAreaId}");
                parameters.Add($"filterareatype={(int)FilterAreaTypeEnum.RecursiveRootToLeaf}");
            }

            if (isComplete)
            {
                parameters.Add($"offset={offset}");
                if (timeStamp.HasValue)
                    parameters.Add($"timestamp={timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null)}");
            }

            if (taskId != 0) parameters.Add($"taskId={taskId}");
            if (templateId != 0) parameters.Add($"templateid={templateId}");

            var includes = new List<string>
    {
        "signatures", "tasks", "propertyuservalues", "properties", "openfields", "userinformation"
    };

            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes.Add("pictureproof");

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                includes.Add("tags");

            parameters.Add($"include={string.Join(",", includes)}");

            var allowed = _roleFunctionsWrapper
                .checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType)
                .ToString().ToLower();
            parameters.Add($"allowedonly={allowed}");

            string action = $"checklists?{string.Join("&", parameters)}";

            var result = await _apiRequestHandler
                .HandleListRequest<ChecklistModel>(action, refresh, isFromSyncService)
                .ConfigureAwait(false);

            List<SignedChecklistModel> signedChecklists = null;
            Dictionary<string, ActionsModel> actionsMap = null;

            if (isComplete && showLocalChecklists)
            {
                var getSignedChecklists = GetLocalSignedChecklistsAsync();
                var getAllActions = _actionService.GetActionsAsync();

                await Task.WhenAll(getSignedChecklists, getAllActions).ConfigureAwait(false);

                signedChecklists = getSignedChecklists.Result;
                actionsMap = getAllActions.Result?
                                          .Where(a => !string.IsNullOrEmpty(a.LocalId?.ToString()))
                                          .GroupBy(a => a.LocalId.ToString())
                                          .Select(g => g.First()) // or handle duplicates as needed
                                          .ToDictionary(a => a.LocalId.ToString(), a => a);


                DebugService.WriteLineWithTime("Getting local Checklists", _debugCat);

                var localChecklists = signedChecklists.Select(signed =>
                {
                    var tasks = signed.Tasks?.Select(task =>
                    {
                        task.CreatePropertyList();
                        task.SetPictureProofMediaItems();

                        return new TasksTaskModel
                        {
                            Name = task.Name,
                            Picture = task.Picture,
                            Status = task.FilterStatus.ToString(),
                            CommentCount = task.LocalComments?.Count ?? 0,
                            TemplateId = task.Id,
                            LocalComments = task.LocalComments,
                            LocalActions = task.LocalActions?
                                .Select(local => actionsMap?.TryGetValue(local.LocalId?.ToString(), out var match) == true ? match : local)
                                .ToList(),
                            ActionsCount = task.LocalActions?.Count ?? 0,
                            Properties = task.Properties,
                            PropertyUserValues = task.PropertyValues,
                            PictureProofMediaItems = task.PictureProofMediaItems ?? new List<MediaItem>(),
                            PropertyValuesString = task.PropertyValuesString,
                            HasPictureProof = (task.PictureProofMediaItems?.Any() ?? false),
                            Signature = task.Signature
                        };
                    }).ToList();

                    return new ChecklistModel
                    {
                        Name = signed.Name,
                        ModifiedAt = signed.Date.ToDateTimeUnspecified(),
                        IsSignatureRequired = signed.Signatures?.Any(s => !s.SignatureImage.IsNullOrEmpty()) ?? false,
                        IsDoubleSignatureRequired = signed.Signatures?.Count > 1,
                        OpenFieldsPropertyUserValues = signed.OpenFields?.ToList() ?? new(),
                        OpenFieldsProperties = signed.OpenFields?.Select(prop => new TemplatePropertyModel
                        {
                            Index = prop.Index,
                            Id = prop.TemplatePropertyId,
                            TitleDisplay = prop.Title,
                            ValueType = prop.ValueTypeEnum
                        }).ToList(),
                        LinkedTaskId = signed.LinkedTaskId,
                        IsRequiredForLinkedTask = signed.IsRequiredForLinkedTask,
                        Tasks = tasks,
                        TemplateId = signed.ChecklistTemplateId,
                        Signatures = signed.Signatures?.Select(sig => new SignatureModel
                        {
                            SignedById = sig.SignedById,
                            SignedBy = sig.SignedBy,
                            SignedAt = sig.SignedAt,
                            IsLocal = true,
                            SignatureImage = string.IsNullOrEmpty(sig.SignatureImage)
                                ? ""
                                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Constants.SignaturesDirectory, sig.SignatureImage)
                        }).ToList()
                    };
                }).ToList();

                result.AddRange(localChecklists);
            }

            var allTasks = result.SelectMany(c => c.Tasks ?? Enumerable.Empty<TasksTaskModel>()).ToList();

            DebugService.WriteLineWithTime("Assigning tasks", _debugCat);

            allTasks.ForEach(t =>
            {
                t.CreatePropertyList();
                t.SetPictureProofMediaItems(false);
            });

            DebugService.WriteLineWithTime("Creating properties", _debugCat);

            await _taskService?.LoadActionCountForTasksAsync(allTasks, refreshActions);

            DebugService.Stop(_debugCat);
            DebugService.WriteLineWithTime("Loading actions", _debugCat);

            return result.OrderByDescending(x => x.ModifiedAt).ToList();
        }

        public async Task<ChecklistModel> GetChecklistAsync(int? id, bool refresh = false, bool isFromSyncService = false)
        {
            if (!id.HasValue)
                return new ChecklistModel();

            List<string> parameters = new();
            var includes = $"include=signatures,tasks,propertyuservalues,properties,openfields,userinformation";

            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes += ",pictureproof";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                includes += ",tags";

            parameters.Add(includes);
            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            parameters.Add($"allowedonly={allowed}");

            // build url
            string action = $"checklist/{id}?";

            if (parameters.Any())
                action += parameters.Aggregate((a, b) => a + "&" + b);

            ChecklistModel apiChecklist = await _apiRequestHandler.HandleRequest<ChecklistModel>(action, refresh, isFromSyncService).ConfigureAwait(false);

            List<TasksTaskModel> tasks = new();

            if (!apiChecklist.Tasks.IsNullOrEmpty())
            {
                tasks.AddRange(apiChecklist.Tasks);
            }

            tasks.ForEach(x =>
            {
                x.CreatePropertyList();
                x.SetPictureProofMediaItems();
            });

            await _taskService.LoadActionCountForTasksAsync(tasks, refresh).ConfigureAwait(false);
            return apiChecklist;
        }



        public async Task<ChecklistTemplateModel> GetChecklistTemplateAsync(int id, bool refresh = false)
        {
            List<ChecklistTemplateModel> checklistTemplates = await GetChecklistTemplatesAsync(true, refresh, includeIncompleteChecklists: false).ConfigureAwait(false);

            ChecklistTemplateModel checklistTemplate = checklistTemplates.FirstOrDefault(item => item.Id == id);

            if (checklistTemplate == null)
                checklistTemplate = await GetChecklistTemplateWithTaskTemplatesAsync(id, refresh).ConfigureAwait(false);

            return checklistTemplate;
        }


        public async Task<ChecklistTemplateModel> GetChecklistTemplateWithTaskTemplatesAsync(int id, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"checklisttemplate/{id}?include=tasks,tasktemplates,openfields,steps,properties,propertyvalues,propertyuservalues,instructionrelations";

            if (CompanyFeatures.RequiredProof)
                uri += ",pictureproof";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri += ",tags";

            ChecklistTemplateModel checklistTemplateModel = await _apiRequestHandler.HandleRequest<ChecklistTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            await _instructionsService.SetWorkInstructionRelations(checklistTemplateModel.TaskTemplates).ConfigureAwait(false);

            checklistTemplateModel.TaskTemplates ??= new List<TaskTemplateModel>();
            foreach (var task in checklistTemplateModel.TaskTemplates)
            {
                if (!string.IsNullOrWhiteSpace(task.DescriptionFile))
                {
                    task.Attachments ??= new List<Attachment>();
                    task.Attachments.Add(new Attachment
                    {
                        Uri = task.DescriptionFile,
                        AttachmentType = "Pdf"
                    });
                }
            }

            return checklistTemplateModel;
        }

        public async Task<TaskTemplateModel> GetChecklistWithIncludes(int id, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"checklist/{id}?include=tasks,steps,properties,propertyvalues,propertyuservalues,instructionrelations&limit=0";

            TaskTemplateModel checklistTemplateModel = await _apiRequestHandler.HandleRequest<TaskTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return checklistTemplateModel;
        }

        public async Task<List<ChecklistTemplateModel>> GetChecklistTemplatesAsync(bool includeTaskTemplates, bool refresh = false, bool isFromSyncService = false, bool includeIncompleteChecklists = true)
        {
            string uri = $"checklisttemplates?areaid={Settings.AreaSettings.WorkAreaId}&filterareatype={(int)FilterAreaTypeEnum.RecursiveRootToLeaf}&limit=0";

            if (includeTaskTemplates)
                uri = $"{uri}&include=tasktemplates,steps,openfields,instructionrelations";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri += ",tags";

            if (CompanyFeatures.CompanyFeatSettings.ChecklistsPropertyValueRegistrationEnabled)
            {
                uri += ",properties";
            }

            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            uri = $"{uri}&allowedonly={allowed}";

            List<ChecklistTemplateModel> result = await _apiRequestHandler.HandleListRequest<ChecklistTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            //Refreshing work instructions for linking
            var instructions = await _instructionsService.GetInstructions(refresh, isFromSyncService).ConfigureAwait(false);

            foreach (var item in result)
            {
                //workaround because work relations from api can come with wrong permissions
                await _instructionsService.SetWorkInstructionRelations(item.TaskTemplates, instructions).ConfigureAwait(false);

                item.TaskTemplates ??= new List<TaskTemplateModel>();
                foreach (var task in item.TaskTemplates)
                {
                    if (!string.IsNullOrWhiteSpace(task.DescriptionFile))
                    {
                        task.Attachments ??= new List<Attachment>();
                        task.Attachments.Add(new Attachment
                        {
                            Uri = task.DescriptionFile,
                            AttachmentType = "Pdf"
                        });
                    }
                }

                //set HasIncompleteChecklists
                if (!CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                    item.HasIncompleteChecklists = false;
            }

            return result;
        }

        public Task PostAndSignTemplateAsync(PostTemplateModel model)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    SignedChecklistModel signedChecklistModel = new SignedChecklistModel(model.LocalGuid)
                    {
                        OpenFields = model.UserValues,
                        ChecklistTemplateId = model.TemplateId,
                        Date = DateTimeHelper.Now,
                        Name = model.TemplateName,
                        Tasks = model.Tasks,
                        Signatures = model.Signatures.Select(x => new SignatureModel(x, true)).ToList(),
                        StartedAt = model.StartedAt,
                        IsRequiredForLinkedTask = model.IsRequiredForLinkedTask,
                        LinkedTaskId = model.LinkedTaskId,
                        IsCompleted = model.IsCompleted,
                        Id = model.Id,
                        CreatedBy = model.CreatedBy,
                        CreatedById = model.CreatedById,
                        ModifiedBy = model.ModifiedBy,
                        ModifiedById = model.ModifiedById,
                        Stages = model.Stages,
                        Version = model.Version
                    };

                    if (InternetHelper.HasNetworkAccess)
                    {
                        if (signedChecklistModel.IsRequiredForLinkedTask.HasValue && signedChecklistModel.IsRequiredForLinkedTask == true && signedChecklistModel.IsCompleted)
                        {
                            // Log possible error - no id for linked task
                            if (!signedChecklistModel.LinkedTaskId.HasValue)
                            {
                                // Crashes.TrackError(new Exception("No linked task id for the checklist: "), new Dictionary<string, string>()
                                // {
                                //     { "signed checklist model: ", $"{JsonSerializer.Serialize(signedChecklistModel, ignoreNullValues: false)}" },
                                // });
                            }

                            // Add for now locally so that user doesn't have to wait for posting the checklist
                            await _taskService.SetMandatoryItemToTask(signedChecklistModel.LinkedTaskId, -1).ConfigureAwait(false);
                        }

                        if (signedChecklistModel.IsCompleted)
                        {
                            string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.checklistAdded);
                            _messageService?.SendClosableInfo(message);
                        }

                        await DeleteLocalChecklistTemplateAsync(model.TemplateId).ConfigureAwait(false);
                        await PostSignedChecklistModelAsyncNew(signedChecklistModel).ConfigureAwait(false);
                    }
                    else
                    {
                        string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.checklistAdded);
                        _messageService?.SendClosableInfo(message);

                        await itemsQueue.EnqueueItemAsync(signedChecklistModel).ConfigureAwait(false);

                        await DeleteLocalChecklistTemplateAsync(model.TemplateId).ConfigureAwait(false);

                        if (signedChecklistModel.IsRequiredForLinkedTask.HasValue && signedChecklistModel.IsRequiredForLinkedTask == true && signedChecklistModel.IsCompleted)
                        {
                            // Log possible error - no id for linked task
                            if (!signedChecklistModel.LinkedTaskId.HasValue)
                            {
                                // Crashes.TrackError(new Exception("No linked task id for the checklist: "), new Dictionary<string, string>()
                                // {
                                //     { "signed checklist model: ", $"{JsonSerializer.Serialize(signedChecklistModel, ignoreNullValues: false)}" },
                                // });
                            }

                            await _taskService.SetMandatoryItemToTask(signedChecklistModel.LinkedTaskId, -1).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task PostChecklistAsync(PostTemplateModel model)
        {
            List<Signature> signatures = new List<Signature> { new Signature { SignedAt = DateTime.UtcNow, SignedById = UserSettings.Id, SignedBy = UserSettings.Fullname } };
            model.Signatures = signatures;
            if (model.Id == 0)
            {
                model.CreatedBy = UserSettings.Fullname;
                model.CreatedById = UserSettings.Id;
                model.ModifiedBy = UserSettings.Fullname;
                model.ModifiedById = UserSettings.Id;
            }
            else
            {
                model.ModifiedBy = UserSettings.Fullname;
                model.ModifiedById = UserSettings.Id;
            }

            await PostAndSignTemplateAsync(model).ConfigureAwait(false);
        }


        private readonly SemaphoreSlim checklistsSemaphore = new SemaphoreSlim(1, 1);
        private readonly HashSet<Guid> processingItems = new();
        public async Task<int> UploadLocalSignedChecklistsAsync()
        {
            int count = 0;
            SignedChecklistModel item = null;
            List<SignedChecklistModel> failedItems = new List<SignedChecklistModel>();

            try
            {
                await checklistsSemaphore.WaitAsync().ConfigureAwait(false);

                while (itemsQueue.HasItems())
                {
                    item = itemsQueue.PeekItem();
                    if (item == null || processingItems.Contains(item.LocalGuid))
                        break;

                    if (item.ChecklistTemplateId == 0) // Broken checklist -> remove it from queue; found in logs in appcenter
                    {
                        await itemsQueue.DequeueItemAsync().ConfigureAwait(false);
                        break;
                    }

                    processingItems.Add(item.LocalGuid);

                    if (await InternetHelper.HasInternetConnection().ConfigureAwait(false) && item != null)
                    {
                        await itemsQueue.DequeueItemAsync().ConfigureAwait(false);

                        bool result = await PostSignedChecklistModelAsyncNew(item, false).ConfigureAwait(false);

                        if (result)
                        {
                            failedItems.Remove(item);
                            count++;
                        }
                        else
                            failedItems.Add(item);

                        processingItems.Remove(item.LocalGuid);
                    }
                    else
                    {
                        processingItems.Remove(item.LocalGuid);
                        break;
                    }
                }
            }
            catch
            {
                if (item != null && !failedItems.Contains(item))
                    failedItems.Add(item);
            }
            finally
            {
                if (failedItems.Count > 0)
                {
                    foreach (var failedItem in failedItems)
                    {
                        if (!itemsQueue.Contains(failedItem))
                            await itemsQueue.EnqueueItemAsync(failedItem).ConfigureAwait(false);
                    }
                }

                processingItems.Clear();

                if (checklistsSemaphore.CurrentCount == 0)
                    checklistsSemaphore.Release();
            }
            return count;
        }

        public async Task AddOrUpdateLocalTemplateAsync(LocalTemplateModel model)
        {
            List<LocalTemplateModel> result = await GetLocalChecklistTemplatesAsync().ConfigureAwait(false);
            int index = result.FindIndex(x => x.Id == model.Id && x.UserId == UserSettings.Id);
            if (index != -1)
            {
                model.StartedAt = result[index].StartedAt;
                result[index] = model;
            }
            else
            {
                result.Add(model);
            }

            string localChecklistTemplatesJson = JsonSerializer.Serialize(result);
            await AsyncAwaiter.AwaitAsync(nameof(ChecklistsService) + localChecklistTemplatesFilename, async () =>
            {
                await _fileService.SaveFileToInternalStorageAsync(localChecklistTemplatesJson, localChecklistTemplatesFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<bool> CheckIfLocalTemplateExistsAsync(int checklistTemplateId)
        {
            List<LocalTemplateModel> result = await GetLocalChecklistTemplatesAsync().ConfigureAwait(false);
            return result.Exists(t => t.Id == checklistTemplateId && t.UserId == UserSettings.Id);
        }

        private async Task DeleteLocalChecklistTemplateAsync(int id)
        {
            List<LocalTemplateModel> result = await GetLocalChecklistTemplatesAsync().ConfigureAwait(false);

            int index = result.FindIndex(x => x.Id == id && x.UserId == UserSettings.Id);
            if (index != -1) { result.RemoveAt(index); }

            string localChecklistTemplatesJson = JsonSerializer.Serialize(result);
            await AsyncAwaiter.AwaitAsync(nameof(ChecklistsService) + localChecklistTemplatesFilename, async () =>
            {
                await _fileService.SaveFileToInternalStorageAsync(localChecklistTemplatesJson, localChecklistTemplatesFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<List<LocalTemplateModel>> GetLocalChecklistTemplatesAsync()
        {
            List<LocalTemplateModel> result = new List<LocalTemplateModel>();

            return await AsyncAwaiter.AwaitResultAsync(nameof(ChecklistsService) + localChecklistTemplatesFilename, async () =>
            {
                string localChecklistTemplatesJson = await _fileService.ReadFromInternalStorageAsync(localChecklistTemplatesFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(localChecklistTemplatesJson))
                    result = JsonSerializer.Deserialize<List<LocalTemplateModel>>(localChecklistTemplatesJson) ?? new List<LocalTemplateModel>();

                return result;
            }).ConfigureAwait(false);
        }

        private async Task<bool> PostSignedChecklistModelAsyncNew(SignedChecklistModel signedChecklistModel, bool saveRequest = true)
        {
            return await AsyncAwaiter.AwaitResultAsync(nameof(PostSignedChecklistModelAsyncNew), async () =>
            {
                var model = new AddChecklistModel();
                bool result = false;
                try
                {
                    if (signedChecklistModel.Signatures.Any(x => !x.SignatureImage.IsNullOrEmpty()))
                    {
                        await _signatureService.UploadSignaturesAsync(signedChecklistModel.Signatures, MediaStorageTypeEnum.ChecklistSignatures, 0).ConfigureAwait(false);
                    }

                    if (signedChecklistModel.Tasks.Any(x => x.PictureProofMediaItems != null && x.PictureProofMediaItems.Any()))
                    {
                        var mediaItems = signedChecklistModel.Tasks.SelectMany(t => t.PictureProofMediaItems ?? Enumerable.Empty<MediaItem>());
                        foreach (var mediaItem in mediaItems)
                        {
                            if (mediaItem.IsLocalFile)
                                await _mediaService.UploadMediaItemAsync(mediaItem, MediaStorageTypeEnum.PictureProof, 0, true).ConfigureAwait(false);
                        }
                    }

                    if (signedChecklistModel.Stages != null)
                    {
                        var stageSignatures = signedChecklistModel.Stages
                            .Where(x => x.Signatures != null)
                            .SelectMany(x => x.Signatures)
                            .Where(x => !x.SignatureImage.IsNullOrEmpty())
                            .ToList();

                        if (stageSignatures.Count > 0)
                        {
                            await _signatureService.UploadSignaturesAsync(stageSignatures, MediaStorageTypeEnum.ChecklistSignatures, 0).ConfigureAwait(false);
                        }
                    }

                    model = new AddChecklistModel()
                    {
                        CompanyId = UserSettings.CompanyId,
                        IsCompleted = signedChecklistModel.IsCompleted,
                        Signatures = signedChecklistModel.Signatures.Select(x => x.ToSignature()).ToList(),
                        TemplateId = signedChecklistModel.ChecklistTemplateId,
                        OpenFieldsPropertyUserValues = signedChecklistModel.OpenFields?
    .Where(x => x.GetFieldValue() != null)
    .ToList(),
                        IsRequiredForLinkedTask = signedChecklistModel.IsRequiredForLinkedTask,
                        LinkedTaskId = signedChecklistModel.LinkedTaskId,
                        Tasks = signedChecklistModel.Tasks.Select(taskTemplate => new TasksTemplateTaskStatusModel()
                        {
                            Status = taskTemplate.FilterStatus.ToApiString(),
                            CompanyId = UserSettings.CompanyId,
                            TemplateId = taskTemplate.Id,
                            PropertyUserValues = taskTemplate.PropertyValues,
                            PictureProof = taskTemplate.PictureProofMediaItems?.Any() ?? false ? new PictureProof()
                            {
                                Media = taskTemplate.PictureProofMediaItems?.Select(pictureProof => new PictureProofMedia()
                                {
                                    ItemName = taskTemplate.Name,
                                    PictureTakenUtc = pictureProof.CreatedAt.ToDateTimeUnspecified().ToUniversalTime(),
                                    UriPart = pictureProof.PictureUrl,
                                    ThumbUriPart = pictureProof.PictureUrl,
                                    UserFullName = !pictureProof.UserFullName.IsNullOrEmpty() ? pictureProof.UserFullName : UserSettings.userSettingsPrefs.Fullname,
                                    UserId = pictureProof.UserId != 0 ? pictureProof.UserId : UserSettings.userSettingsPrefs.Id
                                }).ToList(),
                                ProofTakenByUserId = taskTemplate.PictureProofMediaItems.FirstOrDefault().UserId != 0 ? taskTemplate.PictureProofMediaItems.FirstOrDefault().UserId : UserSettings.userSettingsPrefs.Id,
                                ProofTakenUtc = DateTime.UtcNow
                            } : null,
                            Signature = taskTemplate.Signature,
                            Id = signedChecklistModel.Id > 0 ? taskTemplate.ItemId : 0,
                        }).ToList(),
                        Id = signedChecklistModel.Id > 0 ? signedChecklistModel.Id : 0,
                        CreatedBy = signedChecklistModel.CreatedBy,
                        CreatedById = signedChecklistModel.CreatedById,
                        ModifiedBy = signedChecklistModel.ModifiedBy,
                        ModifiedById = signedChecklistModel.ModifiedById,
                        Version = signedChecklistModel.Version
                    };

                    if (signedChecklistModel.Stages != null)
                    {
                        model.Stages = signedChecklistModel.Stages.Where(x => x.Id != -1).Select(stageTemplate => new StageAddModel()
                        {
                            CompanyId = UserSettings.CompanyId,
                            CreatedById = signedChecklistModel.Id > 0 ? signedChecklistModel.CreatedById : UserSettings.Id,
                            ModifiedById = signedChecklistModel.ModifiedById,
                            Signatures = stageTemplate.Signatures,
                            TaskTemplateIds = stageTemplate.TaskTemplateIds,
                            StageTemplateId = stageTemplate.Id,
                            Id = stageTemplate.StageId ?? 0,
                            Status = stageTemplate.IsSigned ? "done" : "todo",
                            ShiftNotes = stageTemplate.ShiftNotes,
                        }).ToList();
                    }


                    var url = $"checklist/add?fulloutput=true";

                    if (signedChecklistModel.Id > 0)
                        url = $"checklist/change/{signedChecklistModel.Id}?fulloutput=true";

                    HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(url, model, false, saveRequest).ConfigureAwait(false);

                    if (response != null &&
                    (response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.BadRequest))
                    {
                        var reason = await response.Content.ReadAsStringAsync();
                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ErrorSendingChecklist, reason); });
                        throw new HttpRequestException($"Error: {reason}");
                    }

                    result = response?.IsSuccessStatusCode ?? false;
                    if (result)
                    {
                        var signedChecklist = JsonSerializer.Deserialize<ChecklistModel>(await response.Content.ReadAsStringAsync());
                        signedChecklist.LocalGuid = signedChecklistModel.LocalGuid;
                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ChecklistAdded, signedChecklist); });

                        var tasksFromApi = signedChecklist.Tasks.ToList();
                        var allActions = await _actionService.GetActionsAsync(refresh: true).ConfigureAwait(false);

                        for (int i = 0; i < tasksFromApi.Count(); i++)
                        {
                            var taskFromApi = tasksFromApi[i];
                            var taskFromModel = signedChecklistModel.Tasks?.Where(x => x.Id == taskFromApi.TemplateId).FirstOrDefault();

                            var actions = await _actionService.GetOpenActionsForTaskTemplateAsync(taskFromApi.TemplateId, allActions).ConfigureAwait(false);
                            if (signedChecklistModel.StartedAt.HasValue)
                            {
                                var resolvedActions = await _actionService.GetResolvedActionsForTaskTemplateAsync(taskFromApi.TemplateId, signedChecklistModel.StartedAt, allActions).ConfigureAwait(false);
                                actions.AddRange(resolvedActions);
                            }

                            if (!taskFromModel?.LocalComments.IsNullOrEmpty() ?? false)
                            {
                                foreach (var comment in taskFromModel.LocalComments)
                                {
                                    if (comment.IsPosted)
                                        continue;

                                    comment.TaskId = (int)taskFromApi.Id;
                                    await _commentService.AddChecklistOrAuditCommentAsync(comment).ConfigureAwait(false);
                                    // Update cache
                                    await _commentService.ChangeLocalForChecklistAuditAsync(comment).ConfigureAwait(false);
                                }
                            }

                            if (actions != null)
                            {
                                actions = actions.Where(action => action.CreatedById == UserSettings.Id && !action.TaskId.HasValue).ToList();

                                if (actions.Any())
                                {
                                    foreach (ActionsModel action in actions)
                                    {
                                        string uri = $"action/settask/{action.Id}";

                                        await _apiRequestHandler.HandlePostRequest(uri, taskFromApi.Id).ConfigureAwait(false);
                                    }
                                }
                            }
                        }

                        result = true;
                    }

                    if (result)
                        await UpdateChecklistTemplatesCacheAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);

                    if (!result && !itemsQueue.Contains(signedChecklistModel))
                        await itemsQueue.EnqueueItemAsync(signedChecklistModel).ConfigureAwait(false);

                    return false;
                }
            }).ConfigureAwait(false);
        }

        private async Task UpdateChecklistTemplatesCacheAsync()
        {
            using var scope = App.Container.CreateScope();
            ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

            await syncService.LoadChecklistTemplatesAsync().ConfigureAwait(false);
        }

        public async Task<List<SignedChecklistModel>> GetLocalSignedChecklistsAsync()
        {
            List<SignedChecklistModel> result = new List<SignedChecklistModel>();

            string signedChecklistsJson = await itemsQueue.GetItemsFromFile();

            if (!string.IsNullOrWhiteSpace(signedChecklistsJson))
                result = JsonSerializer.Deserialize<List<SignedChecklistModel>>(signedChecklistsJson) ?? new List<SignedChecklistModel>();

            return result;
        }

        //TODO: possible remove after testing
        private async Task SaveLocalSignedChecklistsAsync(List<SignedChecklistModel> completedChecklists)
        {
            string signedChecklistsJson = JsonSerializer.Serialize(completedChecklists);

            await AsyncAwaiter.AwaitAsync(nameof(ChecklistsService) + signedChecklistsFilename, async () =>
            {
                await _fileService.SaveFileToInternalStorageAsync(signedChecklistsJson, signedChecklistsFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private static async Task UpdateCompletedChecklistsCacheAsync()
        {
            using var scope = App.Container.CreateScope();
            ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

            var actionsService = scope.ServiceProvider.GetService<IActionsService>();
            await actionsService.GetActionsAsync(refresh: true).ConfigureAwait(false);

            await syncService.LoadCompletedChecklistsAsync().ConfigureAwait(false);
            await syncService.LoadActionsAsync().ConfigureAwait(false);
        }

        public async Task<bool> CheckHasCompletedChecklists(bool refresh = false)
        {
            var areaId = Settings.WorkAreaId;
            string url = $"checklists?iscompleted=true&limit=10&areaid={areaId}";

            var result = await _apiRequestHandler.HandleListRequest<ChecklistModel>(url, refresh).ConfigureAwait(false);

            return result.Any();
        }

        public async Task<bool> DeleteIncompletedChecklist(ChecklistModel item)
        {
            bool result = false;

            if (item == null) return result;

            var uri = $"checklist/setactive/{item.Id}";

            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, false).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    bool.TryParse(json, out result);

                    await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ChecklistDeleted); });
                }
            }
            return result;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
            _actionService.Dispose();
            _instructionsService.Dispose();
        }
    }
}
