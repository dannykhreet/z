using Autofac;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Feed;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Shifts;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using NodaTime;
using Syncfusion.Maui.DataSource.Extensions;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Data
{
    /// <summary>
    /// Service to synchronise local data with the API.
    /// </summary>
    public class SyncService : ISyncService
    {
        private Stopwatch sw;
        private List<string> resultlist = new List<string>();

        private const string mediaUrlsFilename = "mediaurls.json";
        private const string mediaUrlsDirectoryName = "mediaurls";

        private const string descriptionFileUrlsFilename = "descriptionfileurls.json";
        private const string descriptionFileUrlsDirectoryName = "descriptionfileurls";

        private readonly IAuditsService _auditService;
        private readonly IChecklistService _checklistService;
        private readonly ICachingService _cachingService;
        private readonly IUpdateService _updateService;
        private readonly IUserService _userService;
        private readonly IActionsService _actionService;
        private readonly ITasksService _taskService;
        private readonly IShiftService _shiftService;
        private readonly ITaskReportService _taskReportService;
        private readonly IFileService _fileService;
        private readonly IMessageService _messageService;
        private readonly IWorkAreaService _workAreaService;
        private readonly Interfaces.Pdf.IPdfService _pdfService;
        private readonly IPropertyService _taskPropertiesService;
        private readonly ITaskCommentService _taskCommentsService;
        private readonly IApiClient _apiClient;
        private readonly IAssessmentsService _assessmentService;
        private readonly IInstructionsService _instructionsService;
        private readonly ITaskTemplatesSerivce _taskTemplatesSerivce;
        private readonly ITagsService _tagsService;
        private readonly IFeedService _feedService;

        private List<ChecklistModel> completedChecklists;
        private List<CommentModel> comments;
        private List<ChecklistTemplateModel> checklistTemplates;
        private List<AuditsModel> completedAudits;
        private List<AuditTemplateModel> auditTemplates;
        private List<UserProfileModel> companyUsers;
        private List<ActionCommentModel> actionComments;
        private List<ActionsModel> actions;
        private List<BasicTaskModel> tasks;
        private List<TaskTemplateModel> taskTemplates;
        private List<InstructionsModel> instructionTemplates;
        private static Queue<string> attachmentsUrls = new Queue<string>();
        private static Queue<string> mediaFiles = new Queue<string>();
        private const bool _isFromSyncService = true;

        private static CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        public SyncService(
            IAuditsService auditService,
            IChecklistService checklistService,
            IUpdateService updateService,
            IUserService userService,
            IActionsService actionService,
            ITasksService taskService,
            IShiftService shiftService,
            ITaskReportService taskReportService,
            IMessageService messageService,
            IWorkAreaService workAreaService,
            Interfaces.Pdf.IPdfService pdfService,
            IPropertyService taskPropertiesService,
            ITaskCommentService taskCommentsService,
            IApiClient apiClient,
            IAssessmentsService assessmentService,
            IInstructionsService instructionsService,
            IFeedService feedService,
            ITaskTemplatesSerivce taskTemplatesSerivce,
            ITagsService tagsService)
        {
            _auditService = auditService;
            _checklistService = checklistService;
            _updateService = updateService;
            _userService = userService;
            _actionService = actionService;
            _taskReportService = taskReportService;
            _taskService = taskService;
            _shiftService = shiftService;
            _messageService = messageService;
            _workAreaService = workAreaService;
            _pdfService = pdfService;
            _taskPropertiesService = taskPropertiesService;
            _taskCommentsService = taskCommentsService;
            _apiClient = apiClient;
            _assessmentService = assessmentService;
            _instructionsService = instructionsService;
            _taskTemplatesSerivce = taskTemplatesSerivce;
            _tagsService = tagsService;
            _feedService = feedService;
            _fileService = DependencyService.Get<IFileService>();
            _cachingService = DependencyService.Get<ICachingService>();
        }

        #region actions

        public async Task LoadActionsAsync()
        {
#if DEBUG
            long totalElapsed = 0;
            var sw = new Stopwatch();
            sw.Start();
#endif

            await _actionService.GetAssignedAreasActionsAsync(isFromSyncService: _isFromSyncService);

#if DEBUG
            long time1 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time1;
            resultlist.Add($"LoadActionsAsync => GetAssignedAreasActionsAsync: {time1} ms");
#endif

            await _actionService.GetAssignedUsersAsync(isFromSyncService: _isFromSyncService);

#if DEBUG
            long time2 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time2;
            resultlist.Add($"LoadActionsAsync => GetAssignedUsersAsync: {time2} ms");
#endif

            actionComments = await _actionService.GetActionCommentsAsync(includeLocalActionComments: false, isFromSyncService: _isFromSyncService);

#if DEBUG
            long time3 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time3;
            resultlist.Add($"LoadActionsAsync => GetActionCommentsAsync: {time3} ms");
#endif

            actions = await _actionService.GetActionsAsync(isFromSyncService: _isFromSyncService);

#if DEBUG
            long time4 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time4;
            resultlist.Add($"LoadActionsAsync => GetActionsAsync: {time4} ms");
#endif

            await _actionService.GetMyActionsAsync(createdByOrAssignedToMe: true, isFromSyncService: _isFromSyncService);

#if DEBUG
            long time5 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time5;
            resultlist.Add($"LoadActionsAsync => GetMyActionsAsync: {time5} ms");
#endif

            await _actionService.GetAssignedUserActionsAsync(id: UserSettings.Id, isFromSyncService: _isFromSyncService);

#if DEBUG
            long time6 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time6;
            resultlist.Add($"LoadActionsAsync => GetAssignedUserActionsAsync: {time6} ms");
#endif

            await _actionService.GetCreatedActionsAsync(id: UserSettings.Id, isFromSyncService: _isFromSyncService);

#if DEBUG
            long time7 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time7;
            resultlist.Add($"LoadActionsAsync => GetCreatedActionsAsync: {time7} ms");
#endif

            var assignedactions = actions.Where(x => x.AssignedUsers.Any(u => u.Id == UserSettings.Id)).ToList();

#if DEBUG
            long time8 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time8;
            resultlist.Add($"LoadActionsAsync => Filtering assigned actions in memory: {time8} ms");
#endif

            var myactions = await _actionService.GetActionsCount(isFromSyncService: _isFromSyncService);

#if DEBUG
            long time9 = sw.ElapsedMilliseconds - totalElapsed;
            totalElapsed += time9;
            resultlist.Add($"LoadActionsAsync => GetActionsCount: {time9} ms");
#endif

            if (myactions != null)
            {
                int count = myactions.IsOverdueCount + myactions.IsUnresolvedCount;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, Constants.MyActionsChanged, count);
                });
            }

