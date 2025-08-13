using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Services.Instructions;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class InstructionsViewModel : BaseViewModel
    {
        private readonly IInstructionsService _instructionsService;

        #region Properties

        public FilterControl<InstructionsModel, InstructionTypeEnum> InstructionsFilter { get; set; } = new FilterControl<InstructionsModel, InstructionTypeEnum>(null);

        public bool IsSearchBarVisible { get; set; }

        public List<InstructionsModel> WorkInstructions { get; set; }

        public bool IsFromDeeplink { get; set; } = false;

        #endregion

        #region Commands

        public ICommand SearchTextChangedCommand { get; private set; }

        public ICommand NavigateToInstructionTaskTemplates { get; private set; }

        public ICommand DeleteTagCommand { get; private set; }

        #endregion

        public InstructionsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IInstructionsService instructionsService) : base(navigationService, userService, messageService, actionsService)
        {
            _instructionsService = instructionsService;

            SearchTextChangedCommand = new Command((obj) =>
            {
                if (obj is string searchText)
                    InstructionsFilter.SearchText = searchText;
                InstructionsFilter.Filter(InstructionsFilter.StatusFilters, false, useDataSource: false);
            });

            DeleteTagCommand = new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>(obj =>
            {
                if (obj.DataItem is TagModel tag)
                {
                    InstructionsFilter.SearchedTags.Remove(tag);
                    tag.IsActive = !tag.IsActive;
                    InstructionsFilter.Filter(false, false);
                }

            }, CanExecuteCommands);

            NavigateToInstructionTaskTemplates = new Command<InstructionsModel>(async (template) => await ExecuteLoadingActionAsync(() => NavigateToInstructionsTemplateAsync(template)), CanExecuteCommands);
        }

        public override async Task Init()
        {
            Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.instructionsScreenTitle)} - {Settings.AreaSettings.WorkAreaName}";

            await Task.Run(async () => await LoadWorkInstructions());

            MessagingCenter.Subscribe<SyncService>(this, Constants.WorkInstructionsTemplateChanged, async (sender) =>
            {
                await RefreshAsync();
            });

            MessagingCenter.Subscribe<InstructionsService>(this, Constants.WorkInstructionsTemplateNotificationConfirmed, async (sender) =>
            {
                await RefreshAsync();
            });

            await Task.Run(async () => await base.Init());
        }

        private async Task LoadWorkInstructions()
        {
            if (WorkInstructions == null)
                WorkInstructions = await _instructionsService.GetInstructionsForCurrentArea();

            InstructionsFilter.SetUnfilteredItems(WorkInstructions);
            InstructionsFilter.RefreshStatusFilter();
        }

        protected override async Task RefreshAsync()
        {
            if (IsFromDeeplink || InstructionsFilter == null || _instructionsService == null)
                return;

            WorkInstructions = await _instructionsService.GetInstructionsForCurrentArea(refresh: IsRefreshing);

            InstructionsFilter.SetUnfilteredItems(WorkInstructions);
            InstructionsFilter.RefreshStatusFilter();
        }

        private async Task NavigateToInstructionsTemplateAsync(InstructionsModel instruction)
        {
            if (instruction == null)
                return;

            var selectedInstruction = await _instructionsService.GetInstruction(IsFromDeeplink ? instruction.WorkInstructionTemplateId : instruction.Id);
            using (var scope = App.Container.CreateScope())
            {
                var vm = scope.ServiceProvider.GetService<InstructionsItemsViewModel>();
                vm.Instructions = InstructionsFilter.FilteredList;
                vm.SelectedInstruction = selectedInstruction ?? new InstructionsModel();
                vm.WorkInstructionTemplateId = IsFromDeeplink ? instruction.WorkInstructionTemplateId : instruction.Id;
                vm.IsFromDeeplink = IsFromDeeplink;

                await NavigationService.NavigateAsync(viewModel: vm);
            }
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<SyncService>(this, Constants.WorkInstructionsTemplateChanged);
                MessagingCenter.Unsubscribe<InstructionsService>(this, Constants.WorkInstructionsTemplateNotificationConfirmed);
            });
            _instructionsService.Dispose();
            InstructionsFilter.Dispose();
            InstructionsFilter = null;
            WorkInstructions = null;
            SearchTextChangedCommand = null;
            NavigateToInstructionTaskTemplates = null;
            base.Dispose(disposing);
        }
    }
}
