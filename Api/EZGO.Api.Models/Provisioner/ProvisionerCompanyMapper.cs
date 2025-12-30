using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Provisioner
{
    /// <summary>
    /// ProvisionerCompanyMapper; Mapping item, container external company id (as used with within the provisioned data).
    /// </summary>
    public class ProvisionerCompanyMapper
    {
        public int CompanyId { get; set; }
        public string ExternalCompanyId { get; set; }
    }
}
