using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyAuditTemplate; Specific property for audits.
    /// </summary>
    public class PropertyAuditTemplate : BasePropertyObject
    {
        public int AuditTemplateId { get; set; }
    }
}
