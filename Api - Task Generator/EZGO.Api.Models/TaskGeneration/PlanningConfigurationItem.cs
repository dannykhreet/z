using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.TaskGeneration
{
    public class PlanningConfigurationItem
    {
        public string Reason { get; set; }
        public DateTime? DisabledFrom { get; set; }
        public DateTime? DisabledTo { get; set; }
        public List<int> AreaIds { get; set; }
        public List<int> TaskIds { get; set; }
        public List<int> ShiftIds { get; set; }
    }
}
