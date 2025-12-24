using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyValueKind; Kind of property, configured in database.
    /// DB: [propertyvaluekind]
    /// </summary>
    public class PropertyValueKind
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PropertyValue> PropertyValues { get; set; }
        public string ResourceKeyName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
