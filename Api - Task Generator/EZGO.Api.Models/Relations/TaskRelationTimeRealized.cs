using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class TaskRelationTimeRealized
    {
        public int TaskId { get; set; }
        public int CompanyId { get; set; }
        public int RealizedById { get; set; }
        public int RealizedTime { get; set; }
    }
}