#if DEBUG
            sw.Stop();
            resultlist.Add($"LoadActionsAsync => TOTAL TIME: {sw.ElapsedMilliseconds} ms");
#endif

            _actionService.SendActionsChangedMessage();
        }

        public async Task UploadUpdateLocalActionDataAsync(int actionId)
        {
            if (await InternetHelper.HasInternetConnection())
            {
                // if the task is still running we return, not to pile up tasks. It will run again in 15s.
                if (Statics.SynchronizationRunning)
                    return;

                Statics.SynchronizationRunning = true;
                try
                {
                    List<string> postedmessages = new List<string>();

                    postedmessages = await _actionService.UploadLocalActionCommentsAsync();
                    postedmessages ??= new List<string>();

                    List<ActionCommentModel> comments = new List<ActionCommentModel>();
                    if (await _updateService.CheckForUpdatedChatAsync(actionId))
                    {
                        if (actionId > 0)
                        {
                            comments = await _actionService.GetActionCommentsAsync(actionId, includeLocalActionComments: false, refresh: true, isFromSyncService: _isFromSyncService);
                            if (comments.Any())
                            {
                                comments = comments.Where(item => item.UserId != UserSettings.Id).ToList();
                            }
                        }
                    }
                    comments ??= new List<ActionCommentModel>();

                    if (postedmessages.Any() || comments.Any())
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            MessagingCenter.Send(this, Constants.ChatChanged, new ActionCommentUpdateModel { PostedMessageIds = postedmessages, NewComments = comments });
                        });
                    }
                }
                catch (Exception exception)
                {
                    //Crashes.TrackError(exception);
                }
                finally
                {
                    Statics.SynchronizationRunning = false;
                }
            }
        }

        #endregion

        #region Tasks

        public async Task<List<BasicTaskStatusModel>> ReloadTasksStatussesAsync()
        {
            if (await InternetHelper.HasInternetConnection())
            {
                try
                {
                    await Task.WhenAll(
                        _taskService.UploadLocalTaskTimeModelsAsync(),
                        _taskService.UploadLocalTaskStatusModelsAsync()).ConfigureAwait(false);

                    var newStatuses = await _taskService.GetTaskStatusses(Settings.TasksStatussesTimestamp, _isFromSyncService);
                    Settings.TasksStatussesTimestamp = DateTimeHelper.Now;

                    return newStatuses;
                }
                catch { }
            }

            return new List<BasicTaskStatusModel>();
        }

        public async Task<List<TaskExtendedDataBasic>> GetPropertyValuesUpdatesAsync()
        {
            if (await InternetHelper.HasInternetConnection())
            {
                try
                {
                    var newProperyValues = await _taskService.GetTaskExtendedData(Settings.TasksExtendedDataTimestamp, _isFromSyncService);
                    Settings.TasksExtendedDataTimestamp = DateTimeHelper.Now;

                    return newProperyValues;
                }
                catch
                { }
            }

            return new List<TaskExtendedDataBasic>();
        }


        public async Task LoadTasksAsync()
        {
            await ReloadTasksStatussesAsync();

#if DEBUG
            long totals = 0;
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            var currentShift = await _shiftService.GetCurrentShiftAsync();

            LocalDateTime timestamp = TasksTimeStampHelper.CorrectTasksTimeStamp(DateTimeHelper.Now, currentShift);

            await _taskTemplatesSerivce.GetAllTemplatesForAreaAsync(Settings.WorkAreaId, false, isFromSyncService: _isFromSyncService);
#if DEBUG
            long stop0 = sw2.ElapsedMilliseconds;
            totals += stop0;
            string result = "LoadTasks => GetAllTemplatesForAreaAsync, time taken: " + stop0 + " ms";
            resultlist.Add(result);
#endif

            tasks ??= new List<BasicTaskModel>();

            List<BasicTaskModel> newTasks = new List<BasicTaskModel>();

            if (newTasks.Any())
                tasks.AddRange(newTasks);

            newTasks = await _taskService.GetTasksAsync(timestamp, false, isFromSyncService: _isFromSyncService);

#if DEBUG
            long stop1 = sw2.ElapsedMilliseconds;
            totals += stop1;
            result = "LoadTasks => GetTasksAsync, time taken: " + stop1 + " ms";
            resultlist.Add(result);
#endif

            if (newTasks.Any())
                tasks.AddRange(newTasks);

            newTasks = await _taskService.GetOverdueTasksAsync(timestamp, false, isFromSyncService: _isFromSyncService);

#if DEBUG
            long stop2 = sw2.ElapsedMilliseconds - totals;
            totals += stop2;
            result = "LoadTasks => GetOverdueTasksAsync, time taken: " + stop2 + " ms";
            resultlist.Add(result);
#endif

            if (newTasks.Any())
                tasks.AddRange(newTasks);

            newTasks = await _taskService.GetTasksWithActionsAsync(Constants.TasksActionsUrl, isFromSyncService: _isFromSyncService);

#if DEBUG
            long stop3 = sw2.ElapsedMilliseconds - totals;
            totals += stop3;
            result = "LoadTasks => GetTasksWithActionsAsync, time taken: " + stop3 + " ms";
            resultlist.Add(result);
#endif

            if (newTasks.Any())
                tasks.AddRange(newTasks);

            // Get first tappings for tasks.. we are using this to know which user first tapped a task (so only that user can undo the tapping (implementation))
            await _taskService.GetTaskHistoryFirsts(tasksTimestamp: timestamp, isFromSyncService: _isFromSyncService);

#if DEBUG
            long stop4 = sw2.ElapsedMilliseconds - totals;
            totals += stop4;
            result = "LoadTasks => GetTaskHistoryFirsts, time taken: " + stop4 + " ms";
            resultlist.Add(result);
#endif

            taskTemplates = await _taskService.GetTaskTemplatesWithActionsAsync(isFromSyncService: _isFromSyncService);

#if DEBUG
            long stop5 = sw2.ElapsedMilliseconds - totals;
            totals += stop5;
            result = "LoadTasks => GetTaskTemplatesWithActionsAsync, time taken: " + stop5 + " ms";
            resultlist.Add(result);
#endif

            await _taskReportService.GetTaskOverviewReportOnlyAsync(timestamp, isFromSyncService: _isFromSyncService);

#if DEBUG
            long stop6 = sw2.ElapsedMilliseconds - totals;
            totals += stop6;
            result = "LoadTasks => GetTaskOverviewReportOnlyAsync, time taken: " + stop6 + " ms";
            resultlist.Add(result);
#endif

            Settings.TasksTimestamp = timestamp;
            Settings.TasksOverviewTimestamp = timestamp;

#if DEBUG
            sw2.Stop();
            result = "LoadTasks, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }

        public async Task LoadTaskPropertiesAsync()
        {
            await _taskPropertiesService.GetAllPropertiesAsync(refresh: true, isFromSyncService: _isFromSyncService);
        }

        public async Task LoadTaskCommentsAsync()
        {
            comments = await _taskCommentsService.GetAllAsync(isFromSyncService: _isFromSyncService);
        }
        #endregion

        #region Assessments
        public async Task LoadAssessmentTemplatesAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            var areaId = Settings.AssessmentsWorkAreaId != 0 ? Settings.AssessmentsWorkAreaId : Settings.WorkAreaId;
            await _assessmentService.GetAssessmentTemplates(workAreaId: areaId, isFromSyncService: _isFromSyncService);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.AssessmentTemplateChanged);
            });


