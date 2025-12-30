using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.TaskGeneration
{
    public class GenerationConfigurationItem
    {
        public int? AreaId { get; set; }
        public int? ShiftId { get; set; }
        public int? TemplateId { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string PlanningType { get; set; }

    }
}
