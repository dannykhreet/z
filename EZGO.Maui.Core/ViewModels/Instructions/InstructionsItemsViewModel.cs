using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ListLayouts;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using Syncfusion.TreeView.Engine;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class InstructionsItemsViewModel : BaseViewModel
    {
        #region Fields

        private readonly IInstructionsService _instructionsService;

        #endregion

        #region Properties

        public FilterControl<InstructionItem, InstructionTypeEnum> InstructionsFilter { get; set; } = new FilterControl<InstructionItem, InstructionTypeEnum>(null);

        public bool IsSearchBarVisible { get; set; }

        public bool IsDropdownEnabled { get; set; } = true;

        public LayoutManager LayoutManager { get; set; } = new LayoutManager();

        public Rect Rect { get; set; } = new Rect(113, .2, .6, .6);

        public InstructionsModel SelectedInstruction { get; set; }

        public List<InstructionsModel> Instructions { get; set; }

        public List<ITreeDropdownFilterItem> FilterOptions { get; set; }

        public bool IsFromDeeplink { get; set; } = false;

        public bool ContainsTags => SelectedInstruction?.Tags?.Count > 0;

        public bool IsShowChangesPopupOpen { get; set; } = false;

        public List<Api.Models.WorkInstructions.WorkInstructionTemplateChangeNotification> WorkInstructionChanges { get; set; }

        public int WorkInstructionTemplateId { get; set; }

        #endregion

        #region Commands

        public ICommand SearchTextChangedCommand { get; set; }

        public ICommand DropdownTapCommand { get; set; }

        public new ICommand ToggleDropdownCommand { get; set; }

        public ICommand NavigateToCarouselViewCommand { get; set; }

        public ICommand DeleteTagCommand { get; private set; }

        public ICommand ShowChangesCommand { get; private set; }

        public ICommand ConfirmChangesCommand { get; private set; }

        #endregion

        public InstructionsItemsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IInstructionsService instructionsService) : base(navigationService, userService, messageService, actionsService)
        {
            _instructionsService = instructionsService;
            SearchTextChangedCommand = new Command(ExecuteSearchTextChanged, CanExecuteCommands);
            DropdownTapCommand = new Command<object>(DropdownTap, CanExecuteCommands);
            ToggleDropdownCommand = new Command(() => ExecuteLoadingAction(() =>
            {
                if (IsFromDeeplink)
                    return;

                IsDropdownOpen = !IsDropdownOpen;
            }), CanToggleDropdown);
            NavigateToCarouselViewCommand = new Command<object>((obj) => ExecuteLoadingAction(() => NavigateToCarouselView(obj)), CanExecuteCommands);
            DeleteTagCommand = new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>(obj =>
            {
                if (obj.DataItem is TagModel tag)
                {
                    InstructionsFilter.SearchedTags.Remove(tag);
                    tag.IsActive = !tag.IsActive;
                    InstructionsFilter.Filter(false, false);
                }

            }, CanExecuteCommands);

            ShowChangesCommand = new Command(() => ExecuteLoadingAction(async () =>
            {
                if (!await InternetHelper.HasInternetAndApiConnectionAsync())
                {
                    await ShowNoInternetMessage();
                    return;
                }

                IsShowChangesPopupOpen = !IsShowChangesPopupOpen;
                await LoadWorkInstructionChanges();
            }), CanExecuteCommands);

            ConfirmChangesCommand = new Command(() => ExecuteLoadingAction(async () =>
            {
                await ConfirmWorkInstructionChanges();
                IsShowChangesPopupOpen = !IsShowChangesPopupOpen;
            }), CanExecuteCommands);
        }

        private async Task ShowNoInternetMessage()
        {
            var message = TranslateExtension.GetValueFromDictionary(LanguageConstants.onlyOnlineAction);
            await ValidationHelper.DisplayGeneralValidationPopup(message);
        }

        private async Task ConfirmWorkInstructionChanges()
        {
            var result = await _instructionsService.ConfirmWorkInstructionChanges(WorkInstructionTemplateId);
            if (result)
            {
                SelectedInstruction.UnreadChangesNotificationsCount = 0;
                OnPropertyChanged(nameof(SelectedInstruction));
            }
        }

        private async Task LoadWorkInstructionChanges()
        {
            WorkInstructionChanges = await _instructionsService.GetWorkInstructionChanges(WorkInstructionTemplateId);
        }

        private async Task NavigateToCarouselView(object obj)
        {
            using var scope = App.Container.CreateScope();
            var carousel = scope.ServiceProvider.GetService<InstructionsSlideViewModel>();
            carousel.InstructionsFilter = InstructionsFilter;
            carousel.SelectedInstruction = obj as InstructionItem;
            carousel.SelectedIndex = InstructionsFilter.FilteredList.IndexOf(carousel.SelectedInstruction);
            await NavigationService.NavigateAsync(viewModel: carousel);
        }

        public override async Task Init()
        {
            if (!IsFromDeeplink)
            {
                SetSelectedItem();
                await LoadInstructionItems(SelectedInstruction.Id);
            }
            else
            {
                IsDropdownEnabled = false;
                InstructionsFilter.SetUnfilteredItems(SelectedInstruction.InstructionItems ?? new List<InstructionItem>());
                InstructionsFilter.FilterCollection = new List<FilterModel>() { new FilterModel(SelectedInstruction.Name, SelectedInstruction.Id) };
                InstructionsFilter.SetSelectedFilter(SelectedInstruction.Name, SelectedInstruction.Id);
            }

            if (InstructionsFilter?.FilterCollection != null && InstructionsFilter.FilterCollection.Count > 6)
                Rect = new Rect(113, .8, .6, .9);

            FilterOptions = new List<ITreeDropdownFilterItem>(InstructionsFilter?.FilterCollection ?? new List<FilterModel>());

            MessagingCenter.Subscribe<SyncService>(this, Constants.WorkInstructionsTemplateChanged, async (sender) =>
            {
                if (SelectedInstruction != null)
                {
                    SetSelectedItem();
                    await LoadInstructionItems(SelectedInstruction.Id);
                    await LoadWorkInstructionChanges();
                }
            });

            LayoutManager.SetCurrentLayout();
            await base.Init();
        }

        private async Task LoadInstructionItems(int id)
        {
            var selectedInstruction = await _instructionsService.GetInstructionForCurrentArea(id);
            SelectedInstruction = selectedInstruction;
            InstructionsFilter.SetUnfilteredItems(selectedInstruction.InstructionItems);
            InstructionsFilter.RefreshStatusFilter();
        }

        private void SetSelectedItem()
        {
            InstructionsFilter.FilterCollection = Instructions?.Select(x => new FilterModel(x.Name, x.Id)).ToList();
            InstructionsFilter.SetSelectedFilter(SelectedInstruction.Name, SelectedInstruction.Id);
        }

        private void ExecuteSearchTextChanged()
        {
            ExecuteLoadingAction(() => InstructionsFilter.Filter(InstructionsFilter.SelectedFilter, false));
        }

        private async void DropdownTap(object obj)
        {
            IsDropdownOpen = false;

            async Task ChangeFilter(FilterModel filterModel)
            {
                InstructionsFilter.SelectedFilter = filterModel;

                await LoadInstructionsBasedOnFilterAsync();
            }

            if (obj is TreeViewNode treeViewNode && treeViewNode.Content is FilterModel filterOption)
                await ChangeFilter(filterOption);
            else if (obj is FilterModel filter)
                await ChangeFilter(filter);
        }

        protected override async Task RefreshAsync()
        {
            await LoadInstructionsBasedOnFilterAsync();
        }

        private async Task LoadInstructionsBasedOnFilterAsync()
        {
            await LoadInstructionItems(InstructionsFilter.SelectedFilter.Id);
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<SyncService>(this, Constants.WorkInstructionsTemplateChanged);
            _instructionsService.Dispose();
            InstructionsFilter.Dispose();
            Instructions = null;
            LayoutManager = null;
            SelectedInstruction = null;

            SearchTextChangedCommand = null;
            DropdownTapCommand = null;
            NavigateToCarouselViewCommand = null;
            FilterOptions = null;
            base.Dispose(disposing);
        }
    }
}
