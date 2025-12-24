using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class MyActionsStatusCountsStatistic
    {
        public int StartedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int OpenCount { get; set; }
        public int OverdueCount { get; set; }
    }
}
