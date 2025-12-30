using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// TaskRelationSigning; Relation object between task and one or more signatures.
    /// Currently Checklist can have 1 signature.
    /// </summary>
    public class TaskRelationSigning
    {
        public int TaskId { get; set; }

        public int CompanyId { get; set; }
        public List<Signature> Signatures { get; set; }
    }
}
