using System;
using Newtonsoft.Json.Linq;

namespace WebApp.Models.Statistics
{
    public class LogAuditingModel
    {
        public int Id { get; set; }

        public JValue MutatedObject { get; set; }

        public string ObjectType { get; set; }

        public int ObjectId { get; set; }

        public int CompanyId { get; set; }

        public int UserId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOnUTC { get; set; }

    }
}
