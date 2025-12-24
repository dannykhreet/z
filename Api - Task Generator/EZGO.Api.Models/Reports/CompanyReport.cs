using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// CompanyReport; Company Report for use with management reports
    /// </summary>
    public class CompanyReport
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public List<StatsItem> Statistics { get; set; }
    }
}
