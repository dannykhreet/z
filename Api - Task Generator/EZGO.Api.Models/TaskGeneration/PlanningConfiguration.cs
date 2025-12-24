using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.TaskGeneration
{
    public class PlanningConfiguration
    {
        /// <summary>
        /// ConfigurationItems;
        /// Stored in database as JSON. DB: companies_planning_configureation.planning_config
        /// </summary>
        public List<PlanningConfigurationItem> ConfigurationItems { get; set; }
        public int? CompanyId { get; set; }
        public int? Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? UserId { get; set; }
    }
}
