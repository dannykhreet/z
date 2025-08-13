using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Tasks.Editing;
using Syncfusion.Maui.DataSource.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class EditTaskInstructionsViewModel : BaseViewModel
    {
        #region Private Members

        private FileItem PreviousFileItem;

        #endregion

        #region Public Properties 

        /// <summary>
        /// Sets input areas to Enabled when we have internet
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the new task being edited/created.
        /// </summary>
        public EditTaskTemplateModel TemplateModel { get; set; }

        /// <summary>
        /// Task description as steps
        /// </summary>
        public ObservableCollection<StepViewModel> Steps { get; set; }

        /// <summary>
        /// The file attached to instruction
        /// </summary>
        public FileItem InstructionsFile { get; set; }

        /// <summary>
        /// Indicates if you can add more steps to this task
        /// </summary>
        public bool CanAddSteps { get; set; }

        /// <summary>
        /// Indicates if you can add
        /// </summary>
        private bool canAddPdf;

        public bool CanAddPdf
        {
            // For not disable adding new pdf when creating a new template or if the previous item is null
            // Only allow the user to add pdf if there was already the original pdf file for this template
            get => canAddPdf && !TemplateModel.IsNewTemplate && TemplateModel.OriginalInstructionsFile != null;
            set
            {
                canAddPdf = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands 

        /// <summary>
        /// A command to add a brand new step to the collection
        /// </summary>
        public ICommand AddStepCommand => new Command(AddStep, CanExecuteCommands);

        /// <summary>
        /// A command to delete a given step
        /// </summary>
        public ICommand DeleteStepCommand => new Command<StepViewModel>(RemoveStep, CanExecuteCommands);

        /// <summary>
        /// Adds media item to a given step
        /// </summary>
        public ICommand AddMediaToStepCommand => new Command<StepViewModel>(async (a) => await AddMediaAsync(a), CanExecuteCommands);

        /// <summary>
        /// Adds pdf box to this task
        /// </summary>
        public ICommand AddPdfCommand => new Command(AddPdf, CanExecuteCommands);

        /// <summary>
        /// Deletes pdf box from this task
        /// </summary>
        public ICommand DeletePdfCommand => new Command(DeletePdf, CanExecuteCommands);

        /// <summary>
        /// Changes the attached pdf file
        /// </summary>
        public ICommand ChangePdfCommand => new Command(async () => await ChangePdf(), CanExecuteCommands);

        /// <summary>
        /// Submits the current edit process
        /// </summary>
        public IAsyncRelayCommand SubmitCommand => new AsyncRelayCommand(async () =>
            await ExecuteLoadingAction(SubmitAsync),
            CanExecuteCommands);
        #endregion

        #region Constructor

        private readonly IMediaHelper _mediaHelper;
        private readonly IMediaService _mediaService;
        private readonly ITaskTemplatesSerivce _tasksService;

        public EditTaskInstructionsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMediaHelper mediaHelper,
            IMediaService mediaService,
            ITaskTemplatesSerivce taskTemplatesSerivce) : base(navigationService, userService, messageService, actionsService)
        {
            _mediaHelper = mediaHelper;
            _mediaService = mediaService;
            _tasksService = taskTemplatesSerivce;
        }

        public override async Task Init()
        {
            Steps = new ObservableCollection<StepViewModel>(TemplateModel.Steps.Select(step => new StepViewModel(step)));
            InstructionsFile = TemplateModel.InstructionsFile;
            PreviousFileItem = InstructionsFile;

            CanAddPdf = !Steps.Any() && InstructionsFile == null;
            CanAddSteps = InstructionsFile == null;
            IsEnabled = await MessageHelper.ErrorMessageIsNotSent(_messageService);
        }
        #endregion

        #region Command Methods

        /// <summary>
        /// Adds a new step
        /// </summary>
        private void AddStep()
        {
            var newIndex = Steps.LastOrDefault()?.Index + 1 ?? 1;
            Steps.Add(new StepViewModel()
            {
                Index = newIndex,
            });

            CanAddPdf = false;
        }

        /// <summary>
        /// Removes a step from the steps collection
        /// </summary>
        /// <param name="step">The step to remove</param>
        private void RemoveStep(StepViewModel step)
        {
            var indexRemoved = Steps.IndexOf(step);

            if (indexRemoved == -1)
                return;

            Steps.RemoveAt(indexRemoved);

            // Update indexes
            int i = 1;
            foreach (var item in Steps)
                item.Index = i++;

            if (!Steps.Any())
                CanAddPdf = true;
        }

        /// <summary>
        /// Adds media item to a step
        /// </summary>
        /// <param name="step"></param>
        private async Task AddMediaAsync(StepViewModel step)
        {
            var mediaOptions = new List<MediaOption> { MediaOption.TakePhoto, MediaOption.PhotoGallery, MediaOption.VideoGallery, MediaOption.Video };

            if (step.MediaItem != null)
                mediaOptions.Add(MediaOption.RemoveMedia);

            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOptions);
            if (dialogResult.IsSuccess)
                step.MediaItem = dialogResult.Result;
        }

        /// <summary>
        /// Adds pdf item to this task
        /// </summary>
        private void AddPdf()
        {
            InstructionsFile = TemplateModel.OriginalInstructionsFile;
            InstructionsFile ??= FileItem.Empty;
            CanAddPdf = false;
            CanAddSteps = false;
        }

        /// <summary>
        /// Removes the pdf item from this task
        /// </summary>
        private void DeletePdf()
        {
            // Current instruction exists
            if (InstructionsFile?.IsEmpty == false)
            {
                // Keep it as previous
                PreviousFileItem = InstructionsFile;
            }

            InstructionsFile = null;
            CanAddPdf = true;
            CanAddSteps = true;
        }

        /// <summary>
        /// Changes the existing pdf file of this task
        /// </summary>
        private async Task ChangePdf()
        {
            var dialog = await _mediaHelper.PickPdfFileAsync();
            if (dialog.IsSuccess)
            {
                InstructionsFile = dialog.Result;
            }
        }

        #endregion

        #region Submitting & Validation

        private async Task SubmitAsync()
        {
            // If we're editing
            if (TemplateModel.IsEditingEnabled)
            {
                Page page = NavigationService.GetCurrentPage();
                // Submit as normal
                var errors = Validate();
                if (errors.Any())
                {
                    var title = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationMessage) + ":\n";
                    var cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationClose);
                    var errorString = errors.JoinString(x => x, "\n");
                    if (page != null)
                        await page.DisplayAlert(title, errorString, cancel);
                }
                else
                {
                    // Upload steps media
                    await _mediaService.UploadMediaItemsAsync(Steps.Where(x => x.MediaItem != null).Select(x => x.MediaItem), MediaStorageTypeEnum.TaskSteps, 0);

                    // Upload main task template media
                    if (TemplateModel.MediaItem != null)
                        await _mediaService.UploadMediaItemAsync(TemplateModel.MediaItem, MediaStorageTypeEnum.Tasks, 0);

                    // Submit the steps
                    SubmitSteps();

                    // TOOD upload instructions file
                    // No need to upload it for now because changing the file is not yet supported

                    // Submit instructions file
                    SubmitInstructionsFile();

                    var succeeded = await _tasksService.UpdateOrCreateTemplateAsync(TemplateModel.GetUpdatedObject());

                    if (succeeded)
                        await NavigationService.RemoveLastPagesAsync(3);
                    else
                    {
                        var close = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationClose);
                        var title = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationError);
                        var msg = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationUpdateError);
                        if (page != null)
                            await page.DisplayAlert(title, msg, close);
                    }
                }
            }
            // Editing wasn't enabled
            else
                // Go back the main page
                await NavigationService.RemoveLastPagesAsync(3);

        }

        private void SubmitSteps()
        {
            // Set the steps 
            TemplateModel.Steps = Steps.Select(x => new StepModel()
            {
                Id = x.Id,
                Index = x.Index,
                Description = x.Description,
                Picture = (x.MediaItem != null && x.MediaItem.IsVideo == false) ? x.MediaItem.PictureUrl : null,
                TaskTemplateId = x.TaskTemplateId,
                Video = x.MediaItem?.VideoUrl,
                VideoThumbnail = (x.MediaItem != null && x.MediaItem.IsVideo) ? x.MediaItem.PictureUrl : null,
                MediaIsLocal = x.MediaItem?.IsLocalFile ?? default,
            }).ToList();
        }

        private void SubmitInstructionsFile()
        {
            TemplateModel.InstructionsFile = InstructionsFile;
        }

        private List<string> Validate()
        {
            var emptySteps = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationEmptyStep);
            var emptyFile = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorPdfError);

            var errors = Steps.Where(x => x.IsEmpty())
                          .Select(x => emptySteps.Format(x.Index))
                          .ToList();

            if (InstructionsFile?.IsEmpty ?? false)
                errors.Add(emptyFile);

            return errors;
        }

        public override async Task CancelAsync()
        {
            // If we are in editing mode
            if (TemplateModel.IsEditingEnabled == true)
            {
                // Submit the recurrency
                SubmitSteps();

                // Also submit instructions file
                SubmitInstructionsFile();

                // NOTE we can check here for the validation erros but since we going to the previous page it doesn't make much sense
                // var errors = Validade();
            }

            // Go back as usual
            await base.CancelAsync();
        }

        protected override void Dispose(bool disposing)
        {
            _tasksService.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
