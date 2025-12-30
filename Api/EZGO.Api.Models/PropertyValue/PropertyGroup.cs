using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyGroup; Group of property objects. 
    /// DB: [propertyvaluegroup]
    /// </summary>
    public class PropertyGroup
    {
        public int Id { get; set; }
        public List<Property> Properties { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public PropertyGroup()
        {
            Properties = new List<Property>();
        }
    }
}
