using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Feed;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Feed;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Utils;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;

namespace EZGO.Maui.Core.ViewModels.Feed
{
    public class FeedViewModel : BaseViewModel
    {
        private readonly IFeedService _feedSerice;
        private readonly IMediaHelper _mediaHelper;
        private readonly IMediaService _mediaService;
        private readonly IReportService _reportService;
        private readonly IUpdateService _updateService;
        private readonly ISyncService _syncService;
        private readonly IInternetHelper _internetHelper;
        private int currentOffset = 0;
        private const int limit = 10;
        public bool CanLoadMoreMainFeed { get; set; }
        public bool IsLoadingMoreMainFeed { get; set; }

        public bool CanLoadMoreFactoryUpdate { get; set; }
        public bool IsLoadingMoreFactoryUpdate { get; set; }

        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);


        private int MainFeedId;
        private int FactoryUpdatesFeedId;

        public List<FactoryFeed> Feeds { get; set; }

        public MyEzFeedStats MyEzFeedStats { get; set; }

        public ObservableCollection<FeedMessageItemModel> MainFeedMessages { get; set; } = new ObservableCollection<FeedMessageItemModel>();
        public ObservableCollection<FeedMessageItemModel> FactoryUpdateMessages { get; set; } = new ObservableCollection<FeedMessageItemModel>();
        public AddFeedItemModel NewPost { get; set; }
        public AddFeedItemModel NewItem { get; set; }
        public FeedMessageItemModel SelectedFactoryUpdate { get; set; }

        public ObservableCollection<UserProfileModel> LikedUsers { get; set; }
        public FeedMessageItemModel SelectedItemForEditDelete { get; set; }

        public bool IsAddCommentPopupOpen { get; set; }
        public bool IsConfirmationPopupOpen { get; set; }
        public bool IsEditDeletePopupOpen { get; set; }
        public bool IsLikedUserPopupOpen { get; set; }

        public IAsyncCommand<object> LikeCommand { get; private set; }
        public IAsyncCommand<object> CommentsCommand { get; private set; }
        public IAsyncCommand DeleteCommentCommand { get; private set; }
        public IAsyncCommand<ObservableCollection<MediaItem>> AddPhotoMediaItemCommand { get; private set; }
        public IAsyncCommand<ObservableCollection<MediaItem>> AddVideoMediaItemCommand { get; private set; }
        public IAsyncCommand<ObservableCollection<MediaItem>> AddDocumentMediaItemCommand { get; private set; }
        public ICommand SubmitNewCommentCommand { get; private set; }
        public IAsyncCommand<AddFeedItemModel> SubmitNewPostCommand { get; private set; }
        public ICommand OpenCloseAddCommentPopupCommand { get; private set; }
        public ICommand OpenCloseAddFactoryUpdateCommand { get; private set; }
        public ICommand EditItemCommand { get; private set; }
        public ICommand OpenCloseDeleteCommentPopupCommand { get; private set; }
        public ICommand OpenCloseEditDeletePopupCommand { get; private set; }
        public ICommand OpenCloseConfirmationPopupCommand { get; private set; }
        public ICommand SwipeStartedCommand { get; private set; }
        public ICommand OpenCloseLikedUsersCommand { get; private set; }
        public ICommand RemoveMediaItemCommand { get; private set; }
        public ICommand LoadMainFeedMoreCommand => new Command(async () => await LoadMainFeedMoreAsync());
        public ICommand LoadFactoryUpdateMoreCommand => new Command(async () => await LoadFactoryUpdateMoreSync());


        private async Task DeleteItem(FeedMessageItemModel selectedCommentForDelete)
        {
            if (!await _internetHelper.HasInternetConnection())
            {
                await NoInternetConnectionAlert();
                return;
            }

            if (selectedCommentForDelete == null)
                return;

            var result = await _feedSerice.DeleteItem(selectedCommentForDelete);
            if (result)
            {
                if (selectedCommentForDelete.ItemType == Api.Models.Enumerations.FeedItemTypeEnum.Person)
                {
                    var mainMessage = MainFeedMessages.FirstOrDefault(m => m.Id == selectedCommentForDelete.ParentId);
                    if (mainMessage != null)
                    {
                        mainMessage.CommentCount--;
                        var comments = await _feedSerice.GetComments(mainMessage.FeedId, mainMessage.Id, true);
                        mainMessage.Comments = new ObservableCollection<FeedMessageItemModel>(comments);
                    }
                }
                else
                    await RefreshAsync().ConfigureAwait(false);

                SelectedItemForEditDelete = null;
            }
        }

