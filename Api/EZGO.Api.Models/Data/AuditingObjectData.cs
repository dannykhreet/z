using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Data
{
    public class AuditingObjectData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public int? ParentObjectId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Description { get; set; }
        public string OriginalObjectDataJson { get; set; }
        public string MutatedObjectDataJson { get; set; }
    }
}
