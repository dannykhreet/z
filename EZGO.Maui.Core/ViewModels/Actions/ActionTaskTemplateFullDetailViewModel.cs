using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Tasks;
using MediaManager;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionTaskTemplateFullDetailViewModel : BaseViewModel
    {
        public ActionTaskTemplateFullDetailViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public BasicTaskTemplateModel SelectedTask { get; set; }

        public override async Task Init()
        {
            await Task.Run(async () => await CrossMediaManager.Current.PlayFromTaskTemplate(SelectedTask));
            await base.Init();
        }
    }
}
