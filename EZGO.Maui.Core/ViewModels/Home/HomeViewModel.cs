using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Menu;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;

namespace EZGO.Maui.Core.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly ITasksService _tasksService;
        public MenuViewModel MenuViewModel { get; private set; }

        public int TodoCount { get; set; }

        public int OkCount { get; set; }

        public int NotOkCount { get; set; }

        public int SkippedCount { get; set; }

        public bool ActionOnTheSpotEnabled { get => CompanyFeatures.CompanyFeatSettings.ActionOnTheSpotEnabled; }

        public bool IsAssessmentButtonVisible => UserSettings.RoleType == RoleTypeEnum.Manager;


        public IAsyncCommand NavigateToNewActionCommand { get; private set; }

        public HomeViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            ITasksService tasksService,
            MenuViewModel menuViewModel) : base(navigationService, userService, messageService, actionsService)
        {
            _tasksService = tasksService;
            MenuViewModel = menuViewModel;

            NavigateToNewActionCommand = new AsyncCommand(NavigateToNewActionAsync);
            Settings.MenuLocation = MenuLocation.None;
        }

        public override async Task Init()
        {

            Settings.MenuLocation = MenuLocation.Home;
            Settings.SubpageReporting = MenuLocation.None;
            Settings.SubpageActions = MenuLocation.None;
            Settings.SubpageTasks = MenuLocation.None;

            string result = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.homeScreenWelcomText)} - {Settings.WorkAreaName}";

            Title = string.Format(result.ReplaceLanguageVariablesCumulative(), UserSettings.Firstname, UserSettings.Id);

            await Task.Run(async () => await base.Init());

            MessagingCenter.Subscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage, (sender) =>
            {
                RefreshInfo();
            });

            await Task.Run(async () => await FillTasksTodayAsync());
        }

        /// <summary>
        /// Fetches all todays reporting, this ia a copy from the taskviewpage's today
        /// </summary>
        /// <returns></returns>
        private async Task FillTasksTodayAsync()
        {
            List<BasicTaskModel> today = await _tasksService.GetTasksForPeriodAsync(TaskPeriod.Today, refresh: IsRefreshing);
            if (today.Any())
            {
                OkCount = today.Count(x => x.FilterStatus == TaskStatusEnum.Ok);
                NotOkCount = today.Count(x => x.FilterStatus == TaskStatusEnum.NotOk);
                SkippedCount = today.Count(x => x.FilterStatus == TaskStatusEnum.Skipped);
                TodoCount = today.Count(x => x.FilterStatus == TaskStatusEnum.Todo);
            }
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.IsFromHomeScreen = true;
            actionNewViewModel.ActionType = ActionType.Task;
            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Unsubscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage); });
            _tasksService.Dispose();
            base.Dispose(disposing);
        }

        private void RefreshInfo()
        {
            string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.homeScreenWelcomText);
            Title = string.Format(result.ReplaceLanguageVariablesCumulative(), UserSettings.Firstname, UserSettings.Id);

            Logo = UserSettings.CompanyLogoUrl;
            Fullname = UserSettings.Fullname;
            Picture = UserSettings.UserPictureUrl != Constants.NoProfilePicture2 ? UserSettings.UserPictureUrl : null; ;
        }
    }
}
