using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ActionsUsersStatistic
    {
        public int TotalCount { get; set; }
        public List<ActionsUserStatistic> TopUsers { get; set; }
        public ActionsUsersStatistic()
        {
            TotalCount = 0;
            TopUsers = new List<ActionsUserStatistic>();
        }

    }
}
