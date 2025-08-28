using EZGO.Api.Models;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Audits
{
    public class AuditSignModel
    {
        public int AuditId { get; set; }

        public int CompanyId { get; set; }

        public IEnumerable<Signature> Signatures { get; set; }
    }
}
