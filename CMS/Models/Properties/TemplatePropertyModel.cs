using System;
namespace WebApp.Models.Properties
{
    public class TemplatePropertyModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public int FieldType { get; set; }
        public int TaskTemplateId { get; set; }
        public int PropertyId { get; set; }
        public int UnitKindId { get; set; }
        public string PropertyValueDisplay { get; set; }
        public string TitleDisplay { get; set; }
        public int? PropertyValueId { get; set; }
        public PropertyModel Property { get; set; }
        public PropertyValueModel PropertyValue { get; set; }

        public bool isNew { get; set; }

        public int? PrimaryIntValue { get; set; }
        public int? SecondaryIntValue { get; set; }
        public decimal? PrimaryDecimalValue { get; set; }
        public decimal? SecondaryDecimalValue { get; set; }
        public string PrimaryStringValue { get; set; }
        public string SecondaryStringValue { get; set; }
        public string PrimaryTimeValue { get; set; }
        public string SecondaryTimeValue { get; set; }
        public DateTime? PrimaryDateTimeValue { get; set; }
        public DateTime? SecondaryDateTimeValue { get; set; }
        public bool? BoolValue { get; set; }

        public int? ChecklistTemplateId { get; set; }
        public int? AuditTemplateId { get; set; }
        public int? PropertyGroupId { get; set; }
        public bool? IsRequired { get; set; }
        public int? ValueType { get; set; }
    }

}
