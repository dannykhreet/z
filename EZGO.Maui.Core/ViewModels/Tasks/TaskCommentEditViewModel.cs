using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Tasks
{
    public class TaskCommentEditViewModel : BaseViewModel
    {
        #region Translations 

        private readonly string ValidationErrorText = TranslateExtension.GetValueFromDictionary(LanguageConstants.commentValidationErrorText);
        private readonly string ValidationErrorTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.commentValidationErrorTitle);
        private readonly string ErrorTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.commentCannotSaveErrorTitle);
        private readonly string ErrorText = TranslateExtension.GetValueFromDictionary(LanguageConstants.commentCannotSaveErrorText);

        #endregion

        #region Public Properties

        public const int NumberOfMediaElements = 6;

        [DoNotNotify]
        public long TaskId { get; set; }

        [DoNotNotify]
        public int TaskTemplateId { get; set; }

        [DoNotNotify]
        public ActionType Type { get; set; }


        public ObservableCollection<MediaItem> MediaElements { get; set; }

        public string CommentText { get; set; }

        public bool IsBusySaving { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Used only if <see cref="Type"/> is different than a <see cref="ActionType.Task"/>.
        /// </summary>
        /// <value>Stores current task template model with the most recent comments.</value>
        [DoNotNotify]
        public BasicTaskTemplateModel LocalTask { get; set; }

        /// <summary>
        /// Model of the comment being displayed or <see langword="null"/> if <see cref="IsNew"/> is <see langword="true"/>.
        /// </summary>
        public CommentModel Comment { get; set; }

        public bool IsNew { get; set; }

        public bool EditingEnabled { get; set; }

        public bool ChangeButtonVisible => !IsNew && !EditingEnabled && SupportsEditing;

        public bool SupportsEditing { get; set; }

        public List<TagModel> AllTags { get; set; }

        #endregion

        #region Commands

        public IAsyncRelayCommand ChangeMediaItemCommand => new AsyncRelayCommand<MediaItem>(async (mediaItem) =>
        {
            if (mediaItem == null)
                return;

            await ExecuteLoadingActionAsync(async () =>
            {
                if (EditingEnabled)
                {
                    await PickMediaAsync(mediaItem);
                }
                else
                {
                    await NavigateToDetailAsync(mediaItem);
                }
            });
        }, CanExecuteCommands);

        public IAsyncRelayCommand SubmitCommand => new AsyncRelayCommand(async () =>
        {
            if (IsBusySaving || IsLoading)
                return;

            IsBusySaving = true;
            await SubmitAsync();
            IsBusySaving = false;
        }, CanExecuteCommands);

        public ICommand EditCommand => new Command(() =>
        {
            EditingEnabled = true;
        }, CanExecuteCommands);

        #endregion       

        private readonly IMediaHelper _mediaHelper;
        private readonly ITaskCommentService _taskComments;
        private readonly ITagsService _tagsService;

        public TaskCommentEditViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMediaHelper mediaHelper,
            ITaskCommentService taskCommentService,
            ITagsService tagsService) : base(navigationService, userService, messageService, actionsService)
        {
            _mediaHelper = mediaHelper;
            _taskComments = taskCommentService;
            _tagsService = tagsService;
        }

        public override async Task Init()
        {
            if (IsNew)
            {
                EditingEnabled = true;
                AllTags = await Task.Run(async () => await _tagsService.GetTagModelsAsync(tagableObjectEnum: Api.Models.Enumerations.TagableObjectEnum.Comment));
            }
            else
            {
                CommentText = Comment?.CommentText;
                EditingEnabled = false;
                MediaElements = new ObservableCollection<MediaItem>(Comment?.Attachments ?? new List<MediaItem>());
                AllTags = await Task.Run(async () => await _tagsService.GetTagModelsAsync(activeTags: Comment?.Tags, tagableObjectEnum: Api.Models.Enumerations.TagableObjectEnum.Comment));
            }
            await base.Init();
        }

        private async Task PickMediaAsync(MediaItem mediaItem)
        {
            List<MediaOption> mediaOptions = new List<MediaOption> {
                MediaOption.TakePhoto,
                MediaOption.PhotoGallery,
                MediaOption.VideoGallery,
                MediaOption.Video
            };

            if (!mediaItem.IsEmpty)
                mediaOptions.Add(MediaOption.RemoveMedia);

            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOptions);

            if (dialogResult.Result == null && dialogResult.IsSuccess && !dialogResult.IsRemoved)
                return;

            if (dialogResult.IsSuccess)
                mediaItem.CopyFrom(dialogResult.Result ?? MediaItem.Empty());
        }

        private async Task SubmitAsync()
        {
            // Validate first
            var nonEmptyMediaElements = MediaElements.Where(x => !x.IsEmpty);
            if (string.IsNullOrWhiteSpace(CommentText) && !nonEmptyMediaElements.Any())
            {
                var page = NavigationService.GetCurrentPage();
                if (page != null)
                    await page.DisplayAlert(ValidationErrorTitle, ValidationErrorText, "OK");
                return;
            }

            // Task comment
            if (Type == ActionType.Task)
            {
                try
                {
                    var model = new CommentModel()
                    {
                        UserId = UserSettings.Id,
                        CreatedBy = UserSettings.Fullname,
                        CompanyId = UserSettings.CompanyId,
                        TaskId = (int)TaskId,
                        TaskTemplateId = TaskTemplateId,
                        CommentText = CommentText,
                        CommentDate = DateTime.UtcNow,
                        Attachments = nonEmptyMediaElements.ToList(),
                        Tags = TagsHelper.GetActiveTagsList(AllTags),
                    };

                    bool success;
                    if (IsNew)
                    {
                        success = await _taskComments.AddAsync(model);

                        if (success)
                            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.TaskCommentAdded, (int)TaskId); });
                    }
                    else
                    {
                        model.Id = Comment.Id;
                        model.InternalId = Comment.InternalId;
                        success = await _taskComments.ChangeAsync(model);
                    }

                    if (success)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.TaskCommentChanged); });
                        await CancelAsync();
                    }
                    else
                    {
                        var page = NavigationService.GetCurrentPage();
                        if (page != null)
                            await page.DisplayAlert(ErrorTitle, ErrorText, "OK");
                    }
                }
                catch (Exception ex) when (ex is ArgumentNullException || ex is HttpRequestException)
                {
                    Page page = NavigationService.GetCurrentPage();
                    string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);
                    await MainThread.InvokeOnMainThreadAsync(async () => await page.DisplayActionSheet("There was a problem with uploading media files. Please try again later.", null, ok));

                    return;
                }
            }
            // Checklist/audit comment
            else
            {
                if (IsNew)
                {
                    LocalTask.LocalComments ??= new List<CommentModel>();
                    var comment = new CommentModel()
                    {
                        Attachments = nonEmptyMediaElements.ToList(),
                        CommentDate = DateTime.UtcNow,
                        UserId = UserSettings.Id,
                        CreatedBy = UserSettings.Fullname,
                        CompanyId = UserSettings.CompanyId,
                        CommentText = CommentText,
                        TaskTemplateId = TaskTemplateId,
                        Tags = TagsHelper.GetActiveTagsList(AllTags),
                    };
                    LocalTask.LocalComments.Add(comment);
                    await _taskComments.AddToChecklistAuditAsync(comment);
                }
                else
                {
                    // Update the comment
                    Comment.Attachments = nonEmptyMediaElements.ToList();
                    Comment.CommentDate = DateTime.UtcNow;
                    Comment.UserId = UserSettings.Id;
                    Comment.CreatedBy = UserSettings.Fullname;
                    Comment.CompanyId = UserSettings.CompanyId;
                    Comment.CommentText = CommentText;
                    Comment.TaskTemplateId = TaskTemplateId;
                    Comment.Tags = TagsHelper.GetActiveTagsList(AllTags);
                    await _taskComments.ChangeLocalForChecklistAuditAsync(Comment);
                }

                LocalTask.UpdateActionBubbleCount();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, Constants.TaskCommentChanged);
                    MessagingCenter.Send(this, Constants.TaskTemplateCommentAdded);
                });
                await CancelAsync();
            }
        }

        private async Task NavigateToDetailAsync(MediaItem mediaItem)
        {
            var photos = MediaElements.Where(item => !item.IsEmpty).ToList();
            var index = photos.IndexOf(mediaItem);

            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
            var items = new List<ThumbnailGridDetailModel>();
            photos.ForEach(async x =>
            {
                if (!x.IsVideo)
                {
                    items.Add(new ThumbnailGridDetailModel
                    {
                        Picture = x.PictureUrl,
                        PDFStream = x.PictureUrl?.ToLower().EndsWith(".pdf") ?? false ? await x.GetFileStream() : null,
                        IsLocalMedia = x.IsLocalFile
                    });
                }
                else
                {
                    items.Add(new ThumbnailGridDetailModel
                    {
                        Video = x.VideoUrl,
                        IsLocalMedia = x.IsLocalFile
                    });
                }
            });
            itemsDetailViewModel.Items = new List<IDetailItem>(items);
            itemsDetailViewModel.SenderClassName = nameof(TaskCommentEditViewModel);
            itemsDetailViewModel.IsLocalPicture = items[index].IsLocalMedia;
            itemsDetailViewModel.SelectedItem = items[index];
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        /// <summary>
        /// On-property changed event for <see cref="EditingEnabled"/>
        /// </summary>
#pragma warning disable IDE0051 // Remove unused private members
        private void OnEditingEnabledChanged()
        {
            if (EditingEnabled)
            {
                // Fill in the missing media elements with empty media items
                if (MediaElements != null)
                {
                    var mediaElementsFilled = MediaElements.Concat(Enumerable.Range(0, NumberOfMediaElements - MediaElements.Count).Select(x => MediaItem.Empty()));
                    MediaElements = new ObservableCollection<MediaItem>(mediaElementsFilled);
                }
                else
                {
                    MediaElements = new ObservableCollection<MediaItem>(Enumerable.Range(1, NumberOfMediaElements).Select(x => MediaItem.Empty()));
                }
            }
        }
#pragma warning restore IDE0051 // Remove unused private members

        protected override void Dispose(bool disposing)
        {
            _taskComments.Dispose();
            //_mediaHelper.di
            base.Dispose(disposing);
        }
    }
}
