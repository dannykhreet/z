using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ActionsReportExtended
    {
        public List<ActionsStartedResolvedStatistic> ActionsStartedAndResolved { get; set; }
        public ActionCountsStatistic ActionCounts { get; set; }
        public ActionsUsersStatistic ActionsCreated { get; set; }
        public ActionsUsersStatistic ActionsAssigned { get; set; }
    }
}
