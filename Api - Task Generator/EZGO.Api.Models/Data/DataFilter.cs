using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class AutomatedDataFilter
    {
        public int CompanyId { get; set; }
        public int HoldingId { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string ProcedureName { get; set; }
    }
}
