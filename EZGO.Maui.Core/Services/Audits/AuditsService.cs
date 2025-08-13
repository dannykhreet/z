using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Local;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using NodaTime;
using System.Diagnostics;
using itemsQueue = EZGO.Maui.Classes.QueueManager<EZGO.Maui.Core.Models.Audits.SignedAuditModel>;

namespace EZGO.Maui.Core.Services.Audits
{
    public class AuditsService : IAuditsService
    {
        private const string _cat = "[AuditsService]\n\t";
        private readonly IFileService _fileService;

        private readonly IActionsService _actionService;
        private readonly ISignatureService _signatureService;
        private readonly ITasksService _taskService;
        private readonly IPropertyService _propertyService;
        private readonly ITaskCommentService _commentService;
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IInstructionsService _instructionsService;
        private readonly IMessageService _messageService;
        private readonly IMediaService _mediaService;
        private readonly IRoleFunctionsWrapper _roleFunctionsWrapper;
        private readonly IInternetHelper _internetHelperWrapper;


        private const string signedAuditsFilename = "signedaudits.json";
        private const string localAuditTemplatesFilename = "localaudittemplates.json";

        public AuditsService(
            ISignatureService signatureService,
            IActionsService actionsService,
            ITasksService tasksService,
            IPropertyService propertyService,
            ITaskCommentService taskCommentService,
            IApiRequestHandler apiRequestHandler,
            IInstructionsService instructionsService,
            IMessageService messageService,
            IMediaService mediaService,
            IFileService fileService,
            IRoleFunctionsWrapper roleFunctionsWrapper,
            IInternetHelper internetHelperWrapper)
        {
            _fileService = fileService;
            _signatureService = signatureService;
            _actionService = actionsService;
            _taskService = tasksService;
            _propertyService = propertyService;
            _taskService = tasksService;
            _commentService = taskCommentService;
            _apiRequestHandler = apiRequestHandler;
            _instructionsService = instructionsService;
            _messageService = messageService;
            _mediaService = mediaService;
            _roleFunctionsWrapper = roleFunctionsWrapper;
            _internetHelperWrapper = internetHelperWrapper;
        }

        public async Task<List<AuditTemplateModel>> GetReportAuditTemplatesAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            string uri = string.Format("audittemplates?areaid={0}&filterareatype={1}&allowedonly={2}", Settings.AreaSettings.ReportWorkAreaId, (int)FilterAreaTypeEnum.RecursiveRootToLeaf, allowed);

            List<AuditTemplateModel> result = await _apiRequestHandler.HandleListRequest<AuditTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<AuditTemplateModel>> GetAuditTemplatesAsync(bool includeTaskTemplates, bool refresh = false, bool isFromSyncService = false)
        {
            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            string uri = string.Format("audittemplates?areaid={0}&filterareatype={1}&allowedonly={2}&include=steps,properties,openfields,instructionrelations", Settings.AreaSettings.WorkAreaId, (int)FilterAreaTypeEnum.RecursiveRootToLeaf, allowed);

            if (includeTaskTemplates)
                uri = $"{uri},tasktemplates";

            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                uri = $"{uri},pictureproof";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri += ",tags";

            List<AuditTemplateModel> result = await _apiRequestHandler.HandleListRequest<AuditTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            foreach (var item in result)
            {
                //workaround because work relations from api can come with wrong permissions
                await _instructionsService.SetWorkInstructionRelations(item.TaskTemplates).ConfigureAwait(false);

                if (item.TaskTemplates != null)
                {
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
                }
            }

            return result;
        }

        public async Task<AuditTemplateModel> GetAuditTemplateAsync(int id, bool refresh = false)
        {
            List<AuditTemplateModel> auditTemplates = await GetAuditTemplatesAsync(includeTaskTemplates: true, refresh: refresh).ConfigureAwait(false);

            auditTemplates = auditTemplates.Where(item => item.Id == id).ToList();

            if (auditTemplates.IsNullOrEmpty())
                return await GetAuditTemplateWithIncludesAsync(id, refresh: refresh).ConfigureAwait(false);

            return auditTemplates.FirstOrDefault();
        }


