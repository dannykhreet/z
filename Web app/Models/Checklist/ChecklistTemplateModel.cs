using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using WebApp.Models.Properties;

namespace WebApp.Models.Checklist
{
    public class ChecklistTemplateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Role { get; set; }
        public string AreaPath { get; set; }
        public int AreaId { get; set; }
        public bool IsDoubleSignatureRequired { get; set; }
        public bool IsSignatureRequired { get; set; }
        public int CompanyId { get; set; }
        public bool HasIncompleteChecklists { get; set; }
        public bool isNew { get; set; }
        public string AreaPathIds { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public ApplicationSettings ApplicationSetttings { get; set; }
        public List<StageTemplateModel> StageTemplates { get; set; }
        public List<ChecklistTaskTemplatesModel> TaskTemplates { get; set; }
        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }
        public List<Tag> Tags { get; set; }
        public int? SharedTemplateId { get; set; }
    }
}
