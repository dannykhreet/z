using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// AuditRelationSigning; Relation object between audit and one or more signatures.
    /// Currently Audit can have 2 signatures.
    /// </summary>
    public class AuditRelationSigning
    {
        public int AuditId { get; set; }

        public int CompanyId { get; set; }
        public List<Signature> Signatures { get; set; }
    }
}
