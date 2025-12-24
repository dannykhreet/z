using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// AreaSystemInformation; Partial data storage of area objects. Directly references several data items in the database. Are not actively used in apps.
    /// </summary>
    public class AreaSystemInformation
    {
        /// <summary>
        /// DB: companies_area.lft
        /// </summary>
        public int? Left { get; set; }
        /// <summary>
        /// DB: companies_area.rght
        /// </summary>
        public int? Right { get; set; }
        /// <summary>
        /// DB: companies_area.tree_id
        /// </summary>
        public int? TreeId { get; set; }
        /// <summary>
        /// DB: companies_area.custom_shifts
        /// </summary>
        public bool HasCustomShifts { get; set; }
        /// <summary>
        /// DB: companies_area.is_system
        /// </summary>
        public bool IsSystem { get; set; }
        /// <summary>
        /// DB: companies_area.system_role
        /// </summary>
        public string SystemRole { get; set; }
        /// <summary>
        /// DB: companies_area.days_to_get_data
        /// </summary>
        public int DaysToGetDate { get; set; }
    }
}
