using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmNotificationFailure
    {
        public int ActionId { get; set; }
        public int FailureCount { get; set; }
        public int MinutesSinceLastFailure { get; set; }
    }
}
