using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.TaskManagement
{
    public class DowntimePlanning
    {
        public string Reason { get; set; }
        public DateTime? DisabledFrom { get; set; }
        public DateTime? DisabledTo { get; set; }
        public List<int> AreaIds { get; set; }
        public List<int> TaskIds { get; set; }
        public List<int> ShiftIds { get; set; }
    }
}
