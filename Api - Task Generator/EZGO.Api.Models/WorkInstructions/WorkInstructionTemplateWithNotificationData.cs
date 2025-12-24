using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    public class WorkInstructionTemplateWithNotificationData
    {
        public WorkInstructionTemplate WorkInstructionTemplate { get; set; }
        public string NotificationComment { get; set; }
    }
}
