using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    /// <summary>
    /// New task view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.ViewModels.BaseViewModel" />
    public class EditTaskViewModel : BaseViewModel
    {
        #region Public Properties 

        /// <summary>
        /// Sets input areas to Enabled when we have internet
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets all of the available roles.
        /// </summary>
        public List<EnumListItem<RoleTypeEnum>> Roles => EnumListItem<RoleTypeEnum>.FromEnumValues(Statics.AppRoles, translate: true);

        /// <summary>
        /// Gets or sets the task being create/edited
        /// </summary>
        /// <value>
        /// The new task.
        /// </value>
        public EditTaskTemplateModel TemplateModel { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the navigate to next page command.
        /// </summary>
        /// <value>
        /// The navigate to next page command.
        /// </value>
        public ICommand NavigateToNextPageCommand => new Command(() =>
        {
            ExecuteLoadingAction(NavigateToNextPageAsync);
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the add media command.
        /// </summary>
        /// <value>
        /// The add media command.
        /// </value>
        public ICommand AddMediaCommand => new Command(() =>
        {
            ExecuteLoadingAction(AddMediaAsync);
        }, CanExecuteCommands);

        #endregion

        #region Initialization

        private readonly IMediaHelper _mediaHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditTaskViewModel"/> class to edit an existing task template.
        /// </summary>
        /// <param name="template">The template to edit.</param>
        public EditTaskViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMediaHelper mediaHelper) : base(navigationService, userService, messageService, actionsService)
        {
            _mediaHelper = mediaHelper;

            TemplateModel = EditTaskTemplateModel.New();
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            IsEnabled = await MessageHelper.ErrorMessageIsNotSent(_messageService);

            await base.Init();
        }

        #endregion

        /// <summary>
        /// Navigates to next page asynchronous.
        /// </summary>
        private async Task NavigateToNextPageAsync()
        {
            var errors = Validate();
            if (errors.Any())
            {
                Page page = NavigationService.GetCurrentPage();
                string title = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationMessage) + ":\n";
                var message = errors.JoinString(x => x, "\n");
                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationClose);

                await page.DisplayAlert(title, message, cancel);
            }
            else
            {
                using var scope = App.Container.CreateScope();
                var editTaskRecurrenceViewModel = scope.ServiceProvider.GetService<EditTaskRecurrenceViewModel>();
                editTaskRecurrenceViewModel.TemplateModel = TemplateModel;
                await NavigationService.NavigateAsync(viewModel: editTaskRecurrenceViewModel);
            }
        }

        /// <summary>
        /// Adds the media asynchronous.
        /// </summary>
        private async Task AddMediaAsync()
        {
            List<MediaOption> mediaOptions = new List<MediaOption> { MediaOption.TakePhoto, MediaOption.PhotoGallery, MediaOption.VideoGallery, MediaOption.Video };

            if (TemplateModel.MediaItem != null)
                mediaOptions.Add(MediaOption.RemoveMedia);

            var dialogResult = await _mediaHelper.PickMediaAsync(mediaOptions);
            if (dialogResult.IsSuccess)
                TemplateModel.MediaItem = dialogResult.Result;
        }

        /// <summary>
        /// Validates the state of the model
        /// </summary>
        /// <returns>The list of errors found in the model.
        /// <para>If there are no errors an empty list is returned.</para></returns>
        private List<string> Validate()
        {
            string empty = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationEmpty);
            string name = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationName);
            string desc = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationDescription);

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(TemplateModel.Name))
                errors.Add(empty.Format(name));

            if (string.IsNullOrWhiteSpace(TemplateModel.Description))
                errors.Add(empty.Format(desc));

            return errors;
        }
    }
}
