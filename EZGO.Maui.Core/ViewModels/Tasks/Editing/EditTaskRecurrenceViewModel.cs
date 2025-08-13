using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Shifts;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Tasks.Editing;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class EditTaskRecurrenceViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// Sets input areas to Enabled when we have internet
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the new task being edited/created.
        /// </summary>
        public EditTaskTemplateModel TemplateModel { get; set; }

        #region Recurrency editor view models

        public RecurrencyEditorNoReccurencyViewModel NoRecurrencyViewModel { get; set; }

        public RecurrencyEditorShiftsViewModel ShiftRecurrencyViewModel { get; set; }

        public RecurrencyEditorWeekViewModel WeekRecurrencyViewModel { get; set; }

        public RecurrencyEditorMonthViewModel MonthRecurrencyViewModel { get; set; }

        #endregion

        /// <summary>
        /// The name of the work area currently selected
        /// </summary>
        public string WorkAreaName { get; set; }

        /// <summary>
        /// Selected index in the recurrency picker
        /// </summary>
        //public int SelectedRecurrencyIndex
        //{
        //    get => RecurrencyTypes?.Select(x => x.Value).IndexOf(TemplateModel?.Recurrency?.RecurrencyType ?? default) ?? 0;
        //    set => TemplateModel.Recurrency.RecurrencyType = value < RecurrencyTypes?.Count ? RecurrencyTypes[value].Value : default;
        //}

        /// <summary>
        /// All available recurrency types
        /// </summary>
        public List<EnumListItem<RecurrencyTypeEnum>> RecurrencyTypes => EnumListItem<RecurrencyTypeEnum>.FromEnumValues(new[] { RecurrencyTypeEnum.NoRecurrency, RecurrencyTypeEnum.Shifts, RecurrencyTypeEnum.Week, RecurrencyTypeEnum.Month });

        /// <summary>
        /// Display names of all available recurrency types
        /// </summary>
        public List<string> RecurrencyTypesNames => RecurrencyTypes?.Select(x => x.DisplayName).ToList() ?? new List<string>();

        #endregion

        #region

        public ICommand NavigateToTaskInstructionCommand => new Command(() =>
        {
            ExecuteLoadingAction(NavigateToTaskInstructionAsync);
        }, CanExecuteCommands);

        /// <summary>
        /// The command to change the work area
        /// </summary>
        public ICommand ChangeAreaCommand => new Command(async () =>
        {
            // Create view model
            using var scope = App.Container.CreateScope();
            var vm = scope.ServiceProvider.GetService<WorkAreaViewModel>();
            vm.ChooseAreaOnly = true;

            // Hook into the area selected event
            vm.AreaSelected += async (area) =>
            {
                // Set the area 
                TemplateModel.Recurrency.AreaId = area.Id;
                WorkAreaName = area.Name;

                // Go back 
                await NavigationService?.CloseAsync();
            };

            // Go to area selection page
            await NavigationService?.NavigateAsync(viewModel: vm);
        }, CanExecuteCommands);

        #endregion

        #region Services

        private readonly IShiftService _shiftService;
        private readonly IWorkAreaService _areaService;

        #endregion

        public EditTaskRecurrenceViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IWorkAreaService workAreaService,
            IShiftService shiftService) : base(navigationService, userService, messageService, actionsService)
        {
            _shiftService = shiftService;
            _areaService = workAreaService;
        }

        public override async Task Init()
        {
            IsEnabled = await MessageHelper.ErrorMessageIsNotSent(_messageService);

            var shifts = await Task.Run(async () => await _shiftService.GetShiftsAsync());
            var area = await Task.Run(async () => await _areaService.GetWorkAreaAsync(TemplateModel.Recurrency.AreaId));
            if (area != null)
                WorkAreaName = area.Name;

            // Create view models for the recurrency editors
            NoRecurrencyViewModel = new RecurrencyEditorNoReccurencyViewModel(shifts, TemplateModel.Recurrency);
            ShiftRecurrencyViewModel = new RecurrencyEditorShiftsViewModel(shifts, TemplateModel.Recurrency);
            WeekRecurrencyViewModel = new RecurrencyEditorWeekViewModel(TemplateModel.Recurrency);
            MonthRecurrencyViewModel = new RecurrencyEditorMonthViewModel(TemplateModel.Recurrency);

            await Task.Run(async () => await base.Init());
        }

        /// <summary>
        /// Navigates to the next page
        /// </summary>
        private async Task NavigateToTaskInstructionAsync()
        {
            SubmitRecurrency();

            var errors = Validate();
            if (errors.Any())
            {
                Page page = NavigationService.GetCurrentPage();

                string title = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationMessage) + ":\n";
                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationClose);
                var msg = errors.JoinString(x => x, "\n");

                await page.DisplayAlert(title, msg, cancel);
            }
            else
            {
                using var scope = App.Container.CreateScope();
                var editTaskInstructionsViewModel = scope.ServiceProvider.GetService<EditTaskInstructionsViewModel>();
                editTaskInstructionsViewModel.TemplateModel = TemplateModel;
                await NavigationService.NavigateAsync(viewModel: editTaskInstructionsViewModel);
            }
        }

        private void SubmitRecurrency()
        {
            switch (TemplateModel.Recurrency.RecurrencyType)
            {
                case RecurrencyTypeEnum.NoRecurrency:
                    NoRecurrencyViewModel.Submit();
                    break;
                case RecurrencyTypeEnum.Shifts:
                    ShiftRecurrencyViewModel.Submit();
                    break;
                case RecurrencyTypeEnum.Week:
                    WeekRecurrencyViewModel.Submit();
                    break;
                case RecurrencyTypeEnum.Month:
                    MonthRecurrencyViewModel.Submit();
                    break;
            }
        }

        private List<string> Validate()
        {
            var errors = new List<string>();
            string areaError = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationNoWorkArea);

            if (TemplateModel.Recurrency.AreaId == 0)
                errors.Add(areaError);

            errors.AddRange(TemplateModel.ValidateRecurrency());

            return errors;
        }

        public override async Task CancelAsync()
        {
            // If we're editing 
            if (TemplateModel.IsEditingEnabled == true)
            {
                // Submit the recurrency
                SubmitRecurrency();

                // NOTE we can check here for the validation erros but since we going to the page back so it deasn't have much sense
                // var errors = Validade();
            }

            // Go back as usual
            await base.CancelAsync();
        }

        protected override void Dispose(bool disposing)
        {
            _shiftService.Dispose();
            _areaService.Dispose();
            base.Dispose(disposing);
        }
    }
}
