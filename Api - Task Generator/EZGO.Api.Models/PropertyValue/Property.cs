using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// Property; Property is a dynamic item that can be added to all the main object of the ezgo platform.
    /// This object is based on a property value structure. Where the primary information of this property is located in this object.
    /// All specific information that is needed with a main object (action, template etc) is located in one of the Property[MainObjectName] items.
    /// The data itself that is inputted by the user is handled by the property value items.
    /// DB: [properties]
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Id; Id of property, will be primary key
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// PropertyGroupId; PropertyGroupId of a specific group.
        /// </summary>
        public int PropertyGroupId { get; set; }
        /// <summary>
        ///
        /// </summary>
        public int? PropertyValueKindId { get; set; }
        /// <summary>
        ///
        /// </summary>
        public int? PropertyValueId { get; set; }
        /// <summary>
        /// TemplateId; TemplateId depending on functionality this contains the checklisttemplate, tasktemplate or audittemplateid
        /// </summary>
        public int? TemplateId { get; set;  }

        /// <summary>
        /// Name; Name of property can be used for display if needed
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ShortName; Name of property can be used for display on spaces that don't have to much space.
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// Description; Description of the property, can be used for describing what the property does, will be displayed in the management portals.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// FieldType, The FieldType determine how the property is handled; These can include, range, a single value, larger than, smaller than, etc.
        /// </summary>
        public PropertyFieldTypeEnum FieldType { get; set; } //single value, range, larger then x, smaller than y
        /// <summary>
        /// FieldKindType; The FieldKindType is used for determining what kind of property it is; e.g. pressure, temperature etc. Based on this also the available units that can be used for measuring will be determine.
        /// </summary>
        [ObsoleteAttribute("This property is obsolete. Use UnitKind instead.", false)]
        public PropertyFieldKindTypeEnum? FieldKindType { get; set; } //temperature, pressure, size
        /// <summary>
        /// ValueType; ValueType is the type of data that will be stored by the user; Based on this value the correct columns where to post the data are being determined.
        /// This will be used for Input by user.
        /// </summary>
        public PropertyValueTypeEnum ValueType { get; set; } //integer, decimal, string
        /// <summary>
        /// DisplayValueType; This will be used for knowing in the Management portal what kind of data to display and for formatting reasons in the client apps. Most of the time this will be the same as the ValueType.
        /// </summary>
        public PropertyValueTypeEnum? DisplayValueType { get; set; } //integer, decimal, string
        /// <summary>
        /// DefaultValueUnitType; Based on the FieldKindType a unit type can found. E.g. for temperature this would be Celcius, Fahrenheid or Kelvin.
        /// </summary>
        [ObsoleteAttribute("This property is obsolete. Use Unit instead.", false)]
        public PropertyValueUnitMeasurementTypeEnum? DefaultValueUnitType { get; set; } //celsius, meters, bar
        /// <summary>
        /// DefaultValueUnitTypeDisplay; Display string (e.g. for celsius this could be the c item). This can be used with custom types or for specific other types.
        /// </summary>
        public string DefaultValueUnitTypeDisplay { get; set; }
        /// <summary>
        /// DisplayType; Display type is used for determining how to display the item, this can be a chosen icon, or a way of displaying. (e.g. SquareGrey)
        /// </summary>
        public PropertyDisplayTypeEnum? DisplayType { get; set; } //square (default), custom icon, structure
        /// <summary>
        /// Type; Type of the property, currently 2 types aer supported. One is display only, the other one is input.
        /// When display only, no user action is needed, can be used for just displaying an icon.
        /// </summary>
        public PropertyTypeEnum Type { get; set; }
        /// <summary>
        /// Unit; Unit of measurement that can be used e.g. celcius, meters etc.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public MeasurementUnit Unit { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete
        /// <summary>
        /// UnitKind; Kind of measurement that should be done e.g. temperature, meters etc.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public MeasurementUnitKind UnitKind { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete
        /// <summary>
        ///
        /// </summary>
        public PropertyValueKind PropertyValueKind { get; set; }
        /// <summary>
        ///
        /// </summary>
        public PropertyValue PropertyValue { get; set; }
        /// <summary>
        /// ResourceKeyName; Resource key can be used if its a general property that is used throughout the client apps. This will be the key of a item in the language table.
        /// </summary>
        public string ResourceKeyName { get; set; }
        /// <summary>
        /// ResourceKeyShortName; Resource key can be used if its a general property that is used throughout the client apps. This will be the key of a item in the language table.
        /// </summary>
        [ObsoleteAttribute("This property is obsolete.", false)]
        public string ResourceKeyShortName { get; set; }
        /// <summary>
        /// IsCustomerSpecific; Property specific for one customer (customer only). These property are only added based on the company id.
        /// NOTE! a customer specific property can only be used for 1 customer or 1 company account if they have multiple companies. This is not the same as a normal property which is only added to a specific company (which can also occure but this is a licencing thing)
        /// </summary>
        public bool IsCustomerSpecific { get; set; }
        /// <summary>
        /// IsSystem; System property, will be used for certain specific functionality or is supplied by ez-factory, these properties should also have a resource key and possible extended values;
        /// </summary>
        public bool IsSystem { get; set; }
        /// <summary>
        /// CreatedAt;
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt;
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}
