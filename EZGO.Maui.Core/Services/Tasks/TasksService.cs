using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Shifts;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Shifts;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using NodaTime;
using Syncfusion.Maui.DataSource.Extensions;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Tasks
{
    public class TasksService : ITasksService
    {
        private const string _cat = "[TasksService]\n\t";
        private readonly IApiRequestHandler _apiRequestHandler;

        private readonly IActionsService _actionService;
        private readonly IShiftService _shiftService;
        private readonly IFileService _fileService;
        private readonly ICachingService _cachingService;
        private readonly IPropertyService _propertiesService;
        private readonly IInstructionsService _instructionsService;
        private readonly ITaskTemplatesSerivce _taskTemplatesSerivce;
        private readonly IRoleFunctionsWrapper _roleFunctionsWrapper;
        private readonly IInternetHelper _internetHelperWrapper;

        private const string localTaskTimeModelsFilename = "localtasktimemodels.json";
        private const string localTaskStatusModelsFilename = "localtaskstatusmodels.json";
        private const string localTaskPictureProofFilename = "localtaskpictureproof.json";
        private const string tasksDirectoryName = "tasks";
        private const string multiskipTaskStatusFilename = "multiskiptaskstatus.json";

        public TasksService(
            IShiftService shiftService,
            IActionsService actionsService,
            IPropertyService propertyService,
            IApiRequestHandler apiRequestHandler,
            IInstructionsService instructionsService,
            ITaskTemplatesSerivce taskTemplatesSerivce,
            IFileService fileService,
            ICachingService cachingService,
            IRoleFunctionsWrapper roleFunctionsWrapper,
            IInternetHelper internetHelperWrapper)
        {
            _shiftService = shiftService;
            _actionService = actionsService;
            _propertiesService = propertyService;
            _apiRequestHandler = apiRequestHandler;
            _fileService = fileService;
            _cachingService = cachingService;
            _instructionsService = instructionsService;
            _taskTemplatesSerivce = taskTemplatesSerivce;
            _roleFunctionsWrapper = roleFunctionsWrapper;
            _internetHelperWrapper = internetHelperWrapper;
        }

        /// <summary>
        /// Get cached tasktemplate for action task display
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TaskTemplateModel> GetTaskTemplateAsync(int id, bool refresh = false, bool isFromSyncService = false)
        {

            var includes = new List<string> { "actions", "steps", "properties" };
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
                includes.Add("pictureproof");

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                includes.Add("tags");

            var include = includes.Aggregate((a, b) => a + ',' + b);

            string uri = $"tasktemplate/{id}?include={include}";

            TaskTemplateModel taskTemplate = await _apiRequestHandler.HandleRequest<TaskTemplateModel>(uri, refresh, isFromSyncService);

            return taskTemplate;
        }

        public async Task<List<BasicTaskModel>> GetTasksWithActionsAsync(string uri, bool refresh = false, bool isFromSyncService = false)
        {
            if (uri.IsNullOrEmpty())
                uri = Constants.TasksActionsUrl;

            List<BasicTaskModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(uri, refresh, isFromSyncService);

            return result;
        }

        public async Task<List<TaskTemplateModel>> GetTaskTemplatesWithActionsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            const string uri = "tasktemplatesactions";

            List<TaskTemplateModel> result = await _apiRequestHandler.HandleListRequest<TaskTemplateModel>(uri, refresh, isFromSyncService);

            return result;
        }

        /// <summary>
        /// Get cached task for action task display
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BasicTaskModel> GetTaskAsync(long id, string type, bool refresh = false)
        {
            List<BasicTaskModel> tasks = new List<BasicTaskModel>();
            string url = Constants.TasksActionsUrl;

            switch (type)
            {
                case "checklist":
                    url = Constants.TaskChecklistActionsUrl;
                    tasks = await GetTasksWithActionsAsync(url, refresh: refresh);
                    break;
                case "audit":
                    url = Constants.TaskAuditActionsUrl;
                    tasks = await GetTasksWithActionsAsync(url, refresh: refresh);
                    break;
                case "task":
                    tasks = await GetTasksWithActionsAsync(url, refresh: refresh);
                    break;
                default:
                    break;
            }

            tasks = tasks.Where(item => item.Id == id).ToList();

            if (tasks.IsNullOrEmpty())
            {
                var uri = "task/" + id.ToString();

                BasicTaskModel result = await _apiRequestHandler.HandleRequest<BasicTaskModel>(uri, refresh);

                return result;
            }

            return tasks.FirstOrDefault();
        }

        /// <summary>
        /// Get cached taskstatusses for syncing
        /// </summary>
        /// <param name="nowTimestamp"></param>
        /// <returns></returns>
        public async Task<List<BasicTaskStatusModel>> GetTaskStatusses(LocalDateTime? nowTimestamp, bool isFromSyncService = false)
        {
            nowTimestamp ??= Settings.AppSettings.TasksStatussesTimestamp;
            string timestamp = nowTimestamp.Value.ToString(Constants.ApiDateTimeFormat, null);

            string uri = $"tasks/statusses?timestamp={timestamp}&areaid={Settings.AreaSettings.WorkAreaId}";
            List<BasicTaskStatusModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskStatusModel>(uri, isFromSyncService: isFromSyncService);

            return result;
        }

        public async Task<List<TaskExtendedDataBasic>> GetTaskExtendedData(LocalDateTime? nowTimestamp, bool isFromSyncService = false)
        {
            nowTimestamp ??= Settings.AppSettings.TasksExtendedDataTimestamp;
            string timestamp = nowTimestamp.Value.ToString(Constants.ApiDateTimeFormat, null);

            string uri = $"tasks/extendeddata?timestamp={timestamp}&areaid={Settings.AreaSettings.WorkAreaId}";
            var result = await _apiRequestHandler.HandleListRequest<TaskExtendedDataBasic>(uri, isFromSyncService: isFromSyncService);

            return result;
        }

        /// <summary>
        /// Get first tappings for tasks (to prevent undo by other user(id))
        /// </summary>
        /// <param name="tasksTimestamp"></param>
        /// <returns></returns>
        public async Task GetTaskHistoryFirsts(IEnumerable<BasicTaskModel> tasks = null, LocalDateTime? tasksTimestamp = null, bool refresh = false, bool isFromSyncService = false)
        {
            tasksTimestamp ??= Settings.AppSettings.TasksTimestamp;
            var timestamp = tasksTimestamp.Value.ToString(Constants.ApiDateTimeFormat, null);

            string uri = $"tasks/historyfirsts?timestamp={timestamp}";
            var result = await _apiRequestHandler.HandleListRequest<BasicTaskStatusModel>(uri, refresh, isFromSyncService);

            if (tasks != null)
            {
                if (result.Any())
                {
                    foreach (BasicTaskStatusModel status in result)
                    {
                        IEnumerable<BasicTaskModel> tasksWithHistory = tasks.Where(item => item.Id == status.TaskId).ToList();
                        foreach (BasicTaskModel basicTaskModel in tasksWithHistory)
                        {
                            basicTaskModel.OriginalSignature = new Models.SignatureModel
                            {
                                SignedAt = status.SignedAt,
                                SignedById = status.SignedById,
                                SignedBy = status.SignedBy,
                                Status = status.Status
                            };
                        }
                    }
                }
            }
        }

        private string GetTasksCreateRequestUrl(string uri, LocalDateTime? tasksTimestamp = null)
        {
            tasksTimestamp ??= Settings.AppSettings.TasksTimestamp;

            int workAreaId = Settings.AreaSettings.WorkAreaId;

            // build parameters
            List<string> parameters = new List<string>();

            if (workAreaId != 0)
            {
                parameters.Add("areaid=" + workAreaId);
                parameters.Add("filterareatype=" + (int)FilterAreaTypeEnum.RecursiveRootToLeaf);
            }

            parameters.Add("timestamp=" + tasksTimestamp.Value.ToString(Constants.ApiDateTimeFormat, null));

            parameters.Add("include=" + GetIncludes());

            parameters.Add("limit=0");

            parameters.Add("allowedonly=" + _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower());

            if (parameters.Any())
                uri = $"{uri}?{parameters.Aggregate((a, b) => a + "&" + b)}";

            return uri;
        }

        private string GetIncludes()
        {
            List<string> includes = new List<string> { "steps", "areapaths", "instructionrelations", "userinformation" };
            if (CompanyFeatures.CompanyFeatSettings.TasksPropertyValueRegistrationEnabled)
            {
                includes.Add("properties");
                includes.Add("propertyuservalues");
            }
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof)
            {
                includes.Add("pictureproof");
            }
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
            {
                includes.Add("tags");
            }
            return includes.Aggregate((a, b) => a + ',' + b);
        }

        private async Task<List<BasicTaskModel>> GetTasksAsync(string uri, LocalDateTime? tasksTimestamp, bool loadActionCount, bool refresh = false, bool isFromSyncService = false)
        {
            var requestUrl = GetTasksCreateRequestUrl(uri, tasksTimestamp);

            List<BasicTaskModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(requestUrl, refresh, isFromSyncService);

            if (tasksTimestamp.HasValue || refresh) Settings.AppSettings.TasksTimestamp = tasksTimestamp ?? LocalDateTime.FromDateTime(DateTime.Now);

            if (tasksTimestamp.HasValue && result == null)
            {
                // fallback to latest timestamp
                requestUrl = GetTasksCreateRequestUrl(uri);
                result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(requestUrl, refresh, isFromSyncService);
            }
            result ??= new List<BasicTaskModel>();

            if (loadActionCount)
            {
                await GetTaskHistoryFirsts(result, refresh: refresh, isFromSyncService: isFromSyncService);
                await LoadOpenActionCountForTasksAsync(result);
            }

            await LoadLocalInformationForTasksAsync(result);

            //workaround because work relations from api can come with wrong permissions
            await _instructionsService.SetWorkInstructionRelations(result);

            foreach (var task in result)
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

            return result;
        }

        public async Task<List<BasicTaskModel>> GetTasksAsync(LocalDateTime? tasksTimestamp = null, bool loadActionCount = true, bool refresh = false, bool isFromSyncService = false)
        {
            List<BasicTaskModel> tasks = await GetTasksAsync("tasks", tasksTimestamp, loadActionCount, refresh: refresh, isFromSyncService: isFromSyncService);

            return tasks;
        }

        public async Task<List<BasicTaskModel>> GetOverdueTasksAsync(LocalDateTime? tasksTimestamp = null, bool loadActionCount = true, bool refresh = false, bool isFromSyncService = false)
        {
            List<BasicTaskModel> tasks = await GetTasksAsync(uri: "tasksoverdue", tasksTimestamp: tasksTimestamp, loadActionCount: loadActionCount, refresh: refresh, isFromSyncService: isFromSyncService);
            if (tasks.Any())
            {
                tasks.ForEach(x => x.IsOverdue = true);
            }
            return tasks;
        }

        public async Task<List<BasicTaskModel>> GetTasksForPeriodAsync(TaskPeriod period, LocalDateTime? tasksTimestamp = null, bool refresh = false, bool includeProperties = false)
        {
            List<BasicTaskModel> tasks;
            ShiftModel currentShift = await _shiftService.GetCurrentShiftAsync(refresh: refresh);

            // We always need a timestamp
            // It is set to the current tasks timestamp but it will be correct below if needed
            tasksTimestamp ??= Settings.TasksTimestamp;

            // Correct it if needed
            tasksTimestamp = TasksTimeStampHelper.CorrectTasksTimeStamp(tasksTimestamp.Value, currentShift);

            if (period == TaskPeriod.OverDue)
                tasks = await GetOverdueTasksAsync(tasksTimestamp: tasksTimestamp, refresh: refresh);
            else
            {
                tasks = await GetTasksAsync(loadActionCount: false, tasksTimestamp: tasksTimestamp, refresh: refresh);
                if (tasks.Any())
                {
                    switch (period)
                    {
                        case TaskPeriod.Today:
                            tasks = tasks.Where(item =>
                                item.ShiftId.HasValue && item.ShiftId.Value == currentShift?.Id ||
                                (TaskFilterOptions.HasTodayRecurrency(item))
                               && (item.StartDate.Value.Date == tasksTimestamp.Value.ToDateTimeUnspecified().Date && item.DueAt.Value.Date >= tasksTimestamp.Value.ToDateTimeUnspecified().Date)).ToList();
                            break;
                        case TaskPeriod.Shift:
                            tasks = tasks.Where(item => item.ShiftId.HasValue && item.ShiftId.Value == currentShift?.Id).ToList();
                            break;
                        case TaskPeriod.Week:
                            tasks = tasks.Where(item =>
                                TaskFilterOptions.HasWeekRecurrency(item)).ToList();//.OrderBy(item => item.DueAt).ToList();
                            break;
                    }

                    if (includeProperties)
                    {
                        await LoadOpenActionCountForTasksAsync(tasks, refresh: refresh);

                        await LoadLocalInformationForTasksAsync(tasks);

                        await GetTaskHistoryFirsts(tasks, refresh: refresh);

                        await _propertiesService.LoadTaskPropertiesAsync(tasks, includeProperties, refresh);
                    }
                }
            }

            return tasks;
        }

        public async Task<List<BasicTaskModel>> GetTasksForShiftAsync(LocalDateTime? timeStamp, bool refresh = false, bool isFromSyncService = false, bool filterTasksFromTheFuture = true)
        {
            timeStamp ??= DateTimeHelper.Now.AddDays(-1);
            string timestamp = timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null);
            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();

            var include = GetIncludes();

            string uri = $"tasks/shift?timestamp={timestamp}&areaid={Settings.WorkAreaId}&filterareatype=1&include={include}&limit=0&allowedonly={allowed}";

            List<BasicTaskModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(uri, refresh, isFromSyncService);

            if (filterTasksFromTheFuture)
                result = result.Where(x => x.StartDate.HasValue && x.StartDate.Value <= DateTime.Today).ToList();

            await _propertiesService.LoadTaskPropertiesAsync(result, true, refresh);
            result.ForEach(x => x.SetPictureProofMediaItems());
            await _instructionsService.SetWorkInstructionRelations(result);

            return result;
        }

        public async Task<List<BasicTaskModel>> GetTasksForYesterday(LocalDateTime? timeStamp, bool refresh = false, bool isFromSyncService = false, bool filterTasksFromTheFuture = true)
        {
            timeStamp ??= DateTimeHelper.Now.AddDays(-1);
            string timestamp = timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null);
            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();


            var include = GetIncludes();

            string uri = $"tasks/yesterday?timestamp={timestamp}&areaid={Settings.WorkAreaId}&include={include}&limit=0&allowedonly={allowed}";

            List<BasicTaskModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(uri, refresh, isFromSyncService);

            if (filterTasksFromTheFuture)
                result = result.Where(x => x.StartDate.HasValue && x.StartDate.Value <= DateTime.Today).ToList();

            await _propertiesService.LoadTaskPropertiesAsync(result, true, refresh);
            result.ForEach(x => x.SetPictureProofMediaItems());
            await _instructionsService.SetWorkInstructionRelations(result);

            return result;
        }

        public async Task<List<BasicTaskModel>> GetTasksForLastWeek(LocalDateTime? timeStamp, bool refresh = false, bool isFromSyncService = false, bool filterTasksFromTheFuture = true)
        {
            timeStamp ??= DateTimeHelper.Now.AddDays(-1);
            string timestamp = timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null);
            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();

            string uri = $"tasks/lastweek?timestamp={timestamp}&areaid={Settings.WorkAreaId}&filterareatype=1&include={GetIncludes()}&limit=0&allowedonly={allowed}";

            List<BasicTaskModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(uri, refresh, isFromSyncService);

            if (filterTasksFromTheFuture)
                result = result.Where(x => x.StartDate.HasValue && x.StartDate.Value <= DateTime.Today).ToList();

            await _propertiesService.LoadTaskPropertiesAsync(result, true, refresh);
            result.ForEach(x => x.SetPictureProofMediaItems());
            await _instructionsService.SetWorkInstructionRelations(result);

            return result;
        }

        public async Task SetTaskStatusAsync(TaskStatusEnum status, BasicTaskModel modifiedTask, bool postTaskStatus = true)
        {
            await AsyncAwaiter.AwaitAsync(nameof(TasksService) + nameof(SetTaskStatusAsync), async () =>
            {
                LocalTaskStatusModel statusModel = new LocalTaskStatusModel
                {
                    TaskId = modifiedTask.Id,
                    Status = (int)status,
                    SignedById = modifiedTask.Signature?.SignedById ?? UserSettings.Id,
                    SignedAtUtc = modifiedTask.Signature?.SignedAt ?? DateTime.UtcNow,
                };

                // If the status is posted we set the Posted = true, the local status is then saved and lives until the sync is performed, (if Posted == true) it will not be re-posted
                // but will be counted in the results.
                //if (await InternetHelper.HasInternetConnection())
                //{
                if (postTaskStatus)
                {
                    await PostTaskStatusAsync(statusModel);
                    statusModel.Posted = true;
                }

                if (!await _internetHelperWrapper.HasInternetConnection())
                    await AddLocalTaskStatusModelAsync(statusModel);
                //}
                //else
                //    await AddLocalTaskStatusModelAsync(statusModel);

            }
            );
        }

        public async Task AlterTaskCacheDataAsync(BasicTaskModel modifiedTask)
        {
            static void alteringFunction(BasicTaskModel task, BasicTaskModel modifiedTask)
            {
                task.FilterStatus = modifiedTask.FilterStatus;
                task.Status = modifiedTask.Status;
                task.Signature = modifiedTask.Signature;
                task.OriginalSignature = modifiedTask.OriginalSignature;
                task.TimeTaken = modifiedTask.TimeTaken;
                task.TimeRealizedBy = modifiedTask.TimeRealizedBy;
                task.PropertyUserValues = modifiedTask.PropertyUserValues;
                task.Comment = modifiedTask.Comment;
                task.CompletedDeeplinkId = modifiedTask.CompletedDeeplinkId;
            }

            // Alter taskoverdue cache
            var uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: GetTasksCreateRequestUrl("tasksoverdue")).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<BasicTaskModel>(uri, (task) => alteringFunction(task, modifiedTask), (task) => task.Id == modifiedTask.Id);

            // Alter other tasks cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: GetTasksCreateRequestUrl("tasks")).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<BasicTaskModel>(uri, (task) => alteringFunction(task, modifiedTask), (task) => task.Id == modifiedTask.Id);
        }

        public async Task SetTaskRealizedTimeAsync(long taskId, int realizedTime)
        {
            TaskTimeModel taskTimeModel = new TaskTimeModel
            {
                TaskId = taskId,
                CompanyId = UserSettings.CompanyId,
                RealizedById = UserSettings.Id,
                RealizedTime = realizedTime
            };

            await PostTaskRealizedTimeAsync(taskTimeModel);
        }

        public async Task LoadOpenActionCountForTaskTemplatesAsync(IEnumerable<BasicTaskTemplateModel> taskTemplates, bool refresh = false)
        {
            if (taskTemplates == null || !taskTemplates.Any())
                return;

            taskTemplates.ForEach(taskTemplate =>
            {
                taskTemplate.OpenActionCount = 0;
            });

            List<ActionsModel> actions = await _actionService.GetActionsAsync(withIncludes: false, applySort: false, includeLocalActions: true, refresh: refresh).ConfigureAwait(false);

            if (actions.Where(x => x.IsResolved.HasValue && !x.IsResolved.Value).Any())
            {
                IEnumerable<(int taskTemplateId, int openActionCount)> taskTemplateIds = actions.Where(item => item.TaskTemplateId.HasValue && item.IsResolved.HasValue && !item.IsResolved.Value).GroupBy(item => item.TaskTemplateId.Value)
                    .Select(group => (taskTemplateId: group.Key, openActionCount: group.Count()));

                foreach ((int taskTemplateId, int openActionCount) in taskTemplateIds)
                {
                    IEnumerable<BasicTaskTemplateModel> taskTemplatesWithAction = taskTemplates.Where(item => item.Id == taskTemplateId);

                    foreach (BasicTaskTemplateModel basicTaskModel in taskTemplatesWithAction)
                    {
                        basicTaskModel.OpenActionCount = openActionCount;
                        basicTaskModel.UpdateActionBubbleCount();
                    }
                }
            }
        }

        public async Task LoadActionCountForTasksAsync(IEnumerable<TasksTaskModel> tasks, bool refresh = false)
        {
#if DEBUG
            var st = Stopwatch.StartNew();
            Debug.WriteLine("Started loading Action Count", _cat);
#endif

            var taskList = tasks?.ToList();
            if (taskList == null || taskList.Count == 0)
                return;

            var actions = await _actionService
                .GetActionsAsync(withIncludes: false, applySort: false, refresh: refresh)
                .ConfigureAwait(false);

#if DEBUG
            Debug.WriteLine($"Loading actions took: {st.ElapsedMilliseconds} ms", _cat);
            var lastElapsed = st.ElapsedMilliseconds;
#endif

            if (actions == null || actions.Count == 0)
                return;

            // Build lookup tables for faster access
            var taskMap = taskList.ToDictionary(t => t.Id);
            var groupedActions = actions
                .Where(a => a.TaskId.HasValue)
                .GroupBy(a => a.TaskId.Value);

            foreach (var group in groupedActions)
            {
                if (taskMap.TryGetValue(group.Key, out var taskModel))
                {
                    var actionList = group.ToList();
                    taskModel.ActionsCount = actionList.Count;
                    taskModel.Actions = actionList;
                }
            }

#if DEBUG
            st.Stop();
            Debug.WriteLine($"Setting count took: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
#endif
        }


        public async Task<List<BasicTaskModel>> GetTasksForShiftAsync(LocalDateTime tasksTimestamp, int shiftId, bool refresh = false, bool isFromSyncService = false)
        {
            int workAreaId = Settings.WorkAreaId;
            var uri = "tasks";

            // build parameters
            List<string> parameters = new List<string>();

            if (workAreaId != 0)
            {
                parameters.Add("areaid=" + workAreaId);
                parameters.Add("filterareatype=" + (int)FilterAreaTypeEnum.RecursiveRootToLeaf);
            }

            parameters.Add("timestamp=" + tasksTimestamp.ToString(Constants.ApiDateTimeFormat, null));

            var includes = new List<string> { "steps" };
            if (CompanyFeatures.RequiredProof)
                includes.Add("pictureproof");

            var include = includes.Aggregate((a, b) => a + ',' + b);

            parameters.Add($"include={include}");

            parameters.Add("limit=0");
            parameters.Add($"shiftid={shiftId}");

            if (parameters.Any())
                uri = $"{uri}?{parameters.Aggregate((a, b) => a + "&" + b)}";

            var result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(uri, refresh, isFromSyncService);

            return result;
        }

        public async Task<List<BasicTaskModel>> GetPreviousByRangeAsync(LocalDateTime from, LocalDateTime to, bool refresh = false, bool isFromSyncService = false)
        {
            int workAreaId = Settings.WorkAreaId;

            // build parameters
            List<string> parameters = new List<string>();

            if (workAreaId != 0)
            {
                parameters.Add("areaid=" + workAreaId);
                parameters.Add("filterareatype=" + (int)FilterAreaTypeEnum.RecursiveRootToLeaf);
            }

            parameters.Add("starttimestamp=" + from.ToString(Constants.ApiDateTimeFormat, null));
            parameters.Add("endtimestamp=" + to.ToString(Constants.ApiDateTimeFormat, null));

            var include = GetIncludes();

            parameters.Add($"include={include}");

            parameters.Add("limit=0");

            var uri = "tasks";

            if (parameters.Any())
                uri = $"{uri}?{parameters.Aggregate((a, b) => a + "&" + b)}";

            List<BasicTaskModel> result = await _apiRequestHandler.HandleListRequest<BasicTaskModel>(uri, refresh, isFromSyncService);
            await _instructionsService.SetWorkInstructionRelations(result);

            return result;
        }

        public async Task LoadOpenActionCountForTasksAsync(IEnumerable<BasicTaskModel> tasks, bool refresh = false)
        {
            if (tasks == null || tasks.Count() <= 0)
                return;

            List<ActionsModel> actions = await _actionService.GetActionsAsync(withIncludes: false, applySort: false, refresh: refresh);

            IEnumerable<(int taskTemplateId, int openActionCount)> taskTemplateIds = actions
                .Where(x => x.IsResolved.HasValue && !x.IsResolved.Value)
                .Where(item => item.TaskTemplateId.HasValue && item.IsResolved.HasValue && !item.IsResolved.Value)
                .GroupBy(item => item.TaskTemplateId.Value)
                .Select(group => (taskTemplateId: group.Key, openActionCount: group.Count()))
                .ToList();

            foreach (var task in tasks)
            {
                // FirstOrDefault on a tuple will return a tuple with default values of int, which is (0,0)
                var (taskTemplateId, openActionCount) = taskTemplateIds.Where(x => x.taskTemplateId == task.TemplateId).FirstOrDefault();
                task.OpenActionCount = openActionCount;
            }
        }

        public async Task LoadOpenActionCountForTaskAsync(BasicTaskModel task, bool refresh = false)
        {
            if (task == null)
                return;

            List<ActionsModel> actions = await _actionService.GetActionsAsync(withIncludes: false, applySort: false, refresh: refresh, tasktemplateId: task.TemplateId);

            if (actions != null)
                task.OpenActionCount = actions.Count;
        }

        private async Task LoadLocalInformationForTasksAsync(IEnumerable<BasicTaskModel> tasks)
        {
            List<TaskTimeModel> taskTimeModels = await GetLocalTaskTimeModelsAsync() ?? new List<TaskTimeModel>();

            taskTimeModels ??= new List<TaskTimeModel>();

            if (taskTimeModels.Any())
            {
                foreach (TaskTimeModel taskTimeModel in taskTimeModels)
                {
                    BasicTaskModel task = tasks?.SingleOrDefault(item => item.Id == taskTimeModel.TaskId);

                    if (task != null)
                        task.TimeTaken = taskTimeModel.RealizedTime;
                }
            }

            List<LocalTaskStatusModel> taskStatusModels = await GetLocalTaskStatusModelsAsync() ?? new List<LocalTaskStatusModel>();

            taskStatusModels ??= new List<LocalTaskStatusModel>();

            if (taskStatusModels.Any())
            {
                foreach (LocalTaskStatusModel statusModel in taskStatusModels)
                {
                    BasicTaskModel task = tasks?.SingleOrDefault(item => item.Id == statusModel.TaskId);

                    if (task != null)
                    {
                        task.FilterStatus = (TaskStatusEnum)statusModel.Status;
                        task.Status = ((TaskStatusEnum)statusModel.Status).ToString().ToLower();
                    }
                }
            }
        }


        #region localTaskTimeModel

        List<TaskTimeModel> taskTimeModels;

        public async Task UploadLocalTaskTimeModelsAsync()
        {
            await AsyncAwaiter.AwaitAsync(nameof(UploadLocalTaskTimeModelsAsync), async () =>
            {
                // Load current cached models before inserting the new one
                taskTimeModels = await GetLocalTaskTimeModelsAsync();

                // Process and remove items one by one to keep the list size small
                while (taskTimeModels.Count > 0)
                {
                    var taskTimeModel = taskTimeModels[0];

                    await PostTaskRealizedTimeAsync(taskTimeModel);

                    taskTimeModels.RemoveAt(0);

                    await SaveLocalTaskTimeModelsAsync(taskTimeModels);
                }
            });
        }

        private async Task PostTaskRealizedTimeAsync(TaskTimeModel taskTimeModel)
        {
            string action = $"task/settimerealized/{taskTimeModel.TaskId}";

            await _apiRequestHandler.HandlePostRequest(action, taskTimeModel);
        }

        private async Task AddLocalTaskTimeModelAsync(TaskTimeModel taskTimeModel)
        {
            await AsyncAwaiter.AwaitAsync(nameof(AddLocalTaskTimeModelAsync), async () =>
            {
                taskTimeModels = await GetLocalTaskTimeModelsAsync();

                var existingTimeModel = taskTimeModels.SingleOrDefault(item => item.TaskId == taskTimeModel.TaskId);

                if (existingTimeModel != null)
                    taskTimeModels.Remove(existingTimeModel);

                taskTimeModels.Add(taskTimeModel);

                await SaveLocalTaskTimeModelsAsync(taskTimeModels);
            });
        }

        private async Task<List<TaskTimeModel>> GetLocalTaskTimeModelsAsync()
        {
            taskTimeModels = new List<TaskTimeModel>();

            string taskTimeModelsJson = await _fileService.ReadFromInternalStorageAsync(localTaskTimeModelsFilename, tasksDirectoryName);

            if (!taskTimeModelsJson.IsNullOrWhiteSpace())
                taskTimeModels = JsonSerializer.Deserialize<List<TaskTimeModel>>(taskTimeModelsJson);

            return taskTimeModels;
        }

        private async Task SaveLocalTaskTimeModelsAsync(List<TaskTimeModel> taskTimeModels)
        {
            string taskTimeModelsJson = JsonSerializer.Serialize(taskTimeModels);

            await _fileService.SaveFileToInternalStorageAsync(taskTimeModelsJson, localTaskTimeModelsFilename, tasksDirectoryName);
        }

        #endregion

        #region localTaskStatusModel    

        private List<LocalTaskStatusModel> taskStatusModels = new List<LocalTaskStatusModel>();
        public async Task UploadLocalTaskStatusModelsAsync()
        {
            var sm = new SemaphoreSlim(1, 1);
            await sm.WaitAsync();
            try
            {
                taskStatusModels = await GetLocalTaskStatusModelsAsync();
                if (taskStatusModels == null)
                    return;

                var Iterations = taskStatusModels.Count - 1;
                for (int i = Iterations; i >= 0; i--)
                {
                    try
                    {
                        LocalTaskStatusModel taskStatusModel = taskStatusModels.FirstOrDefault();

                        if (taskStatusModel is null) continue;

                        if (!taskStatusModel.Posted)
                        {
                            await PostTaskStatusAsync(taskStatusModel);
                        }

                        taskStatusModels.Remove(taskStatusModel);

                        await SaveLocalTaskStatusModelsAsync(taskStatusModels);
                    }
                    catch { break; }
                }
            }
            finally
            {
                sm.Release();
            }
        }

        private async Task AddLocalTaskStatusModelAsync(LocalTaskStatusModel statusModel)
        {
            // Update the shared cache with the latest version
            var models = await GetLocalTaskStatusModelsAsync();

            lock (taskStatusModels)
            {
                taskStatusModels = models;

                var existingModel = taskStatusModels.SingleOrDefault(item => item.TaskId == statusModel.TaskId);

                if (existingModel != null)
                    taskStatusModels.Remove(existingModel);

                taskStatusModels.Add(statusModel);
            }

            await SaveLocalTaskStatusModelsAsync(taskStatusModels);
        }

        private async Task<List<LocalTaskStatusModel>> GetLocalTaskStatusModelsAsync()
        {
            taskStatusModels = new List<LocalTaskStatusModel>();

            string taskStatusModelsJson = await _fileService.ReadFromInternalStorageAsync(localTaskStatusModelsFilename, tasksDirectoryName);

            if (!taskStatusModelsJson.IsNullOrWhiteSpace())
                taskStatusModels = JsonSerializer.Deserialize<List<LocalTaskStatusModel>>(taskStatusModelsJson);

            return taskStatusModels;
        }

        private async Task SaveLocalTaskStatusModelsAsync(List<LocalTaskStatusModel> taskStatusModels)
        {
            string taskStatusModelsJson = JsonSerializer.Serialize(taskStatusModels);

            await _fileService.SaveFileToInternalStorageAsync(taskStatusModelsJson, localTaskStatusModelsFilename, tasksDirectoryName);
        }

        private async Task PostTaskStatusAsync(LocalTaskStatusModel statusModel)
        {
            string action = $"task/setstatus/{statusModel.TaskId}";

            await _apiRequestHandler.HandlePostRequest(action, statusModel);

            // for task status updates we use the local status list, this list is pushed by the syncservice so no need here to update the taskscache
            //UpdateTasksCache();
        }

        public async Task PostTaskStatusWithReasonAsync(MultiTaskStatusWithReason multiTaskStatus)
        {
            string action = $"tasks/setstatusseswithreason";

            await _apiRequestHandler.HandlePostRequest(action, multiTaskStatus);
        }


        public async Task<List<BasicTaskModel>> GetTasksForPeriodsAsync(LocalDateTime? tasksTimestamp, bool refresh, TaskPeriod? taskPeriod = null)
        {
            var currentShift = await _shiftService.GetCurrentShiftAsync(refresh);

            tasksTimestamp ??= Settings.TasksTimestamp;

            // Correct it if needed
            tasksTimestamp = TasksTimeStampHelper.CorrectTasksTimeStamp(tasksTimestamp.Value, currentShift);

            List<BasicTaskModel> tasks;

            if (taskPeriod.HasValue && taskPeriod.Value == TaskPeriod.OverDue)
            {
                tasks = await GetOverdueTasksAsync(tasksTimestamp, loadActionCount: false, refresh);

                foreach (var task in tasks)
                {
                    task.TaskPeriods = TaskPeriodTypes.OverDue;
                }
            }
            else
            {
                tasks = await GetTasksAsync(loadActionCount: false, tasksTimestamp: tasksTimestamp, refresh: refresh);

                Filter(tasks, tasksTimestamp, currentShift);
            }

            List<Task> toDoTasks = new()
            {
                LoadOpenActionCountForTasksAsync(tasks, refresh: refresh),
                LoadLocalInformationForTasksAsync(tasks),
                GetTaskHistoryFirsts(tasks, refresh: refresh),
                _propertiesService.LoadTaskPropertiesAsync(tasks, includeProperties: true, refresh)
            };

            if (CompanyFeatures.RequiredProof)
                toDoTasks.Add(SetHasPictureProof(tasks, refresh: refresh));

            await Task.WhenAll(toDoTasks);
            return tasks;
        }

        public async Task SetHasPictureProof(IEnumerable<BasicTaskModel> tasks = null, bool refresh = false, bool isFromSyncService = false)
        {
            var taskTemplates = await _taskTemplatesSerivce.GetAllTemplatesForAreaAsync(Settings.WorkAreaId, refresh: refresh, isFromSyncService: isFromSyncService);
            foreach (var task in tasks)
            {
                task.HasPictureProof = taskTemplates.FirstOrDefault(t => t.Id == task.TemplateId)?.HasPictureProof ?? false;
                task.SetPictureProofMediaItems();
            }
        }

        public async Task SetTaskPictureProof(BasicTaskModel task)
        {
            if (!task.PictureProofMediaItems.Any())
                return;

            var pictureProof = new PictureProof()
            {
                Media = task.PictureProofMediaItems.Select(pictureProof => new PictureProofMedia()
                {
                    ItemName = task.Name,
                    PictureTakenUtc = pictureProof.CreatedAt.ToDateTimeUnspecified().ToUniversalTime(),
                    UriPart = pictureProof.PictureUrl,
                    ThumbUriPart = pictureProof.PictureUrl,
                    UserFullName = UserSettings.Fullname,
                    UserId = UserSettings.Id
                }).ToList(),
                ProofTakenByUserId = UserSettings.Id,
                ProofTakenUtc = DateTime.UtcNow
            };

            string action = $"task/setpictureproof/{task.Id}";

            await _apiRequestHandler.HandlePostRequest(action, pictureProof);
        }

        private void Filter(List<BasicTaskModel> tasks, LocalDateTime? tasksTimestamp, ShiftModel currentShift)
        {
            foreach (var task in tasks)
            {
                int currentShiftId = 0;
                if (task.ShiftId.HasValue)
                    currentShiftId = (int)task.ShiftId;

                if ((task.ShiftId.HasValue && task.ShiftId.Value == currentShift.Id) ||
                        (TaskFilterOptions.HasTodayRecurrency(task))
                       && (tasksTimestamp.HasValue && task.StartDate.Value.Date == tasksTimestamp.Value.ToDateTimeUnspecified().Date && task.DueAt.Value.Date >= tasksTimestamp.Value.ToDateTimeUnspecified().Date))
                {
                    task.TaskPeriods |= TaskPeriodTypes.Today;
                }

                if (currentShiftId == currentShift.Id && currentShift.Id != 0)
                {
                    task.TaskPeriods |= TaskPeriodTypes.Shift;
                }

                if (TaskFilterOptions.HasWeekRecurrency(task))
                {
                    task.TaskPeriods |= TaskPeriodTypes.Week;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
            _shiftService.Dispose();
            _actionService.Dispose();
            //_cachingService.
            _propertiesService.Dispose();
            //_fileService.d
        }


        #region Local Picture proof
        public async Task SaveLocalPictureProof(BasicTaskModel task)
        {
            LocalTaskPictureProofModel localTaskPictureProofModel = new LocalTaskPictureProofModel()
            {
                TaskId = task.Id,
                PictureProofMediaItems = task.PictureProofMediaItems,
                UserId = UserSettings.Id
            };

            await AddLocalPicureProofModelAsync(localTaskPictureProofModel);
        }

        private async Task<List<LocalTaskPictureProofModel>> GetLocalPictureProofAsync()
        {

            return await AsyncAwaiter.AwaitResultAsync(nameof(TasksService) + localTaskPictureProofFilename, async () =>
            {
                var pictureProofModels = new List<LocalTaskPictureProofModel>();

                string pictureProofJson = await _fileService.ReadFromInternalStorageAsync(localTaskPictureProofFilename, tasksDirectoryName);

                if (!pictureProofJson.IsNullOrWhiteSpace())
                    pictureProofModels = JsonSerializer.Deserialize<List<LocalTaskPictureProofModel>>(pictureProofJson);

                return pictureProofModels.Where(p => p.UserId == UserSettings.Id).ToList();
            });
        }

        private async Task AddLocalPicureProofModelAsync(LocalTaskPictureProofModel localTaskPictureProofModel)
        {
            var pictureProofModels = await GetLocalPictureProofAsync();

            LocalTaskPictureProofModel existingModel = pictureProofModels.SingleOrDefault(item => item.TaskId == localTaskPictureProofModel.TaskId);

            if (existingModel != null)
                pictureProofModels.Remove(existingModel);

            pictureProofModels.Add(localTaskPictureProofModel);

            await SaveLocalPictureProofAsync(pictureProofModels);
        }

        private async Task SaveLocalPictureProofAsync(List<LocalTaskPictureProofModel> pictureProofModels)
        {
            await AsyncAwaiter.AwaitAsync(nameof(TasksService) + localTaskPictureProofFilename, async () =>
            {
                string pictureProofJson = JsonSerializer.Serialize(pictureProofModels);

                await _fileService.SaveFileToInternalStorageAsync(pictureProofJson, localTaskPictureProofFilename, tasksDirectoryName);
            });
        }

        public async Task UploadLocalPictureProofAsync()
        {
            var sm = new SemaphoreSlim(1, 1);
            await sm.WaitAsync();
            try
            {
                var pictureProofModels = await GetLocalPictureProofAsync();

                var Iterations = pictureProofModels.Count - 1;

                for (int i = Iterations; i >= 0; i--)
                {
                    try
                    {
                        var pictureProofModel = pictureProofModels[i];

                        using var scope = App.Container.CreateScope();
                        var mediaService = scope.ServiceProvider.GetService<IMediaService>();

                        var mediaItems = pictureProofModel.PictureProofMediaItems.Where(p => p.IsLocalFile);
                        foreach (var mediaItem in mediaItems)
                        {
                            await mediaService.UploadMediaItemAsync(mediaItem, MediaStorageTypeEnum.PictureProof, 0, true);
                        }

                        var model = new BasicTaskModel()
                        {
                            Id = pictureProofModel.TaskId,
                            PictureProofMediaItems = pictureProofModel.PictureProofMediaItems
                        };

                        await SetTaskPictureProof(model);

                        pictureProofModels.Remove(pictureProofModel);
                        await SaveLocalPictureProofAsync(pictureProofModels);
                    }
                    catch { break; }
                }
            }
            finally
            {
                sm.Release();
            }
        }
        #endregion

        #region Local Multiskip task
        public async Task<List<MultiTaskStatusWithReason>> GetLocalMultiskipTaskAsync()
        {

            return await AsyncAwaiter.AwaitResultAsync(nameof(TasksService) + multiskipTaskStatusFilename, async () =>
            {
                var multiTaskModel = new List<MultiTaskStatusWithReason>();

                string multiTaskJson = await _fileService.ReadFromInternalStorageAsync(multiskipTaskStatusFilename, tasksDirectoryName);

                if (!multiTaskJson.IsNullOrWhiteSpace())
                    multiTaskModel = JsonSerializer.Deserialize<List<MultiTaskStatusWithReason>>(multiTaskJson);

                return multiTaskModel;
            });
        }

        public async Task AddLocalMultiskipTaskAsync(MultiTaskStatusWithReason multiTaskStatus)
        {
            var multskipTaskModel = await GetLocalMultiskipTaskAsync();

            multskipTaskModel.Add(multiTaskStatus);

            await SaveLocalMultiskipTaskAsync(multskipTaskModel);
        }

        private async Task SaveLocalMultiskipTaskAsync(List<MultiTaskStatusWithReason> multskipTaskModels)
        {
            await AsyncAwaiter.AwaitAsync(nameof(TasksService) + multiskipTaskStatusFilename, async () =>
            {
                string multiTaskJson = JsonSerializer.Serialize(multskipTaskModels);

                await _fileService.SaveFileToInternalStorageAsync(multiTaskJson, multiskipTaskStatusFilename, tasksDirectoryName);
            });
        }

        public async Task UploadLocalMultiTaskStatusAsync()
        {
            var sm = new SemaphoreSlim(1, 1);
            await sm.WaitAsync();
            try
            {
                var multiskipTaskModels = await GetLocalMultiskipTaskAsync();

                var Iterations = multiskipTaskModels.Count - 1;

                for (int i = Iterations; i >= 0; i--)
                {
                    try
                    {
                        var multiskipTaskModel = multiskipTaskModels[i];

                        await PostTaskStatusWithReasonAsync(multiskipTaskModel);

                        multiskipTaskModels.Remove(multiskipTaskModel);
                        await SaveLocalMultiskipTaskAsync(multiskipTaskModels);
                    }
                    catch { break; }
                }
            }
            finally
            {
                sm.Release();
            }
        }
        #endregion

        public async Task SetMandatoryItemToTask(long? taskId, int itemId)
        {
            if (taskId == null)
                return;

            static void alteringFunction(BasicTaskModel task, int itemId)
            {
                task.CompletedDeeplinkId = itemId;
            }

            // Alter taskoverdue cache
            var uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: GetTasksCreateRequestUrl("tasksoverdue")).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<BasicTaskModel>(uri, (task) => alteringFunction(task, itemId), (task) => task.Id == taskId).ConfigureAwait(false);

            // Alter other tasks cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: GetTasksCreateRequestUrl("tasks")).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<BasicTaskModel>(uri, (task) => alteringFunction(task, itemId), (task) => task.Id == taskId).ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.MandatoryItemFinished, new MandatoryItemFinishedMessageArgs()
                {
                    ItemId = itemId,
                    TaskId = taskId
                });
            });
        }
    }
}
