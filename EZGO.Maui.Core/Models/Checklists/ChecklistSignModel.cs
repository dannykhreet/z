using EZGO.Api.Models;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Checklists
{
    public class ChecklistSignModel
    {
        public int ChecklistId { get; set; }

        public int CompanyId { get; set; }

        public IEnumerable<Signature> Signatures { get; set; }
    }
}