#if DEBUG
            sw2.Stop();
            string result = "LoadAssessmentTemplatesAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }

        public async Task LoadAssessmentAsync(int id)
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            await _assessmentService.GetAssessments(id, isFromSyncService: _isFromSyncService).ConfigureAwait(false);

#if DEBUG
            sw2.Stop();
            string result = "LoadAssessmentAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }

        public async Task LoadAssessmentsAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            await _assessmentService.GetAssessments(isFromSyncService: _isFromSyncService).ConfigureAwait(false);

#if DEBUG
            sw2.Stop();
            string result = "LoadAssessmentsAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }

        #endregion

        #region Instructions
        private async Task LoadInstructionTemplatesAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            instructionTemplates = await _instructionsService.GetInstructions(isFromSyncService: _isFromSyncService);
            await _instructionsService.GetInstructionsForCurrentArea(isFromSyncService: _isFromSyncService).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.WorkInstructionsTemplateChanged);
            });
#if DEBUG
            sw2.Stop();
            string result = "LoadInstructionTemplatesAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }
        #endregion

        #region Tags
        private async Task LoadTagsAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            await _tagsService.GetTagsAsync(isFromSyncService: _isFromSyncService).ConfigureAwait(false);

#if DEBUG
            sw2.Stop();
            string result = "LoadTagsAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }
        #endregion

        #region EzFeed
        public async Task LoadEzFeedAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            await _feedService.GetMainFeedMessages(isFromSyncService: _isFromSyncService, limit: 10).ConfigureAwait(false);
            await _feedService.GetFactoryUpdatesMessages(isFromSyncService: _isFromSyncService, limit: 10).ConfigureAwait(false);

#if DEBUG
            sw2.Stop();
            string result = "LoadEzFeedAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }
        #endregion
        public async Task GetLocalDataAsync()
        {
            if (Statics.SynchronizationRunning)
                return;

            Statics.SynchronizationRunning = true;
            var sw = Stopwatch.StartNew();
            resultlist.Clear();

            try
            {
                //Actions should load first
                await LoadActionsAsync().ConfigureAwait(false);
                resultlist.Add($"[1] LoadActionsAsync completed in {sw.ElapsedMilliseconds} ms");

                var highPriorityTasks = new List<Task>
                {
                    LoadTagsAsync(),
                    LoadShiftsAsync(),
                    LoadCompanyUsersAsync(),
                    LoadTasksAsync(),
                    LoadTaskPropertiesAsync(),
                    LoadTaskCommentsAsync(),
                    LoadChecklistTemplatesAsync(),
                    LoadAuditTemplatesAsync(),
                    LoadAssessmentTemplatesAsync(),
                    LoadInstructionTemplatesAsync(),
                    LoadWorkAreasAsync()
                };

                await Task.WhenAll(highPriorityTasks).ConfigureAwait(false);
                resultlist.Add($"[2] High-priority batch completed in {sw.ElapsedMilliseconds} ms");

                _ = Task.Run(async () =>
                {
                    var lowPriorityTasks = new List<Task>
                    {
                            LoadCompletedChecklistsAsync(),
                            LoadCompletedAuditsAsync(),
                            LoadAssessmentsAsync(),
                            LoadEzFeedAsync()
                    };

                    try
                    {
                        await Task.WhenAll(lowPriorityTasks).ConfigureAwait(false);
                        resultlist.Add($"[3] Background sync completed in {sw.ElapsedMilliseconds} ms");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Background sync error: {ex}");
                    }
                });

                Settings.UpdateDates();

                resultlist.Add($"[4] Total sync duration: {sw.ElapsedMilliseconds} ms");
                Debug.WriteLine(string.Join(Environment.NewLine, resultlist));

                _ = InitializeMediaDownloadAsync();
#if DEBUG
                OutputOfflineSyncOverview();
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SyncService] GetLocalDataAsync failed: {ex}");
            }
            finally
            {
                Statics.SynchronizationRunning = false;
                sw.Stop();
            }
        }

