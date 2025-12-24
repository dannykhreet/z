using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// BasePropertyObject; Based on the BasePropertyObject (and inherited objects) a basic setup for a property with a template is set.
    /// When getting data, most of the time the Property and UnitKinds are fully supplied. When posting only the primary values need to be added (so only the property-/unit kind- id and the 'value' parameters are needed.
    /// Index will default to 0, and sorting will be done based on property name when displaying information if not supplied.
    /// </summary>
    public class BasePropertyObject
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int? PropertyValueId { get; set; }
        public int? PropertyGroupId { get; set; }

        [ObsoleteAttribute("This property is obsolete. Use PropertyValueDisplay instead.", false)]
        public int? UnitKindId { get; set; }
        public int? PrimaryIntValue { get; set; } //primary item for range or just item value
        public int? SecondaryIntValue { get; set; } //secundary item for range
        /// <summary>
        /// NOTE! will be removed, was a typo.
        /// </summary>
        [ObsoleteAttribute("This property is obsolete. Will be removed. (was a typo use SecondaryIntValue)", false)]
        public int? SecondaryValue { get; set; } //secundary item for range
        public decimal? PrimaryDecimalValue { get; set; } //primary item for range or just item value
        public decimal? SecondaryDecimalValue { get; set; } //secundary item for range
        public string PrimaryStringValue { get; set; } ///primary item for range or just item value or icon value
        public string SecondaryStringValue { get; set; }   //secundary item for range
        public DateTime? PrimaryDateTimeValue { get; set; } ///primary item for range or just item value
        public DateTime? SecondaryDateTimeValue { get; set; }   //secundary item for range
        public string PrimaryTimeValue { get; set; } ///primary item for range or just item value or icon value (hh:mm)
        public string SecondaryTimeValue { get; set; }   //secundary item for range (hh:mm)
        public bool? BoolValue { get; set; } //bool value

        [ObsoleteAttribute("This property is obsolete. Use PropertyValueDisplay instead.", false)]
        public string UnitTypeDisplay { get; set; } //custom display can differ per property
        public string PropertyValueDisplay { get; set; } //custom display can differ per property
        public string TitleDisplay { get; set; } //custom display for specific template can differ per property
        public PropertyDisplayTypeEnum? DisplayType { get; set; } //square (default), custom icon, structure
        public string CustomDisplayType { get; set; } //custom display type used when DisplayType = Custom or for other functionalities
        public Property Property { get; set; }
        public PropertyValue PropertyValue { get; set; }
#pragma warning disable CS0618 // Type or member is obsolete
        public MeasurementUnitKind UnitKind { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete
        /// <summary>
        /// DefaultValueUnitType; Based on the FieldKindType a unit type can found. E.g. for temperature this would be Celcius, Fahrenheid or Kelvin.
        /// </summary>
        [ObsoleteAttribute("This property is obsolete. Use UnitKind instead.", false)]
        public PropertyValueUnitMeasurementTypeEnum? ValueUnitType { get; set; } //celsius, meters, bar
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool? IsRequired { get; set; }
        public PropertyValueTypeEnum? ValueType { get; set; } //integer, decimal, string
        public PropertyFieldTypeEnum? FieldType { get; set; } //single value, range, larger then x, smaller than y if not configured same as connected property.
        public int Index { get; set; }
    }
}
