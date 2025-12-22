using EZGO.Api.Models.WorkInstructions;
using System.Collections.Generic;
using WebApp.Models.WorkInstructions;

namespace WebApp.ViewModels
{
    public class WorkInstructionChangeNotificationsViewModel : BaseViewModel
    {
        public List<WorkInstructionTemplateChangesNotificationModel> ChangeNotifications { get; set; }
        public List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate> WorkInstructions { get; set; }
    }
}
