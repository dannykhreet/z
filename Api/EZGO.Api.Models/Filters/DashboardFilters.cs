using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// DashboardFilters; Dashboard filters used for filtering dashboard requests.
    /// </summary>
    public class DashboardFilters
    {
        public bool UseStatisticsTotals { get; set; }
        public bool UseAnnouncements { get; set; }
        public bool UseCompanyOverview { get; set; }
        public bool UseCompletedChecklists { get; set; }
        public bool UseCompletedAudits { get; set; }
        public bool UseCompletedTasks { get; set; }
        public bool UseActions { get; set; }
    }
}
