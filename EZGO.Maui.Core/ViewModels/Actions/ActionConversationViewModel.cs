using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using Syncfusion.Maui.DataSource.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionConversationViewModel : BaseViewModel
    {
        #region Private Properties

        private int currentIndex;
        private readonly static SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);

        private const int maxCommentImages = 5;

        private int _maxAssignedResourcesShown = 3;

        //private DateTime lastChatDateTime;

        #endregion

        #region Public Properties

        public List<BasicActionsModel> Actions { get; set; }

        public ObservableCollection<UserBasic> TopUsers { get; set; }
        public ObservableCollection<ResourceSelectionModel> AssignedResources { get; set; }

        public BasicActionsModel SelectedAction { get; set; }

        public long? TaskId { get; set; }

        public int? TaskTemplateId { get; set; }

        public string ParentName { get; set; }

        public bool Last { get; set; }

        public bool First { get; set; }

        public string Comment { get; set; }

        public bool HasMedia { get; set; }

        public string VideoCountText { get; set; }

        public string ImageCountText { get; set; }


        public MediaItem CommentVideo { get; set; }

        public List<MediaItem> CommentImages { get; set; } = new List<MediaItem>();

        public bool HasImages { get; set; }

        public List<MediaItem> MediaItems { get; set; }

        public ObservableCollection<BasicActionCommentModel> Chat { get; set; }

        public bool HasTaskId { get; set; }

        public FilterModel SelectedFilter { get; set; }

        public List<FilterModel> FilterOptions { get; set; }

        public bool CanCompleteAction { get; set; }

        public bool IsUltimoVisible => CompanyFeatures.CompanyFeatSettings.MarketUltimoEnabled;

        public string UltimoMessage { get; set; }
        #endregion

        #region Commands
        public IAsyncRelayCommand TakePhotoCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(async () => await AddCommentMediaAsync(MediaOption.TakePhoto));
        }, CanExecuteCommands);

        public IAsyncRelayCommand PhotoGalleryCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(async () => await AddCommentMediaAsync(MediaOption.PhotoGallery));
        }, CanExecuteCommands);

        public IAsyncRelayCommand VideoCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(async () => await AddCommentMediaAsync(MediaOption.Video));
        }, CanExecuteCommands);

        public ICommand RemoveVideoCommand => new Command(RemoveVideo);

        public ICommand RemoveImagesCommand => new Command(RemoveImages);

        private bool IsNavigatinng = false;
        public IAsyncRelayCommand EditCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(async () =>
            {
                if (!IsNavigatinng)
                {
                    IsNavigatinng = true;
                    await NavigateToEditAsync();
                    IsNavigatinng = false;
                }
            });
        });

        public IAsyncRelayCommand DetailCommand => new AsyncRelayCommand<MediaItem>(async mediaItem =>
        {
            await NavigateToDetailAsync(mediaItem);
        });

        public IAsyncCommand<ResourceSelectionModel> ShowMoreResourcesCommand { get; private set; }

        public IAsyncRelayCommand CommentDetailCommand => new AsyncRelayCommand<List<MediaItem>>(async mediaItems =>
        {
            await NavigateToCommentDetailAsync(mediaItems);
        });

        public ICommand ParentCommand => new Command(async () =>
        {
            await NavigateToActionParentAsync();
        });

        public IAsyncRelayCommand NextCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(GoNextAsync);
        }, CanExecuteCommands);

        public IAsyncRelayCommand PreviousCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(GoPreviousAsync);
        }, CanExecuteCommands);

        public IAsyncRelayCommand ActionSolvedCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(async () => await ToggleActionStatus(ActionStatusEnum.Solved));
        }, CanExecuteCommands);

        public IAsyncRelayCommand AddCommentCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(async () => await AddCommentAsync());
        }, CanExecuteCommands);

        public IAsyncRelayCommand ShowVideoCommand => new AsyncRelayCommand<MediaItem>(async mediaItem =>
        {
            await NavigateToVideoPlayerAsync(mediaItem);
        });

        #endregion

        #region Services
        private readonly IMediaHelper _mediaHelper;
        private readonly IMediaService _mediaService;
        private readonly IWorkAreaService _workAreaService;


        public ActionConversationViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMediaHelper mediaHelper,
            IMediaService mediaService,
            IWorkAreaService workAreaService) : base(navigationService, userService, messageService, actionsService)
        {
            _mediaHelper = mediaHelper;
            _mediaService = mediaService;
            _workAreaService = workAreaService;

            ShowMoreResourcesCommand = new AsyncCommand<ResourceSelectionModel>(async (obj) =>
            {
                if (obj != null && obj.Text == TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextAndMore))
                {
                    _maxAssignedResourcesShown = int.MaxValue;
                    await SetAssignedResources();
                }
            });
        }

        ~ActionConversationViewModel()
        { // Breakpoint here
        }

        #endregion

        public override async Task Init()
        {
            Settings.AppSettings.SubpageActions = MenuLocation.ActionsConversation;

            await LoadActionAsync();

            SetUltimoMessage();

            base.Init();

            MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
            {
                await UpdateComments();
            });

            MessagingCenter.Subscribe<ActionsService, ActionCommentModel>(this, Constants.ChatChanged, async (sender, comment) =>
            {
                await BuildActionChatAsync(localComment: comment);
            });

            MessagingCenter.Subscribe<SyncService, ActionCommentUpdateModel>(this, Constants.ChatChanged, async (sender, args) =>
            {
                await BuildActionChatAsync(postedLocalIds: args.PostedMessageIds, newComments: args.NewComments);
            });

            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionChanged, async (sender) =>
            {
                await ItemChanged();
            });

            MessagingCenter.Subscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged, async (sender, args) =>
            {
                if (args?.TypeOfChange == ActionChangedMessageArgs.ChangeType.SetToResolved)
                    await ItemChanged();
            });
        }


        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
            MessagingCenter.Unsubscribe<ActionsService, ActionCommentModel>(this, Constants.ChatChanged);
            MessagingCenter.Unsubscribe<SyncService, ActionCommentUpdateModel>(this, Constants.ChatChanged);
            MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionChanged);
            //_mediaHelper.
            //_mediaService.di
            _workAreaService.Dispose();

            base.Dispose(disposing);
        }

        #region Events

        private async Task ItemChanged()
        {
            if (_actionService != null)
            {
                if (SelectedAction?.Id != 0)
                {
                    var action = await _actionService?.GetOnlineActionAsync(SelectedAction?.Id ?? 0);
                    await SetChangedAction(action);
                }
                else
                {
                    var action = await _actionService?.GetLocalActionAsync(SelectedAction?.LocalId ?? -1);
                    await SetChangedAction(action);
                }
            }
        }

        private async Task SetChangedAction(ActionsModel action)
        {
            if (action != null)
            {
                SelectedAction = action.ToBasic();
                await SetActionOwner();
                await SetAssignedResources();
                SetUltimoMessage();
                await Task.Run(() => SetMediaItems());
            }
        }
        #endregion

        #region Action

        private async Task LoadActionAsync()
        {
            if (TaskId.HasValue)
            {
                List<ActionsModel> taskActions = await _actionService.GetAllActionsForTaskAsync(TaskId.Value);

                taskActions ??= new List<ActionsModel>();

                if (TaskTemplateId.HasValue)
                {
                    List<ActionsModel> taskTemplateActions = await _actionService.GetOpenActionsForTaskTemplateAsync(TaskTemplateId.Value);

                    taskTemplateActions ??= new List<ActionsModel>();

                    if (taskTemplateActions.Any())
                        taskActions.AddRange(taskTemplateActions);
                }

                Actions = taskActions.ToBasicList<BasicActionsModel, ActionsModel>();
            }

            SelectedAction ??= Actions?.FirstOrDefault();

            if (SelectedAction == null)
                await CancelAsync();
            else
            {
                currentIndex = Actions.FindIndex(item => item.Id == SelectedAction.Id);

                await BuildExtendedActionAsync();
            }

            SetHasTaskId();
        }

        private void SetHasTaskId()
        {
            if (SelectedAction?.TaskId != 0 || SelectedAction?.TaskTemplateId != 0)
            {
                HasTaskId = true;
            }
            else
            {
                HasTaskId = false;
            }
        }



        private async Task BuildExtendedActionAsync()
        {
            await AsyncAwaiter.AwaitAsync($"{nameof(ActionConversationViewModel)}Action", async () =>
            {
#if DEBUG
                var sw2 = new Stopwatch();
                sw2.Start();
#endif
                Last = Actions.ElementAtOrDefault(currentIndex + 1) == null;
                First = Actions.ElementAtOrDefault(currentIndex - 1) == null;

                SelectedAction.Comments ??= new List<ActionCommentModel>();
                SelectedAction.AssignedUsers ??= new List<UserBasic>();
                Chat = new ObservableCollection<BasicActionCommentModel>();
                TopUsers = new ObservableCollection<UserBasic>();
                ParentName = SelectedAction?.Parent?.TaskName;

                var assignedResourcesTask = SetAssignedResources();
                var usersTask = SetActionOwner();
                var commentsTask = SetComments();
                var canCompleteActionTask = Task.Run(() => SetCanCompleteAction());
                var mediaItemsTask = Task.Run(() => SetMediaItems());

                // Wait for all tasks to complete
                await Task.WhenAll(assignedResourcesTask, usersTask, commentsTask, mediaItemsTask, canCompleteActionTask);

#if DEBUG
                Debug.WriteLine($"\nSetting Action parameters took: {sw2.ElapsedMilliseconds} ms\n");
                sw2.Stop();
#endif
            });
        }

        private async Task SetActionOwner()
        {
            var users = new List<UserBasic>();
            if (SelectedAction != null)
            {
                if (SelectedAction.CreatedById == UserSettings.Id)
                    users.Add(new UserBasic { Id = UserSettings.Id, Name = UserSettings.Fullname, Picture = UserSettings.UserPictureUrl });
                else
                {
                    var user = new UserProfileModel();
                    if (_userService != null)
                        user = await _userService?.GetCompanyUserAsync(SelectedAction.CreatedById);
                    user ??= new UserProfileModel();

                    if (user != null)
                        users.Add(new UserBasic { Id = user.Id, Name = user.FullName, Picture = user.Picture });
                }
                users.AddRange(SelectedAction.AssignedUsers ?? new List<UserBasic>());
            }

            TopUsers = new ObservableCollection<UserBasic>(users);
        }

        private async Task SetComments()
        {
            // Always fetch both backend and local comments for the selected action
            if (SelectedAction != null)
            {
                var allComments = await _actionService.GetActionCommentsAsync(SelectedAction.Id, includeLocalActionComments: true, refresh: await InternetHelper.HasInternetConnection());


                if (allComments != null && allComments.Any())
                {
                    SelectedAction.Comments = allComments
                        .Distinct(new ActionCommentComparer())
                        .OrderBy(item => item.ModifiedAt ?? item.CreatedAt ?? DateTime.Now)
                        .ToList();

                    SetCommentMediaItems(SelectedAction.Comments);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Chat = new ObservableCollection<BasicActionCommentModel>(
                            SelectedAction.Comments.ToBasicList<BasicActionCommentModel, ActionCommentModel>()
                        );
                        OnPropertyChanged(nameof(Chat));
                    });

                    // Mark as viewed if needed
                    if (SelectedAction.UnviewedCommentNr > 0)
                    {
                        SelectedAction.UnviewedCommentNr = 0;
                        await _actionService.SetActionCommentsViewedAsync(SelectedAction.Id);
                    }
                }
                else
                {
                    // No comments found, clear chat
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Chat = new ObservableCollection<BasicActionCommentModel>();
                        OnPropertyChanged(nameof(Chat));
                    });
                }
            }
        }

        private void SetMediaItems()
        {
            if (SelectedAction.LocalMediaItems != null && SelectedAction.LocalMediaItems.Any())
                MediaItems = SelectedAction.LocalMediaItems;
            else
            {
                List<MediaItem> mediaItems = new List<MediaItem>();

                if (SelectedAction.Images != null)
                {
                    foreach (string image in SelectedAction.Images)
                    {
                        MediaItem mediaItem = new MediaItem
                        {
                            PictureUrl = image
                        };

                        mediaItems.Add(mediaItem);
                    }
                }

                if (SelectedAction.Videos != null)
                {
                    foreach (string video in SelectedAction.Videos)
                    {
                        int videoIndex = SelectedAction.Videos.IndexOf(video);
                        string thumbnailUrl = SelectedAction.VideoThumbNails?[videoIndex];

                        MediaItem mediaItem = new MediaItem
                        {
                            IsVideo = true,
                            PictureUrl = thumbnailUrl,
                            VideoUrl = video
                        };

                        mediaItems.Add(mediaItem);
                    }
                }

                MediaItems = mediaItems;
            }
        }

        private void SetCanCompleteAction()
        {
            CanCompleteAction = SelectedAction.IsMine || UserSettings.RoleType == RoleTypeEnum.Manager || SelectedAction.IsResolved;
        }

        private async Task SetAssignedResources()
        {
            var assignedResourceNamesList = new List<ResourceSelectionModel>();

            if (SelectedAction?.AssignedUsers != null && SelectedAction.AssignedUsers.Any())
            {
                foreach (var item in SelectedAction.AssignedUsers)
                {
                    assignedResourceNamesList.Add(new ResourceSelectionModel() { Text = item.Name + ",", ShowIcon = true });
                }
            }

            if (assignedResourceNamesList.Count > _maxAssignedResourcesShown)
            {
                assignedResourceNamesList = assignedResourceNamesList.Take(3).ToList();
                assignedResourceNamesList.Add(new ResourceSelectionModel()
                {
                    Text = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextAndMore),
                    ShowIcon = false
                });
            }
            else
            {
                if (SelectedAction?.AssignedAreas != null && SelectedAction.AssignedAreas.Any())
                {
                    var workAreas = await _workAreaService?.GetBasicWorkAreasAsync();
                    workAreas = _workAreaService?.GetFlattenedBasicWorkAreas(workAreas);
                    foreach (var area in SelectedAction.AssignedAreas)
                    {
                        var item = workAreas.FirstOrDefault(w => w.Id == area.Id);
                        if (item != null)
                        {
                            var splittedFullName = item.FullDisplayName.Split(" -> ");
                            var txt = "";
                            if (splittedFullName.Count() > 2)
                            {
                                txt += "../";
                                txt += string.Join("/", splittedFullName.TakeLast(2));
                            }
                            else
                            {
                                txt = string.Join("/", splittedFullName);
                            }

                            assignedResourceNamesList.Add(new ResourceSelectionModel() { Text = txt + ",", ShowIcon = true });
                        }

                        if (assignedResourceNamesList.Count > _maxAssignedResourcesShown)
                        {
                            assignedResourceNamesList.Add(new ResourceSelectionModel()
                            {
                                Text = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextAndMore),
                                ShowIcon = false
                            });
                            break;
                        }
                    }
                }
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AssignedResources = new ObservableCollection<ResourceSelectionModel>(assignedResourceNamesList);
            });
        }

        private async Task GoNextAsync()
        {
            await GotToIndexAsync(currentIndex + 1);
        }

        private async Task GoPreviousAsync()
        {
            await GotToIndexAsync(currentIndex - 1);
        }

        private async Task GotToIndexAsync(int index)
        {
            if (Actions.ElementAtOrDefault(index) != null)
            {
                SelectedAction = Actions[index];

                currentIndex = Actions.FindIndex(item => item.Id == SelectedAction.Id);

                await BuildExtendedActionAsync();
                SetHasTaskId();
            }
        }

        private async Task ToggleActionStatus(ActionStatusEnum status)
        {
            if (SelectedAction != null)
            {
                if (SelectedAction.FilterStatus != status)
                {
                    Page page = NavigationService?.GetCurrentPage();

                    string confirm = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertConfirmAction);
                    string yes = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertYesButtonTitle);
                    string no = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertNoButtonTitle);

                    string result = await page?.DisplayActionSheet(confirm, null, null, yes, no);

                    if (result == yes && await _actionService?.SetActionResolvedAsync(SelectedAction))
                    {
                        SelectedAction.FilterStatus = status;

                        await AddCompletedComment();
                    }

                    _statusBarService?.HideStatusBar();
                }
            }
            MessagingCenter.Send(this, Constants.ActionsChanged);
        }

        private async Task AddCompletedComment()
        {
            var commentText = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionEditedTitle);
            var completed = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCompleted);

            string comment = $"{commentText} {completed}";

            ActionCommentModel actionCommentModel = new ActionCommentModel
            {
                ActionId = SelectedAction.Id,
                Comment = comment,
                UserId = UserSettings.Id
            };

            if (_actionService != null)
                await _actionService?.AddActionCommentAsync(actionCommentModel);


        }

        private void SetUltimoMessage()
        {
            var ultimoStatus = SelectedAction.UltimoStatus;
            UltimoMessage = ultimoStatus switch
            {
                "NONE" => TranslateExtension.GetValueFromDictionary(LanguageConstants.ultimoNotSent),
                "READY_TO_BE_SENT" => $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.ultimoReadySent)} {SelectedAction.UltimoStatusDateTime?.ToLocalTime()}",
                "SENT" => $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.ultimoSent)} {SelectedAction.UltimoStatusDateTime?.ToLocalTime()}",
                "ERROR" => $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.ultimoErrorSent)} {SelectedAction.UltimoStatusDateTime?.ToLocalTime()}",
                _ => TranslateExtension.GetValueFromDictionary(LanguageConstants.ultimoNotSent),
            };
        }

        #endregion

        #region Comments

        private async Task UpdateComments()
        {
            if (FifteenSecondLock.Wait(0))
            {
                try
                {
                    var syncService = App.Container.GetService<ISyncService>();
                    await syncService.UploadUpdateLocalActionDataAsync(SelectedAction.Id);
                }
                finally
                {
                    FifteenSecondLock.Release();
                }
            }
        }

