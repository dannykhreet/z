using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.LogAuditing;

namespace WebApp.ViewModels
{
    public class AuditingDetailsViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public DateTime CreatedOn { get; set; }
        public  string Description { get; set; }
        public string OriginalObjectDataJson { get; set; }
        public string MutatedObjectDataJson { get; set; }
        public List<AuditingLogChange> AuditingLogChanges { get; set; }

    }
}
