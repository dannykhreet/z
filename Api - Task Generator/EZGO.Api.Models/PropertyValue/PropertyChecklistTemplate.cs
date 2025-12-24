using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyChecklistTemplate; Specific property for checklists.
    /// </summary>
    public class PropertyChecklistTemplate : BasePropertyObject
    {
        public int ChecklistTemplateId { get; set; }
    }
}
