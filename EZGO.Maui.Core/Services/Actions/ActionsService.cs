using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Utils;
using Plugin.Media.Abstractions;
using System.Diagnostics;
using System.Text;

namespace EZGO.Maui.Core.Services.Actions
{
    public class ActionsService : IActionsService
    {
        private const string _cat = "[ActionsService]\n\t";
        private readonly IApiRequestHandler _apiRequestHandler;
        private List<string> loadingResultlist = new List<string>();

        private readonly IFileService _fileService;
        private readonly IMediaService _mediaService;
        private readonly IWorkAreaService _workAreaService;
        private readonly IMessageService _messageService;

        private const string localActionsFilename = "localactions.json";
        private const string localActionCommentsFilename = "localcomments.json";
        private const string localResolvedActionIdsFilename = "localresolvedactionids.json";
        private const string localResolvedLocalActionIdsFilename = "localresolvedlocalactionids.json";
        private const string localCommentsViewedActionIds = "localcommentsviewedactionids.json";

        public ActionsService(IMediaService mediaService, IApiRequestHandler apiRequestHandler, IWorkAreaService workAreaService, IMessageService messageService, IFileService fileService)
        {
            _fileService = fileService;
            _mediaService = mediaService;
            _apiRequestHandler = apiRequestHandler;
            _workAreaService = workAreaService;
            _messageService = messageService;

        }

        public async Task<List<ActionCommentModel>> GetActionCommentsAsync(int actionId = 0, bool includeLocalActionComments = true, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = "actioncomments";
            if (actionId != 0) { uri += $"?actionid={actionId}"; }

            List<ActionCommentModel> result = await _apiRequestHandler.HandleListRequest<ActionCommentModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            result.ForEach(x => x.ModifiedAt = x.ModifiedAt?.ToLocalTime());

            if (includeLocalActionComments)
            {
                List<ActionCommentModel> localActionComments = await GetLocalActionCommentsAsync().ConfigureAwait(false);

                if (localActionComments?.Any() ?? false)
                {
                    if (actionId != 0)
                    {
                        // Only include local comments for this action
                        localActionComments = localActionComments
                            .Where(c => c.ActionId == actionId || (c.ActionId == 0 && c.LocalActionId == actionId))
                            .ToList();
                    }
                    result.AddRange(localActionComments);
                }
            }

            return result;
        }

        public async Task<List<ActionsModel>> GetReportActionsAsync(int tasktemplateId = 0, int assignedUserId = 0, bool withIncludes = true, bool applySort = true, bool refresh = false, bool isFromSyncService = false)
        {
            string uri = "actions?include=unviewedcommentnr,mainparent&limit=0";
            if (tasktemplateId != 0) { uri += $"&tasktemplateid={tasktemplateId}"; }
            if (assignedUserId != 0) { uri += $"&assigneduserid={assignedUserId}"; }

            List<ActionsModel> result = await _apiRequestHandler.HandleListRequest<ActionsModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            if (result.Any())
            {
                if (withIncludes)
                {
                    await LoadAssignedAreasForActionsAsync(result, refresh).ConfigureAwait(false);
                    await LoadAssignedUsersForActionsAsync(result, refresh).ConfigureAwait(false);
                    await LoadCommentsForActionsAsync(result).ConfigureAwait(false);
                }

                if (applySort)
                    result = result.SortActions();
            }

            return result;
        }

        public async Task<ActionCountersModel> GetActionsCount(bool refresh = false, bool isFromSyncService = false)
        {
            string uri = "actions/started_by_me/counts";
            var actionsCount = await _apiRequestHandler
                                .HandleRequest<ActionCountersModel>(uri, refresh, isFromSyncService)
                                .ConfigureAwait(false);

            return actionsCount;
        }