        public async Task<AuditTemplateModel> GetAuditTemplateWithIncludesAsync(int id, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = $"audittemplate/{id}?include=tasktemplates,steps,properties,openfields,instructionrelations";

            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                uri = $"{uri},pictureproof";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri += ",tags";

            AuditTemplateModel auditTemplateModel = await _apiRequestHandler.HandleRequest<AuditTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            //workaround because work relations from api can come with wrong permissions
            await _instructionsService.SetWorkInstructionRelations(auditTemplateModel?.TaskTemplates).ConfigureAwait(false);

            return auditTemplateModel;
        }

        public async Task<List<AuditsModel>> GetReportAuditsAsync(LocalDateTime? startTimeStamp, LocalDateTime? endTimeStamp, bool refresh = false, int limit = 0, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false, int auditTemplateId = 0)
        {
            // build parameters
            List<string> parameters = new List<string>();

            if (Settings.AreaSettings.ReportWorkAreaId != 0)
            {
                parameters.Add("areaid=" + Settings.AreaSettings.ReportWorkAreaId);
                parameters.Add("filterareatype=" + (int)FilterAreaTypeEnum.RecursiveRootToLeaf);
            }
            parameters.Add("iscomplete=true");
            parameters.Add($"limit={limit}");
            parameters.Add($"offset={offset}");
            //if (timeStamp.HasValue)
            //    parameters.Add($"timestamp={timeStamp.Value.ToString(Constants.ApiDateTimeFormat)}");
            if (startTimeStamp.HasValue)
                parameters.Add($"&starttimestamp={startTimeStamp.Value.ToString(Constants.ApiDateTimeFormat, null)}");
            if (endTimeStamp.HasValue)
                parameters.Add($"&endtimestamp={endTimeStamp.Value.ToString(Constants.ApiDateTimeFormat, null)}");
            if (auditTemplateId != 0)
                parameters.Add($"&templateid={auditTemplateId}");

            var includes = new List<string> { "signatures", "tasks", "propertyuservalues", "properties", "openfields" };
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes.Add("pictureproof");

            var include = includes.Aggregate((a, b) => a + ',' + b);

            parameters.Add($"include={include}");

            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            parameters.Add($"allowedonly={allowed}");

            string action = "audits?";

            if (parameters.Any())
                action += parameters.Aggregate((a, b) => a + "&" + b);

            List<AuditsModel> result = await _apiRequestHandler.HandleListRequest<AuditsModel>(action, refresh, isFromSyncService).ConfigureAwait(false);

            result.ForEach(x => x.Tasks = x.Tasks?.OrderBy(y => y.Index).ToList()); // TODO remove after this issue is fixed on the API side

            List<TasksTaskModel> tasks = new List<TasksTaskModel>();

            foreach (AuditsModel audit in result)
            {
                if (audit.Tasks != null && audit.Tasks.Any())
                {
                    tasks.AddRange(audit.Tasks);
                }
            }

            tasks.ForEach(x =>
            {
                x.CreatePropertyList();
                x.SetPictureProofMediaItems();
            });

            await _taskService.LoadActionCountForTasksAsync(tasks, refresh).ConfigureAwait(false);

            return result;
        }

