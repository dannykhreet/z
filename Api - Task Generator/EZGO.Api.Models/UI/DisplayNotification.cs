using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.UI
{
    /// <summary>
    /// DisplayNotification; Display notification object, contains notification for a certain user. 
    /// </summary>
    public class DisplayNotification
    {
        public int UserId { get; set; }
        public List<DisplayNotificationItem> Notifications { get; set; }

        public bool HasNotifications { get
            {
                return (Notifications != null && Notifications.Count > 0);
            }
        }

        public DisplayNotification() {
            Notifications = new List<DisplayNotificationItem>();
        }

    }
}