        //NOTE! AssignedUserId can not be used with AssignedAreaId for now.
        public async Task<List<ActionsModel>> GetActionsAsync(
            int userId = 0, int assignedUserId = 0, int tasktemplateId = 0,
            bool withIncludes = true, bool applySort = true, bool includeLocalActions = true,
            bool refresh = false, int assignedAreadId = 0, bool isFromSyncService = false,
            bool createdByOrAssignedToMe = false, int limit = 0)
        {
#if DEBUG
            var st = new Stopwatch();
            st.Start();
#endif
            //var resolvedFrom = DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd");
            //var resolvedto = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var resolvedCutOffDate = DateTime.UtcNow.AddDays(-60).ToString("dd-MM-yyyy");
            var baseParams = new List<string> { "include=unviewedcommentnr,mainparent" };
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                baseParams[0] += ",tags";

            if (assignedAreadId != 0) baseParams.Add($"assignedareaid={assignedAreadId}");
            if (tasktemplateId != 0) baseParams.Add($"tasktemplateid={tasktemplateId}");
            if (userId != 0) baseParams.Add($"createdbyid={userId}");
            if (createdByOrAssignedToMe != false)
                baseParams.Add($"createdByOrAssignedToMe={true}");
            else if (assignedUserId != 0) baseParams.Add($"assigneduserid={assignedUserId}");

            baseParams.Add($"limit={limit}");

            var allParams = new List<string>(baseParams)
            {
                //"isunresolved=true",
                //"isoverdue=true",
                //"isresolved=true",
                //$"resolvedfrom={resolvedFrom}",
                //$"resolvedto={resolvedto}",
                $"resolvedcutoffdate={resolvedCutOffDate}"
            };
            var uri = "actions?" + string.Join("&", allParams);
            var actions = await _apiRequestHandler
                                .HandleListRequest<ActionsModel>(uri, refresh, isFromSyncService)
                                .ConfigureAwait(false);

            // de‑duplicate while preserving order
            var seen = new HashSet<int?>();
            var result = new List<ActionsModel>();
            foreach (var a in actions)
                if (seen.Add(a.Id))
                    result.Add(a);

#if DEBUG
            Debug.WriteLine($"Fetched and merged actions in: {st.ElapsedMilliseconds} ms", _cat);
            var lastElapsed = st.ElapsedMilliseconds;
#endif

            if (includeLocalActions)
            {
                var localActions = await GetLocalActionsAsync().ConfigureAwait(false);
                var resolvedIds = await GetLocalResolvedActionIdsAsync(localResolvedLocalActionIdsFilename).ConfigureAwait(false);

                if (localActions?.Any() == true)
                {
                    var resultDict = result?.ToDictionary(a => (int?)a.Id) ?? new Dictionary<int?, ActionsModel>();

                    foreach (var local in localActions)
                    {
                        if (local.Id > 0 && resultDict.ContainsKey(local.Id))
                            result.Remove(resultDict[local.Id]);

                        if (resolvedIds.Contains(local.LocalId))
                            local.IsResolved = true;

                        result.Add(local);
                    }

#if DEBUG
                    Debug.WriteLine($"Loaded local actions in: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                    lastElapsed = st.ElapsedMilliseconds;
#endif
                }

                var resolvedOnlineIds = await GetLocalResolvedActionIdsAsync(localResolvedActionIdsFilename).ConfigureAwait(false);

                var resolvedSet = new HashSet<int?>(resolvedOnlineIds);
                foreach (var action in result)
                {
                    if (resolvedSet.Contains(action.Id))
                        action.IsResolved = true;
                }

#if DEBUG
                Debug.WriteLine($"Marked resolved actions in: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif
            }

            if (result?.Any() == true)
            {
                if (withIncludes)
                {
#if DEBUG
                    var sw = Stopwatch.StartNew();
                    var totals = 0L;
#endif

                    await LoadAssignedAreasForActionsAsync(result, refresh).ConfigureAwait(false);
#if DEBUG
                    loadingResultlist.Add($"GetAssignedAreasAsync: {sw.ElapsedMilliseconds} ms");
                    totals += sw.ElapsedMilliseconds;
#endif

                    await LoadAssignedUsersForActionsAsync(result, refresh).ConfigureAwait(false);
#if DEBUG
                    loadingResultlist.Add($"GetAssignedUsersAsync: {sw.ElapsedMilliseconds - totals} ms");
                    totals += sw.ElapsedMilliseconds - totals;
#endif

                    await LoadCommentsForActionsAsync(result, refresh).ConfigureAwait(false);
#if DEBUG
                    loadingResultlist.Add($"GetActionCommentsAsync: {sw.ElapsedMilliseconds - totals} ms");
                    sw.Stop();
#endif
                }

                if (applySort)
                    result = result.SortActions();

#if DEBUG
                Debug.WriteLine($"Sorting and includes took: {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif

                // Convert timestamps to local time in one pass
                foreach (var action in result)
                {
                    action.CreatedAt = action.CreatedAt?.ToLocalTime();
                    action.ModifiedAt = action.ModifiedAt?.ToLocalTime();
                    action.ResolvedAt = action.ResolvedAt?.ToLocalTime();
                }
            }

            return result ?? new List<ActionsModel>();
        }

        public async Task<List<ActionUser>> GetAssignedUsersAsync(bool refresh = false, bool isFromSyncService = false)
        {
            const string uri = "actions/assignedusers";

            List<ActionUser> result = await _apiRequestHandler.HandleListRequest<ActionUser>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ActionArea>> GetAssignedAreasActionsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            const string uri = "actions/assignedareas";

            List<ActionArea> result = await _apiRequestHandler.HandleListRequest<ActionArea>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<ActionsModel> GetOnlineActionAsync(int id, bool isFromSyncService = false)
        {
            string uri = $"action/{id}?include=comments,assignedusers,mainparent,assignedareas";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri += ",tags";

            ActionsModel result = await _apiRequestHandler.HandleRequest<ActionsModel>(uri, true, isFromSyncService).ConfigureAwait(false);

            result.CreatedAt = result.CreatedAt?.ToLocalTime();
            result.ModifiedAt = result.ModifiedAt?.ToLocalTime();
            result.ResolvedAt = result.ResolvedAt?.ToLocalTime();

            return result;
        }

        public async Task<ActionsModel> GetLocalActionAsync(int localId)
        {
            List<ActionsModel> actions = await GetActionsAsync(applySort: false).ConfigureAwait(false);

            ActionsModel action = actions?.FirstOrDefault(item => item.LocalId.HasValue && item.LocalId.Value == localId);

            return action;
        }

        public async Task<List<ActionsModel>> GetAssignedUserActionsAsync(int id, bool refresh = false, bool isFromSyncService = false)
        {
            IEnumerable<ActionsModel> actions = await GetActionsAsync(assignedUserId: id, refresh: refresh, isFromSyncService: isFromSyncService).ConfigureAwait(false);


            if (actions.Any())
                actions = actions.SortActions();

            return actions.ToList();
        }

        public async Task<IEnumerable<ActionsModel>> GetActionsWithAssignedAreaAsync(bool refresh, int assignedAreaId, bool excludeSelfCreatedActions = false)
        {
            List<ActionsModel> result = new List<ActionsModel>();

            var workAreas = await _workAreaService.GetBasicWorkAreasAsync().ConfigureAwait(false);
            var flattenedWorkAreas = _workAreaService.GetFlattenedBasicWorkAreas(workAreas);
            var assignedArea = flattenedWorkAreas.Where(a => a.Id == assignedAreaId).ToList();
            var flattenedAreas = _workAreaService.GetFlattenedBasicWorkAreas(assignedArea);
            var workAreasId = flattenedAreas.Select(f => f.Id).ToList();

            var areaActions = await GetAssignedAreasActionsAsync().ConfigureAwait(false);
            areaActions = areaActions.Where(a => workAreasId.Contains(a.Id)).ToList();

            var actions = await GetActionsAsync().ConfigureAwait(false);
            foreach (var areaAction in areaActions)
            {
                var query = actions.Where(a => a.Id == areaAction.ActionId);
                if (excludeSelfCreatedActions)
                {
                    query = query.Where(a => a.CreatedById != UserSettings.Id);
                }
                var foundAction = query.FirstOrDefault();
                if (foundAction != null)
                    result.Add(foundAction);
            }
            return result;
        }

        public async Task<List<ActionsModel>> GetCreatedActionsAsync(int id, bool refresh = false, bool isFromSyncService = false)
        {
            IEnumerable<ActionsModel> actions = await GetActionsAsync(userId: id, refresh: refresh, isFromSyncService: isFromSyncService).ConfigureAwait(false);

            actions = actions.Where(action => action.CreatedById == id);

            if (actions.Any())
                actions = actions.SortActions();

            return actions.ToList();
        }

        public async Task<List<ActionsModel>> GetMyActionsAsync(bool createdByOrAssignedToMe, bool refresh = false, bool isFromSyncService = false)
        {
            var myActions = await GetActionsAsync(createdByOrAssignedToMe: createdByOrAssignedToMe, refresh: refresh, isFromSyncService: isFromSyncService);

            if (myActions.Any())
                myActions = myActions.SortActions();

            return myActions.ToList();
        }

        public async Task<bool> AddActionCommentAsync(ActionCommentModel comment)
        {
            bool result;

            comment.CreatedAt = DateTime.Now;
            comment.ModifiedAt = DateTime.Now;
            comment.CreatedBy = UserSettings.Fullname;
            comment.UserId = UserSettings.Id;
            comment.LocalId = Guid.NewGuid().ToString("N");
            comment.UnPosted = true;
            await AddLocalActionCommentAsync(comment).ConfigureAwait(false);

            result = true;
            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ChatChanged, comment); });

            return result;
        }

        public async Task<bool> SetActionCommentsViewedAsync(int id)
        {
            bool result;

            if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                result = await PostCommentsViewedAsync(id).ConfigureAwait(false);
            else
            {
                await AddLocalCommentsViewedActionIdAsync(id).ConfigureAwait(false);

                result = true;
            }

            return result;
        }

        public async Task<bool> SetActionResolvedAsync(BasicActionsModel resolvedAction)
        {
            bool result;

            if (await InternetHelper.HasInternetConnection().ConfigureAwait(false) && resolvedAction.Id != 0)
                result = await PostActionResolvedAsync(resolvedAction.Id).ConfigureAwait(false);
            else if (resolvedAction.Id != 0)
            {
                await AddLocalResolvedActionIdAsync(resolvedAction.Id, localResolvedActionIdsFilename).ConfigureAwait(false);

                result = true;
            }
            else
            {
                await AddLocalResolvedActionIdAsync(resolvedAction.LocalId, localResolvedLocalActionIdsFilename).ConfigureAwait(false);

                result = true;
            }


            if (result)
            {
                var action = await GetOnlineActionAsync(resolvedAction.Id).ConfigureAwait(false);

                //update cache
                if (action != null && action.TaskTemplateId.HasValue && action.TaskTemplateId.Value > 0)
                    await GetReportActionsAsync(tasktemplateId: action.TaskTemplateId.Value, refresh: true).ConfigureAwait(false);

                MessagingCenter.Send(this, Constants.ActionChanged, new ActionChangedMessageArgs()
                {
                    TaskTemplateId = action?.TaskTemplateId ?? -1,
                    TaskId = action?.TaskId ?? -1,
                    TypeOfChange = ActionChangedMessageArgs.ChangeType.SetToResolved,
                });
            }

            SendActionsChangedMessage();

            return result;
        }

        public async Task<bool> AddActionAsync(ActionsModel action)
        {
            string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionAdded);
            _messageService?.SendClosableInfo(message);

            action.Images ??= new List<string>();

            const string uri = "action/add";
            bool result = await SaveActionAsync(uri, action).ConfigureAwait(false);
            if (result)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, Constants.ActionChanged, new ActionChangedMessageArgs()
                    {
                        TaskTemplateId = action?.TaskTemplateId ?? -1,
                        TaskId = action?.TaskId ?? -1,
                        TypeOfChange = ActionChangedMessageArgs.ChangeType.Created,
                    });
                });
            }
            return result;
        }

        public async Task<bool> AddActionToAuditAsync(ActionsModel action)
        {
            string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionAdded);
            _messageService?.SendClosableInfo(message);

            action.Images ??= new List<string>();

            const string uri = "audit/actions/add";

            bool result = await SaveActionAsync(uri, action).ConfigureAwait(false);

            return result;
        }

        public async Task<bool> AddActionToChecklistAsync(ActionsModel action)
        {
            string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionAdded);
            _messageService?.SendClosableInfo(message);

            action.Images ??= new List<string>();

            const string uri = "checklist/actions/add";

            bool result = await SaveActionAsync(uri, action).ConfigureAwait(false);

            return result;
        }

        public async Task<bool> UpdateActionAsync(ActionsModel action)
        {
            action.Images ??= new List<string>();

            string uri = $"action/change/{action.Id}";

            bool result = await SaveActionAsync(uri, action).ConfigureAwait(false);

            return result;
        }

        public async Task<List<ActionsModel>> GetOpenActionsForTaskAsync(long taskId)
        {
            List<ActionsModel> actions = await GetActionsAsync().ConfigureAwait(false);

            List<ActionsModel> result = actions.Where(item => item.TaskId.HasValue && item.TaskId.Value == taskId && item.IsResolved.HasValue && !item.IsResolved.Value).ToList();

            return result;
        }

        public async Task<List<ActionsModel>> GetOpenActionsForTaskTemplateAsync(int taskTemplateId, List<ActionsModel> actions = null)
        {
            if (actions.IsNullOrEmpty())
                actions = await GetActionsAsync().ConfigureAwait(false);

            List<ActionsModel> result = actions.Where(item => item.TaskTemplateId.HasValue && item.TaskTemplateId.Value == taskTemplateId && item.IsResolved.HasValue && !item.IsResolved.Value).ToList();

            return result;
        }

        public async Task<List<ActionsModel>> GetResolvedActionsForTaskTemplateAsync(int taskTemplateId, DateTime? startedAt = null, List<ActionsModel> actions = null)
        {
            if (actions.IsNullOrEmpty())
                actions = await GetActionsAsync().ConfigureAwait(false);

            var result = actions.Where(item => item.TaskTemplateId.HasValue && item.TaskTemplateId.Value == taskTemplateId && item.IsResolved.HasValue && item.IsResolved.Value);

            if (startedAt != null)
                result = result.Where(r => r.CreatedAt > startedAt);

            return result.ToList();
        }

        public async Task<List<ActionsModel>> GetAllActionsForTaskAsync(long taskId)
        {
            List<ActionsModel> actions = await GetActionsAsync().ConfigureAwait(false);

            List<ActionsModel> result = actions.Where(item => item.TaskId.HasValue && item.TaskId.Value == taskId).ToList();

            return result;
        }

        public async Task<List<ActionsModel>> GetAllActionsForTaskTemplateAsync(int taskTemplateId)
        {
            List<ActionsModel> actions = await GetActionsAsync().ConfigureAwait(false);

            List<ActionsModel> result = actions.Where(item => item.TaskTemplateId.HasValue && item.TaskTemplateId.Value == taskTemplateId).ToList();

            return result;
        }

        private string GetUrl(UrlParameters urlParameters, StringBuilder uri)
        {
            List<string> parameters = new List<string>();
            parameters.Add("include=unviewedcommentnr,mainparent");

            if (urlParameters.UserId != 0)
            {
                parameters.Add($"createdbyid={urlParameters.UserId}");
            }
            else if (urlParameters.AssignedUserId != 0)
            {
                parameters.Add($"assigneduserid={urlParameters.AssignedUserId}");
            }

            if (urlParameters.TaskId != 0)
            {
                parameters.Add($"taskid={urlParameters.TaskId}");
            }
            parameters.Add("limit=0");

            uri.Append(parameters.Aggregate((a, b) => a + '&' + b));

            return uri.ToString();
        }


        #region updatelocals

        public async Task UploadLocalResolvedActionIdsAsync()
        {
            List<int?> actionIds = await GetLocalResolvedActionIdsAsync(localResolvedActionIdsFilename).ConfigureAwait(false);

            var Iterations = actionIds?.Count ?? 0;
            for (int i = 0; i < Iterations; i++)
            {
                int? actionId = actionIds[i];

                if (await InternetHelper.HasInternetConnection())
                {
                    bool result = await PostActionResolvedAsync((int)actionId).ConfigureAwait(false);

                    if (result)
                    {
                        actionIds.Remove(actionId);

                        await SaveLocalResolvedActionIdsAsync(actionIds, localResolvedActionIdsFilename).ConfigureAwait(false);
                    }
                }
                else
                    break;
            }
        }

        public async Task UploadLocalCommentsViewedActionIdsAsync()
        {
            List<int> actionIds = await GetLocalCommentsViewedActionIdsAsync().ConfigureAwait(false);

            var Iterations = actionIds?.Count ?? 0;
            for (int i = 0; i < Iterations; i++)
            {
                int actionId = actionIds[i];

                if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    bool result = await PostCommentsViewedAsync(actionId).ConfigureAwait(false);

                    //if (result)
                    {
                        actionIds.Remove(actionId);

                        await SaveLocalCommentsViewedActionIdsAsync(actionIds).ConfigureAwait(false);
                    }
                }
                else
                    break;
            }
        }

        private async Task<bool> UploadMediaItemsAsync(ActionsModel action)
        {
            IEnumerable<MediaItem> mediaItems = action.LocalMediaItems;

            while (mediaItems.Any())
            {
                MediaItem mediaItem = mediaItems.FirstOrDefault();

                if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    await _mediaService.UploadMediaItemAsync(mediaItem, MediaStorageTypeEnum.Actions, 0).ConfigureAwait(false);

                    if (mediaItem.IsVideo)
                    {
                        action.Videos ??= new List<string>();
                        action.VideoThumbNails ??= new List<string>();

                        action.Videos.Add(mediaItem.VideoUrl);
                        action.VideoThumbNails.Add(mediaItem.PictureUrl);
                    }
                    else
                    {
                        action.Images ??= new List<string>();

                        action.Images.Add(mediaItem.PictureUrl);
                    }

                    action.LocalMediaItems.Remove(mediaItem);
                }
                else
                    break;
            }

            bool result = !mediaItems.Any();

            return result;
        }

        private async Task<bool> UploadMediaItemsAsync(ActionCommentModel comment)
        {
            List<MediaItem> mediaItems = comment.LocalMediaItems;

            while (mediaItems.Any())
            {
                MediaItem mediaItem = mediaItems.FirstOrDefault();

                if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    await _mediaService.UploadMediaItemAsync(mediaItem, MediaStorageTypeEnum.ActionComments, 0).ConfigureAwait(false);

                    if (mediaItem.IsVideo)
                    {
                        comment.Video = mediaItem.VideoUrl;
                        comment.VideoThumbnail = mediaItem.PictureUrl;
                    }
                    else
                    {
                        comment.Images ??= new List<string>();

                        comment.Images.Add(mediaItem.PictureUrl);
                    }

                    comment.LocalMediaItems.Remove(mediaItem);
                }
                else
                    break;
            }

            bool result = !mediaItems.Any();

            return result;
        }

        #endregion

        private readonly static SemaphoreSlim SetViewedAllLock = new SemaphoreSlim(1, 1);

        private async Task<bool> PostCommentsViewedAsync(int id)
        {
            if (!SetViewedAllLock.Wait(0))
            {
                return false;
            }
            try
            {
                bool result;

                string uri = $"actioncomment/setviewedall/{id}";


                using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, id).ConfigureAwait(false))
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    bool.TryParse(responseContent, out result);

                    await UpdateActionsCacheAsync().ConfigureAwait(false);
                }

                return result;
            }
            catch { return false; }
            finally
            {
                SetViewedAllLock.Release();
            }
        }

        private async Task<bool> PostActionResolvedAsync(int id)
        {
            bool result;

            string uri = $"action/setresolved/{id}";

            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, true).ConfigureAwait(false))
            {
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                bool.TryParse(responseContent, out result);

                await UpdateActionsCacheAsync().ConfigureAwait(false);
            }

            return result;
        }

        private async Task<bool> SaveActionAsync(string uri, ActionsModel action)
        {
            bool result;
#if DEBUG
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            System.Diagnostics.Debug.WriteLine($"[SAVE]::Started saving Action::{timer.ElapsedMilliseconds}ms");
#endif
            if ((action.LocalMediaItems == null || !action.LocalMediaItems.Any()) && await InternetHelper.HasInternetConnection().ConfigureAwait(false))
            {

                int id = await PostActionAsync(uri, action).ConfigureAwait(false);
                result = id > 0;
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[SAVE]::Saving Action locally:: {timer.ElapsedMilliseconds}ms");
#endif
                action.ApiUri = uri;

                await AddLocalActionAsync(action).ConfigureAwait(false);

                result = true;

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[SAVE]::Action changed message:: {timer.ElapsedMilliseconds}ms");
#endif
            }

            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ActionChanged); });
            //SendActionsChangedMessage();

            return result;
        }

        private async Task<bool> PostActionCommentAsync(ActionComment comment)
        {
            bool result = false;

            const string uri = "actioncomment/add";

            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, comment).ConfigureAwait(false))
            {
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                int.TryParse(responseContent, out int actionCommentId);

                if (actionCommentId > 0)
                {
                    // the call is only made in a planned update so the actionscache will be updated anyways
                    //await UpdateActionsCacheAsync();
                    result = true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // userid is not allowed to send this, we delete it from the list as a result
                    result = true;
                }
            }

            return result;
        }

        private async Task<int> PostActionAsync(string uri, ActionsModel action)
        {
            int result = -1;
#if DEBUG
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            System.Diagnostics.Debug.WriteLine($"[POST]::Sending Post to API::{timer.ElapsedMilliseconds}ms");
#endif
            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, action).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[POST]::Recived response from API::{timer.ElapsedMilliseconds}ms");
