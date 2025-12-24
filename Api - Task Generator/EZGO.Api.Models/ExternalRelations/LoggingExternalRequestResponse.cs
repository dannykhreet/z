using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.ExternalRelations
{
    public class LoggingExternalRequestResponse
    {
        /*
         * DB: logging_external_requestresponse
         * id
         * object_type
         * object_id
         * company_id
         * request
         * response
         * connector_type
         * description
         * created_at
         * created_by_id
         * modified_at
         * modified_by_id
         */

        public int Id { get; set; }
        public string ObjectType { get; set; }
        public int ObjectId { get; set; }
        public int CompanyId { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ConnectorType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int CreatedById { get; set; }
        public int ModifiedById { get; set; }
    }
}
