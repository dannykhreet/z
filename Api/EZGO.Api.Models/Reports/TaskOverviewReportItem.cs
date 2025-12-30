using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// TaskOverviewReportItem; For use with TaskOverviewReport.
    /// </summary>
    public class TaskOverviewReportItem
    {
        public string Status { get; set; }
        public string Type { get; set; }

        public string SourceType { get; set; }
        public int NrOfItems { get; set; }

    }
}
