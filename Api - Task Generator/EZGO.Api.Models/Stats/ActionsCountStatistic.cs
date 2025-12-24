using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class ActionsCountStatistic
    {
        public int? CountNr { get; set; }
        public int? CountNrUnresolved { get; set; }
        public int? CountNrResolved { get; set; }
        public int? CountNrOverdue { get; set; }
    }
}
