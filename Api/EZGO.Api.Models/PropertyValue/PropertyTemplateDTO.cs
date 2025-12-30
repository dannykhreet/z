using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// Simplified property object to use as a data transfer object for the template related data for properties
    /// </summary>
    public class PropertyTemplateDTO
    {
        /// <summary>
        /// Id of the property
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Property id
        /// </summary>
        public int PropertyId { get; set; }
        /// <summary>
        /// Name to display for this property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Footer to display for this property
        /// </summary>
        public string Footer { get; set; }
        /// <summary>
        /// Unit to display
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// Symbol to use for the unit
        /// </summary>
        public string UnitSymbol { get; set; }
        /// <summary>
        /// Integer = 0,
        /// Decimal = 1,
        /// String = 2,
        /// Date = 3,
        /// Time = 4,
        /// DateTime = 5,
        /// Boolean = 6
        /// </summary>
        public PropertyValueTypeEnum ValueType { get; set; }
        /// <summary>
        /// Default value to display
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// True if the property is mandatory
        /// </summary>
        public bool IsMandatory { get; set; }
        /// <summary>
        /// Optional. Expected user value is exactly this
        /// </summary>
        public string TargetValue { get; set; }
        /// <summary>
        /// Optional. Expected user value is lower then this upper limit
        /// </summary>
        public string UpperValueLimit { get; set; }
        /// <summary>
        /// Optional. Expected user value is higher then this lower limit
        /// </summary>
        public string LowerValueLimit { get; set; }
        /// <summary>
        /// Custom = 0,
        /// SingleValue = 1,
        /// Range = 2,
        /// UpperLimit = 3,
        /// LowerLimit = 4,
        /// EqualTo = 5,
        /// UpperLimitEqualTo = 6, //not yet implemented
        /// LowerLimitEqualTo = 7 //not yet implemented
        /// </summary>
        public PropertyFieldTypeEnum? FieldType { get; set; }
        /// <summary>
        /// Index of property
        /// </summary>
        public int Index { get; set; }
    }
}
