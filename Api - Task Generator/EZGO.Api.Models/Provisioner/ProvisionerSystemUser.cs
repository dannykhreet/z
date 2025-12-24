using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Provisioner
{
    /// <summary>
    /// ProvisionerSystemUser; System user with a specific company. Will be used for saving data.
    /// </summary>
    public class ProvisionerSystemUser
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
    }
}