#if DEBUG
        private void OutputOfflineSyncOverview()
        {
            int actionsCount = actions?.Count ?? 0;
            int tasksCount = tasks?.Count ?? 0;
            int auditsCount = completedAudits?.Count ?? 0;
            int checklistsCount = completedChecklists?.Count ?? 0;
            int mediaFilesCount = (mediaFiles?.Count ?? 0) + (attachmentsUrls?.Count ?? 0);

            int total = actionsCount + tasksCount + auditsCount + checklistsCount + mediaFilesCount;

            Debug.WriteLine("=========== OFFLINE DATASET OVERVIEW ===========");
            Debug.WriteLine($"Actions:      {actionsCount}");
            Debug.WriteLine($"Tasks:        {tasksCount}");
            Debug.WriteLine($"Audits:       {auditsCount}");
            Debug.WriteLine($"Checklists:   {checklistsCount}");
            Debug.WriteLine($"Media Files:  {mediaFilesCount}");
            Debug.WriteLine($"-----------------------------------------------");
            Debug.WriteLine($"TOTAL:        {total}");
            Debug.WriteLine("===============================================");
        }
#endif


        /// <summary>
        /// Uploads the local data asynchronous.
        /// </summary>
        public async Task UploadLocalDataAsync()
        {
            if (Statics.SynchronizationRunning)
                return;

            Statics.SynchronizationRunning = true;

            try
            {
                var tasks = new List<Task>();

                // if on the actionconversationpage this is handled by the quicktimer
                if (Settings.SubpageActions != Enumerations.MenuLocation.ActionsConversation)
                {
                    await _actionService.UploadLocalActionsWithComments().ConfigureAwait(false);
                }
                else
                {
                    await _actionService.UploadLocalActionsAsync().ConfigureAwait(false);
                }


                int count = await _auditService.UploadLocalSignedAuditsAsync().ConfigureAwait(false);

                if (count > 0)
                {
                    tasks.Add(LoadCompletedAuditsAsync());
                }

                count = await _checklistService.UploadLocalSignedChecklistsAsync().ConfigureAwait(false);

                if (count > 0)
                {
                    tasks.Add(LoadCompletedChecklistsAsync());
                }

                count = await _taskCommentsService.UploadLocalCommentsAsync();

                if (count > 0)
                {
                    tasks.Add(LoadTaskCommentsAsync());
                }

                tasks.Add(_actionService.UploadLocalResolvedActionIdsAsync());
                tasks.Add(_actionService.UploadLocalCommentsViewedActionIdsAsync());
                tasks.Add(_taskService.UploadLocalPictureProofAsync());
                tasks.Add(_taskService.UploadLocalMultiTaskStatusAsync());

                tasks.Add(_taskService.UploadLocalTaskTimeModelsAsync());
                tasks.Add(_taskService.UploadLocalTaskStatusModelsAsync());

                tasks.Add(_taskPropertiesService.UploadLocalPropertyValues());

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                //Crashes.TrackError(exception);
            }
            finally
            {
                Statics.SynchronizationRunning = false;
            }
        }

        public async Task UpdateLocalDataAsync()
        {
            if (Statics.SynchronizationRunning) return;

            try
            {
                Statics.SynchronizationRunning = true;
                await Task.Run(() => RunUpdateFlow());
            }
            finally
            {
                Statics.SynchronizationRunning = false;
            }

        }

        private async Task RunUpdateFlow()
        {
            await UploadUnpostedData().ConfigureAwait(false);

            //Uploading comments handled outside of action conversation page
            if (Settings.SubpageActions != Enumerations.MenuLocation.ActionsConversation)
                await _actionService.UploadLocalActionCommentsAsync().ConfigureAwait(false);

            IEnumerable<UpdateCheckItem> updateCheckItems = await _updateService.CheckForUpdatedItemsAsync();

            if (updateCheckItems?.Any() ?? false)
            {
                foreach (UpdateCheckItem updateCheckItem in updateCheckItems)
                {
                    switch (updateCheckItem.UpdateCheckType)
                    {
                        case UpdateCheckTypeEnum.Actions:
                            // Stop the actions syncing while we are in the actionconversationpage and details
                            // but not on the actionspage
                            if (Settings.SubpageActions != Enumerations.MenuLocation.ActionsConversation)
                            {
                                await LoadActionsAsync().ConfigureAwait(false);
                            }
                            break;
                        case UpdateCheckTypeEnum.Audits:
                            await LoadCompletedAuditsAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.AuditTemplates:
                            await LoadAuditTemplatesAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.Checklists:
                            await LoadCompletedChecklistsAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.ChecklistTemplates:
                            await LoadChecklistTemplatesAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.Tasks:
                            // Stop task syncing when we're in the tasks page
                            // This sync is done by the task view model
                            if (Settings.MenuLocation != Enumerations.MenuLocation.Tasks)
                            {
                                try
                                {
                                    Statics.TaskSyncRunning = true;
                                    await LoadTasksAsync().ConfigureAwait(false);
                                }
                                catch { }
                                finally { Statics.TaskSyncRunning = false; }
                            }
                            break;
                        case UpdateCheckTypeEnum.Users:
                            await LoadCompanyUsersAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.Assessments:
                            // Stop assessment syncing when we're in the assessments page
                            // This sync is done by the assessment view model
                            if (Settings.MenuLocation != Enumerations.MenuLocation.Assessments)
                            {
                                try
                                {
                                    Statics.AssessmentsSyncRunning = true;
                                    await LoadAssessmentTemplatesAsync().ConfigureAwait(false);
                                }
                                catch { }
                                finally { Statics.AssessmentsSyncRunning = false; }
                            }
                            break;
                        case UpdateCheckTypeEnum.EzFeed:
                            // Stop assessment syncing when we're in the feed page
                            // This sync is done by the feed view model
                            if (Settings.MenuLocation != Enumerations.MenuLocation.Feed)
                            {
                                try
                                {
                                    Statics.EzFeedSyncRunning = true;
                                    await LoadEzFeedAsync().ConfigureAwait(false);
                                }
                                catch { }
                                finally { Statics.EzFeedSyncRunning = false; }
                            }
                            break;
                        case UpdateCheckTypeEnum.Comments:
                            if (Settings.MenuLocation != Enumerations.MenuLocation.Tasks)
                            {
                                try
                                {
                                    Statics.TaskSyncRunning = true;
                                    await LoadTaskCommentsAsync().ConfigureAwait(false);
                                }
                                catch { }
                                finally { Statics.TaskSyncRunning = false; }
                            }
                            break;
                        case UpdateCheckTypeEnum.WorkInstructions:
                            await LoadInstructionTemplatesAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.AssessmentTemplates:
                            await LoadAssessmentTemplatesAsync().ConfigureAwait(false);
                            break;
                        case UpdateCheckTypeEnum.OpenChecklists:
                            if (Settings.MenuLocation != Enumerations.MenuLocation.Checklist)
                            {
                                try
                                {
                                    Statics.TaskSyncRunning = true;
                                    await LoadChecklistTemplatesAsync().ConfigureAwait(false);
                                }
                                catch { }
                                finally { Statics.TaskSyncRunning = false; }
                            }
                            break;
                    }
                }
            }
        }

        public async Task LoadCompletedChecklistsAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            Settings.CompletedChecklistsTimestamp = DateTimeHelper.Now;
            completedChecklists = await _checklistService.GetChecklistsAsync(isComplete: true, limit: CompletedChecklistsViewModel.Limit, timeStamp: Settings.CompletedChecklistsTimestamp, isFromSyncService: _isFromSyncService).ConfigureAwait(false);

            await _taskService.GetTasksWithActionsAsync(Constants.TaskChecklistActionsUrl, isFromSyncService: _isFromSyncService);
