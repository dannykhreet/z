using EZGO.Api.Models.Basic;
using System;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstructionTemplateChangeNotificationViewedModel
    {
        public int Id { get; set; }
        public UserBasic ViewedUser { get; set; }
        public int WorkInstructionTemplateId { get; set; }
        public int WorkInstructionTemplateChangeNotificationId { get; set; }
        public DateTime? ViewedAt { get; set; }
    }
}
