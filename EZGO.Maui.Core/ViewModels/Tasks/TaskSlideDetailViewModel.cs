using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Tasks;
using MediaManager;

namespace EZGO.Maui.Core.ViewModels.Tasks
{
    public class TaskSlideDetailViewModel : BaseViewModel
    {
        public TaskSlideDetailViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public BasicTaskModel SelectedTask { get; set; }
        public bool AreDatesVisible { get; set; } = true;

        public override async Task Init()
        {
            CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.One;
            CrossMediaManager.Current.MediaItemFinished += Current_MediaItemFinished;

            await Task.Run(async () => await CrossMediaManager.Current.PlayFromTask(SelectedTask));
            await Task.Run(async () => await base.Init());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CrossMediaManager.Current.MediaItemFinished -= Current_MediaItemFinished;
        }

        private void Current_MediaItemFinished(object sender, MediaManager.Media.MediaItemEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CrossMediaManager.Current.Pause();
            });
        }
    }
}
