using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class StageTaskTemplateRelation
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int StageTemplateId { get; set; }
        public int TaskTemplateId { get; set; }
    }
}
