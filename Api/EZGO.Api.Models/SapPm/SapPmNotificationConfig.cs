using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmNotificationConfig
    {
        public int FunctionalLocationId { get; set; }
        public string MaintPriority { get; set; }
        public string Notificationtype { get; set; }
        public string NotificationTitle { get; set; }
    }
}
