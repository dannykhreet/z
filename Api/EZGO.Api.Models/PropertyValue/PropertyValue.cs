using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyValue; Property value. Usually part of a property and used as a choice with a property item.
    /// DB: [propertyvalue]
    /// </summary>
    public class PropertyValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitSymbol { get; set; }
        public string UnitAbbreviation { get; set; }
        public string ValueSymbol { get; set; }
        public string ValueAbbreviation { get; set; }
        public string ResourceKeyName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        //public bool IsSystem { get; set; }
        public int PropertyValueKindId { get; set; }
        public PropertyValueTypeEnum DefaultValueType { get; set; }
        public PropertyValue()
        {

        }
    }
}
