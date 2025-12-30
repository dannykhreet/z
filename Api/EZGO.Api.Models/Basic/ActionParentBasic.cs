using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// ActionParentBasic; Basic item for use with actions. 
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class ActionParentBasic
    {
        public int? ActionId { get; set; }
        public int? AuditId { get; set; }
        public int? ChecklistId { get; set; }
        public int? AuditTemplateId { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public int? TaskId { get; set; }
        public int? TaskTemplateId { get; set; }
        public string TaskName { get; set; }
        public string ChecklistTemplateName { get; set; }
        public string AuditTemplateName { get; set; }
        public string Type { get; set; }
    }
}
