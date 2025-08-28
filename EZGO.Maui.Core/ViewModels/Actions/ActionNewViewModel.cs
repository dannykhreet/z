using Autofac;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionNewViewModel : BaseViewModel
    {
        private readonly IMediaHelper _mediaHelper;
        private readonly IMediaService _mediaService;
        private readonly IWorkAreaService _workAreaService;
        private readonly ITagsService _tagsService;


        private double timedifference;

        public int TaskTemplateId { get; set; }

        public long? TaskId { get; set; }
        public bool IsFromHomeScreen { get; set; } = false;

        public ActionType ActionType { get; set; }

        public ActionsModel Action { get; set; }

        public DateTime MaxDate { get; set; } = DateTime.Now.AddYears(10);

        private string description;

        public bool IsUltimoEnabled { get; set; }

        public bool IsUltimoSwitchVisible => CompanyFeatures.CompanyFeatSettings.MarketUltimoEnabled;

        public List<TagModel> Tags { get; set; }

        [DoNotNotify]
        public BasicTaskTemplateModel LocalTask { get; set; }


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get => description;
            set
            {
                description = value;

                OnPropertyChanged();

                _statusBarService.HideStatusBar();
            }
        }

        private string comment;

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get => comment;
            set
            {
                comment = value;

                OnPropertyChanged();

                _statusBarService.HideStatusBar();
            }
        }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        /// <value>
        /// The due date.
        /// </value>
        public DateTime? DueDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Gets or sets resources.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public ObservableCollection<ResourceModel> Resources { get; set; }

        /// <summary>
        /// Gets or sets the selected resources.
        /// </summary>
        /// <value>
        /// The selected resources.
        /// </value>
        public List<ResourceModel> SelectedResources { get; set; } = new List<ResourceModel>();

        /// <summary>
        /// Gets or sets the popup resources.
        /// </summary>
        /// <value>
        /// The popup resources.
        /// </value>
        public ObservableCollection<ResourceModel> PopupResources { get; set; }

        /// <summary>
        /// Gets or sets the selected user names.
        /// </summary>
        /// <value>
        /// The selected user names.
        /// </value>
        public ObservableCollection<ResourceSelectionModel> SelectedResourceNames { get; set; }

        public ResourceModel SelectedItem { get; set; }

        private bool isUserPopupOpen;

        /// <summary>
        /// Gets or sets a value indicating whether the user popup is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user popup is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserPopupOpen
        {
            get => isUserPopupOpen;
            set
            {
                isUserPopupOpen = value;

                OnPropertyChanged();

                if (value == false)
                    _statusBarService.HideStatusBar();
            }
        }

        /// <summary>
        /// Gets or sets the popup users.
        /// </summary>
        /// <value>
        /// The popup users.
        /// </value>
        public ObservableCollection<UserProfileModel> PopupUsers { get; set; }

        private string autoCompleteText;

        /// <summary>
        /// Gets or sets the automatic complete text.
        /// </summary>
        /// <value>
        /// The automatic complete text.
        /// </value>
        public string AutoCompleteText
        {
            get => autoCompleteText;
            set
            {
                autoCompleteText = value;

                OnPropertyChanged();
            }
        }

        private MediaItem mediaItem1;

        public MediaItem MediaItem1
        {
            get => mediaItem1;

            set
            {
                mediaItem1 = value;

                OnPropertyChanged();
            }
        }

        private MediaItem mediaItem2;

        public MediaItem MediaItem2
        {
            get => mediaItem2;

            set
            {
                mediaItem2 = value;

                OnPropertyChanged();
            }
        }

        private MediaItem mediaItem3;

        public MediaItem MediaItem3
        {
            get => mediaItem3;

            set
            {
                mediaItem3 = value;

                OnPropertyChanged();
            }
        }

        private MediaItem mediaItem4;

        public MediaItem MediaItem4
        {
            get => mediaItem4;

            set
            {
                mediaItem4 = value;

                OnPropertyChanged();
            }
        }

        private MediaItem mediaItem5;

        public MediaItem MediaItem5
        {
            get => mediaItem5;

            set
            {
                mediaItem5 = value;

                OnPropertyChanged();
            }
        }

        private MediaItem mediaItem6;

        public MediaItem MediaItem6
        {
            get => mediaItem6;

            set
            {
                mediaItem6 = value;

                OnPropertyChanged();
            }
        }

        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;

                OnPropertyChanged();
            }
        }

        //public override bool CanExecuteCommands()
        //{
        //    return !(IsLoading || IsRefreshing);
        //}

        //public override bool CanExecuteCommands(object commandParameter)
        //{
        //    return !(IsLoading || IsRefreshing);
        //}

        /// <summary>
        /// Gets the submit command.
        /// </summary>
        /// <value>
        /// The submit command.
        /// </value>
        public IAsyncCommand SubmitCommand => new AsyncCommand(async () =>
            await ExecuteLoadingActionAsync(SubmitAsync));

        /// <summary>
        /// Gets the open user popup command.
        /// </summary>
        /// <value>
        /// The open user popup command.
        /// </value>
        public ICommand OpenUserPopupCommand => new Microsoft.Maui.Controls.Command(() =>
            ExecuteLoadingAction(OpenUserPopup), CanExecuteCommands);

        /// <summary>
        /// Gets the close user popup command.
        /// </summary>
        /// <value>
        /// The close user popup command.
        /// </value>
        public ICommand CloseUserPopupCommand => new Microsoft.Maui.Controls.Command(() =>
            ExecuteLoadingAction(CloseUserPopup), CanExecuteCommands);

        /// <summary>
        /// Gets the auto complete selected command.
        /// </summary>
        /// <value>
        /// The auto complete selected command.
        /// </value>
        public ICommand AutoCompleteSelectedCommand => new Microsoft.Maui.Controls.Command<SelectionChangedEventArgs>(AutoCompleteSelected);

        /// <summary>
        /// Gets the remove resource command.
        /// </summary>
        /// <value>
        /// The remove resource command.
        /// </value>
        public ICommand RemoveResourceCommand => new Microsoft.Maui.Controls.Command<ResourceModel>((resourceModel) =>
        {
            ExecuteLoadingAction(() => RemoveResource(resourceModel));
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the add media command.
        /// </summary>
        /// <value>
        /// The add media command.
        /// </value>
        public ICommand AddMediaCommand => new Microsoft.Maui.Controls.Command<string>(async (mediaNumber) =>
        {
            await ExecuteLoadingActionAsync(async () => await AddMediaAsync(mediaNumber));
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the submit user popup command.
        /// </summary>
        /// <value>
        /// The submit user popup command.
        /// </value>
        public ICommand SubmitUserPopupCommand => new Microsoft.Maui.Controls.Command(() =>
            ExecuteLoadingAction(SubmitUserPopup), CanExecuteCommands);


        public ICommand SubmitDueDateCommand => new Microsoft.Maui.Controls.Command<object>((obj) =>
            ExecuteLoadingAction(() => SubmitDueDate(obj)), CanExecuteCommands);


        public ActionNewViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMediaHelper mediaHelper,
            IMediaService mediaService,
            IWorkAreaService workAreaService,
            ITagsService tagsService) : base(navigationService, userService, messageService, actionsService)
        {
            _mediaHelper = mediaHelper;
            _mediaService = mediaService;
            _workAreaService = workAreaService;
            _tagsService = tagsService;
        }

        ~ActionNewViewModel()
        { // Breakpoint here
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
             {
                 timedifference = DateTime.Now.Subtract(DateTime.UtcNow).TotalMinutes;

                 await SetResourcesForAutocomplete();

                 if (Action != null)
                 {
                     IsUltimoEnabled = Action.SendToUltimo;
                     await Task.Run(async () => await LoadAction());
                 }
                 else
                     Tags = await _tagsService.GetTagModelsAsync(tagableObjectEnum: TagableObjectEnum.Action);

                 await base.Init();
             });

            MessagingCenter.Subscribe<SyncService, List<UserProfileModel>>(this, Constants.ReloadUserDataMessage, async (sender, args) =>
            {
                await ReloadUsersData(args);
            });
        }

        /// <summary>
        /// Reloads users data
        /// </summary>
        /// <param name="args">New users data</param>
        /// <returns></returns>
        private async Task ReloadUsersData(List<UserProfileModel> args)
        {
            foreach (var item in Resources.Where(r => r.ActionResourceType == ActionResourceType.User).ToList())
            {
                Resources.Remove(item);
            }
            var userResources = args.Where(u => u.Id != UserSettings.Id).Select(u => new ResourceModel() { Id = u.Id, Text = u.FullName, ActionResourceType = ActionResourceType.User, Picture = u.Picture }).ToList();
            await Resources.AddRange(userResources);
        }

        private async Task SetResourcesForAutocomplete()
        {
            List<UserProfileModel> users = await _userService.GetCompanyUsersAsync();

            var userResources = users.Where(u => u.Id != UserSettings.Id).Select(u => new ResourceModel() { Id = u.Id, Text = u.FullName, ActionResourceType = ActionResourceType.User, Picture = u.Picture });

            Resources = new ObservableCollection<ResourceModel>(userResources);

            var allWorkAreas = await _workAreaService.GetBasicWorkAreasAsync();
            var workAreas = _workAreaService.GetFlattenedBasicWorkAreas(allWorkAreas);


            foreach (var workArea in workAreas)
            {
                if (workArea.Parent == null)
                {
                    Resources.Add(new ResourceModel() { Id = workArea.Id, Text = workArea.Name, ActionResourceType = ActionResourceType.Area });
                }
                else
                {
                    var text = "";
                    if (workArea.Parent.Parent != null)
                    {
                        text += ".../";
                    }
                    text += workArea.Parent.Name + "/" + workArea.Name;
                    Resources.Add(new ResourceModel() { Id = workArea.Id, Text = text, ActionResourceType = ActionResourceType.Area });
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<SyncService, List<UserProfileModel>>(this, Constants.ReloadUserDataMessage);
            PopupUsers = null;
            PopupResources = null;
            Action = null;
            MediaItem1 = null;
            MediaItem2 = null;
            MediaItem3 = null;
            MediaItem4 = null;
            MediaItem5 = null;
            MediaItem6 = null;
            Resources = null;
            SelectedResources = null;
            SelectedResourceNames = null;
            _workAreaService.Dispose();
            //_mediaService.
            //_mediaHelper.
            base.Dispose(disposing);
        }

        /// <summary>
        /// Opens the user popup.
        /// </summary>
        private void OpenUserPopup()
        {
            PopupResources = new ObservableCollection<ResourceModel>(SelectedResources);

            IsUserPopupOpen = true;
        }

        /// <summary>
        /// Closes the user popup.
        /// </summary>
        private void CloseUserPopup()
        {
            if (PopupResources != null)
            {
                IsUserPopupOpen = false;
                foreach (ResourceModel resourceModel in PopupResources)
                {
                    if (!SelectedResources.Contains(resourceModel) && !Resources.Contains(resourceModel))
                        Resources.Add(resourceModel);
                }
                AutoCompleteText = string.Empty;
            }
        }

        private async Task LoadAction()
        {
            if (Action == null)
                return;

            DueDate = Action.DueDate?.AddMinutes(timedifference) ?? DateTime.Now;
            IsUltimoEnabled = Action.SendToUltimo;
            //DueDate = Action.DueDate.Value;
            Description = Action.Description;
            Comment = Action.Comment;
            Tags = await _tagsService.GetTagModelsAsync(activeTags: Action.Tags, tagableObjectEnum: TagableObjectEnum.Action);

            ObservableCollection<ResourceModel> popupResources = new ObservableCollection<ResourceModel>();
            ObservableCollection<ResourceModel> resources = new ObservableCollection<ResourceModel>(Resources ?? new ObservableCollection<ResourceModel>());

            if (Action.AssignedUsers != null)
            {
                foreach (UserBasic user in Action.AssignedUsers)
                {
                    // Myself as resource
                    if (user.Id == UserSettings.Id)
                    {
                        ResourceModel myself = new ResourceModel() { Id = user.Id, ActionResourceType = ActionResourceType.User, Picture = user.Picture, Text = user.Name };
                        popupResources.Add(myself);
                        continue;
                    }

                    ResourceModel resourceModel = Resources?.FirstOrDefault(item => item.Id == user.Id && item.ActionResourceType == ActionResourceType.User);

                    if (resourceModel != null)
                    {
                        popupResources.Add(resourceModel);
                        resources.Remove(resourceModel);
                    }
                }

                PopupResources = popupResources;
                Resources = resources;
            }

            if (Action.AssignedAreas != null)
            {
                foreach (AreaBasic area in Action.AssignedAreas)
                {

                    ResourceModel resourceModel = Resources?.FirstOrDefault(item => item.Id == area.Id && item.ActionResourceType == ActionResourceType.Area);

                    if (resourceModel != null)
                    {
                        popupResources.Add(resourceModel);
                        resources.Remove(resourceModel);
                    }
                }

                PopupResources = popupResources;
                Resources = resources;
            }

            int mediaCount = 1;

            if (Action.Images != null)
            {
                foreach (string image in Action.Images)
                {
                    if (image == null)
                        continue;

                    MediaItem mediaItem = new MediaItem
                    {
                        PictureUrl = image
                    };

                    SetMedia(mediaCount, mediaItem);
                    mediaCount++;
                }
            }

            if (Action.Videos != null)
            {
                foreach (string video in Action.Videos)
                {
                    if (video == null)
                        continue;

                    int videoIndex = Action.Videos.IndexOf(video);
                    string thumbnailUrl = Action.VideoThumbNails?[videoIndex] ?? "";

                    MediaItem mediaItem = new MediaItem
                    {
                        IsVideo = true,
                        PictureUrl = thumbnailUrl,
                        VideoUrl = video
                    };

                    SetMedia(mediaCount, mediaItem);
                    mediaCount++;
                }
            }

            if (Action.LocalMediaItems != null)
            {
                foreach (MediaItem mediaItem in Action.LocalMediaItems)
                {
                    if (mediaItem == null)
                        continue;

                    SetMedia(mediaCount, mediaItem);
                    mediaCount++;
                }
            }

            SubmitUserPopup();
        }

        /// <summary>
        /// Submits the new action.
        /// </summary>
        private async Task SubmitAsync()
        {
#if DEBUG
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Statring submit of Action time: {timer.ElapsedMilliseconds}ms");
#endif
            IsBusy = true;
            bool hasInternet = await InternetHelper.HasInternetConnection();
            bool isError = false;
            bool result = false;

            bool setScreen = await SetScreenForPage();
            if (!setScreen) { IsBusy = false; _statusBarService.HideStatusBar(); return; }

            // Run the submit on not awaited task and exit the page
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await Task.Run(async () =>
            {
                try
                {
                    if (hasInternet)
                        await UploadMediaItemsAsync();
                }
                catch (Exception ex) when (ex is ArgumentNullException || ex is HttpRequestException)
                {
                    IsBusy = false;
                    isError = true;
                    Page page = NavigationService.GetCurrentPage();
                    string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);
                    await MainThread.InvokeOnMainThreadAsync(async () => await page.DisplayActionSheet("There was a problem with uploading media files. Please try again later.", null, ok));

                    return;
                }


                List<MediaItem> mediaItems = GetMediaItems();

                ActionsModel action = new();
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Sending media items of Action time: {timer.ElapsedMilliseconds}ms");
#endif
                if (hasInternet) await Task.Run(() => SetMediaItemsForAction(mediaItems, action)); else action.LocalMediaItems = mediaItems;

                action.Tags = TagsHelper.GetActiveTagsList(Tags);
                action.DueDate = DueDate.Value;
                action.Description = Description;
                action.Comment = Comment;
                action.AssignedUsers = SelectedResources.Where(item => item.ActionResourceType == ActionResourceType.User).Select(item => new UserBasic { Id = item.Id, Name = item.Text, Picture = item.Picture }).ToList();
                action.AssignedAreas = SelectedResources.Where(item => item.ActionResourceType == ActionResourceType.Area).Select(item => new AreaBasic { Id = item.Id }).ToList();
                action.CompanyId = UserSettings.userSettingsPrefs.CompanyId;
                action.CreatedById = UserSettings.userSettingsPrefs.Id;
                action.CreatedBy = UserSettings.userSettingsPrefs.Fullname;
                action.ModifiedAt = DateTime.Now;
                action.SendToUltimo = IsUltimoEnabled;
                action.UltimoStatusDateTime = DateTime.UtcNow;

                if (Action != null)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Starting creation of Action time: {timer.ElapsedMilliseconds}ms");
#endif
                    action.DueDate = DueDate.HasValue ? DueDate.Value : Action.DueDate;
                    action.Id = Action.Id;
                    action.TaskTemplateId = Action.TaskTemplateId;
                    if (action.TaskTemplateId == 0) action.TaskTemplateId = null;
                    action.TaskId = Action.TaskId;
                    if (action.TaskId == 0) action.TaskId = null;
                    action.IsResolved = Action.IsResolved;
                    action.CreatedById = Action.CreatedById;
                    action.CreatedBy = Action.CreatedBy;
                    action.UnviewedCommentNr = Action.UnviewedCommentNr;
                    action.CreatedAt = Action.CreatedAt;
                    action.LocalId = Action.LocalId;
                    action.Parent = Action.Parent;
                    action.SendToUltimo = IsUltimoEnabled;

                    // If the action id is not greater than 0, it's a local action that's edited, so it should not be seen as an update
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Sending Action to API time: {timer.ElapsedMilliseconds}ms");
#endif

                    if (action.Id > 0)
                    {
                        var checkIfActionChanged = await CheckIfActionFieldsChanged(action);
                        if (checkIfActionChanged)
                            result = await _actionService.UpdateActionAsync(action);
                    }
                    else
                        result = await _actionService.AddActionAsync(action);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Recived response fron API time: {timer.ElapsedMilliseconds}ms");
#endif
                    // ActionComments
                    if (result && action.Id > 0)
                    {
                        Task.Run(async () => await GetCommentsForAction(action));
                    }
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Recived comments for Action time: {timer.ElapsedMilliseconds}ms");
#endif
                }
                else
                {

                    if (!IsFromHomeScreen)
                        action.TaskTemplateId = TaskTemplateId;
                    else
                        action.TaskTemplateId = null;

                    if (action.TaskTemplateId == 0) action.TaskTemplateId = null;
                    action.IsResolved = false;
                    action.UnviewedCommentNr = 0;
                    action.CreatedAt = DateTime.Now;
                    action.Parent = new ActionParentBasic();

                    if (TaskId.HasValue)
                        action.TaskId = (int)TaskId.Value;

                    result = await IsAddActionByActionTypeSuccess(result, action);
                }

                if (LocalTask != null)
                {
                    LocalTask.LocalActions ??= new List<ActionsModel>();
                    LocalTask.LocalActions.Add(action);
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Finishing submition of Action time: {timer.ElapsedMilliseconds}ms");
#endif
                if (result)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, Constants.ActionChanged);
                    });
                }
                else
                {
                    _statusBarService.HideStatusBar();
                }
            });
            if (isError)
                return;
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Starting Cancelation: {timer.ElapsedMilliseconds}ms");
#endif
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await CancelAsync();
#if DEBUG
            timer.Stop();
            System.Diagnostics.Debug.WriteLine($"[SUBMIT]::Ending submition of Action time: {timer.ElapsedMilliseconds}ms");
