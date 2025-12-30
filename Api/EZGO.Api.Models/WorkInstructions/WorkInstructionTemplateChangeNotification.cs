using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    public class WorkInstructionTemplateChangeNotification
    {
        public int Id { get; set; }
        public int WorkInstructionTemplateId { get; set; }
        public int CompanyId { get; set; }
        public string NotificationComment { get; set; }
        public List<WorkInstructionTemplateChange> NotificationData { get; set; }
        public List<WorkInstructionTemplateChangeNotificationViewed> NotificationViewedStats { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int ModifiedById { get; set; }
        public string ModifiedBy { get; set; }
    }
}
