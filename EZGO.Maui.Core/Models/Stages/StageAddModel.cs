using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Api.Models;

namespace EZGO.Maui.Core.Models.Stages
{
    public class StageAddModel
    {
        public int CompanyId { get; internal set; }
        public int? CreatedById { get; internal set; }
        public int? ModifiedById { get; internal set; }
        public List<Signature> Signatures { get; internal set; }
        public List<int> TaskTemplateIds { get; internal set; }
        public int StageTemplateId { get; internal set; }
        public int Id { get; internal set; }
        public string Status { get; internal set; } = "not ok";
        public string ShiftNotes { get; internal set; }
    }
}