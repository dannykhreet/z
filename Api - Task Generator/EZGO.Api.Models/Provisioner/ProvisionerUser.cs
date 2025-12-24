using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Provisioner
{
    /// <summary>
    /// ProvisionUser; Contains fields that directly map to EZGO fields and contains 
    /// </summary>
    public class ProvisionerUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Upn { get; set; }
        public int CompanyId { get; set; }
        public int? Id { get; set; }
        public int? AreaId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeStartDateString { get; set; }
        public string EmployeeEndDateString { get; set; }
        public string ExternalCompanyIdentifier { get; set; }
        public string ExternalActiveString { get; set; }
        public int ModifiedByUserId { get; set; }
        public bool SetInactiveInverted { get { return (this.ExternalActiveString == "ja" || this.ExternalActiveString == "yes" ||this.ExternalActiveString == "true");  } }
        public bool SetInactive { get { return (this.ExternalActiveString == "nee" || this.ExternalActiveString == "no" || this.ExternalActiveString == "false"); } }

    }
}
