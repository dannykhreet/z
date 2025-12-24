using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// Simplified property object to be used for transfering data about properties.
    /// All data regarding properties that is used by the Gen4 client should be in this data transfer object.
    /// </summary>
    public class PropertyDTO
    {
        /// <summary>
        /// Template info for this property
        /// </summary>
        public PropertyTemplateDTO PropertyTemplate { get; set; }
        /// <summary>
        /// User value info for this property
        /// </summary>
        public PropertyUserValueDTO UserValue { get; set; }
    }
}
