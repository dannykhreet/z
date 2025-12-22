using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.WorkInstructions;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstructionTemplateWithNotificationData
    {
        public WorkInstructionTemplate WorkInstructionTemplate { get; set; }

        public string NotificationComment { get; set; }

    }
}
