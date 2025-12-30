using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyTaskTemplate; Property specifically for task templates.
    /// </summary>
    public class PropertyTaskTemplate : BasePropertyObject
    {
        public int TaskTemplateId { get; set; }
        public bool IsActive { get; set; }
    }
}
