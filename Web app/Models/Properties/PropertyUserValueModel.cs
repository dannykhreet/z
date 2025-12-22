using System;
namespace WebApp.Models.Properties
{
    public class PropertyUserValueModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int TaskId { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int TemplatePropertyId { get; set; }
        public decimal? UserValueDecimal { get; set; }
        public int? UserValueInt { get; set; }
        public string UserValueString { get; set; }
        public string UserValueTime { get; set; }
        public DateTime? UserValueDate { get; set; }
        public bool? UserBoolValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? RegisteredAt { get; set; }
    }
}