        public async Task<List<AuditsModel>> GetAuditsAsync(bool isComplete = false, bool useAreaId = true, bool refresh = false, int limit = 0, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false)
        {
#if DEBUG
            var st = new Stopwatch();
            st.Start();
            Debug.WriteLine("Started loading Audits", _cat);
#endif
            // check workarea is present and or is changed
            int workAreaId = useAreaId ? Settings.AreaSettings.WorkAreaId : 0;

            // build parameters
            List<string> parameters = new List<string>();

            if (workAreaId != 0)
            {
                parameters.Add("areaid=" + workAreaId);
                parameters.Add("filterareatype=" + (int)FilterAreaTypeEnum.RecursiveRootToLeaf);
            }

            if (isComplete)
            {
                parameters.Add("iscomplete=" + isComplete.ToString().ToLower());
                parameters.Add($"limit={limit}");
                parameters.Add($"offset={offset}");
                if (timeStamp.HasValue)
                    parameters.Add($"timestamp={timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null)}");
            }

            var includes = new List<string> { "signatures", "tasks", "propertyuservalues", "properties", "openfields" };
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes.Add("pictureproof");

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                includes.Add("tags");

            var include = includes.Aggregate((a, b) => a + ',' + b);

            parameters.Add($"include={include}");

            // build url
            string action = "audits?";

            if (parameters.Any())
                action += parameters.Aggregate((a, b) => a + "&" + b);

            List<AuditsModel> result = await _apiRequestHandler.HandleListRequest<AuditsModel>(action, refresh, isFromSyncService).ConfigureAwait(false);

#if DEBUG
            Debug.WriteLine($"Retrieved Audits took: {st.ElapsedMilliseconds} ms", _cat);
            var lastElapsed = st.ElapsedMilliseconds;
#endif
            result.ForEach(x => x.Tasks = x.Tasks?.OrderBy(y => y.Index ?? 0).ToList()); // TODO remove after this issue is fixed on the API side
#if DEBUG
            Debug.WriteLine($"Setting Tasks Order took: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
            lastElapsed = st.ElapsedMilliseconds;
#endif
            if (isComplete)
            {
                List<SignedAuditModel> localSignedAudits = await GetLocalSignedAuditsAsync().ConfigureAwait(false);
#if DEBUG
                Debug.WriteLine($"Getting local SignedAudits took: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif
                var allActions = await _actionService.GetActionsAsync().ConfigureAwait(false);
                AuditsModel[] localAudits = new AuditsModel[localSignedAudits.Count()];

                for (int i = 0; i < localAudits.Count(); i++)
                {
                    var signedAuditModel = localSignedAudits[i];
                    var template = await GetAuditTemplateAsync(localSignedAudits[i].AuditTemplateId).ConfigureAwait(false);
                    AuditsModel auditModel = new AuditsModel
                    {
                        Name = signedAuditModel.Name,
                        ModifiedAt = signedAuditModel.Date.ToDateTimeUnspecified(),
                        IsSignatureRequired = !signedAuditModel.Signatures.Where(s => !s.SignatureImage.IsNullOrEmpty()).ToList().IsNullOrEmpty(),
                        IsDoubleSignatureRequired = signedAuditModel.Signatures != null && signedAuditModel.Signatures.Count() > 1,
                        MaxTaskScore = template?.MaxScore ?? 10,
                        MinTaskScore = template?.MinScore ?? 0,
                        ScoreType = template.ScoreType,
                        OpenFieldsPropertyUserValues = signedAuditModel.OpenFieldsValues?.ToList() ?? new List<UserValuesPropertyModel>(),
                        LinkedTaskId = signedAuditModel.LinkedTaskId,
                        IsRequiredForLinkedTask = signedAuditModel.IsRequiredForLinkedTask,
                        OpenFieldsProperties = signedAuditModel.OpenFieldsValues?.Select(property =>
                        {
                            var result = new TemplatePropertyModel()
                            {
                                Index = property.Index,
                                Id = property.TemplatePropertyId,
                                TitleDisplay = property.Title,
                                ValueType = property.ValueTypeEnum
                            };
                            return result;
                        }).ToList() ?? new List<TemplatePropertyModel>(),
                        Tasks = signedAuditModel.Tasks.Select(taskTemplate =>
                            {
                                var result = new TasksTaskModel()
                                {
                                    Name = taskTemplate.Name,
                                    Picture = taskTemplate.Picture,
                                    Status = taskTemplate.FilterStatus.ToString(),
                                    CommentCount = taskTemplate.LocalComments?.Count ?? 0,
                                    TemplateId = taskTemplate.Id,
                                    LocalComments = taskTemplate.LocalComments,
                                    LocalActions = taskTemplate.LocalActions,
                                    ActionsCount = taskTemplate.LocalActions?.Count ?? 0,
                                    Score = taskTemplate.Score,
                                    Signature = taskTemplate.Signature
                                };
                                taskTemplate.CreatePropertyList();
                                taskTemplate.SetPictureProofMediaItems();

                                if (result.LocalActions?.Any() ?? false)
                                    result.LocalActions = result.LocalActions.Select(action => allActions?.FirstOrDefault(a => a.LocalId == action.LocalId) ?? action).ToList();

                                result.PictureProofMediaItems = taskTemplate.PictureProofMediaItems ?? new List<MediaItem>();
                                result.PropertyValuesString = taskTemplate.PropertyValuesString;
                                result.HasPictureProof = result.PictureProofMediaItems.Any();
                                return result;
                            }).ToList(),
                        TemplateId = signedAuditModel.AuditTemplateId,
                        Signatures = signedAuditModel.Signatures.ToList(),

                        //Signatures = signedAuditModel.Signatures.Select(item => new SignatureModel
                        //{
                        //    SignedById = item.SignedById,
                        //    SignedBy = item.SignedBy,
                        //    SignatureImage = !item.SignatureImage.IsNullOrEmpty() ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Constants.SignaturesDirectory, item.SignatureImage ?? "") : "",
                        //    SignedAt = item.SignedAt.Value,
                        //    IsLocal = true
                        //}).ToList()
                    };

                    var maxScore = signedAuditModel.Tasks.Select(x => (x.Weight * template.MaxScore)).Sum();
                    int score = 0;
                    switch (template.ScoreType)
                    {
                        case "score":
                            score = (int)Math.Round(signedAuditModel.Tasks?.Select(x => (x.Weight * x.Score)).Sum() ?? 0);
                            break;
                        default:
                            score = (int)Math.Round(signedAuditModel.Tasks?.Where(x => x.FilterStatus == (TaskStatusEnum.Ok)).Select(x => (x.Weight * template.MaxScore)).Sum() ?? 0);
                            break;
                    }
                    if (maxScore > 0)
                    {
                        auditModel.TotalScore = (int)Math.Round((decimal)(100 * score) / maxScore);
                    }

                    localAudits[i] = auditModel;
                }
#if DEBUG
                Debug.WriteLine($"Loading Local Audits took: {st.ElapsedMilliseconds - lastElapsed}", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif

                result.AddRange(localAudits);
#if DEBUG
                Debug.WriteLine($"Adding Range took: {st.ElapsedMilliseconds - lastElapsed}", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif
                int nr = 0;
                var tasksC = result.Where(x => !x.Tasks.IsNullOrEmpty()).Select(x =>
                {
                    nr += x.Tasks.Count();
                    return x.Tasks;
                }).ToList();
#if DEBUG
                Debug.WriteLine($"Selected Tasks without nulls: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif

                TasksTaskModel[] tasks = new TasksTaskModel[nr];

                int index = 0;
                for (int i = 0; i < tasksC.Count(); i++)
                {
                    var auditTasks = tasksC.ElementAt(i);
                    for (int j = 0; j < auditTasks.Count(); j++)
                    {
                        tasks[index] = auditTasks[j];
                        tasks[index].CreatePropertyList();
                        tasks[index].SetPictureProofMediaItems();
                        index++;
                    }
                }
#if DEBUG
                Debug.WriteLine($"Finished method execution: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif
                await _taskService.LoadActionCountForTasksAsync(tasks, refresh).ConfigureAwait(false);
#if DEBUG
                st.Stop();
                Debug.WriteLine($"Loaded Action Count For Tasks took: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
#endif
            }

            return result;
        }

        public async Task<List<AuditsModel>> GetAuditAsync(int id, bool refresh = false, bool isFromSyncService = false)
        {
            List<string> parameters = new();

            var includes = new List<string> { "signatures", "tasks", "propertyuservalues", "properties", "openfields" };
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes.Add("pictureproof");

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                includes.Add("tags");

            var include = includes.Aggregate((a, b) => a + ',' + b);

            parameters.Add($"include={include}");

            // build url
            string action = $"audit/{id}?";

            if (parameters.Any())
                action += parameters.Aggregate((a, b) => a + "&" + b);

            AuditsModel apiAudit = await _apiRequestHandler.HandleRequest<AuditsModel>(action, refresh, isFromSyncService).ConfigureAwait(false);


            List<AuditsModel> result = new() { apiAudit };

            List<TasksTaskModel> tasks = new();

            foreach (AuditsModel audit in result)
            {
                if (!audit.Tasks.IsNullOrEmpty())
                {
                    tasks.AddRange(audit.Tasks);
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

        public Task PostAndSignTemplateAsync(PostTemplateModel model)
        {
            return Task.Factory.StartNew(async () =>
            {
                SignedAuditModel signedAuditModel = new SignedAuditModel
                {
                    AuditTemplateId = model.TemplateId,
                    Date = DateTimeHelper.Now,
                    Name = model.TemplateName,
                    Tasks = model.Tasks,
                    Signatures = model.Signatures.Select(x => new SignatureModel(x, true)).ToList(),
                    OpenFieldsValues = model.UserValues?.ToList(),
                    StartedAt = model.StartedAt,
                    IsRequiredForLinkedTask = model.IsRequiredForLinkedTask,
                    LinkedTaskId = model.LinkedTaskId,
                    IsCompleted = model.IsCompleted,
                    Id = model.Id,
                    CreatedBy = model.CreatedBy,
                    CreatedById = model.CreatedById,
                    ModifiedBy = model.ModifiedBy,
                    ModifiedById = model.ModifiedById,
                    Version = model.Version
                };

                if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    if (signedAuditModel.IsRequiredForLinkedTask.HasValue && signedAuditModel.IsRequiredForLinkedTask == true)
                    {
                        // Add for now locally so that user doesn't have to wait for posting the checklist                       
                        await _taskService.SetMandatoryItemToTask(signedAuditModel.LinkedTaskId, -1).ConfigureAwait(false);
                    }

                    string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.auditAdded);
                    _messageService?.SendClosableInfo(message);

                    await DeleteLocalAuditTemplateAsync(model.TemplateId).ConfigureAwait(false);
                    await PostSignedAuditModelNewAsync(signedAuditModel).ConfigureAwait(false);
                }
                else
                {
                    string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.auditAdded);
                    _messageService?.SendClosableInfo(message);

                    await itemsQueue.EnqueueItemAsync(signedAuditModel).ConfigureAwait(false);

                    await DeleteLocalAuditTemplateAsync(model.TemplateId).ConfigureAwait(false);

                    if (signedAuditModel.IsRequiredForLinkedTask.HasValue && signedAuditModel.IsRequiredForLinkedTask == true)
                    {
                        await _taskService.SetMandatoryItemToTask(signedAuditModel.LinkedTaskId, -1).ConfigureAwait(false);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task PostAuditAsync(PostTemplateModel model)
        {
            List<Signature> signatures = new List<Signature> { new Signature { SignedAt = DateTime.UtcNow, SignedById = UserSettings.Id, SignedBy = UserSettings.Fullname } };
            model.Signatures = signatures;
            if (model.Id == 0)
            {
                model.CreatedBy = UserSettings.Fullname;
                model.CreatedById = UserSettings.Id;
            }
            else
            {
                model.ModifiedBy = UserSettings.Fullname;
                model.ModifiedById = UserSettings.Id;
            }
            await PostAndSignTemplateAsync(model);
        }

        private readonly SemaphoreSlim auditsSemaphore = new SemaphoreSlim(1, 1);
        private readonly HashSet<Guid> processingItems = new();
        public async Task<int> UploadLocalSignedAuditsAsync()
        {
            int count = 0;
            SignedAuditModel item = null;
            List<SignedAuditModel> failedItems = new List<SignedAuditModel>();

            try
            {
                await auditsSemaphore.WaitAsync().ConfigureAwait(false);

                while (itemsQueue.HasItems())
                {
                    item = itemsQueue.PeekItem();
                    if (item == null || processingItems.Contains(item.LocalGuid))
                        break;

                    if (item.AuditTemplateId == 0) // Broken checklist -> remove it from queue; found in logs in appcenter
                    {
                        await itemsQueue.DequeueItemAsync().ConfigureAwait(false);
                        break;
                    }

                    if (item.CreatedById != UserSettings.userSettingsPrefs.Id)
                    {
                        await itemsQueue.DequeueItemAsync().ConfigureAwait(false);
                        failedItems.Add(item);
                    }

                    processingItems.Add(item.LocalGuid);

                    if (await InternetHelper.HasInternetConnection().ConfigureAwait(false) && item != null)
                    {
                        await itemsQueue.DequeueItemAsync().ConfigureAwait(false);

                        bool result = await PostSignedAuditModelNewAsync(item, false).ConfigureAwait(false);

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

                if (auditsSemaphore.CurrentCount == 0)
                    auditsSemaphore.Release();
            }
            return count;
        }

        public async Task AddOrUpdateLocalTemplateAsync(LocalTemplateModel model)
        {
            List<LocalTemplateModel> result = await GetLocalAuditTemplates().ConfigureAwait(false);
            int index = result.FindIndex(x => x.Id == model.Id && x.UserId == UserSettings.Id);
            if (index != -1) { result[index] = model; }
            else { result.Add(model); }

            string localAuditTemplatesJson = JsonSerializer.Serialize(result);

            await AsyncAwaiter.AwaitAsync(CreateKey(localAuditTemplatesFilename), async () =>
            {
                await _fileService.SaveFileToInternalStorageAsync(localAuditTemplatesJson, localAuditTemplatesFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<bool> CheckIfLocalTemplateExistsAsync(int checklistTemplateId)
        {
            List<LocalTemplateModel> result = await GetLocalAuditTemplates().ConfigureAwait(false);
            return result.Exists(t => t.Id == checklistTemplateId && t.UserId == UserSettings.Id);
        }

        public async Task<List<LocalTemplateModel>> GetLocalAuditTemplates()
        {
            List<LocalTemplateModel> result = new List<LocalTemplateModel>();

            return await AsyncAwaiter.AwaitResultAsync(CreateKey(localAuditTemplatesFilename), async () =>
            {
                string localAuditTemplatesJson = await _fileService.ReadFromInternalStorageAsync(localAuditTemplatesFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(localAuditTemplatesJson))
                    result = JsonSerializer.Deserialize<List<LocalTemplateModel>>(localAuditTemplatesJson) ?? new List<LocalTemplateModel>();

                return result;
            }).ConfigureAwait(false);
        }

        private async Task<bool> PostSignedAuditModelNewAsync(SignedAuditModel signedAuditModel, bool saveRequest = true)
        {
            return await AsyncAwaiter.AwaitResultAsync(nameof(PostSignedAuditModelNewAsync), async () =>
            {
                var model = new AddAuditModel();
                bool result = false;
                try
                {

                    if (signedAuditModel.Signatures.Any(x => !x.SignatureImage.IsNullOrEmpty()))
                    {
                        await _signatureService.UploadSignaturesAsync(signedAuditModel.Signatures, MediaStorageTypeEnum.AuditSignatures, 0).ConfigureAwait(false);
                    }

                    if (signedAuditModel.Tasks.Any(x => x.PictureProofMediaItems != null && x.PictureProofMediaItems.Any()))
                    {
                        var mediaItems = signedAuditModel.Tasks.SelectMany(t => t.PictureProofMediaItems ?? Enumerable.Empty<MediaItem>());
                        foreach (var mediaItem in mediaItems)
                        {
                            await _mediaService.UploadMediaItemAsync(mediaItem, MediaStorageTypeEnum.PictureProof, 0, true).ConfigureAwait(false);
                        }
                    }

                    AddAuditModel addChecklistModel = new AddAuditModel
                    {
                        TemplateId = signedAuditModel.AuditTemplateId,
                        CompanyId = UserSettings.userSettingsPrefs.CompanyId,
                        OpenFieldsPropertyUserValues = signedAuditModel.OpenFieldsValues?.Where(x => !x.GetFieldValue().IsNullOrEmpty()).ToList(),
                        IsCompleted = true,
                        Signatures = signedAuditModel.Signatures.Select(x => x.ToSignature()).ToList(),
                        IsRequiredForLinkedTask = signedAuditModel.IsRequiredForLinkedTask,
                        LinkedTaskId = signedAuditModel.LinkedTaskId,
                        Tasks = signedAuditModel.Tasks.Select(taskTemplate => new TasksTemplateAuditTaskStatusModel()
                        {
                            Status = taskTemplate.FilterStatus.ToApiString(),
                            CompanyId = UserSettings.userSettingsPrefs.CompanyId,
                            Score = taskTemplate.Score ?? 0,
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
                        }).ToList(),
                        CreatedBy = signedAuditModel.CreatedBy,
                        CreatedById = signedAuditModel.CreatedById,
                        ModifiedBy = signedAuditModel.ModifiedBy,
                        ModifiedById = signedAuditModel.ModifiedById,
                        Version = signedAuditModel.Version
                    };

                    HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest($"audit/add?fulloutput=true", addChecklistModel, true, saveRequest).ConfigureAwait(false);

                    if (response != null && response.StatusCode == System.Net.HttpStatusCode.NotFound) // Network error
                        throw new HttpRequestException(response.ReasonPhrase);

                    result = response?.IsSuccessStatusCode ?? false;

                    if (result)
                    {
                        var signedAudit = JsonSerializer.Deserialize<AuditsModel>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                        var tasksWithActions = signedAuditModel.Tasks.ToList();
                        var allActions = await _actionService.GetActionsAsync(refresh: true).ConfigureAwait(false);

                        for (int i = 0; i < tasksWithActions.Count(); i++)
                        {
                            var taskTemplate = tasksWithActions[i];
                            var task = signedAudit.Tasks.Where(x => x.TemplateId == taskTemplate.Id).FirstOrDefault();

                            var actions = await _actionService.GetOpenActionsForTaskTemplateAsync(taskTemplate.Id, allActions).ConfigureAwait(false);
                            if (signedAuditModel.StartedAt.HasValue)
                            {
                                var resolvedActions = await _actionService.GetResolvedActionsForTaskTemplateAsync(taskTemplate.Id, signedAuditModel.StartedAt, allActions).ConfigureAwait(false);
                                actions.AddRange(resolvedActions);
                            }

                            if (!taskTemplate.LocalComments.IsNullOrEmpty())
                            {
                                foreach (var comment in taskTemplate.LocalComments)
                                {
                                    comment.TaskId = (int)task.Id;
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

                                        await _apiRequestHandler.HandlePostRequest(uri, task.Id).ConfigureAwait(false);
                                    }
                                }
                            }
                        }

                        result = true;
                    }

                    if (result)
                    {
                        await UpdateAuditTemplatesAsync().ConfigureAwait(false);
                    }

                    return result;


                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);

                    if (!result && !itemsQueue.Contains(signedAuditModel))
                        await itemsQueue.EnqueueItemAsync(signedAuditModel).ConfigureAwait(false);

                    return false;
                }
            });
        }

        private async Task UpdateAuditTemplatesAsync()
        {
            using (var scope = App.Container.CreateScope())
            {
                ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

                await syncService.LoadAuditTemplatesAsync().ConfigureAwait(false);
            }
        }

        private async Task<List<SignedAuditModel>> GetLocalSignedAuditsAsync()
        {
            List<SignedAuditModel> result = new List<SignedAuditModel>();

            string signedAuditsJson = await itemsQueue.GetItemsFromFile().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(signedAuditsJson))
                result = JsonSerializer.Deserialize<List<SignedAuditModel>>(signedAuditsJson) ?? new List<SignedAuditModel>();

            return result;
        }

        private async Task SaveLocalSignedAuditsAsync(List<SignedAuditModel> completedAudits)
        {
            string signedAuditsJson = JsonSerializer.Serialize(completedAudits);

            await AsyncAwaiter.AwaitAsync(CreateKey(signedAuditsFilename), async () =>
             {
                 await _fileService.SaveFileToInternalStorageAsync(signedAuditsJson, signedAuditsFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
             });
        }

        private async Task DeleteLocalAuditTemplateAsync(int id)
        {
            List<LocalTemplateModel> result = await GetLocalAuditTemplates();

            int index = result.FindIndex(x => x.Id == id && x.UserId == UserSettings.Id);
            if (index != -1) { result.RemoveAt(index); }

            string localAuditTemplatesJson = JsonSerializer.Serialize(result);
            await AsyncAwaiter.AwaitAsync(localAuditTemplatesFilename, async () =>
             {
                 await _fileService.SaveFileToInternalStorageAsync(localAuditTemplatesJson, localAuditTemplatesFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
             });
        }

        private static async Task UpdateCompletedAuditsCacheAsync()
        {
            using (var scope = App.Container.CreateScope())
            {
                ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

                await syncService.LoadCompletedAuditsAsync().ConfigureAwait(false);
                await syncService.LoadActionsAsync().ConfigureAwait(false);
            }
        }

        private string CreateKey(string filename)
        {
            return $"{nameof(AuditsService)}{filename}";
        }

        public async Task<bool> CheckHasCompletedAudits(bool refresh, bool isFromSyncService = false)
        {
            var areaId = Settings.WorkAreaId;

            string uri = $"audits?iscompleted=true&limit=10&areaid={areaId}";

            var list = await _apiRequestHandler.HandleListRequest<AuditsModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return !list.IsNullOrEmpty();
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
            _actionService.Dispose();
            //_commentService
            //_propertyService
            //_signatureService.
            //_taskService.dis
            //fileService
            // Dispose all Services
        }


    }
}