        private async Task<bool> SubmitNewItem(AddFeedItemModel model)
        {
            if (model == null)
                return false;

            if (!await _internetHelper.HasInternetConnection())
            {
                await NoInternetConnectionAlert();
                return false;
            }

            if (model.FeedType != Api.Models.Enumerations.FeedTypeEnum.FactoryUpdates)
            {
                var title = model.Description.Value?.Take(250);
                model.Title.Value = title == null ? null : string.Concat(title);
            }

            model.Description.Validatate();
            model.Title.Validatate();

            if (!(model.Description.IsValid && model.Title.IsValid))
                return false;

            NewPost = new AddFeedItemModel(FeedItemTypeEnum.Company, FeedTypeEnum.MainFeed)
            {
                FeedId = MainFeedId
            };

            List<MediaItem> mediaItems = new();

            try
            {
                if (await InternetHelper.HasInternetConnection())
                    mediaItems = await UploadMediaItemsAsync(model);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is HttpRequestException)
            {
                Page page = NavigationService.GetCurrentPage();
                string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);
                await MainThread.InvokeOnMainThreadAsync(async () => await page.DisplayActionSheet("There was a problem with uploading media files. Please try again later.", null, ok));

                return true;
            }

            var item = new FeedMessageItemModel
            {
                CompanyId = UserSettings.CompanyId,
                Description = model.Description.Value,
                Title = model.Title.Value,
                ItemType = model.FeedItemType,
                IsSticky = model.IsSticky,
                IsHighlighted = model.IsHighlighted,
                FeedId = model.FeedId,
                ItemDate = DateTime.UtcNow,
                UserId = UserSettings.Id,
                ParentId = model.ParentId,
                MediaItems = mediaItems,
                FeedType = model.FeedType
            };

            item.MediaItems.RemoveAll(x => x.IsEmpty);

            item.ConvertMediaItemsToAttachmentsAndMedia();

            var id = await _feedSerice.PostFeedItemAsync(item);
            if (id > 0)
            {
                if (item.FeedType == FeedTypeEnum.MainFeed && item.ParentId != null)
                {
                    var mainMessage = MainFeedMessages.FirstOrDefault(m => m.Id == item.ParentId);
                    if (mainMessage != null)
                    {
                        mainMessage.CommentCount++;
                        var comments = await _feedSerice.GetComments(item.FeedId, item.ParentId.Value, true);
                        mainMessage.Comments = new ObservableCollection<FeedMessageItemModel>(comments);
                    }
                    //update my stats
                    MyEzFeedStats.MyCommentsTotal++;
                }
                else
                    await RefreshAsync().ConfigureAwait(false);
            }

