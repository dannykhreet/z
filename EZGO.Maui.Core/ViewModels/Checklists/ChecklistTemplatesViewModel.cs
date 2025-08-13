using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Services.Checklists;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Checklists;
using Syncfusion.Maui.DataSource;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    /// <summary>
    /// Checklist templates view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.ViewModels.BaseViewModel" />
    public class ChecklistTemplatesViewModel : BaseViewModel
    {
        private readonly IChecklistService _checklistService;
        private readonly IUpdateService _updateService;
        private readonly ISyncService _syncService;


        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>
        /// The data source.
        /// </value>
        public DataSource DataSource { get; set; } = new DataSource();

        /// <summary>
        /// Gets or sets the checklist templates.
        /// </summary>
        /// <value>
        /// The checklist templates.
        /// </value>
        private List<ChecklistTemplateModel> _checklistTemplates;
        public List<ChecklistTemplateModel> ChecklistTemplates
        {
            get { return _checklistTemplates; }
            set { _checklistTemplates = value; }
        }

        public FilterControl<ChecklistTemplateModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<ChecklistTemplateModel, TaskStatusEnum>(null);

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;

                OnPropertyChanged();
            }
        }

        private bool isSearchBarVisible;

        public bool IsSearchBarVisible
        {
            get { return isSearchBarVisible; }
            set
            {
                isSearchBarVisible = value;

                OnPropertyChanged();
            }
        }

        private bool hasCompletedChecklists;

        public bool HasCompletedChecklists
        {
            get => hasCompletedChecklists;
            set
            {
                hasCompletedChecklists = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the navigate to task templates command.
        /// </summary>
        /// <value>
        /// The navigate to task templates command.
        /// </value>
        public ICommand NavigateToTaskTemplatesCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToTaskTemplatesAsync(obj);
            });
        }, CanExecuteCommands);


        /// <summary>
        /// Navigate to the completed checklists
        /// </summary>
        public ICommand NavigateToCompletedChecklistsCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToCompletedChecklistsAsync();
            });
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the search text changed command.
        /// </summary>
        /// <value>
        /// The search text changed command.
        /// </value>
        public ICommand SearchTextChangedCommand => new Command((obj) =>
        {
            if (obj is string searchText)
                TaskFilter.SearchText = searchText;
            TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
        }, CanExecuteCommands);

        public ICommand DeleteTagCommand => new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>((obj) =>
        {
            if (obj.DataItem is TagModel tag)
            {
                TaskFilter.SearchedTags.Remove(tag);
                tag.IsActive = !tag.IsActive;
                TaskFilter.Filter(false, false);
            }
        }, CanExecuteCommands);

        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ChecklistTemplatesViewModel"/> class.
        /// </summary>
        public ChecklistTemplatesViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IUpdateService updateService,
            ISyncService syncService,
            IChecklistService checklistService) : base(navigationService, userService, messageService, actionsService)
        {
            _checklistService = checklistService;
            _updateService = updateService;
            _syncService = syncService;

            TaskFilter.NestedTagsAccessor = (obj) =>
            {
                var tags = obj.Tags != null ? new List<Tag>(obj.Tags) : new List<Tag>();

                if (obj.TaskTemplates != null)
                {
                    tags.AddRange(obj.TaskTemplates.Where(x => x.Tags != null).SelectMany(x => x.Tags));
                }

                return tags;
            };
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.checklistScreenTitle)} - {Settings.WorkAreaName}";

            await base.Init();
            await Task.Run(async () =>
            {
                await LoadChecklistTemplatesAsync(false);
            });
            if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
            {
                MessagingCenter.Subscribe<ChecklistsService, ChecklistModel>(this, Constants.ChecklistAdded, async (sender, checklist) =>
                {
                    await LoadChecklistTemplatesAsync(true);
                    FilterChecklistTemplates();
                });

                MessagingCenter.Subscribe<ChecklistsService>(this, Constants.ChecklistDeleted, async (sender) =>
                {
                    await LoadChecklistTemplatesAsync(true);
                    FilterChecklistTemplates();
                });

                MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
                {
                    try
                    {
                        if (await FifteenSecondLock.WaitAsync(0))
                        {
                            var ids = await _updateService?.CheckForUpdatedChecklistsAsync();
                            if (ids.Count > 0)
                            {
                                await LoadChecklistTemplatesAsync(true).ConfigureAwait(false);
                                FilterChecklistTemplates();
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

                });
            }
        }

        protected override async Task RefreshAsync()
        {
            await LoadChecklistTemplatesAsync(true);
            FilterChecklistTemplates();
        }

        /// <summary>
        /// Loads the checklist templates asynchronous.
        /// </summary>
        private async Task LoadChecklistTemplatesAsync(bool isRefreshing)
        {
            List<ChecklistTemplateModel> checklistTemplates = await _checklistService.GetChecklistTemplatesAsync(includeTaskTemplates: true, refresh: isRefreshing);
            ChecklistTemplates = checklistTemplates;

            TaskFilter.SetUnfilteredItems(ChecklistTemplates);
            TaskFilter.Filter(TaskFilter.StatusFilters, false, false);

            OnPropertyChanged(nameof(ChecklistTemplates));

            HasItems = checklistTemplates?.Any() ?? false;

            HasCompletedChecklists = await Task.Run(async () => await _checklistService.CheckHasCompletedChecklists(refresh: IsRefreshing).ConfigureAwait(false));
        }

        /// <summary>
        /// Filters the task templates.
        /// </summary>
        private void FilterChecklistTemplates()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                DataSource.Filter = null;
            else
                DataSource.Filter = obj => obj is ChecklistTemplateModel item && item.Name.ToUpperInvariant().Contains(SearchText.ToUpperInvariant());

            DataSource.RefreshFilter();

            HasItems = DataSource?.Items?.Any() ?? false;

            OnPropertyChanged(nameof(NavigateToTaskTemplatesCommand));
            OnPropertyChanged(nameof(NavigateToCompletedChecklistsCommand));
        }

        /// <summary>
        /// Search text changed.
        /// </summary>
        private void SearchTextChanged()
        {
            FilterChecklistTemplates();
        }

        /// <summary>
        /// Navigates to task templates asynchronous.
        /// </summary>
        /// <param name="obj">Command object.</param>
        private async Task NavigateToTaskTemplatesAsync(object obj)
        {
            async Task NavigateToTemplate(object obj)
            {
                if (obj is ChecklistTemplateModel item)
                {
                    using var scope = App.Container.CreateScope();

                    if (item.HasIncompleteChecklists ?? false)
                    {
                        var incompleteChecklistsViewModel = scope.ServiceProvider.GetService<IncompleteChecklistsViewModel>();
                        incompleteChecklistsViewModel.ChecklistTemplateId = item.Id;
                        incompleteChecklistsViewModel.Picture = item.Picture;
                        incompleteChecklistsViewModel.Name = item.Name;
                        incompleteChecklistsViewModel.ChecklistTemplates = ChecklistTemplates?.ToBasicList<BasicChecklistTemplateModel, ChecklistTemplateModel>();

                        await NavigationService.NavigateAsync(viewModel: incompleteChecklistsViewModel);
                        return;
                    }

                    var taskTemplatesViewModel = scope.ServiceProvider.GetService<TaskTemplatesViewModel>();
                    taskTemplatesViewModel.ChecklistTemplateId = item.Id;
                    taskTemplatesViewModel.ChecklistTemplates = ChecklistTemplates?.ToBasicList<BasicChecklistTemplateModel, ChecklistTemplateModel>();
                    taskTemplatesViewModel.ShouldClearStatuses = CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled;

                    await NavigationService.NavigateAsync(viewModel: taskTemplatesViewModel);
                }
            }

            if (obj is Syncfusion.Maui.ListView.ItemTappedEventArgs data)
                await NavigateToTemplate(data.DataItem);
            else
                await NavigateToTemplate(obj);
        }

        /// <summary>
        /// Navigates to task templates asynchronous.
        /// </summary>
        private async Task NavigateToCompletedChecklistsAsync()
        {
            await NavigationService.NavigateAsync<CompletedChecklistsViewModel>();
        }

        protected override void Dispose(bool disposing)
        {
            if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Unsubscribe<ChecklistsService, ChecklistModel>(this, Constants.ChecklistAdded);
                    MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
                });
            }
            _checklistService.Dispose();
            ChecklistTemplates = null;
            base.Dispose(disposing);
        }
    }
}