#endif
                    await UpdateActionsCacheAsync().ConfigureAwait(false);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[POST]::Updated cache::{timer.ElapsedMilliseconds}ms");
#endif
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    bool isNumber = int.TryParse(json, out result);
                    if (!isNumber) result = action.Id;
                }
            }

            return result;
        }


        #region localActions
        private bool uploadLocalActionBusy;
        private List<ActionsModel> localActions = new List<ActionsModel>();

        public async Task UploadLocalActionsWithComments()
        {
            await UploadLocalActionsAsync().ConfigureAwait(false);
            await UploadLocalActionCommentsAsync().ConfigureAwait(false);
        }

        public async Task UploadLocalActionsAsync()
        {
            if (uploadLocalActionBusy)
                return;

            uploadLocalActionBusy = true;

            try
            {
                localActions = await GetLocalActionsAsync().ConfigureAwait(false);
                localActions ??= new List<ActionsModel>();

                var ids = await GetLocalResolvedActionIdsAsync(localResolvedLocalActionIdsFilename).ConfigureAwait(false);

                // create my list for the current company
                var mylocalActions = localActions.Where(x => x.CompanyId == UserSettings.CompanyId).ToList();

                if (mylocalActions.Any())
                {
                    var Iterations = mylocalActions.Count - 1;

                    for (int i = Iterations; i >= 0; i--)
                    {
                        ActionsModel action = mylocalActions[i];

                        bool mediaResult = true;

                        try
                        {
                            if (action.LocalMediaItems != null && action.LocalMediaItems.Any())
                            {
                                action.LocalMediaItems.ForEach(x => x.MediaFile = new MediaFile(x.MediaFile.Path, () => File.OpenRead(x.MediaFile.Path), null, null));
                                mediaResult = await UploadMediaItemsAsync(action).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        if (await InternetHelper.HasInternetAndApiConnectionAsync())
                        {
                            int id = await PostActionAsync(action.ApiUri, action).ConfigureAwait(false);

                            if (id > 0)
                            {
                                await SetActionIdInLocalComments(action.LocalId, id).ConfigureAwait(false);
                                if (ids.Contains(action.LocalId))
                                {
                                    bool isResolved = await PostActionResolvedAsync(id).ConfigureAwait(false);
                                    action.IsResolved = isResolved;
                                    ids.Remove(action.LocalId);
                                }
                                localActions.Remove(action);
                                await SaveLocalActionsAsync(localActions).ConfigureAwait(false);
                                //localActions = await GetLocalActionsAsync();
                            }
                            else
                            {
                                // we need to remove this here since we are also handling updates, which could return nothing (response != 200), and thus -1 if they fail
                                localActions.Remove(action);
                                await SaveLocalActionsAsync(localActions).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await SaveLocalActionsAsync(localActions).ConfigureAwait(false);
                            break;
                        }
                    }
                    SendActionsChangedMessage();

                    ids ??= new List<int?>();

                    await SaveLocalResolvedActionIdsAsync(ids, localResolvedLocalActionIdsFilename).ConfigureAwait(false);

                    //update cache
                    await GetActionsAsync(refresh: true).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {

            }
            finally { uploadLocalActionBusy = false; }
        }

        private async Task SetActionIdInLocalComments(int? localId, int id)
        {
            var localComments = await GetLocalActionCommentsAsync().ConfigureAwait(false);
            var comment = localComments.FirstOrDefault(c => c.LocalActionId == localId);
            if (comment != null)
            {
                comment.ActionId = id;
                await SaveLocalActionCommentsAsync(localComments).ConfigureAwait(false);
            }
        }

        private async Task AddLocalActionAsync(ActionsModel action)
        {
            localActions = await GetLocalActionsAsync().ConfigureAwait(false);

            action.CompanyId = UserSettings.CompanyId;

            if (action.LocalId.HasValue)
            {
                ActionsModel existingAction = localActions.SingleOrDefault(item => item.LocalId.HasValue && item.LocalId.Value == action.LocalId.Value);

                if (existingAction != null)
                    localActions.Remove(existingAction);
            }
            else
            {
                int localId = 1;

                if (localActions.Any())
                    localId = localActions.Where(item => item.LocalId.HasValue).Max(item => item.LocalId.Value) + 1;

                action.LocalId = localId;
            }

            localActions.Add(action);

            await SaveLocalActionsAsync(localActions).ConfigureAwait(false);
        }

        private async Task<List<ActionsModel>> GetLocalActionsAsync()
        {
            localActions = new List<ActionsModel>();

            string localActionsJson = await _fileService.ReadFromInternalStorageAsync(localActionsFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);

            if (!localActionsJson.IsNullOrWhiteSpace())
                localActions = JsonSerializer.Deserialize<List<ActionsModel>>(localActionsJson);



            return localActions;
        }

        private async Task SaveLocalActionsAsync(List<ActionsModel> actions)
        {
            string localActionsJson = JsonSerializer.Serialize(actions);

            await _fileService.SaveFileToInternalStorageAsync(localActionsJson, localActionsFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
        }

        #endregion


        #region localActionComments

        private bool localActionCommentsBusy;
        private static readonly SemaphoreSlim PostLocalActionsSemaphore = new SemaphoreSlim(1, 1);
        private List<ActionCommentModel> localComments = new List<ActionCommentModel>();

        public async Task<List<string>> UploadLocalActionCommentsAsync()
        {
            List<string> localids = new List<string>();

            if (localActionCommentsBusy)
                return new List<string>();

            localActionCommentsBusy = true;
            bool lockTaken = false;

            try
            {
                await PostLocalActionsSemaphore.WaitAsync().ConfigureAwait(false);
                lockTaken = true;
                localComments = await GetLocalActionCommentsAsync().ConfigureAwait(false);
                localComments ??= new List<ActionCommentModel>();

                var mylocalComments = localComments.Where(x => x.CompanyId == UserSettings.CompanyId && x.ActionId != 0).ToList();

                if (mylocalComments.Any())
                {
                    var Iterations = mylocalComments.Count;
                    for (int i = 0; i < Iterations; i++)
                    {
                        ActionCommentModel actionComment = mylocalComments[i];

                        bool mediaResult = true;

                        if (actionComment.LocalMediaItems != null && actionComment.LocalMediaItems.Any())
                        {
                            actionComment.LocalMediaItems.ForEach(x => x.MediaFile = new MediaFile(x.MediaFile.Path, () => File.OpenRead(x.MediaFile.Path), null, null));
                            mediaResult = await UploadMediaItemsAsync(actionComment).ConfigureAwait(false);
                        }

                        if (mediaResult && InternetHelper.HasInternetAndApiConnectionAsync().Result)
                        {
                            bool result = PostActionCommentAsync(actionComment).Result;

                            if (result)
                            {
                                localids.Add(actionComment.LocalId);

                                localComments.Remove(actionComment);

                                await SaveLocalActionCommentsAsync(localComments).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await SaveLocalActionCommentsAsync(localComments).ConfigureAwait(false);

                            break;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                localActionCommentsBusy = false;
                if (lockTaken)
                    PostLocalActionsSemaphore.Release();
            }

            return localids;
        }

        private async Task AddLocalActionCommentAsync(ActionCommentModel comment)
        {
            while (localActionCommentsBusy)
            {
                // wait
            }

            localActionCommentsBusy = true;

            try
            {
                comment.CompanyId = UserSettings.CompanyId;

                var localActionComments = await GetLocalActionCommentsAsync().ConfigureAwait(false);

                localActionComments.Add(comment);

                await SaveLocalActionCommentsAsync(localActionComments).ConfigureAwait(false);
            }
            catch { }
            finally { localActionCommentsBusy = false; }
        }

        private async Task<List<ActionCommentModel>> GetLocalActionCommentsAsync()
        {
            List<ActionCommentModel> localActionComments = new List<ActionCommentModel>();

            string localActionCommentsJson = await _fileService.ReadFromInternalStorageAsync(localActionCommentsFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);

            if (!localActionCommentsJson.IsNullOrWhiteSpace())
                localActionComments = JsonSerializer.Deserialize<List<ActionCommentModel>>(localActionCommentsJson);

            return localActionComments;
        }

        private async Task SaveLocalActionCommentsAsync(List<ActionCommentModel> comments)
        {
            string localActionCommentsJson = JsonSerializer.Serialize(comments);

            await _fileService.SaveFileToInternalStorageAsync(localActionCommentsJson, localActionCommentsFilename, Constants.PersistentDataDirectory).ConfigureAwait(false);
        }

        #endregion

        #region local actionIds
        private async Task AddLocalResolvedActionIdAsync(int? actionId, string filename)
        {
            List<int?> actionIds = await GetLocalResolvedActionIdsAsync(filename).ConfigureAwait(false);

            actionIds.Add(actionId);

            await SaveLocalResolvedActionIdsAsync(actionIds, filename).ConfigureAwait(false);
        }

        private async Task<List<int?>> GetLocalResolvedActionIdsAsync(string filename)
        {
            List<int?> actionIds = new List<int?>();

            string actionIdsJson = await _fileService.ReadFromInternalStorageAsync(filename, Constants.PersistentDataDirectory).ConfigureAwait(false);

            if (!actionIdsJson.IsNullOrWhiteSpace())
                actionIds = JsonSerializer.Deserialize<List<int?>>(actionIdsJson);

            return actionIds;
        }

        private async Task SaveLocalResolvedActionIdsAsync(List<int?> actionIds, string filename)
        {
            string actionIdsJson = JsonSerializer.Serialize(actionIds);

            await _fileService.SaveFileToInternalStorageAsync(actionIdsJson, filename, Constants.PersistentDataDirectory).ConfigureAwait(false);
        }


        private async Task AddLocalCommentsViewedActionIdAsync(int actionId)
        {
            List<int> actionIds = await GetLocalCommentsViewedActionIdsAsync().ConfigureAwait(false);

            actionIds.Add(actionId);

            await SaveLocalCommentsViewedActionIdsAsync(actionIds).ConfigureAwait(false);
        }

        private async Task<List<int>> GetLocalCommentsViewedActionIdsAsync()
        {
            List<int> actionIds = new List<int>();

            string actionIdsJson = await _fileService.ReadFromInternalStorageAsync(localCommentsViewedActionIds, Constants.SessionDataDirectory).ConfigureAwait(false);

            if (!actionIdsJson.IsNullOrWhiteSpace())
                actionIds = JsonSerializer.Deserialize<List<int>>(actionIdsJson);

            return actionIds;
        }

        private async Task SaveLocalCommentsViewedActionIdsAsync(List<int> actionIds)
        {
            string actionIdsJson = JsonSerializer.Serialize(actionIds);

            await _fileService.SaveFileToInternalStorageAsync(actionIdsJson, localCommentsViewedActionIds, Constants.SessionDataDirectory).ConfigureAwait(false);
        }

        #endregion


        public async Task LoadAssignedAreasForActionsAsync(IEnumerable<ActionsModel> actions, bool refresh)
        {
            if (actions == null)
                return;

            List<ActionArea> assignedAreas = await GetAssignedAreasActionsAsync(refresh).ConfigureAwait(false);

            if (assignedAreas.IsNullOrEmpty())
                return;

            foreach (ActionsModel action in actions)
            {
                List<AreaBasic> areas = assignedAreas.Where(item => item.ActionId == action.Id)
                    .Select(item => new AreaBasic { Id = item.Id, Name = item.Name }).ToList();

                action.AssignedAreas = areas;
            }
        }

        public async Task LoadAssignedUsersForActionsAsync(IEnumerable<ActionsModel> actions, bool refresh)
        {
            if (actions == null)
                return;

            List<ActionUser> assignedUsers = await GetAssignedUsersAsync(refresh).ConfigureAwait(false);

            foreach (ActionsModel action in actions)
            {
                if (action.AssignedUsers == null || !action.AssignedUsers.Any())
                {
                    List<UserBasic> users = assignedUsers.Where(item => item.ActionId == action.Id)
                        .Select(item => new UserBasic { Id = item.Id, Name = item.Name, Picture = item.Picture }).ToList();

                    action.AssignedUsers = users;
                }
            }
        }

        private async Task LoadCommentsForActionsAsync(IEnumerable<ActionsModel> actions, bool includeLocalActionComments = true, bool refresh = false)
        {
            List<ActionCommentModel> comments = await GetActionCommentsAsync(includeLocalActionComments: includeLocalActionComments, refresh: refresh).ConfigureAwait(false);

            foreach (ActionsModel action in actions)
            {
                List<ActionCommentModel> actionComments;
                if (action.Id == 0)
                {
                    actionComments = comments?.Where(c => c.LocalActionId == action.LocalId).ToList();
                }
                else
                {
                    actionComments = comments?.Where(item => item.ActionId == action.Id).ToList();
                }
                actionComments ??= new List<ActionCommentModel>();
                action.Comments = actionComments;
            }
        }

        private async Task UpdateActionsCacheAsync()
        {
            using (var scope = App.Container.CreateScope())
            {
                ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

                await syncService.LoadActionsAsync().ConfigureAwait(false);
            }
        }

        public void SendActionsChangedMessage(string messageKey = Constants.ActionsChanged)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, messageKey);
            });

        }

        public void Dispose()
        {
            //_apiRequestHandler?.Dispose();
            //_mediaService
            _workAreaService?.Dispose();
            //_fileService.Disopse();
        }
    }
}
