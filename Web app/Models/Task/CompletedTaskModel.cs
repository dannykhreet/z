using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Task
{
    public class CompletedTaskModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int? ActionsCount { get; set; }
        public int? CommentCount { get; set; }

        public int TemplateId { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? StartAt { get; set; }

        public string Picture { get; set; }
        public string TaskType { get; set; }
        public string RecurrencyType { get; set; }
        public CompletedTaskSignature Signature { get; set; }
        public PictureProof PictureProof { get; set; }

    }
}
