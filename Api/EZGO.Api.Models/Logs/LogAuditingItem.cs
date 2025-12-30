using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Logs
{
    /// <summary>
    /// LogAuditingItem; Auditing item (not to be confused with the audit structure in the API) used for data audit trail. 
    /// </summary>
    public class LogAuditingItem
    {
        //"id" int8, "original_object" text, "mutated_object" text, "object_type" varchar, "object_id" int4, "company_id" int4, "user_id" int4, "description" text, "created_on" timestamptz
        public long Id { get; set; }
        public string OriginalObject { get; set; }
        public string MutatedObject { get; set; }
        public string ObjectType { get; set; }
        public int? ObjectId { get; set; }
        public int? CompanyId { get; set; }
        public int? UserId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUTC { get; set; }
    }
}
