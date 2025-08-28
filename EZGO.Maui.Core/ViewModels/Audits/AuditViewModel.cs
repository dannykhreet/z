using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Audits;
using Syncfusion.Maui.DataSource;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class AuditViewModel : BaseViewModel
    {
        private const string _cat = "[AuditViewModel]:\n\t";

        private readonly IAuditsService _auditService;
        private List<AuditTemplateModel> auditTemplates;

        public string SearchText { get; set; }

        public bool IsSearchBarVisible { get; set; }

        public bool HasCompletedAudits { get; set; }

        public DataSource AuditsDataSource { get; set; } = new DataSource();

        public List<AuditTemplateModel> AuditTemplates { get; set; }

        public FilterControl<AuditTemplateModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<AuditTemplateModel, TaskStatusEnum>(null);

        /// <summary>
        /// Gets the navigate to task templates command.
        /// </summary>
        /// <value>
        /// The navigate to task templates command.
        /// </value>
        public ICommand NavigateToTaskTemplatesCommand => new Command<object>(obj => ExecuteLoadingAction(async () => await NavigateToTaskTemplatesAsync(obj)), CanExecuteCommands);

        public ICommand NavigateToCompletedAuditsCommand => new Command(() => ExecuteLoadingAction(async () => await NavigateToCompletedAuditsAsync()), CanExecuteCommands);

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

        protected override void RefreshCanExecute()
        {
            (NavigateToTaskTemplatesCommand as Command)?.ChangeCanExecute();
            (NavigateToCompletedAuditsCommand as Command)?.ChangeCanExecute();
            (SearchTextChangedCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        public AuditViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAuditsService auditsService) : base(navigationService, userService, messageService, actionsService)
        {
            _auditService = auditsService;
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

        ~AuditViewModel()
        { // Breakpoint here
        }

        public override async Task Init()
        {
            Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.auditListScreenTitle)} - {Settings.AreaSettings.WorkAreaName}";
            await Task.Run(async () => await base.Init());

            await Task.Run(async () => await LoadAuditTemplatesAsync());

            MessagingCenter.Subscribe<SyncService>(this, Constants.AuditTemplateChanged, async (sender) =>
            {
                await LoadAuditTemplatesAsync();
            });
        }

        protected override async Task RefreshAsync()
        {
            await LoadAuditTemplatesAsync();

            OnPropertyChanged(nameof(NavigateToTaskTemplatesCommand));
            OnPropertyChanged(nameof(NavigateToCompletedAuditsCommand));
        }

        private async Task LoadAuditTemplatesAsync(bool cached = true)
        {
#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
            Debug.WriteLine("Started loading templates", _cat);
#endif
            auditTemplates = await _auditService.GetAuditTemplatesAsync(includeTaskTemplates: true, refresh: IsRefreshing);
#if DEBUG
            Debug.WriteLine($"Retrived auditTemplates, time: {st.ElapsedMilliseconds}", _cat);
#endif
            AuditTemplates = auditTemplates;

            if (SearchText != null)
                SearchTextChanged();

            if (TaskFilter != null)
            {
                TaskFilter.SetUnfilteredItems(AuditTemplates);
                TaskFilter.Filter(TaskFilter.StatusFilters, false, false);
            }
            OnPropertyChanged(nameof(AuditTemplates));
            HasItems = auditTemplates?.Any() ?? false;
#if DEBUG
            Debug.WriteLine($"Getting completed audits, time: {st.ElapsedMilliseconds}", _cat);
#endif
            HasCompletedAudits = await _auditService.CheckHasCompletedAudits(IsRefreshing);
#if DEBUG
            Debug.WriteLine($"Retrived completed audits, time: {st.ElapsedMilliseconds}", _cat);
#endif
        }

        /// <summary>
        /// Search text changed.
        /// </summary>
        private void SearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                AuditTemplates = auditTemplates;
                //DataSource
            }
            else
                AuditTemplates = auditTemplates.Where(obj => obj is AuditTemplateModel item && item.Name.ToUpperInvariant().Contains(SearchText.ToUpperInvariant())).ToList();

            HasItems = AuditTemplates.Any();
        }

        /// <summary>
        /// Navigates to task templates asynchronous.
        /// </summary>
        /// <param name="obj">Command object.</param>
        private async Task NavigateToTaskTemplatesAsync(object obj)
        {
            if (obj is AuditTemplateModel item)
            {
                using var scope = App.Container.CreateScope();
                var auditTaskTemplatesViewModel = scope.ServiceProvider.GetService<AuditTaskTemplatesViewModel>();

                auditTaskTemplatesViewModel.AuditTemplateId = item.Id;
                auditTaskTemplatesViewModel.AuditTemplates = AuditTemplates?.ToBasicList<BasicAuditTemplateModel, AuditTemplateModel>();

                await NavigationService.NavigateAsync(viewModel: auditTaskTemplatesViewModel);
            }
        }

        private async Task NavigateToCompletedAuditsAsync()
        {
            await NavigationService.NavigateAsync<CompletedAuditViewModel>();
        }

        protected override void Dispose(bool disposing)
        {
            _auditService.Dispose();
            AuditTemplates = null;
            auditTemplates = null;
            TaskFilter?.Dispose();
            TaskFilter = null;
            MessagingCenter.Unsubscribe<SyncService>(this, Constants.AuditTemplateChanged);
            base.Dispose(disposing);
        }
    }
}
