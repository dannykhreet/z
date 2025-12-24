using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class TaskTemplateRelationShift
    {
        public int TaskTemplateId { get; set; }
        public int ShiftId { get; set; }
        public int Day { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
