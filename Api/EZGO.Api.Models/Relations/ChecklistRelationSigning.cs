using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// ChecklistRelationSigning; Relation object between checklist and one or more signatures.
    /// Currently Checklist can have 2 signatures.
    /// </summary>
    public class ChecklistRelationSigning
    {
        public int ChecklistId { get; set; }
        public int CompanyId { get; set; }
        public List<Signature> Signatures { get; set; }
    }
}