#endif
            IsBusy = false;
        }

        private async Task<bool> IsAddActionByActionTypeSuccess(bool result, ActionsModel action)
        {
            switch (ActionType)
            {
                case ActionType.Audit:
                    action.Parent.AuditTemplateId = TaskTemplateId;
                    result = await _actionService.AddActionToAuditAsync(action);
                    break;
                case ActionType.Checklist:
                    action.Parent.ChecklistTemplateId = TaskTemplateId;
                    result = await _actionService.AddActionToChecklistAsync(action);
                    break;
                case ActionType.Task:
                    result = await _actionService.AddActionAsync(action);
                    break;
            }

            return result;
        }

        private async Task<bool> CheckIfActionFieldsChanged(ActionsModel action)
        {
            var changedFields = await GetActionFieldsChangedList(action);
            return changedFields.Count > 0;
        }

        private async Task<List<string>> GetActionFieldsChangedList(ActionsModel action)
        {
            List<string> items = new List<string>();
            string item = string.Empty;
            if (!action.Comment.Equals(Action.Comment))
            {
                item = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionDetailScreenCommentsSectionTitle);
                items.Add(item);
            }

            if (!action.Description.Equals(Action.Description))
            {
                item = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionScreenTitle);
                items.Add(item);
            }

            IEnumerable<int> oldUserIds = Action.AssignedUsers?.Select(user => user.Id) ?? new List<int>();
            IEnumerable<int> newUserIds = action.AssignedUsers?.Select(user => user.Id) ?? new List<int>();

            if (oldUserIds.Union(newUserIds).Any(w => !(oldUserIds.Contains(w) && newUserIds.Contains(w))))
            {
                item = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionDetailScreenResourcesSectionTitle);
                items.Add(item);
            }

            if (!action.DueDate.Value.Date.Equals(Action.DueDate.Value.Date))
            {
                item = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenDueDate);
                items.Add(item);
            }

            List<string> oldMediaUrls = Action.Images ?? new List<string>();

            if (Action.Videos != null)
                oldMediaUrls.AddRange(Action.Videos);

            List<string> newMediaUrls = action.Images ?? new List<string>();

            if (action.Videos != null)
                newMediaUrls.AddRange(action.Videos);

            if (oldMediaUrls.Union(newMediaUrls).Any(x => !(oldMediaUrls.Contains(x) && newMediaUrls.Contains(x))))
            {
                item = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionMediaEdited);
                items.Add(item);
            }

            IEnumerable<int> oldTagsIds = Action.Tags?.Select(tag => tag.Id) ?? new List<int>();
            IEnumerable<int> newTagsIds = action.Tags?.Select(tag => tag.Id) ?? new List<int>();

            if (oldTagsIds.Union(newTagsIds).Any(x => !(oldTagsIds.Contains(x) && newTagsIds.Contains(x))))
            {
                item = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextTags);
                items.Add(item);
            }

            if (Action.SendToUltimo != action.SendToUltimo)
            {
                items.Add("Ultimo");
            }
            return items;
        }

        private async Task GetCommentsForAction(ActionsModel action)
        {
            string changedItems = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionEditedTitle);
            var items = await GetActionFieldsChangedList(action);

            if (items.Any())
            {
                string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionChanged);
                _messageService.SendClosableInfo(message);

                string comment = $"{changedItems} {items.Aggregate((a, b) => $"{a} , {b}")}";

                ActionCommentModel actionComment = new ActionCommentModel
                {
                    ActionId = action.Id,
                    Comment = comment,
                    UserId = UserSettings.Id
                };

                await _actionService.AddActionCommentAsync(actionComment);
            }
        }


        private static void SetMediaItemsForAction(List<MediaItem> mediaItems, ActionsModel action)
        {
            List<string> images = new List<string>();
            List<string> videos = new List<string>();
            List<string> videoThumbnails = new List<string>();

            foreach (MediaItem mediaItem in mediaItems)
            {
                if (!mediaItem.IsLocalFile)
                {
                    if (mediaItem.IsVideo)
                    {
                        videos.Add(mediaItem.VideoUrl);
                        videoThumbnails.Add(mediaItem.PictureUrl);
                    }
                    else

                        images.Add(mediaItem.PictureUrl);
                }
            }

            if (images.Any())
                action.Images = images;

            if (videos.Any())
                action.Videos = videos;

            if (videoThumbnails.Any())
                action.VideoThumbNails = videoThumbnails;
        }

        private async Task<bool> SetScreenForPage()
        {
            Page page = NavigationService.GetCurrentPage();
            string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);

            if (!DueDate.HasValue)
            {
                await GetValueFromDictionary(LanguageConstants.createActionScreenDueDateLabel, page, ok);
                return false;
            }

            if (string.IsNullOrEmpty(Comment))
            {
                await GetValueFromDictionary(LanguageConstants.createActionScreenCommentsLabel, page, ok);
                return false;
            }

            if (string.IsNullOrEmpty(Description))
            {
                await GetValueFromDictionary(LanguageConstants.createActionScreenActionLabel, page, ok);
                return false;
            }

            return true;
        }

        private static async Task GetValueFromDictionary(string key, Page page, string ok)
        {
            string message = TranslateExtension.GetValueFromDictionary(key);
            await page.DisplayActionSheet(message, null, ok);
        }

        /// <summary>
        /// Handles the event when a selection is made in the auto complete control.
        /// </summary>
        /// <param name="args">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void AutoCompleteSelected(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.FirstOrDefault() is ResourceModel resourceModel)
            {
                PopupResources.Add(resourceModel);
                Resources.Remove(resourceModel);

                AutoCompleteText = string.Empty;
                SelectedItem = null;
            }
        }

        /// <summary>
        /// Removes the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        private void RemoveResource(ResourceModel resourceModel)
        {
            if (resourceModel != null)
            {
                PopupResources.Remove(resourceModel);
                Resources.Add(resourceModel);
            }
        }

        /// <summary>
        /// Submits the user popup.
        /// </summary>
        private void SubmitUserPopup()
        {
            SelectedResources = PopupResources?.ToList() ?? new List<ResourceModel>();
            var SelectedResourceNamesList = new List<ResourceSelectionModel>();

            if (SelectedResources.Any())
            {
                int index = 1;
                int resourceCount = SelectedResources.Count;

                foreach (ResourceModel resourceModel in SelectedResources)
                {
                    string name = resourceModel.Text;

                    if (index < resourceCount)
                        name = $"{name},";

                    if (index > 3 && resourceCount > 3)
                    {
                        SelectedResourceNamesList.Add(new ResourceSelectionModel
                        {
                            Text = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextAndMore),
                            ShowIcon = false
                        });

                        break;
                    }

                    ResourceSelectionModel resourceSelectionModel = new ResourceSelectionModel
                    {
                        Text = name,
                        ShowIcon = true
                    };

                    SelectedResourceNamesList.Add(resourceSelectionModel);

                    index++;
                }
            }

            SelectedResourceNames = new ObservableCollection<ResourceSelectionModel>(SelectedResourceNamesList);
            AutoCompleteText = string.Empty;

            IsUserPopupOpen = false;
            PopupResources = null;
        }

        private void SubmitDueDate(object obj)
        {
            if (obj is DateTime datetime)
            {
                DueDate = datetime;
            }
        }

        /// <summary>
        /// Adds the media asynchronous.
        /// </summary>
        /// <param name="mediaNumber">The media number.</param>
        private async Task AddMediaAsync(string mediaNumber)
        {
            int mediaNumberInt = int.Parse(mediaNumber);

            List<MediaOption> mediaOptions = new List<MediaOption> { MediaOption.TakePhoto, MediaOption.PhotoGallery, MediaOption.VideoGallery, MediaOption.Video };

            if (IsMediaSet(mediaNumberInt))
                mediaOptions.Add(MediaOption.RemoveMedia);

            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOptions);

            if (dialogResult.Result == null && dialogResult.IsSuccess && !dialogResult.IsRemoved)
                return;

            if (dialogResult.IsSuccess)
                SetMedia(mediaNumberInt, dialogResult.Result);
        }

        private void SetMedia(int mediaNumber, MediaItem mediaItem)
        {
            switch (mediaNumber)
            {
                case 1:
                    MediaItem1 = mediaItem;
                    break;
                case 2:
                    MediaItem2 = mediaItem;
                    break;
                case 3:
                    MediaItem3 = mediaItem;
                    break;
                case 4:
                    MediaItem4 = mediaItem;
                    break;
                case 5:
                    MediaItem5 = mediaItem;
                    break;
                case 6:
                    MediaItem6 = mediaItem;
                    break;
            }
        }

        /// <summary>
        /// Determines whether the specified media is set.
        /// </summary>
        /// <param name="mediaNumber">The media number.</param>
        /// <returns>
        ///   <c>true</c> if media is set; otherwise, <c>false</c>.
        /// </returns>
        private bool IsMediaSet(int mediaNumber)
        {
            bool result = false;

            switch (mediaNumber)
            {
                case 1:
                    result = MediaItem1 != null;
                    break;
                case 2:
                    result = MediaItem2 != null;
                    break;
                case 3:
                    result = MediaItem3 != null;
                    break;
                case 4:
                    result = MediaItem4 != null;
                    break;
                case 5:
                    result = MediaItem5 != null;
                    break;
                case 6:
                    result = MediaItem6 != null;
                    break;
            }

            return result;
        }

        private List<MediaItem> GetMediaItems()
        {
            List<MediaItem> mediaItems = new List<MediaItem>();

            if (MediaItem1 != null)
                mediaItems.Add(MediaItem1);

            if (MediaItem2 != null)
                mediaItems.Add(MediaItem2);

            if (MediaItem3 != null)
                mediaItems.Add(MediaItem3);

            if (MediaItem4 != null)
                mediaItems.Add(MediaItem4);

            if (MediaItem5 != null)
                mediaItems.Add(MediaItem5);

            if (MediaItem6 != null)
                mediaItems.Add(MediaItem6);

            return mediaItems;
        }

        private async Task UploadMediaItemsAsync()
        {
            IEnumerable<MediaItem> mediaItems = GetMediaItems();

            await _mediaService.UploadMediaItemsAsync(mediaItems, MediaStorageTypeEnum.Actions, 0);
        }
    }
}
