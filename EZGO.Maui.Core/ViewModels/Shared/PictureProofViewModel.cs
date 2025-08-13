using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Shared
{
    public class PictureProofViewModel : BaseViewModel
    {
        private readonly string ValidationErrorText = TranslateExtension.GetValueFromDictionary(LanguageConstants.pictureProofValidationErrorText);

        public const int NumberOfExtraMediaElements = 5;

        private readonly IMediaHelper _mediaHelper;
        private readonly IWatermarkGenerator _watermarkGenerator;
        private readonly IFileService _fileService;

        public ObservableCollection<MediaItem> MediaElements { get; set; }
        public List<MediaItem> ChangedMediaElements { get; set; } = new List<MediaItem>();

        public MediaItem MainMediaElement { get; set; }

        public BasicTaskTemplateModel SelectedTaskTemplate { get; set; }
        public BasicTaskModel SelectedTask { get; set; }

        public TaskStatusEnum? TaskStatus { get; set; }

        public int? Score { get; set; }

        public bool IsNew { get; set; }

        public bool IsBusySaving { get; set; }

        public bool EditingEnabled { get; set; }

        public bool IsContinueButtonVisible { get; set; }

        public bool ChangeButtonVisible => !IsNew && !EditingEnabled && SupportsEditing;

        public bool SupportsEditing { get; set; }

        #region Commands

        public ICommand EditCommand => new Command(() =>
        {
            EditingEnabled = true;
            IsContinueButtonVisible = true;
        }, CanExecuteCommands);

        public IAsyncRelayCommand SubmitCommand => new AsyncRelayCommand(async () =>
        {
            await ExecuteLoadingAction(SubmitAsync);
        }, CanExecuteCommands);

        public IAsyncRelayCommand ChangeMediaItemCommand => new AsyncRelayCommand<MediaItem>(async (mediaItem) =>
        {
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


        #endregion

        private async Task NavigateToDetailAsync(MediaItem mediaItem)
        {
            MainMediaElement ??= MediaItem.Empty();
            var photos = new List<MediaItem>();
            if (!MainMediaElement.IsEmpty)
                photos.Add(MainMediaElement);
            if (MediaElements?.Count > 0)
                photos.AddRange(MediaElements.Where(item => !item.IsEmpty && !item.IsVideo).ToList());

            using var scope = App.Container.CreateScope();
            var actionDetailViewModel = scope.ServiceProvider.GetService<ActionDetailViewModel>();
            actionDetailViewModel.MediaItems = photos;
            actionDetailViewModel.SelectedMediaItem = mediaItem;
            actionDetailViewModel.CurrentIndex = photos.IndexOf(mediaItem);

            await NavigationService.NavigateAsync(viewModel: actionDetailViewModel);
        }


        private async Task SubmitAsync()
        {
            // Validate first
            MediaElements ??= new ObservableCollection<MediaItem>();
            MainMediaElement ??= new MediaItem();
            var nonEmptyMediaElements = new List<MediaItem>();

            if (!MainMediaElement.IsEmpty)
                nonEmptyMediaElements.Add(MainMediaElement);

            nonEmptyMediaElements.AddRange(MediaElements.Where(x => !x.IsEmpty).ToList());

            if (!nonEmptyMediaElements.Any())
            {
                if (NavigationService != null)
                {
                    var page = NavigationService.GetCurrentPage();
                    if (page != null)
                    {
                        await page.DisplayAlert(null, ValidationErrorText, "OK");
                    }
                }
                return;
            }

            try
            {
                if (SelectedTask != null)
                {
                    if (nonEmptyMediaElements.Any())
                    {
                        foreach (var item in nonEmptyMediaElements)
                        {
                            if (item.IsLocalFile && item.MediaFile != null)
                            {
                                await Task.Run(async () =>
                                {
                                    item.PictureUrl = await _watermarkGenerator.GeneratePictureProofWatermark(item.MediaFile.Path, SelectedTask.Name, item.CreatedAt);
                                    item.UserFullName = UserSettings.userSettingsPrefs.Fullname;
                                    item.UserId = UserSettings.userSettingsPrefs.Id;
                                });
                            }
                        }
                    }

                    SelectedTask.PictureProofMediaItems = nonEmptyMediaElements;
                    await TaskHelper.UploadTaskPictureProofAsync(SelectedTask);
                    if (TaskStatus.HasValue)
                        await TaskHelper.UploadTaskStatusAsync(SelectedTask, TaskStatus.Value, false, null);
                    else
                        await TaskHelper.SetTaskStatusAsync(SelectedTask, TaskStatus.Value, null);
                }
                else if (SelectedTaskTemplate != null)
                {
                    if (nonEmptyMediaElements.Any())
                    {
                        foreach (var item in nonEmptyMediaElements)
                        {
                            //check if item with that datetime exists - image not changed
                            string pictureProofFilename = string.Format(Constants.PictureProofFilenameFormat, item.CreatedAt.ToFileTime());
                            bool imageExists = _fileService?.CheckIfFileExists(pictureProofFilename, Constants.PictureProofsDirectory) ?? false;

                            if (!imageExists && item.MediaFile != null)
                            {
                                await Task.Run(async () =>
                                {
                                    item.PictureUrl = await _watermarkGenerator.GeneratePictureProofWatermark(item.MediaFile.Path, SelectedTaskTemplate.Name, item.CreatedAt);
                                    item.UserFullName = UserSettings.userSettingsPrefs.Fullname;
                                    item.UserId = UserSettings.userSettingsPrefs.Id;
                                    if (SelectedTaskTemplate.IsLocalMedia)
                                        item.IsLocalFile = true;
                                });
                            }
                        }
                    }
                    SelectedTaskTemplate.PictureProofMediaItems = nonEmptyMediaElements;

                    if (TaskStatus.HasValue)
                    {
                        TaskHelper.SetTaskStatusAsync(SelectedTaskTemplate, TaskStatus.Value, null);
                    }

                    if (Score.HasValue)
                    {
                        TaskHelper.SetTaskScore(SelectedTaskTemplate, Score.Value);
                    }

                    await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.PictureProofChanged, SelectedTaskTemplate); });
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, Constants.PictureProofChanged);
                });
            }
            catch (System.Exception e)
            {
                //Crashes.TrackError(e);
                Debug.WriteLine($"[PictureProofError:] {e.Message}");
                throw;
            }
            finally
            {
                await CancelAsync();
            }
        }

        public PictureProofViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMediaHelper mediaHelper,
            IWatermarkGenerator watermarkGenerator
            ) : base(navigationService, userService, messageService, actionsService)
        {
            _mediaHelper = mediaHelper;
            _watermarkGenerator = watermarkGenerator;
            _fileService = DependencyService.Get<IFileService>();
        }

        public override async Task Init()
        {
            if (IsNew)
            {
                MediaElements = new ObservableCollection<MediaItem>(Enumerable.Range(1, NumberOfExtraMediaElements).Select(x => MediaItem.Empty()));
                MainMediaElement = MediaItem.Empty();

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await PickMediaAsync(MainMediaElement);
                });

                EditingEnabled = true;
                IsContinueButtonVisible = true;
            }
            else
            {
                EditingEnabled = false;
                if (SelectedTask != null)
                {
                    IsContinueButtonVisible = EditingEnabled || (TaskStatus.HasValue && TaskStatus.Value != SelectedTask.FilterStatus);
                }
                else if (SelectedTaskTemplate != null)
                {
                    IsContinueButtonVisible = EditingEnabled || (TaskStatus.HasValue && TaskStatus.Value != SelectedTaskTemplate.FilterStatus) || (Score.HasValue && Score.Value != SelectedTaskTemplate.Score);
                }
            }
            await Task.Run(async () => await base.Init());
        }


        private async Task PickMediaAsync(MediaItem mediaItem)
        {

            if (mediaItem == null)
                return;

            List<MediaOption> mediaOptions = new List<MediaOption> {
                MediaOption.TakePhoto,
                //MediaOption.PhotoGallery,
            };

            if (!mediaItem.IsEmpty)
                mediaOptions.Add(MediaOption.RemoveMedia);
            IsNew = false;
            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOptions);
            IsNew = true;
            if (dialogResult.Result == null && dialogResult.IsSuccess && !dialogResult.IsRemoved)
                return;

            await Task.Run(() =>
            {
                if (dialogResult.IsSuccess)
                {
                    mediaItem.CopyFrom(dialogResult.Result ?? MediaItem.Empty());
                    mediaItem.CreatedAt = DateTimeHelper.Now;
                }
            });
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
                    var mediaElementsFilled = MediaElements.Concat(Enumerable.Range(0, NumberOfExtraMediaElements - MediaElements.Count).Select(x => MediaItem.Empty()));
                    MediaElements = new ObservableCollection<MediaItem>(mediaElementsFilled);
                }
                else
                {
                    MediaElements = new ObservableCollection<MediaItem>(Enumerable.Range(1, NumberOfExtraMediaElements).Select(x => MediaItem.Empty()));
                }
            }
        }
#pragma warning restore IDE0051 // Remove unused private members
    }
}
