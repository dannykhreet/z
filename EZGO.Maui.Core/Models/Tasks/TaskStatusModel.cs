using EZGO.Api.Models.Enumerations;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TaskStatusModel
    {
        public int CompanyId { get; set; }

        public int? ChecklistTemplateId { get; set; }
        public int? AuditTemplateId { get; set; }

        public int? ChecklistId { get; set; }
        public int? AuditId { get; set; }

        public int TaskTemplateId { get; set; }

        public int UserId { get; set; }

        public int TaskStatus { get; set; }

        public int Score { get; set; }
    }
}
