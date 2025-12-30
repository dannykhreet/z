using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmNotificationOptions
    {
        public SapPmLocation FunctionalLocation { get; set; }
        public List<String> MaintPriority { get; set; }
        public List<String> Notificationtype { get; set; }
        public List<SapPmMaintPriority> MaintPriorityExpanded { get; set; }
        public List<SapPmNotificationType> NotificationTypeExpanded { get; set; }
    }
}
