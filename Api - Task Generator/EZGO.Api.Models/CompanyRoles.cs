using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Roles for a company. Roles are hardcoded in the database but can have a name.
    /// Based on the role fields in the companies_company table.
    /// </summary>
    public class CompanyRoles
    {
        public string BasicDisplayName { get; set; }
        public string ShiftLeaderDisplayName { get; set; }
        public string ManagerDisplayName { get; set; }
    }
}
