using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using WebApp.Models.Properties;

namespace WebApp.Models.Audit
{
    public class AuditTaskTemplatesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DescriptionFile { get; set; }
        public string Description { get; set; }
        public int StepsCount { get; set; }
        public int CompanyId { get; set; }
        public string Picture { get; set; }
        public string Video { get; set; }
        public string VideoThumbnail { get; set; }
        public bool isNew { get; set; }
        public decimal Weight { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public bool HasPictureProof { get; set; }
        public List<TemplatePropertyModel> Properties { get; set; }
        public List<TaskTemplateRelationWorkInstructionTemplate> WorkInstructionRelations { get; set; }
        public List<AuditStepModel> Steps { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}
