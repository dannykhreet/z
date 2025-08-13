using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using MediaManager;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Shared
{
    public class ItemsDetailViewModel : BaseViewModel
    {
        public ItemsDetailViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public string SenderClassName { get; set; }

        public List<IDetailItem> Items { get; set; }

        public IDetailItem SelectedItem { get; set; }

        public string CommentString { get; set; }

        public bool HasComment { get; set; }

        //only for android
        public bool IsSwipeGestureVisible => (SelectedItem?.HasVideo ?? false) && DeviceInfo.Platform == DevicePlatform.Android;

        public bool IsLocalPicture { get; set; }

        public ICommand SwipedLeft => new Command<string>((direction) =>
        {
            ExecuteLoadingAction(async () => await ChangeSelection(direction));
        }, CanExecuteCommands);

        public ICommand SwipedRight => new Command<string>((direction) =>
        {
            ExecuteLoadingAction(async () => await ChangeSelection(direction));
        }, CanExecuteCommands);

        public override async Task Init()
        {
            CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.One;
            CrossMediaManager.Current.MediaItemFinished += Current_MediaItemFinished;

            await Task.Run(async () => await CrossMediaManager.Current.PlayFromItem(SelectedItem));

            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CrossMediaManager.Current.MediaItemFinished -= Current_MediaItemFinished;
        }

        public async override Task CancelAsync()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(SenderClassName, Constants.UpdateSlideIndex, Items.IndexOf(SelectedItem));
                MessagingCenter.Send(SenderClassName, Constants.UpdateSlideIndex, SelectedItem);
            });
            await base.CancelAsync();
        }

        private void Current_MediaItemFinished(object sender, MediaManager.Media.MediaItemEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CrossMediaManager.Current.Pause();
            });
        }

        private async Task ChangeSelection(string direction)
        {
            if (Settings.IsRightToLeftLanguage)
            {
                if (direction == "Left")
                    direction = "Right";
                else
                    direction = "Left";
            }

            var itemIndex = Items.IndexOf(SelectedItem);

            if (direction == "Left")
                await SelectItem(x => Items.IndexOf(x) > itemIndex, 1);
            else
                await SelectItem(x => Items.IndexOf(x) < itemIndex, -1);
        }

        private async Task SelectItem(Func<IDetailItem, bool> predicate, int itemsCount)
        {
            var itemsAwa = Items.Where(predicate).ToList();

            if (itemsAwa.Any())
            {
                var index = Items.IndexOf(SelectedItem);
                index += itemsCount;
                SelectedItem = null;
                var item = Items[index];
                IsLocalPicture = item.IsLocalMedia;
                SelectedItem = item;
                if (SelectedItem.HasVideo)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await CrossMediaManager.Current.PlayFromItem(SelectedItem);
                    });
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(CrossMediaManager.Current.Stop);
                }
            }
        }
    }
}
