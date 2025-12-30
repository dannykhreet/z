using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// ChecklistTemplate; Checklists are based on a checklist template. A checklist template has all the source information to generate a checklist. Checklist templates can be managed through the management portals. 
    /// Database location: [checklists_checklisttemplate]
    /// </summary>
    public class ChecklistTemplate
    {
        #region - fields -
        /// <summary>
        /// Primary key, in other variables and objects usually named as ChecklistTemplateId. DB: [checklists_checklisttemplate.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name; Name of checklist. DB: [checklists_checklisttemplate.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description; Description of checklist. DB: [checklists_checklisttemplate.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Picture; Picture of a checklist, uri part. DB: [checklists_checklisttemplate.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// AreaId; AreaId linked to this template. DB: [checklists_checklisttemplate.area_id]
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// IsDoubleSignatureRequired; Bool if a double signature is required. Note of double signature is required, signature required must also be filled with true. DB: [checklists_checklisttemplate.double_signature_required]
        /// </summary>
        public bool IsDoubleSignatureRequired { get; set; }
        /// <summary>
        /// IsSignatureRequired; Bool if a signature is required, part of template. DB: [checklists_checklisttemplate.signature_required]
        /// </summary>
        public bool IsSignatureRequired { get; set; }
        /// <summary>
        /// Role; Role of the template. Used in display filtering. 
        /// - manager
        /// - basic
        /// - shift_leader
        /// </summary>
        public string Role { get; set; } //create enum
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [checklists_checklisttemplate.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// HasIncompleteChecklists; Bool, check if checklists templates has incomplete checklists. 
        /// </summary>
        public bool? HasIncompleteChecklists { get; set; }
        /// <summary>
        /// Stages; Collection of Stages of the checklist template. Stages contain tasktemplates and may add functionality to checklists. For example, stages may need to be completed in a certain order, can be individually signed and/or can not be altered after signing.
        /// </summary>
        public List<StageTemplate> StageTemplates { get; set; }
        /// <summary>
        /// TaskTemplates; Collection of tasktemplates objects that must be executed with a new checklist. 
        /// </summary>
        public List<TaskTemplate> TaskTemplates { get; set; }
        /// <summary>
        /// Properties; List of properties that can be filled in with a checklist. 
        /// </summary>
        public List<PropertyChecklistTemplate> Properties { get; set; }
        /// <summary>
        /// OpenFieldsProperties; List of open field properties that can be filled in with a checklist. 
        /// </summary>
        public List<PropertyChecklistTemplate> OpenFieldsProperties { get; set; }
        /// <summary>
        /// PropertiesGen4 is a list of simplified properties for easier use in the frontend of the ezgo platform.
        /// </summary>
        public List<PropertyDTO> OpenFieldsPropertiesGen4 { get; set; }
        /// <summary>
        /// AreaPath; AreaPath, consists of the path of areas based on the AreaId with the template. e.g. "Some Area 1 -> Some Area 1.1 -> Some Area 1.1.1"; Used for display purposes in the apps. 
        /// </summary>
        public string AreaPath { get; set; }
        /// <summary>
        /// AreaPathIds; AreaPathIds, consists of the ids of areas based on the AreaId with the template. e.g. "1 -> 2 -> 3"; Used for filter purposes in the apps
        /// </summary>
        public string AreaPathIds { get; set; }
        /// <summary>
        /// HasDerivedItems; Has derived items (checklists) that already have executed. 
        /// </summary>
        public bool? HasDerivedItems { get; set; }
        /// <summary>
        /// ModifiedAt; DateTime the UserProfile was last modified. DB: [checklists_checklisttemplate.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ChecklistTemplateRelationWorkInstructionTemplate> WorkInstructionRelations { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this checklist template
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// SharedTemplateId; Used in new templates when they are based on a shared template.
        /// </summary>
        public int? SharedTemplateId { get; set; }
        /// <summary>
        /// Version; The version of this instance of the object. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// True when the template instance has stages loaded. Property added for convenience.
        /// </summary>
        public bool HasStages { get { return StageTemplates != null && StageTemplates.Count > 0;  } }
        /// <summary>
        /// True when the template contains stages in the DB. Property added for convenience.
        /// </summary>
        public bool ContainsStages { get; set; }
        #endregion

        #region - constructor(s) -
        public ChecklistTemplate()
        {

        }
        #endregion
    }
}
