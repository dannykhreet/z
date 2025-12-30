using EZGO.Api.Models.SapPm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Converters
{
    public static class SapPmConverter
    {
        public static SapPmNotificationConfig ToSapPmNotificationConfig(this SapPmNotification sapPmNotification)
        {
            return new SapPmNotificationConfig()
            {
                FunctionalLocationId = sapPmNotification.FunctionalLocationId,
                MaintPriority = sapPmNotification.MaintPriority,
                Notificationtype = sapPmNotification.NotificationType,
                NotificationTitle = sapPmNotification.NotificationText
            };
        }
    }
}
