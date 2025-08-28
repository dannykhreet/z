using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Checklists;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Checklists
{
    public class IncompleteChecklistsViewModel : BaseViewModel
    {
        private readonly IUpdateService _updateService;
        private readonly IChecklistService _checklistService;

        private ChecklistModel _checklistToDelete { get; set; }

        public int ChecklistTemplateId { get; set; }
        public string Name { get; set; }
        public new string Picture { get; set; }

        public ObservableCollection<ChecklistModel> Checklists { get; set; }
        public List<BasicChecklistTemplateModel> ChecklistTemplates { get; set; }

        public int PagesFromDeepLink { get; set; }
        public BasicTaskModel TaskFromDeepLink { get; set; }
        public bool DeepLinkCompletionIsRequired { get; set; } = false;

        public bool IsFromBookmark { get; set; } = false;

        public bool CanDeleteChecklist { get; private set; }

        public ListViewLayout ListViewLayout { get; set; }

        public bool IsListVisible { get; set; } = true;

        public ICommand ListViewLayoutCommand => new Command<object>((obj) => SetListViewLayout(obj), CanExecuteCommands);

        public IAsyncRelayCommand<object> NavigateToChecklistCommand { get; }
        public ICommand NavigateToNewChecklistCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToNewChecklist();
            });
        }, CanExecuteCommands);

        public ICommand DeleteIncompletedChecklsitCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await DeleteIncompletedChecklist(_checklistToDelete);
            });
        }, CanExecuteCommands);

        public ICommand OpenDeleteChecklsitPopupCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(() =>
            {
                if (obj is ChecklistModel checklist)
                {
                    _checklistToDelete = checklist;
                }
            });
        }, CanExecuteCommands);

        public ICommand CloseDeleteChecklsitPopupCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(() =>
            {
                MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.HideDeletePopup); });
            });
        }, CanExecuteCommands);


        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);

        public IncompleteChecklistsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IChecklistService checklistService,
            IUpdateService updateService
            ) : base(navigationService, userService, messageService, actionsService)
        {
            _checklistService = checklistService;
            _updateService = updateService;

            NavigateToChecklistCommand = new AsyncRelayCommand<object>(async (obj) => await ExecuteLoadingAction(async () => await NavigateToChecklistAsync(obj)), CanExecuteCommands);
        }

        public override async Task Init()
        {
            if (PagesFromDeepLink > 0)
                PagesFromDeepLink++;

            CanDeleteChecklist = UserSettings.RoleType == RoleTypeEnum.Manager || UserSettings.RoleType == RoleTypeEnum.ShiftLeader;

            SetListViewLayout(ListViewLayout.Linear, false);
            await LoadIncompleteChecklistsAsync(true);

            MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
            {
                try
                {
                    if (await FifteenSecondLock.WaitAsync(0))
                    {
                        Debug.WriteLine($"FifteenSecondLock CheckForUpdatedChecklistsAsync");

                        await Task.Run(async () =>
                        {
                            var ids = await _updateService?.CheckForUpdatedChecklistsAsync();
                            if (ids.Count > 0)
                            {
                                await LoadIncompleteChecklistsAsync(true).ConfigureAwait(false);
                            }
                        });
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

            MessagingCenter.Subscribe<ChecklistsService, ChecklistModel>(this, Constants.ChecklistAdded, async (sender, checklist) =>
            {
                await LoadIncompleteChecklistsAsync(true);
            });

            MessagingCenter.Subscribe<SyncService>(this, Constants.ChecklistTemplateChanged, async (sender) =>
            {
                await LoadIncompleteChecklistsAsync(true);
            });

            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
                MessagingCenter.Unsubscribe<ChecklistsService, ChecklistModel>(this, Constants.ChecklistAdded);
                MessagingCenter.Unsubscribe<SyncService>(this, Constants.ChecklistTemplateChanged);
            });
            base.Dispose(disposing);
        }

        private async Task NavigateToChecklistAsync(object obj)
        {
            async Task NavigateToTemplate(object obj)
            {
                if (obj is ChecklistModel item)
                {
                    if (item.Id == -1)
                    {
                        await NavigateToNewChecklist();
                    }
                    else
                    {
                        using var scope = App.Container.CreateScope();
                        var taskTemplatesViewModel = scope.ServiceProvider.GetService<TaskTemplatesViewModel>();
                        taskTemplatesViewModel.ChecklistTemplateId = item.TemplateId;
                        taskTemplatesViewModel.ChecklistTemplates = ChecklistTemplates;
                        taskTemplatesViewModel.IncompleteChecklist = item;
                        taskTemplatesViewModel.PagesFromDeepLink = PagesFromDeepLink;
                        taskTemplatesViewModel.TaskFromDeepLink = TaskFromDeepLink;
                        taskTemplatesViewModel.IsMenuVisible = !IsFromBookmark;
                        taskTemplatesViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;

                        await NavigationService.NavigateAsync(viewModel: taskTemplatesViewModel);
                    }
                }
            }

            if (obj is Syncfusion.Maui.ListView.ItemTappedEventArgs data)
                await NavigateToTemplate(data.DataItem);
            else
                await NavigateToTemplate(obj);
        }

        private async Task NavigateToNewChecklist()
        {
            using var scope = App.Container.CreateScope();
            var taskTemplatesViewModel = scope.ServiceProvider.GetService<TaskTemplatesViewModel>();
            taskTemplatesViewModel.ChecklistTemplateId = ChecklistTemplateId;
            taskTemplatesViewModel.ChecklistTemplates = ChecklistTemplates;
            taskTemplatesViewModel.PagesFromDeepLink = PagesFromDeepLink;
            taskTemplatesViewModel.TaskFromDeepLink = TaskFromDeepLink;
            taskTemplatesViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;
            taskTemplatesViewModel.ShouldClearStatuses = true;

            await NavigationService.NavigateAsync(viewModel: taskTemplatesViewModel);
        }

        private async Task DeleteIncompletedChecklist(ChecklistModel item)
        {
            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.HideDeletePopup); });
            var result = await _checklistService.DeleteIncompletedChecklist(item);
            if (result)
            {
                Checklists.Remove(item);
            }
        }

        private ChecklistModel GetStartNewChecklistButton()
        {
            return new ChecklistModel()
            {
                Id = -1,
                ModifiedAt = DateTime.MaxValue,
                Picture = Picture,
                Name = Name,
                TemplateId = ChecklistTemplateId,
                EditedByUsers = new List<Api.Models.Basic.UserBasic>(),
            };
        }

        public async Task LoadIncompleteChecklistsAsync(bool refresh = false)
        {
            var result = new List<ChecklistModel>();

            if (IsFromBookmark)
                result = await _checklistService.GetIncompleteChecklistsAsync(checklistTemplateId: ChecklistTemplateId, refresh: refresh, refreshActions: false).ConfigureAwait(false);
            else if (PagesFromDeepLink > 0)
                result = await _checklistService.GetIncompleteDeeplinkChecklistsAsync(taskId: TaskFromDeepLink.Id, refresh: refresh).ConfigureAwait(false);
            else
                result = await _checklistService.GetIncompleteChecklistsAsync(checklistTemplateId: ChecklistTemplateId, refresh: refresh, refreshActions: false).ConfigureAwait(false);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Checklists = new ObservableCollection<ChecklistModel>(result);
                var button = GetStartNewChecklistButton();
                Checklists.Insert(0, button);
            });
        }

        // <summary>
        /// Sets the ListView layout.
        /// </summary>
        /// <param name="listViewLayout">The list view layout.</param>
        private void SetListViewLayout(object obj, bool saveToSettings = true)
        {
            if (obj is ListViewLayout listViewLayout)
            {
                if (listViewLayout == ListViewLayout.Grid)
                    IsListVisible = false;
                else
                    IsListVisible = true;

                ListViewLayout = listViewLayout;

                if (saveToSettings)
                    Settings.ListViewLayout = listViewLayout;
            }
        }

        protected override async Task RefreshAsync()
        {
            await LoadIncompleteChecklistsAsync(IsRefreshing);
        }
    }
}