#if DEBUG
        private static int _thread = 0;
#endif
        private async Task BuildActionChatAsync(List<string> postedLocalIds = null, ActionCommentModel localComment = null, List<ActionCommentModel> newComments = null)
        {
            await AsyncAwaiter.AwaitAsync($"{nameof(ActionConversationViewModel)}chat", async () =>
            {
#if DEBUG
                _thread++;
                var thread = _thread;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Debug.WriteLine($"Thread {thread}:: Started execution of BuildActionChatAsync...");
#endif
                postedLocalIds ??= new List<string>();
                newComments ??= new List<ActionCommentModel>();

                Chat ??= new ObservableCollection<BasicActionCommentModel>();

                if (postedLocalIds.Any() || newComments.Any() || localComment != null)
                {
#if DEBUG
                    Debug.WriteLine($"Thread {thread}:: Detected comment changes.. time: {watch.ElapsedMilliseconds}ms");
#endif
                    try
                    {
                        await Task.Run(() =>
                        {
#if DEBUG
                            Debug.WriteLine($"Thread {thread}:: Entered Task.Run function -- time: {watch.ElapsedMilliseconds}ms");
#endif
                            List<ActionCommentModel> comments = new List<ActionCommentModel>();

                            // Add new local comment
                            if (localComment != null)
                            {
#if DEBUG
                                Debug.WriteLine($"Thread {thread}:: Adding local comments to comments -- time: {watch.ElapsedMilliseconds}ms");
#endif
                                if (localComment.LocalActionId == SelectedAction.LocalId)
                                {
                                    if (!Chat.Any(x => x.LocalId == localComment.LocalId))
                                        comments.Add(localComment);
                                }
                            }

                            // Add new external comments
                            if (newComments.Any())
                            {
#if DEBUG
                                Debug.WriteLine($"Thread {thread}:: Adding newComments to comments -- time: {watch.ElapsedMilliseconds}ms");
#endif
                                newComments.ForEach(c =>
                                {

                                    if (c.ActionId == SelectedAction.Id)
                                    {
                                        if (!Chat.Any(x => x.Id == c.Id))
                                            comments.Add(c);
                                    }
                                });
                            }

                            if (comments.Any())
                            {
#if DEBUG
                                Debug.WriteLine($"Thread {thread}:: Started adding comments to chat: {watch.ElapsedMilliseconds}ms");
#endif
                                SetCommentMediaItems(comments);

                                comments = comments.OrderBy(x => x.ModifiedAt).ToList();
                            }

                            // posted comments, unflag them
                            if (postedLocalIds.Any())
                            {
                                Chat?.Where(x => x.UnPosted).ForEach(x =>
                                    {
                                        if (postedLocalIds.Contains(x.LocalId))
                                        {
                                            x.UnPosted = false;
                                        }
                                    });
                            }

                            if (!comments.IsNullOrEmpty())
                                Chat.AddRange(comments);
                            else
                                Chat ??= new ObservableCollection<BasicActionCommentModel>(SelectedAction.Comments.ToBasicList<BasicActionCommentModel, ActionCommentModel>());

                            OnPropertyChanged(nameof(Chat));
                        });

                        if (newComments.Any())
                        {
                            await _actionService.SetActionCommentsViewedAsync(SelectedAction.Id);
                        }
                    }
                    catch { }
                    finally
                    {
#if DEBUG
                        watch.Stop();
                        Debug.WriteLine($"Thread {thread}:: Changing chat took: {watch.ElapsedMilliseconds}ms");
                        _thread = 0;
#endif
                    }
                }
            });
        }

        private void SetCommentMediaItems(IEnumerable<ActionCommentModel> comments)
        {
            foreach (ActionCommentModel comment in comments)
            {
                if (comment.LocalMediaItems != null && comment.LocalMediaItems.Any())
                {
                    comment.VideoMediaItem = comment.LocalMediaItems.SingleOrDefault(item => item.IsVideo);
                    var imageMediaItems = comment.LocalMediaItems.Where(item => !item.IsVideo).ToList();
                    if (!imageMediaItems.IsNullOrEmpty())
                        comment.ImageMediaItems = comment.LocalMediaItems.Where(item => !item.IsVideo).ToList();

                }
                else
                {
                    if (!comment.Video.IsNullOrWhiteSpace() && !comment.VideoThumbnail.IsNullOrWhiteSpace())
                        comment.VideoMediaItem = new MediaItem { VideoUrl = comment.Video, PictureUrl = comment.VideoThumbnail, IsVideo = true };

                    if (comment.Images != null && comment.Images.Any())
                        comment.ImageMediaItems = comment.Images.Select(item => new MediaItem { PictureUrl = item }).ToList();
                }
            }
        }

        private async Task AddCommentAsync()
        {
            if (Comment.IsNullOrWhiteSpace() && !HasMedia)
                return;

            var actionComment = new ActionCommentModel
            {
                ActionId = SelectedAction.Id,
                Comment = Comment ?? string.Empty,
                LocalActionId = SelectedAction.LocalId
            };

            if (HasMedia)
            {
                var mediaItems = GetMediaItems();
                if (await InternetHelper.HasInternetConnection())
                {
                    try
                    {
                        await UploadMediaAsync(mediaItems);
                    }
                    catch (Exception ex) when (ex is ArgumentNullException || ex is HttpRequestException)
                    {
                        var page = NavigationService.GetCurrentPage();
                        var ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                            await page.DisplayActionSheet("There was a problem with uploading media files. Please try again later.", null, ok));
                        return;
                    }

                    var videoMediaItem = mediaItems.FirstOrDefault(item => item.IsVideo);
                    if (videoMediaItem != null)
                    {
                        actionComment.Video = videoMediaItem.VideoUrl;
                        actionComment.VideoThumbnail = videoMediaItem.PictureUrl;
                    }

                    var imageMediaItems = mediaItems.Where(item => !item.IsVideo).ToList();
                    if (imageMediaItems.Count > 0)
                        actionComment.Images = imageMediaItems.Select(item => item.PictureUrl).ToList();
                }
                else
                {
                    actionComment.LocalMediaItems = mediaItems;
                }
            }

            if (await _actionService.AddActionCommentAsync(actionComment))
            {
                Comment = string.Empty;
                RemoveVideo();
                RemoveImages();

                if (currentIndex >= 0 && currentIndex < Actions.Count)
                {
                    var comments = Actions[currentIndex].Comments ??= new List<ActionCommentModel>();
                    comments.Add(actionComment);
                    await SetComments();
                }
            }
        }

        private async Task AddCommentMediaAsync(MediaOption mediaOption)
        {
            if (!CanExecuteMediaCommand(mediaOption))
            {
                await ValidationHelper.DisplayActionChatValidationPopup();
                return;
            }

            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOption);

            if (dialogResult.IsCanceled)
                return;

            var mediaItem = dialogResult.Result;

            if (mediaItem != null)
            {
                if (mediaItem.IsVideo)
                    CommentVideo = mediaItem;
                else
                    CommentImages.Add(mediaItem);

                SetHasMediaAndCountText();
            }
        }

        private void SetHasMediaAndCountText()
        {
            HasMedia = CommentVideo != null || CommentImages.Any();
            HasImages = CommentImages.Any();

            if (HasMedia)
            {
                string imageMessage = TranslateExtension.GetValueFromDictionary(LanguageConstants.chatScreenInputPictureAttached);
                ImageCountText = $"{CommentImages.Count} {imageMessage}";

                string videoMessage = TranslateExtension.GetValueFromDictionary(LanguageConstants.chatScreenInputVideoAttached);
                VideoCountText = $"1 {videoMessage}";
            }
        }

        private void RemoveVideo()
        {
            CommentVideo = null;

            SetHasMediaAndCountText();
        }

        private void RemoveImages()
        {
            CommentImages = new List<MediaItem>();

            SetHasMediaAndCountText();
        }

        private async Task UploadMediaAsync(IEnumerable<MediaItem> mediaItems)
        {
            await _mediaService.UploadMediaItemsAsync(mediaItems, MediaStorageTypeEnum.ActionComments, 0);
        }

        private List<MediaItem> GetMediaItems()
        {
            List<MediaItem> mediaItems = new List<MediaItem>();

            if (CommentVideo != null)
                mediaItems.Add(CommentVideo);

            if (CommentImages.Any())
                mediaItems.AddRange(CommentImages);

            return mediaItems;
        }

        private bool CanExecuteMediaCommand(MediaOption mediaOption)
        {
            switch (mediaOption)
            {
                case MediaOption.PhotoGallery:
                case MediaOption.TakePhoto:
                    return CommentImages.Count() < maxCommentImages;
                case MediaOption.Video:
                    return CommentVideo == null;
                default:
                    return false;
            }
        }

        #endregion

        #region Navigation
        private async Task NavigateToEditAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.Action = SelectedAction.ToModel();

            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        private async Task NavigateToDetailAsync(MediaItem mediaItem)
        {
            if (mediaItem.IsVideo)
            {
                await NavigateToVideoPlayerAsync(mediaItem);
            }
            else
            {
                using var scope = App.Container.CreateScope();
                var actionDetailViewModel = scope.ServiceProvider.GetService<ActionDetailViewModel>();

                actionDetailViewModel.MediaItems = MediaItems.Where(item => !item.IsVideo).ToList();
                actionDetailViewModel.SelectedMediaItem = mediaItem;
                actionDetailViewModel.CurrentIndex = MediaItems.IndexOf(mediaItem);

                await NavigationService.NavigateAsync(viewModel: actionDetailViewModel);
            }
        }

        private async Task NavigateToActionParentAsync()
        {
            if (SelectedAction?.Parent != null)
            {
                using var scope = App.Container.CreateScope();
                var actionTaskTemplateDetailViewModel = scope.ServiceProvider.GetService<ActionTaskTemplateDetailViewModel>();

                actionTaskTemplateDetailViewModel.ActionParent = SelectedAction.Parent;

                await NavigationService.NavigateAsync(viewModel: actionTaskTemplateDetailViewModel);
            }
        }

        private async Task NavigateToCommentDetailAsync(List<MediaItem> mediaItems)
        {
            using var scope = App.Container.CreateScope();
            var actionDetailViewModel = scope.ServiceProvider.GetService<ActionDetailViewModel>();

            actionDetailViewModel.MediaItems = mediaItems;
            actionDetailViewModel.SelectedMediaItem = mediaItems.FirstOrDefault();

            await NavigationService.NavigateAsync(viewModel: actionDetailViewModel);
        }

        private async Task NavigateToVideoPlayerAsync(MediaItem mediaItem)
        {
            using var scope = App.Container.CreateScope();
            var videoPlayerViewModel = scope.ServiceProvider.GetService<VideoPlayerViewModel>();

            videoPlayerViewModel.MediaItem = new ThumbnailGridDetailModel()
            {
                Picture = mediaItem.PictureUrl,
                IsLocalMedia = mediaItem.IsLocalFile,
                Video = mediaItem.VideoUrl
            };

            await NavigationService.NavigateAsync(viewModel: videoPlayerViewModel);
        }

        public async override Task CancelAsync()
        {
            Settings.AppSettings.SubpageActions = MenuLocation.Actions;
            await base.CancelAsync();
        }

        #endregion

    }
}