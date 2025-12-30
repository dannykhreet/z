using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmNotification
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int ActionId { get; set; }
        public int FunctionalLocationId { get; set; }
        public string NotificationText { get; set; }
        public string MaintNotifLongTextForEdit { get; set; }
        public string MaintPriority { get; set; }
        public string NotificationType { get; set; }
        public DateTime? SentToSapOn { get; set; }
        public Int64? SapId { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