#if DEBUG
            sw2.Stop();
            string result = "LoadCompletedChecklistsAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
            sw2.Reset();
#endif
            await _checklistService.CheckHasCompletedChecklists(true).ConfigureAwait(false);
        }

        public async Task LoadInCompleteChecklistsAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            await _checklistService.GetIncompleteChecklistsAsync(isFromSyncService: _isFromSyncService).ConfigureAwait(false);
#if DEBUG
            sw2.Stop();
            string result = "LoadInCompleteChecklistsAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
            sw2.Reset();
#endif
        }

        public async Task LoadCompletedAuditsAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            Settings.CompletedAudtisTimestamp = DateTimeHelper.Now;
            completedAudits = await _auditService.GetAuditsAsync(isComplete: true, limit: CompletedAuditViewModel.Limit, timeStamp: Settings.CompletedAudtisTimestamp, isFromSyncService: _isFromSyncService).ConfigureAwait(false);

            await _taskService.GetTasksWithActionsAsync(Constants.TaskAuditActionsUrl, isFromSyncService: _isFromSyncService).ConfigureAwait(false);
#if DEBUG
            sw2.Stop();
            string result = "LoadCompletedAuditsAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
            sw2.Reset();
#endif
            await _auditService.CheckHasCompletedAudits(true).ConfigureAwait(false);
        }

        public void StartMediaDownload()
        {
            try
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
            }
            catch (ObjectDisposedException ex)
            {

            }
            finally
            {
                cancellationTokenSource = new CancellationTokenSource();
                Task.Run(DownloadMediaAsync, cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public void StopMediaDownload()
        {
            try
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
            }
            catch (ObjectDisposedException ex)
            {

            }
        }

        private static bool IsMediaUrlValid(string mediaUrl, IEnumerable<string> mediaUrls)
        {
            return !string.IsNullOrWhiteSpace(mediaUrl) && mediaUrl != Constants.PlaceholderImage && !mediaUrls.Contains(mediaUrl);
        }

        private static string RemoveMediaFromUrl(string mediaUrl)
        {
            if (!string.IsNullOrWhiteSpace(mediaUrl))
            {
                if (mediaUrl.Contains("/media"))
                    mediaUrl = mediaUrl.Replace("/media", string.Empty);
                else if (mediaUrl.Contains("media/"))
                    mediaUrl = mediaUrl.Replace("media/", string.Empty);
            }

            return mediaUrl;
        }

        public async Task LoadChecklistTemplatesAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            checklistTemplates = await _checklistService.GetChecklistTemplatesAsync(true, isFromSyncService: _isFromSyncService).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ChecklistTemplateChanged); });

#if DEBUG
            sw2.Stop();
            string result = "LoadChecklistTemplatesAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
            sw2.Reset();
#endif
        }

        public async Task LoadAuditTemplatesAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            auditTemplates = await _auditService.GetAuditTemplatesAsync(true, isFromSyncService: _isFromSyncService).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.AuditTemplateChanged);
            });

#if DEBUG
            sw2.Stop();
            string result = "LoadAuditTemplatesAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
            sw2.Reset();
#endif
        }

        private async Task LoadCompanyUsersAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif
            companyUsers = await _userService.GetCompanyUsersAsync(isFromSyncService: _isFromSyncService).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send<SyncService, List<UserProfileModel>>(this, Constants.ReloadUserDataMessage, companyUsers);
            });
#if DEBUG
            sw2.Stop();
            string result = "LoadCompanyUsersAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }

        private async Task LoadShiftsAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            await _shiftService.GetShiftsAsync(isFromSyncService: _isFromSyncService).ConfigureAwait(false);

