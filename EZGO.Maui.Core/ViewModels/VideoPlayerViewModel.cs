using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;

namespace EZGO.Maui.Core.ViewModels
{
    public class VideoPlayerViewModel : BaseViewModel
    {
        public VideoPlayerViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public ThumbnailGridDetailModel MediaItem { get; set; }

        public override async Task Init()
        {
            await base.Init();
        }
    }
}
