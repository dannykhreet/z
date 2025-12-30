using EZGO.Api.Models.Basic;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Checklist; Checklist is executed by a user. Checklists are based on ChecklistTemplates. 
    /// Database location: [checklists_checklist].
    /// </summary>
    public class Checklist
    {
        #region - fields -
        /// <summary>
        /// Primary key, in other variables and objects usually named as ChecklistId. DB: [checklists_checklist.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [checklists_checklist.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// TemplateId; TemplateId of the ChecklistTemplate where this checklist is based on. DB: [checklists_checklist.template_id]
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// IsCompleted; Boolean if checklist is completed; DB: [checklists_checklist.is_completed]
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Name; Name of checklist. Name is located with the template. DB: [checklists_checklisttemplate.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description; Description of checklist. Description is located with the template. DB: [checklists_checklisttemplate.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Picture; Picture of checklist. Uri part. Picture is located with the template. DB: [checklists_checklisttemplate.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// IsDoubleSignatureRequired; Bool if a double signature is required, part of template. Note of double signature is required, signature required must also be filled with true. DB: [checklists_checklisttemplate.double_signature_required]
        /// </summary>
        public bool IsDoubleSignatureRequired { get; set; }
        /// <summary>
        /// IsSignatureRequired; Bool if a signature is required, part of template. DB: [checklists_checklisttemplate.signature_required]
        /// </summary>
        public bool IsSignatureRequired { get; set; }
        /// <summary>
        /// AreaId; Checklist is linked to this area through its template. DB: [checklists_checklisttemplate.area_id]
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// Signatures; Signature objects, filled when a audit is completed. Based on the signature fields in the db with a checklist record.
        /// </summary>
        public List<Signature> Signatures { get; set; }
        /// <summary>
        /// Tasks; Collection of task objects that are done with a checklist. 
        /// </summary>
        public List<TasksTask> Tasks { get; set; }
        /// <summary>
        /// CreatedAt; Technical created at date time. DB: [checklists_checklist.created_at]
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; Technical modified at date time. DB: [checklists_checklist.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// CreatedBy; Contains the username of the user that created this checklist (NOTE: Can be null)
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// ModifiedBy; Contains the username of the user that modified this checklist (NOTE: Can be null)
        /// </summary>
        public string ModifiedBy { get; set; }
        /// <summary>
        /// CreatedById; Contains the user id of the user that created this checklist (NOTE: Can be null)
        /// </summary>
        public int? CreatedById { get; set; }
        /// <summary>
        /// ModifiedById; Contains the user id of the user that modified this checklist (NOTE: Can be null)
        /// </summary>
        public int? ModifiedById { get; set; }
        /// <summary>
        /// CreatedByUser; Contains basic information about the user that created the checklist
        /// </summary>
        public UserBasic CreatedByUser { get; set; }
        /// <summary>
        /// ModifiedByUser; Contains basic information about the user that modified the checklist
        /// </summary>
        public UserBasic ModifiedByUser { get; set; }
        /// <summary>
        /// EditedByUsers; Contains a list of users that have worked on the checklist.
        /// </summary>
        public List<UserBasic> EditedByUsers { get; set; }
        /// <summary>
        /// AreaPath; AreaPath, consists of the path of areas based on the AreaId with the template. e.g. "Some Area 1 -> Some Area 1.1 -> Some Area 1.1.1"; Used for display purposes in the apps. 
        /// </summary>
        public string AreaPath { get; set; }
        /// <summary>
        /// AreaPathIds; AreaPathIds, consists of the ids of areas based on the AreaId with the template. e.g. "1 -> 2 -> 3"; Used for filter purposes in the apps
        /// </summary>
        public string AreaPathIds { get; set; }
        /// <summary>
        /// Properties; List of properties that can be filled in with a checklist. 
        /// </summary>
        public List<PropertyChecklistTemplate> Properties { get; set; }
        /// <summary>
        /// PropertyUserValues; List of values that are filled in by a user based on the property value structure.
        /// </summary>
        public List<PropertyUserValue> PropertyUserValues { get; set; }
        /// <summary>
        /// OpenFieldsProperties; List of open field properties that can be filled in with a checklist. 
        /// </summary>
        public List<PropertyChecklistTemplate> OpenFieldsProperties { get; set; }
        /// <summary>
        /// OpenFieldsPropertyUserValues; List of values that are filled in by a user based on the property value structure of the open fields.
        /// </summary>
        public List<PropertyUserValue> OpenFieldsPropertyUserValues { get; set; }
        /// <summary>
        /// PropertiesGen4 is a list of simplified properties for easier use in the frontend of the ezgo platform.
        /// </summary>
        public List<PropertyDTO> PropertiesGen4 { get; set; }
        /// <summary>
        /// OpenFieldsPropertiesGen4 is a list of simplified properties for easier use in the frontend of the ezgo platform.
        /// </summary>
        public List<PropertyDTO> OpenFieldsPropertiesGen4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        ///
        public List<Stage> Stages { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this checklist's template
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// LinkedTaskId; The task id of the task this checklist is linked to
        /// </summary>
        public long? LinkedTaskId { get; set; }
        /// <summary>
        /// IsRequiredForLinkedTask; Indicates if the checklist is required for completing the linked task at the time of completion
        /// </summary>
        public bool? IsRequiredForLinkedTask { get; set; }
        /// <summary>
        /// Version; The version of the template this object is based on. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
        #endregion
        #region - constructor(s) -
        public Checklist()
        {
        }

        public Checklist(bool preInit)
        {
            if(preInit)
            {
                Tasks = new List<TasksTask>();
                Signatures = new List<Signature>();
            }
        }
        #endregion

    }
}
