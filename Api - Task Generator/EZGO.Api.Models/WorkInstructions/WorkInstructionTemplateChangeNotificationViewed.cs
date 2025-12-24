using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    public class WorkInstructionTemplateChangeNotificationViewed
    {
        public int Id { get; set; }
        public UserBasic ViewedUser { get; set; }
        public int WorkInstructionTemplateId { get; set; }
        public int WorkInstructionTemplateChangeNotificationId { get; set; }
        public DateTime? ViewedAt { get; set; }
    }
}
