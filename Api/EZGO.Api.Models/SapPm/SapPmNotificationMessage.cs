using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmNotificationMessage
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int ActionId { get; set; }
        public SapPmNotificationPayload Payload { get; set; }

    }
}
