using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;


namespace EZGO.Maui.Core.ViewModels
{
    public class ActionReportActionsViewModel : BaseViewModel
    {
        public int UserId { get; set; }
        public int TaskTemplateId { get; set; }
        public int ResourceId { get; set; }

        public TimespanTypeEnum? Period;

        public bool IsBusy { get; set; }
        public bool FilterByTimespan { get; set; } = true;

        public string ReportActionTitle { get; set; } = "";

        public List<BasicActionsModel> Actions { get; set; }

        public ICommand ActionSolvedCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await ToggleActionStatusAsync(obj, ActionStatusEnum.Solved);
            });
        }, CanExecuteCommands);

        public ICommand NavigateToConversationCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToConversationAsync(obj);
            });
        }, CanExecuteCommands);

        public ActionReportActionsViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public override async Task Init()
        {

            MessagingCenter.Subscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged, async (sender, args) =>
            {
                if (args.TypeOfChange == ActionChangedMessageArgs.ChangeType.SetToResolved && args.TaskTemplateId > 0)
                {
                    await LoadData();
                }
            });

            await Task.Run(async () => await LoadData());

            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged);
            base.Dispose(disposing);
        }

        private async Task LoadData()
        {
            if (UserId != 0)
                await LoadUserActionsAsync();
            else if (TaskTemplateId != 0)
                await LoadTaskTemplateActionsAsync();
            else if (ResourceId != 0)
                await LoadAssignedUserActionsAsync();
            else
                Actions = new List<BasicActionsModel>();

            if (FilterByTimespan)
                ImplementTimespanType();

            IsBusy = false;

            HasItems = Actions.Any();
        }

        private void ImplementTimespanType()
        {
            Period = Settings.ReportInterval;
            List<BasicActionsModel> actions = new List<BasicActionsModel>();
            switch (Period)
            {
                case TimespanTypeEnum.LastTwelveDays:
                    actions = Actions.Where(x => x.CreatedAt >= DateTimeHelper.UtcNow.Date.AddDays(-12)).ToList();
                    break;
                case TimespanTypeEnum.LastTwelveWeeks:
                    actions = Actions.Where(x => x.CreatedAt > DateTimeHelper.UtcNow.Date.AddDays(-84)).ToList();
                    break;
                case TimespanTypeEnum.LastTwelveMonths:
                    actions = Actions.Where(x => x.CreatedAt >= DateTimeHelper.UtcNow.Date.AddMonths(-11)).ToList();
                    break;
                case TimespanTypeEnum.ThisYear:
                    actions = Actions.Where(x => x.CreatedAt.Year == DateTimeHelper.UtcNow.Date.Year).ToList();
                    break;
            }

            Actions = actions;
        }

        protected override async Task RefreshAsync()
        {
            await LoadData();
        }

        private async Task LoadTaskTemplateActionsAsync()
        {
            List<ActionsModel> myactions = await _actionService.GetReportActionsAsync(tasktemplateId: TaskTemplateId, refresh: IsRefreshing);

            Actions = myactions.ToBasicList<BasicActionsModel, ActionsModel>();
        }

        private async Task LoadUserActionsAsync()
        {
            List<ActionsModel> myactions = await _actionService.GetReportActionsAsync(refresh: IsRefreshing);

            if (myactions.Any())
            {
                myactions = myactions.Where(x => x.CreatedById == UserId).ToList();
            }

            Actions = myactions.ToBasicList<BasicActionsModel, ActionsModel>();
        }

        private async Task LoadAssignedUserActionsAsync()
        {
            List<ActionsModel> myactions = await _actionService.GetReportActionsAsync(assignedUserId: ResourceId, refresh: IsRefreshing);

            Actions = myactions.ToBasicList<BasicActionsModel, ActionsModel>();
        }

        private async Task ToggleActionStatusAsync(object obj, ActionStatusEnum status)
        {
            if (obj is BasicActionsModel action)
            {
                if (action.FilterStatus != status)
                {
                    var page = NavigationService?.GetCurrentPage();
                    if (page == null)
                        return;

                    string confirm = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertConfirmAction);
                    string yes = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertYesButtonTitle);
                    string no = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertNoButtonTitle);

                    string result = await page.DisplayActionSheet(confirm, null, null, yes, no);

                    if (result == yes && await _actionService.SetActionResolvedAsync(action))
                    {
                        action.FilterStatus = status;
                    }
                    _statusBarService?.HideStatusBar();
                }
            }
        }

        private async Task NavigateToConversationAsync(object obj)
        {
            BasicActionsModel action = null;

            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is BasicActionsModel)
                action = (BasicActionsModel)eventArgs.DataItem;
            else if (obj is BasicActionsModel)
                action = (BasicActionsModel)obj;

            if (action != null)
            {
                var scope = App.Container.CreateScope();
                var vm = scope.ServiceProvider.GetService<ActionConversationViewModel>();

                vm.Actions = Actions;
                vm.SelectedAction = action;

                await NavigationService.NavigateAsync(viewModel: vm);
            }
        }
    }
}
