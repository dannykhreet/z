using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.ExternalRelations
{
    public class ExternalRelation
    {
        /*
         * DB: external_relations
         * id
         * object_type
         * object_id
         * external_id
         * status
         * status_message
         * connector_type
         * company_id
         * created_at
         * modified_at
         * created_by_id
         * modified_by_id
         */

        public int Id { get; set; }
        public string ObjectType { get; set; }
        public int ObjectId { get; set; }
        public int? ExternalId { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public string ConnectorType { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int CreatedById { get; set; }
        public int ModifiedById { get; set; }
    }
}
