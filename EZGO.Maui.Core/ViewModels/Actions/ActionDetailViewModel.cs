using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using MediaManager;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionDetailViewModel : BaseViewModel
    {
        public int CurrentIndex { get; set; }

        public List<MediaItem> MediaItems { get; set; }

        public MediaItem SelectedMediaItem { get; set; }

        public ActionDetailViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public bool IsZooming { get; set; }

        public override async Task Init()
        {
            if (MediaItems != null)
                CurrentIndex = MediaItems.IndexOf(SelectedMediaItem);

            CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.One;
            CrossMediaManager.Current.MediaItemFinished += Current_MediaItemFinished;

            if (SelectedMediaItem.IsVideo)
                await Task.Run(async () => await CrossMediaManager.Current.PlayMediaItem(SelectedMediaItem));

            await base.Init();
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