            return true;
        }


        private async Task<bool> EditItem(AddFeedItemModel model)
        {
            if (model == null)
                return false;

            if (!await _internetHelper.HasInternetConnection())
            {
                await NoInternetConnectionAlert();
                return false;
            }

            if (model.FeedType != FeedTypeEnum.FactoryUpdates)
            {
                var title = model.Description.Value?.Take(250);
                model.Title.Value = title == null ? null : string.Concat(title);
            }

            model.Description.Validatate();
            model.Title.Validatate();

            if (!(model.Description.IsValid && model.Title.IsValid))
                return false;

            List<MediaItem> mediaItems = new();

            try
            {
                if (await InternetHelper.HasInternetConnection())
                    mediaItems = await UploadMediaItemsAsync(model);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is HttpRequestException)
            {
                Page page = NavigationService.GetCurrentPage();
                string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);
                await MainThread.InvokeOnMainThreadAsync(async () => await page.DisplayActionSheet("There was a problem with uploading media files. Please try again later.", null, ok));

                return true;
            }

            var item = new FeedMessageItemModel
            {
                CompanyId = UserSettings.CompanyId,
                Description = model.Description.Value,
                Title = model.Title.Value,
                ItemType = model.FeedItemType,
                IsSticky = model.IsSticky,
                IsHighlighted = model.IsHighlighted,
                FeedId = model.FeedId,
                ItemDate = DateTime.UtcNow,
                UserId = UserSettings.Id,
                ParentId = model.ParentId,
                Id = model.Id,
                FeedType = model.FeedType,
                MediaItems = model.MediaItems.ToList()
            };

            item.MediaItems.RemoveAll(x => x.IsEmpty);

            item.ConvertMediaItemsToAttachmentsAndMedia();

            var result = await _feedSerice.EditFeedItemAsync(item);
            if (result)
            {
                await UpdateItem(item);
            }

            return true;
        }

        private async Task UpdateItem(FeedMessageItemModel item)
        {
            //update comment
            if (item.FeedType == FeedTypeEnum.MainFeed && item.ParentId != null)
            {
                var mainMessage = MainFeedMessages.FirstOrDefault(m => m.Id == item.ParentId);
                if (mainMessage != null)
                {
                    var comments = await _feedSerice.GetComments(item.FeedId, item.ParentId.Value, true);
                    mainMessage.Comments = new ObservableCollection<FeedMessageItemModel>(comments);
                }
            }
            else
            {
                //update factory updates
                if (item.FeedType == Api.Models.Enumerations.FeedTypeEnum.FactoryUpdates && SelectedFactoryUpdate != null)
                {
                    SelectedFactoryUpdate.Title = item.Title;
                    SelectedFactoryUpdate.Description = item.Description;
                    SelectedFactoryUpdate.IsSticky = item.IsSticky;
                    SelectedFactoryUpdate.ModifiedById = UserSettings.Id;
                    SelectedFactoryUpdate.ModifiedByUsername = UserSettings.Username;
                    SelectedFactoryUpdate.MediaItems = item.MediaItems;
                }
                //update main post 
                else
                {
                    var mainFeedItem = MainFeedMessages.FirstOrDefault(m => m.Id == item.Id);
                    if (mainFeedItem != null)
                    {
                        mainFeedItem.Title = item.Title;
                        mainFeedItem.Description = item.Description;
                        mainFeedItem.ModifiedById = UserSettings.Id;
                        mainFeedItem.ModifiedByUsername = UserSettings.Username;
                        mainFeedItem.MediaItems = item.MediaItems;
                    }
                }
            }
        }

        private bool mediaItemPopupIsOpen = false;

        private async Task PickMediaAsync(ObservableCollection<MediaItem> mediaItems, MediaTypeEnum mediaTypeEnum)
        {
            if (mediaItemPopupIsOpen) return;
            mediaItemPopupIsOpen = true;

            MediaItem mediaItem = mediaItems?.FirstOrDefault(x => x.IsEmpty) ?? null;

            if (mediaItem == null)
            {
                await DisplayAttachmentsValidationPopup();
                return;
            }

            List<MediaOption> mediaOptions = GetMediaOptions(mediaTypeEnum);

            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOptions);

            if (dialogResult.Result == null && dialogResult.IsSuccess && !dialogResult.IsRemoved)
                return;

            if (dialogResult.IsSuccess)
            {
                mediaItem.CopyFrom(dialogResult.Result ?? MediaItem.Empty());
                mediaItem.CreatedAt = DateTimeHelper.Now;
                OnPropertyChanged(nameof(NewPost));
            }
            mediaItemPopupIsOpen = false;
        }

        private async Task DisplayAttachmentsValidationPopup()
        {
            Page page = NavigationService.GetCurrentPage();
            var translated = TranslateExtension.GetValueFromDictionary(LanguageConstants.feedMaxNumberOfAttachments);
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            await page.DisplayActionSheet(translated, null, cancel);
        }

        private List<MediaOption> GetMediaOptions(MediaTypeEnum mediaTypeEnum)
        {
            List<MediaOption> result = new();
            switch (mediaTypeEnum)
            {
                case MediaTypeEnum.Image:
                    result = new List<MediaOption>() { MediaOption.TakePhoto, MediaOption.PhotoGallery };
                    break;
                case MediaTypeEnum.Video:
                    result = new List<MediaOption>() { MediaOption.Video, MediaOption.VideoGallery };
                    break;
                case MediaTypeEnum.Docs:
                    result = new List<MediaOption>() { MediaOption.PDF };
                    break;
            }

            return result;
        }

        private async Task LikeMessage(object message)
        {
            if (!await _internetHelper.HasInternetConnection())
            {
                await NoInternetConnectionAlert();
                return;
            }

            if (message != null && message is FeedMessageItemModel messageItem)
            {
                if (messageItem.IsLikedByCurrentUser)
                {
                    messageItem.LikesUserIds.Remove(UserSettings.Id);
                    if (LikedUsers != null)
                        messageItem.LikedByUsers.Remove(LikedUsers.Where(x => x != null).FirstOrDefault(x => x.Id == UserSettings.Id));
                    messageItem.LikeCount--;
                    messageItem.IsLikedByCurrentUser = !messageItem.IsLikedByCurrentUser;
                    await _feedSerice.PostSetMessageLiked(messageItem, false).ConfigureAwait(false);
                    //update my stats
                    MyEzFeedStats.MyLikesTotal--;
                }
                else
                {
                    messageItem.LikesUserIds.Add(UserSettings.Id);
                    messageItem.LikedByUsers.Add(new UserProfileModel
                    {
                        Id = UserSettings.Id,
                        FirstName = UserSettings.Fullname,
                        Picture = UserSettings.UserPictureUrl
                    });
                    messageItem.LikeCount++;
                    messageItem.IsLikedByCurrentUser = !messageItem.IsLikedByCurrentUser;
                    await _feedSerice.PostSetMessageLiked(messageItem, true).ConfigureAwait(false);
                    //update my stats
                    MyEzFeedStats.MyLikesTotal++;
                }
            }
        }

        private async Task GetComments(object message)
        {
            if (message != null && message is FeedMessageItemModel messageItem)
            {
                if (messageItem.CommentCount > 0)
                {
                    var comments = await _feedSerice.GetComments(messageItem.FeedId, messageItem.Id, true);
                    messageItem.Comments = new ObservableCollection<FeedMessageItemModel>(comments);
                }

                if (messageItem.AreCommentsVisible)
                    messageItem.AreCommentsVisible = !messageItem.AreCommentsVisible;
                else
                {
                    if (!await _internetHelper.HasInternetConnection())
                    {
                        await NoInternetConnectionAlert();
                        return;
                    }
                    messageItem.AreCommentsVisible = !messageItem.AreCommentsVisible;
                }
            }
        }

        public override async Task CancelAsync()
        {
            SelectedFactoryUpdate = null;
        }

        protected override void RefreshCanExecute()
        {
            base.RefreshCanExecute();
            (OpenCloseAddCommentPopupCommand as Command)?.ChangeCanExecute();
            (OpenCloseAddFactoryUpdateCommand as Command)?.ChangeCanExecute();
            (OpenCloseEditDeletePopupCommand as Command)?.ChangeCanExecute();
            (OpenCloseConfirmationPopupCommand as Command)?.ChangeCanExecute();
        }

        protected override async Task RefreshAsync()
        {
            var feedData = await _feedSerice.GetFeedDataAsync(refresh: true, limit: limit, offset: 0);
            MainFeedId = feedData.MainFeedId;
            FactoryUpdatesFeedId = feedData.FactoryUpdatesFeedId;
            MainFeedMessages = new ObservableCollection<FeedMessageItemModel>(feedData.MainFeedMessages);
            FactoryUpdateMessages = new ObservableCollection<FeedMessageItemModel>(feedData.FactoryUpdatesMessages);
            CanLoadMoreMainFeed = MainFeedMessages.Count >= limit;
            currentOffset = MainFeedMessages.Count;

            MyEzFeedStats = await _reportService.GetMyEzFeedStatsAsync(refresh: true);
            NewPost = new AddFeedItemModel(Api.Models.Enumerations.FeedItemTypeEnum.Company, Api.Models.Enumerations.FeedTypeEnum.MainFeed)
            {
                FeedId = MainFeedId
            };
            IsLoadingMoreMainFeed = false;
        }

        private async Task LoadMainFeedMoreAsync()
        {
            if (IsLoadingMoreMainFeed || !CanLoadMoreMainFeed)
                return;

            IsLoadingMoreMainFeed = true;
            currentOffset += limit;

            var moreItems = await _feedSerice.GetMainFeedMessages(limit: limit, offset: currentOffset);
            foreach (var item in moreItems)
                MainFeedMessages.Add(item);

            CanLoadMoreMainFeed = moreItems.Count >= limit;

            IsLoadingMoreMainFeed = false;
        }
        private async Task LoadFactoryUpdateMoreSync()
        {
            if (IsLoadingMoreFactoryUpdate || !CanLoadMoreFactoryUpdate)
                return;

            IsLoadingMoreFactoryUpdate = true;
            currentOffset += limit;

            var moreItems = await _feedSerice.GetFactoryUpdatesMessages(limit: limit, offset: currentOffset);
            foreach (var item in moreItems)
                FactoryUpdateMessages.Add(item);

            CanLoadMoreFactoryUpdate = moreItems.Count >= limit;

            IsLoadingMoreFactoryUpdate = false;
        }

        public FeedViewModel(INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IFeedService feedService,
            IMediaHelper mediaHelper,
            IMediaService mediaService,
            IUpdateService updateService,
            ISyncService syncService,
            IReportService reportService,
            IInternetHelper internetHelper) : base(navigationService, userService, messageService, actionsService)
        {
            _feedSerice = feedService;
            _mediaHelper = mediaHelper;
            _mediaService = mediaService;
            _reportService = reportService;
            _updateService = updateService;
            _syncService = syncService;
            _internetHelper = internetHelper;

            LikeCommand = new AsyncCommand<object>(LikeMessage);
            CommentsCommand = new AsyncCommand<object>(GetComments);
            RemoveMediaItemCommand = new Microsoft.Maui.Controls.Command<MediaItem>((obj) =>
            {
                obj.CopyFrom(MediaItem.Empty());
                OnPropertyChanged(nameof(NewPost));
            });
            AddPhotoMediaItemCommand = new AsyncCommand<ObservableCollection<MediaItem>>((ObservableCollection<MediaItem> mediaItems) => PickMediaAsync(mediaItems, MediaTypeEnum.Image));
            AddVideoMediaItemCommand = new AsyncCommand<ObservableCollection<MediaItem>>((ObservableCollection<MediaItem> mediaItems) => PickMediaAsync(mediaItems, MediaTypeEnum.Video));
            AddDocumentMediaItemCommand = new AsyncCommand<ObservableCollection<MediaItem>>((ObservableCollection<MediaItem> mediaItems) => PickMediaAsync(mediaItems, MediaTypeEnum.Docs));
            DeleteCommentCommand = new AsyncCommand(async () =>
            {
                if (SelectedItemForEditDelete != null)
                {
                    await DeleteItem(SelectedItemForEditDelete);

                    if (IsConfirmationPopupOpen)
                        IsConfirmationPopupOpen = !IsConfirmationPopupOpen;
                    if (IsEditDeletePopupOpen)
                        IsEditDeletePopupOpen = !IsEditDeletePopupOpen;
                }
            });

            SubmitNewCommentCommand = new AsyncCommand<AddFeedItemModel>((model) =>
            {
                return ExecuteLoadingAction(async () =>
                {
                    var isValid = false;

                    if (model.Id > 0)
                        isValid = await EditItem(model);
                    else
                        isValid = await SubmitNewItem(model);

                    if (!isValid)
                        return;

                    if (IsAddCommentPopupOpen)
                        IsAddCommentPopupOpen = !IsAddCommentPopupOpen;
                    if (IsEditDeletePopupOpen)
                        IsEditDeletePopupOpen = !IsEditDeletePopupOpen;
                });

            }, CanExecuteCommands);

            SubmitNewPostCommand = new AsyncCommand<AddFeedItemModel>((model) =>
            {
                return ExecuteLoadingAction(async () =>
                {
                    if (model == null)
                        return;
                    if (model.Id > 0)
                        await EditItem(model);
                    else
                        await SubmitNewItem(model);
                });
            }, CanExecuteCommands);

            OpenCloseAddCommentPopupCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                if (obj is FeedMessageItemModel model)
                {
                    NewItem = new AddFeedItemModel(Api.Models.Enumerations.FeedItemTypeEnum.Person, Api.Models.Enumerations.FeedTypeEnum.MainFeed, model.ParentId)
                    {
                        ParentId = model.Id,
                        FeedId = MainFeedId,
                    };
                    IsAddCommentPopupOpen = !IsAddCommentPopupOpen;
                }
            }, CanExecuteCommands);

            OpenCloseLikedUsersCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                if (obj is FeedMessageItemModel model)
                {
                    LikedUsers = model.LikedByUsers;

                    if (LikedUsers == null || !LikedUsers.Any())
                        return;

                    IsLikedUserPopupOpen = !IsLikedUserPopupOpen;
                }
            }, CanExecuteCommands);

            OpenCloseAddFactoryUpdateCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                NewItem = new AddFeedItemModel(Api.Models.Enumerations.FeedItemTypeEnum.Company, Api.Models.Enumerations.FeedTypeEnum.FactoryUpdates)
                {
                    FeedId = FactoryUpdatesFeedId
                };
                IsAddCommentPopupOpen = !IsAddCommentPopupOpen;
            }, CanExecuteCommands);

            EditItemCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                ExecuteLoadingAction(() =>
                {
                    if (obj is FeedMessageItemModel model)
                    {
                        SelectedItemForEditDelete = model;
                    }

                    NewItem = new AddFeedItemModel(SelectedItemForEditDelete.ItemType, SelectedItemForEditDelete.FeedType, SelectedItemForEditDelete.ParentId)
                    {
                        ParentId = SelectedItemForEditDelete.ParentId,
                        Id = SelectedItemForEditDelete.Id,
                        FeedId = SelectedItemForEditDelete.FeedId,
                        Title = new Classes.ValidationRules.ValidatableObject<string>(SelectedItemForEditDelete.Title),
                        Description = new Classes.ValidationRules.ValidatableObject<string>(SelectedItemForEditDelete.Description),
                        IsSticky = SelectedItemForEditDelete.IsSticky,
                        IsHighlighted = SelectedItemForEditDelete.IsHighlighted,
                    };

                    for (int i = 0; i < SelectedItemForEditDelete.MediaItems.Count; i++)
                    {
                        NewItem.MediaItems[i] = SelectedItemForEditDelete.MediaItems[i];

                        if (NewItem.MediaItems[i]?.PictureUrl?.ToLower().EndsWith(".pdf") ?? false)
                            NewItem.MediaItems[i].IsFile = true;
                    }

                    IsAddCommentPopupOpen = !IsAddCommentPopupOpen;
                });

            }, CanExecuteCommands);

            OpenCloseDeleteCommentPopupCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                if (obj is FeedMessageItemModel comment)
                {
                    SelectedItemForEditDelete = comment;
                }
                IsConfirmationPopupOpen = !IsConfirmationPopupOpen;
            }, CanExecuteCommands);

            OpenCloseEditDeletePopupCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                if (obj is FeedMessageItemModel item)
                {
                    SelectedItemForEditDelete = item;
                    IsEditDeletePopupOpen = !IsEditDeletePopupOpen;
                }
            }, CanExecuteCommands);

            OpenCloseConfirmationPopupCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                IsConfirmationPopupOpen = !IsConfirmationPopupOpen;
            }, CanExecuteCommands);

            SwipeStartedCommand = new Microsoft.Maui.Controls.Command<object>((obj) =>
            {
                if (obj is Syncfusion.Maui.ListView.SwipeStartingEventArgs args)
                {
                    var item = (FeedMessageItemModel)args.DataItem;
                    if (!item.CanModifyPost)
                        args.Cancel = true;
                }
            }, CanExecuteCommands);
            CanLoadMoreMainFeed = true;
        }

        public override async Task Init()
        {
            var feedData = await _feedSerice.GetFeedDataAsync(refresh: true, limit: limit, offset: currentOffset);
            MainFeedId = feedData.MainFeedId;
            FactoryUpdatesFeedId = feedData.FactoryUpdatesFeedId;
            MainFeedMessages = new ObservableCollection<FeedMessageItemModel>(feedData.MainFeedMessages);
            FactoryUpdateMessages = new ObservableCollection<FeedMessageItemModel>(feedData.FactoryUpdatesMessages);
            NewPost = new AddFeedItemModel(FeedItemTypeEnum.Company, FeedTypeEnum.MainFeed)
            {
                FeedId = MainFeedId
            };
            MyEzFeedStats = await _reportService.GetMyEzFeedStatsAsync();


            MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
            {
                await HandleUpdateCheck();
            });

            await base.Init();
            CanLoadMoreFactoryUpdate = true;
        }

        private async Task NoInternetConnectionAlert()
        {
            var page = NavigationService.GetCurrentPage();
            string offlineResult = Statics.LanguageDictionary.GetValue("ONLY_ONLINE_ACTION");
            string okResult = Statics.LanguageDictionary.GetValue("BASE_TEXT_OK");
            await page.DisplayActionSheet(offlineResult, null, okResult);
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
            });
            base.Dispose(disposing);
        }

        private async Task HandleUpdateCheck()
        {
            try
            {
                if (await FifteenSecondLock.WaitAsync(0))
                {
                    if (_updateService != null && await _updateService.CheckForUpdatedEzFeedAsync())
                    {
                        //reload ez feed data
                        await _syncService.LoadEzFeedAsync();
                        //update main feed
                        var mainFeed = await _feedSerice.GetMainFeedMessages();
                        UpdateFeed(MainFeedMessages, mainFeed);
                        //update factory updates
                        var factoryUpdates = await _feedSerice.GetFactoryUpdatesMessages();
                        UpdateFeed(FactoryUpdateMessages, factoryUpdates);
                    }

                    if (_updateService != null && await _updateService.CheckForUpdatedEzFeedCommentsAsync())
                    {
                        //update comments
                        var itemsWithOpenComments = MainFeedMessages.Where(m => m.AreCommentsVisible).ToList() ?? new List<FeedMessageItemModel>();
                        foreach (var item in itemsWithOpenComments)
                        {
                            var comments = await _feedSerice.GetComments(item.FeedId, item.Id, true);
                            item.Comments = new ObservableCollection<FeedMessageItemModel>(comments);
                            item.CommentCount = comments.Count;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Debugger.Break();
            }
            finally
            {
                if (FifteenSecondLock.CurrentCount == 0)
                    FifteenSecondLock.Release();
            }
        }

        private void UpdateFeed(ObservableCollection<FeedMessageItemModel> originalList, List<FeedMessageItemModel> modifiedList)
        {
            var modifiedIds = new HashSet<int>(modifiedList.Select(m => m.Id));
            var originalIds = new HashSet<int>(originalList.Select(o => o.Id));

            var itemsToRemove = originalList.Where(a => !modifiedIds.Contains(a.Id)).ToList();
            var itemsToAdd = modifiedList.Where(b => !originalIds.Contains(b.Id)).ToList();

            foreach (var item in itemsToRemove)
            {
                var itemToRemove = originalList.FirstOrDefault(a => a.Id == item.Id);
                if (itemToRemove != null)
                    originalList.Remove(itemToRemove);
            }

            foreach (var item in originalList)
            {
                var modified = modifiedList.FirstOrDefault(m => m.Id == item.Id);
                if (modified != null)
                {
                    item.Title = modified.Title;
                    item.Description = modified.Description;
                    item.IsSticky = modified.IsSticky;
                    item.LikesUserIds = modified.LikesUserIds;
                    item.CommentCount = modified.CommentCount;
                    item.LikeCount = modified.LikeCount;
                    item.MediaItems = modified.MediaItems;
                    item.LikedByUsers = modified.LikedByUsers;
                }
            }

            foreach (var item in itemsToAdd)
            {
                originalList.Add(item);
            }
        }

        private async Task<List<MediaItem>> UploadMediaItemsAsync(AddFeedItemModel model)
        {
            List<MediaItem> mediaItems = new();

            mediaItems = model.MediaItems.Where(x => x.PictureUrl != null || x.VideoUrl != null || x.FileUrl != null).ToList();

            await _mediaService.UploadMediaItemsAsync(mediaItems, MediaStorageTypeEnum.FactoryFeedMessages, 0);

            return mediaItems;
        }
    }
}