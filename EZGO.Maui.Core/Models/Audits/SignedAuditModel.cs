using EZGO.Api.Models;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Audits
{
    public class SignedAuditModel : IQueueableItem
    {
        public int AuditTemplateId { get; set; }

        public LocalDateTime Date { get; set; }

        public DateTime? StartedAt { get; set; }

        public string Name { get; set; }

        public IEnumerable<BasicTaskTemplateModel> Tasks { get; set; }

        public IEnumerable<SignatureModel> Signatures { get; set; }

        public List<UserValuesPropertyModel> OpenFieldsValues { get; set; }

        public bool? IsRequiredForLinkedTask { get; set; }

        public long? LinkedTaskId { get; set; }

        public bool ShouldBePosted { get; set; } = true;

        public bool IsCompleted { get; set; } = true;

        public int Id { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }

        public Guid LocalGuid { get; }

        public string Version { get; set; }

        public SignedAuditModel()
        {
            LocalGuid = Guid.NewGuid();
        }

        [JsonConstructor]
        public SignedAuditModel(Guid localGuid)
        {
            LocalGuid = localGuid;
        }
    }
}
