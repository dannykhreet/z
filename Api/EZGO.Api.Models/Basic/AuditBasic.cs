using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// AuditBasic; Basic audit object used within other classes.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class AuditBasic
    {
        public int AuditId { get; set; }
        public int AuditTemplateId { get; set; }
        public int Name { get; set; }
    }
}