#if DEBUG
            sw2.Stop();
            string result = "LoadShiftsAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }



        private async Task LoadWorkAreasAsync()
        {
#if DEBUG
            var sw2 = new Stopwatch();
            sw2.Start();
#endif

            await _workAreaService.GetBasicWorkAreasAsync().ConfigureAwait(false);

#if DEBUG
            sw2.Stop();
            string result = "LoadWorkAreasAsync, time taken: " + sw2.ElapsedMilliseconds + " ms";
            resultlist.Add(result);
#endif
        }


        private static int downloadedCount;

        private async Task DownloadMediaAsync()
        {
            try
            {
                // Are we already canceled?
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                var sw = new Stopwatch();
                sw.Start();

                // If the download is continued after the connection is restored but all the items are already downloaded
                // return here to prevent showing the 'download completed' message
                if (!mediaFiles.Any() && !attachmentsUrls.Any())
                    return;

                var totalCount = mediaFiles.Count + attachmentsUrls.Count;
                var msg = TranslateExtension.GetValueFromDictionary(LanguageConstants.downloadMediaProgress);

                await SendMessage(totalCount, msg);
#if DEBUG
                var lastTime = sw.ElapsedMilliseconds;
#endif
                for (downloadedCount = 0; downloadedCount < totalCount;)
                {
                    if (!HasConnection())
                        break;

                    if (mediaFiles.Any())
                    {
                        await DownloadMediaFiles(totalCount, msg).ConfigureAwait(false);
                    }

                    if (attachmentsUrls.Any())
                    {
                        await DownloadDescriptionFiles(totalCount, msg).ConfigureAwait(false);
                    }

                    if (!mediaFiles.Any() && !attachmentsUrls.Any())
                        break;
#if DEBUG
                    Debug.WriteLine($"[Downloaded media item {downloadedCount} of {totalCount}]: {sw.ElapsedMilliseconds - lastTime}ms.");
#endif
                }
#if DEBUG
                lastTime = sw.ElapsedMilliseconds;
                Debug.WriteLine($"Media downloaded {Settings.WorkAreaId}: {sw.Elapsed.TotalSeconds:0.00}s.");
#endif
            }
            catch (Exception exception)
            {
                // If we were canclled
                if (exception is OperationCanceledException)
                    // Clear the messgae
                    await MainThread.InvokeOnMainThreadAsync(() => _messageService.SendMessage("", Colors.Transparent, Enumerations.MessageIconTypeEnum.None, false, false, Enumerations.MessageTypeEnum.Clear));
                // If something else failed
                else
                {
                    // Track the error
                    //Crashes.TrackError(exception);
                }
            }
            finally
            {
                sw.Stop();
                if (!HasConnection())
                {
                    await MainThread.InvokeOnMainThreadAsync(() => _messageService.SendMessage(Models.Messaging.Message.Warning(TranslateExtension.GetValueFromDictionary(LanguageConstants.downloadMediaFinished), true)));
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() => _messageService.SendMessage(Models.Messaging.Message.Info(TranslateExtension.GetValueFromDictionary(LanguageConstants.downloadMediaFinished), true)));
                }
                //Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Downloading media finished.", new Dictionary<string, string>() { { Settings.WorkAreaId.ToString(), sw.ElapsedMilliseconds.ToString() + "ms." } });
            }
        }

        private async Task DownloadDescriptionFiles(int totalCount, string msg)
        {
            if (HasConnection())
            {
                var description = attachmentsUrls.Dequeue();
                try
                {
                    var documentUri = string.Format(Constants.MediaBaseUrl, description);

                    var result = await _pdfService.GetPfdAsync(documentUri).ConfigureAwait(false);

                    if (result == null)
                    {
                        attachmentsUrls.Enqueue(description);
                    }
                }
                catch { }
                downloadedCount++;
                await SendMessage(totalCount, msg);
            }
        }

        private async Task DownloadMediaFiles(int totalCount, string msg)
        {
            if (HasConnection())
            {
                var mediaUrl = mediaFiles.Dequeue();
                try
                {
                    var isSuccess = await DownloadMediaAsync(mediaUrl).ConfigureAwait(false);

                    if (!isSuccess)
                    {
                        mediaFiles.Enqueue(mediaUrl);
                    }
                }
                catch { }
                downloadedCount++;
                await SendMessage(totalCount, msg);
            }
        }

        private async Task SendMessage(int totalCount, string msg)
        {
            int count = downloadedCount;
            if (count % 10 == 0)
                await MainThread.InvokeOnMainThreadAsync(() => _messageService.SendMessage(Models.Messaging.Message.Info(msg.Format(count, totalCount), isClosable: false, spinner: true)));

        }

        private bool HasConnection()
        {
            return Connectivity.NetworkAccess.Equals(NetworkAccess.Internet);
        }

        private async Task InitializeMediaDownloadAsync()
        {
            if (Settings.DownloadMedia)
            {
                IEnumerable<string> mediaUrls = GetMediaUrls();
                IEnumerable<string> descriptionFileUrls = attachmentsUrls;

                await Task.WhenAll(
                                   SaveLocalUrlsAsync(mediaUrls, mediaUrlsFilename, mediaUrlsDirectoryName),
                                   SaveLocalUrlsAsync(descriptionFileUrls, descriptionFileUrlsFilename, descriptionFileUrlsDirectoryName)).ConfigureAwait(false);

                StartMediaDownload();
            }
        }

        private async Task<bool> DownloadMediaAsync(string mediaUrl)
        {
            try
            {
                var url = (new Uri(new Uri(Constants.MediaBaseUrl), mediaUrl)).ToString();
                //await ImageService.Instance.LoadUrl(url, TimeSpan.FromDays(10000)).Retry(3, 200).PreloadAsync();
                return true;
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("404"))
                    return true;
                else
                    return false;
            }
        }

        private async Task SaveLocalUrlsAsync(IEnumerable<string> urls, string filename, string directoryName)
        {
            string urlsJson = JsonSerializer.Serialize(urls);

            await _fileService.SaveFileToInternalStorageAsync(urlsJson, filename, directoryName);
        }


        private async Task<List<string>> GetLocalUrlsAsync(string filename, string directoryName)
        {
            List<string> urls = new List<string>();

            string urlsJson = await _fileService.ReadFromInternalStorageAsync(filename, directoryName).ConfigureAwait(false);

            if (!urlsJson.IsNullOrWhiteSpace())
                urls = JsonSerializer.Deserialize<List<string>>(urlsJson);

            return urls;
        }

        private IEnumerable<string> GetMediaUrls()
        {
            List<string> mediaPicturesUrls = new();

            var commentsAttachmentsUrls = comments.SelectMany(x => x.Attachments.Select(a => a.PictureUrl));
            foreach (var url in commentsAttachmentsUrls)
            {
                AddMediaPictureUrl(mediaPicturesUrls, url);
            }

            // Completed checklists
            if (completedChecklists != null)
            {
                foreach (ChecklistModel checklistModel in completedChecklists)
                {
                    if (checklistModel.Tasks != null)
                    {
                        foreach (TasksTaskModel checklistModelTask in checklistModel.Tasks)
                        {
                            if (checklistModelTask.Attachments?.Count > 0)
                                checklistModelTask.Attachments.Where(x => x.AttachmentType.ToLower() == "pdf")
                                    .ForEach(x => attachmentsUrls.Enqueue(x.Uri));

                            AddMediaPictureUrl(mediaPicturesUrls, checklistModelTask.Picture);
                            if (checklistModelTask.PictureProof?.Media != null)
                            {
                                foreach (var pictureproofurl in checklistModelTask.PictureProof.Media)
                                {
                                    AddMediaPictureUrl(mediaPicturesUrls, pictureproofurl.UriPart);
                                }
                            }

                        }
                        if (checklistModel.Signatures != null)
                        {
                            foreach (var signature in checklistModel.Signatures)
                            {
                                AddMediaPictureUrl(mediaPicturesUrls, signature.SignatureImage);
                            }
                        }
                    }

                    AddMediaPictureUrl(mediaPicturesUrls, checklistModel.Picture);
                }
            }

            // Checklist templates
            if (checklistTemplates != null)
            {
                foreach (ChecklistTemplateModel checklistTemplate in checklistTemplates)
                {
                    if (checklistTemplate.TaskTemplates != null)
                    {
                        foreach (TaskTemplateModel taskTemplate in checklistTemplate.TaskTemplates)
                        {
                            if (taskTemplate.Attachments?.Count > 0)
                                taskTemplate.Attachments.Where(x => x.AttachmentType.ToLower() == "pdf")
                                    .ForEach(x => attachmentsUrls.Enqueue(x.Uri));

                            var thumbnail = !string.IsNullOrEmpty(taskTemplate.VideoThumbnail) ? taskTemplate.VideoThumbnail : taskTemplate.Picture;
                            AddMediaPictureUrl(mediaPicturesUrls, thumbnail);
                        }
                    }

                    AddMediaPictureUrl(mediaPicturesUrls, checklistTemplate.Picture);
                }

                var stepsUrls = checklistTemplates
                    .Where(x => x.TaskTemplates != null)
                    .SelectMany(x => x.TaskTemplates)
                    .Where(x => x.Steps != null)
                    .SelectMany(x => x.Steps)
                    .Select(x => x.IsVideo ? x.VideoThumbnail : x.Picture)
                    .Distinct()
                    .ToList();

                foreach (var url in stepsUrls)
                {
                    AddMediaPictureUrl(mediaPicturesUrls, url);
                }
            }

            // Completed audits
            if (completedAudits != null)
            {
                foreach (AuditsModel completedAudit in completedAudits)
                {
                    if (completedAudit.Tasks != null)
                    {
                        foreach (TasksTaskModel completedAuditTask in completedAudit.Tasks)
                        {
                            if (completedAuditTask.Attachments?.Count > 0)
                                completedAuditTask.Attachments.Where(x => x.AttachmentType.ToLower() == "pdf")
                                    .ForEach(x => attachmentsUrls.Enqueue(x.Uri));

                            AddMediaPictureUrl(mediaPicturesUrls, completedAuditTask.Picture);

                            if (completedAuditTask.PictureProof?.Media != null)
                            {
                                foreach (var pictureproofurl in completedAuditTask.PictureProof.Media)
                                {
                                    AddMediaPictureUrl(mediaPicturesUrls, pictureproofurl.UriPart);
                                }
                            }
                        }
                        if (completedAudit.Signatures != null)
                        {
                            foreach (var signature in completedAudit.Signatures)
                            {
                                AddMediaPictureUrl(mediaPicturesUrls, signature.SignatureImage);
                            }
                        }
                    }
                    AddMediaPictureUrl(mediaPicturesUrls, completedAudit.Picture);

                }
            }

            // Audtis templates
            if (auditTemplates != null)
            {
                foreach (AuditTemplateModel auditTemplateModel in auditTemplates)
                {
                    if (auditTemplateModel.TaskTemplates != null)
                    {
                        foreach (TaskTemplateModel auditTemplate in auditTemplateModel.TaskTemplates)
                        {
                            if (auditTemplate.Attachments?.Count > 0)
                                auditTemplate.Attachments.Where(x => x.AttachmentType.ToLower() == "pdf")
                                    .ForEach(x => attachmentsUrls.Enqueue(x.Uri));

                            var thumbnail = !string.IsNullOrEmpty(auditTemplate.VideoThumbnail) ? auditTemplate.VideoThumbnail : auditTemplate.Picture;
                            AddMediaPictureUrl(mediaPicturesUrls, thumbnail);
                        }
                    }
                    AddMediaPictureUrl(mediaPicturesUrls, auditTemplateModel.Picture);
                }

                var stepsUrls = auditTemplates
                    .Where(x => x.TaskTemplates != null)
                    .SelectMany(x => x.TaskTemplates)
                    .Where(x => x.Steps != null)
                    .SelectMany(x => x.Steps)
                    .Select(x => x.IsVideo ? x.VideoThumbnail : x.Picture)
                    .Distinct()
                    .ToList();

                foreach (var url in stepsUrls)
                {
                    AddMediaPictureUrl(mediaPicturesUrls, url);
                }
            }

            // User's profile pictures
            if (companyUsers != null)
            {
                foreach (UserProfileModel userProfileModel in companyUsers)
                {
                    AddMediaPictureUrl(mediaPicturesUrls, userProfileModel.Picture);
                }
            }

            // Action comments
            if (actionComments != null)
            {
                foreach (ActionCommentModel actionComment in actionComments)
                {
                    // Images 
                    if (actionComment.Images != null)
                    {
                        foreach (string commentImage in actionComment.Images)
                        {
                            AddMediaPictureUrl(mediaPicturesUrls, commentImage);
                        }
                    }

                    // Video thumbnails
                    if (!actionComment.VideoThumbnail.IsNullOrWhiteSpace())
                    {
                        AddMediaPictureUrl(mediaPicturesUrls, actionComment.VideoThumbnail);
                    }
                }
            }

            // Actions
            if (actions != null)
            {
                foreach (ActionsModel action in actions)
                {
                    if (action.Images != null)
                    {
                        foreach (string imageUrl in action.Images)
                        {
                            AddMediaPictureUrl(mediaPicturesUrls, imageUrl);
                        }
                    }

                    if (action.VideoThumbNails != null)
                    {
                        foreach (string thumbnailUrl in action.VideoThumbNails)
                        {
                            AddMediaPictureUrl(mediaPicturesUrls, thumbnailUrl);
                        }
                    }
                }
            }

            // Tasks
            if (tasks != null)
            {
                foreach (BasicTaskModel task in tasks)
                {
                    if (task.Attachments?.Count > 0)
                        task.Attachments.Where(x => x.AttachmentType.ToLower() == "pdf")
                            .ForEach(x => attachmentsUrls.Enqueue(x.Uri));

                    var thumbnail = !string.IsNullOrEmpty(task.VideoThumbnail) ? task.VideoThumbnail : task.Picture;
                    AddMediaPictureUrl(mediaPicturesUrls, thumbnail);

                    if (task.PictureProof?.Media != null)
                    {
                        foreach (var pictureproofurl in task.PictureProof.Media)
                        {
                            AddMediaPictureUrl(mediaPicturesUrls, pictureproofurl.UriPart);
                        }
                    }
                }

                var stepsUrls = tasks
                    .Where(x => x.HasSteps)
                    .SelectMany(x => x.Steps)
                    .Select(x => x.IsPicture ? x.Picture : x.VideoThumbnail)
                    .Distinct()
                    .ToList();

                foreach (var url in stepsUrls)
                {
                    AddMediaPictureUrl(mediaPicturesUrls, url);
                }
            }

            // Task templates
            if (taskTemplates != null)
            {
                foreach (TaskTemplateModel taskTemplate in taskTemplates)
                {
                    if (taskTemplate.Attachments?.Count > 0)
                        taskTemplate.Attachments.Where(x => x.AttachmentType.ToLower() == "pdf")
                            .ForEach(x => attachmentsUrls.Enqueue(x.Uri));

                    AddMediaPictureUrl(mediaPicturesUrls, taskTemplate.Picture);
                }
            }

            // Instructions
            if (instructionTemplates != null)
            {
                foreach (var template in instructionTemplates)
                {
                    AddMediaPictureUrl(mediaPicturesUrls, template.Picture);
                }

                var instructionUrls = instructionTemplates
                   .Where(x => x.NumberOfInstructionItems > 0)
                   .SelectMany(x => x.InstructionItems)
                   .Select(x => x.Picture)
                   .Distinct()
                   .ToList();

                foreach (var url in instructionUrls)
                {
                    AddMediaPictureUrl(mediaPicturesUrls, url);
                }
            }


            // Retrieve files that are already cached and use them to make sure that items won't get cached multiple times.
            IEnumerable<string> cachedFiles = _cachingService.GetCachedFilenames();

            List<string> mediaUrlsToCache = new List<string>();

            foreach (string pictureUrl in mediaPicturesUrls)
            {
                string requestCacheFilename = _cachingService.GetRequestCacheFilename(string.Format(Constants.MediaBaseUrl, pictureUrl));

                if (!cachedFiles.Contains(requestCacheFilename))
                    mediaUrlsToCache.Add(pictureUrl);
            }

            mediaFiles = new Queue<string>(mediaUrlsToCache);

            return mediaUrlsToCache;
        }

        private static void AddMediaPictureUrl(List<string> mediaPicturesUrls, string url)
        {
            if (url.IsNullOrEmpty())
                return;

            var safeUrl = RemoveMediaFromUrl(url);
            if (IsMediaUrlValid(safeUrl, mediaPicturesUrls))
                mediaPicturesUrls.Add(safeUrl);
        }

        private readonly HashSet<Guid> processingItems = new();
        public async Task UploadUnpostedData()
        {
            await AsyncAwaiter.AwaitAsync(nameof(UploadUnpostedData), async () =>
             {
                 var requestHelper = RequestHelper.Instance();

                 while (requestHelper.HasAny())
                 {
                     if (!await InternetHelper.HasInternetConnection())
                         break;

                     var request = await requestHelper.ReadRequest();

                     if (request == null || processingItems.Contains(request.LocalGuid))
                         break;

                     processingItems.Add(request.LocalGuid);

                     var stringContent = new System.Net.Http.StringContent(request.ContentAsString);
                     var result = await _apiClient.PostAsync(request.Uri, stringContent);
                     if (!result.IsSuccessStatusCode)
                         await requestHelper.AddRequest(request.Uri, request);

                     processingItems.Remove(request.LocalGuid);
                 }
             }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
            _auditService.Dispose();
            _checklistService.Dispose();
            _updateService.Dispose();
            _userService.Dispose();
            _actionService?.Dispose();
            _taskReportService.Dispose();
            _taskService.Dispose();
            _shiftService.Dispose();
            _workAreaService.Dispose();
            _pdfService.Dispose();
            _taskPropertiesService.Dispose();
            _taskCommentsService.Dispose();
            _apiClient.Dispose();
        }
    }
}
