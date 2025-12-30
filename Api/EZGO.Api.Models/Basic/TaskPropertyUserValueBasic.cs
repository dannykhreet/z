using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// TaskPropertyUserValueBasic; Basic object used within other classes.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class TaskPropertyUserValueBasic
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int? TaskId { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// TemplatePropertyId; Directly references template property id (e.g. tasks_tasktemplate_properties.id or checklists_checklisttemplate_properties.id or audits_auditstemplate_properties.id).
        /// </summary>
        public int TemplatePropertyId { get; set; }
        public int? UserValueInt { get; set; }
        public string UserValueString { get; set; }
        public decimal? UserValueDecimal { get; set; }
        public string UserValueTime { get; set; }
        public DateTime? UserValueDate { get; set; }
        public bool? UserValueBool { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }
    }
}
