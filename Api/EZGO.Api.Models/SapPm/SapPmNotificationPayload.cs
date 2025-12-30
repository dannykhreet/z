using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmNotificationPayload
    {
        public string NotificationText { get; set; }
        public string MaintPriority { get; set; }
        public string NotificationType { get; set; }
        public string ReportedByUser { get; set; }
        public string NotificationCreationDate { get; set; }
        public string NotificationCreationTime { get; set; }
        public string MaintNotifLongTextForEdit { get; set; }
        public string MaintenancePlannerGroup { get; set; }
        public string MaintenancePlanningPlant { get; set; }
        public string MainWorkCenter { get; set; }
        public string MainWorkCenterPlant { get; set; }
        public string FunctionalLocation { get; set; }

    }
}
