using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class ToolFilter
    {
        public int CompanyId { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
