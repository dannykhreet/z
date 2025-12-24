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
    /// TaskTemplate; Template for a Task, depending on the template and the recurrency Task data is handled differently.
    /// Database location: [tasks_tasktemplate].
    /// </summary>
    public class TaskTemplate
    {
        #region - fields - 
        /// <summary>
        /// Primary key, in other variables and objects usually named as TemplateId or TaskTemplateId.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ChecklistTemplateId; Possible linked ChecklistTemplates (if task is a checklist item)
        /// </summary>
        public int? ChecklistTemplateId { get; set; }
        /// <summary>
        /// AuditTemplateId; Possible linked AuditTemplates (if task is a audit item)
        /// </summary>
        public int? AuditTemplateId { get; set; }
        /// <summary>
        /// Name; Name of task. DB: [tasks_tasktemplate.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// AreaId; Task is linked to this area. DB: [tasks_tasktemplate.area_id]
        /// </summary>
        public int? AreaId { get; set; }
        /// <summary>
        /// Index; Index, sort order; DB: [tasks_tasktemplate.index]
        /// </summary>
        public int? Index { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [tasks_tasktemplate.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Picture; Picture of task. Uri part. DB: [tasks_tasktemplate.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// Description; Description of audit.  DB: [tasks_tasktemplate.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// DescriptionFile; DescriptionFile uri to a pdf.  DB: [tasks_tasktemplate.description_file]
        /// </summary>
        public string DescriptionFile { get; set; }
        /// <summary>
        /// MachineStatus; Machine status; located with template.
        /// - running
        /// - stopped
        /// - not_applicable
        /// DB: [tasks_tasktemplate.machine_status]
        /// </summary>
        public string MachineStatus { get; set; }
        /// <summary>
        /// PlannedTime; Planned time, time in minutes. DB: [tasks_tasktemplate.planned_time]
        /// </summary>
        public int? PlannedTime { get; set; }
        /// <summary>
        /// Type; Type of task. Uri part.
        /// - checklist
        /// - audit
        /// - task
        /// DB: [tasks_tasktemplate.type]
        /// </summary>
        public string Type { get; set; } //TODO create ENUM
        /// <summary>
        /// Role; Role of the template. Used in display filtering. 
        /// - manager
        /// - basic
        /// - shift_leader
        /// DB: [tasks_tasktemplate.role]
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Video; Video of task. Uri part.  DB: [tasks_tasktemplate.video]
        /// </summary>
        public string Video { get; set; }
        /// <summary>
        /// VideoThumbnail; VideoThumbnail of task. Uri part. DB: [tasks_tasktemplate.video_thumbnail]
        /// </summary>
        public string VideoThumbnail { get; set; }
        /// <summary>
        /// Weight; Weight is used with audit items. DB: [tasks_tasktemplate.weight]
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// ActionsCount; Number of linked actions with this task.
        /// </summary>
        public int? ActionsCount { get; set; }
        /// <summary>
        /// ActionCollection; Actions linked to this task.
        /// </summary>
        public List<ActionsAction> Actions { get; set; }
        /// <summary>
        /// Steps; Steps are part of the template of a Task, if a Template has no Steps this will not be filled.
        /// </summary>
        public int? StepsCount { get; set; }
        /// <summary>
        /// Steps; Steps are part of the template of a Task, if a Template has no Steps this will not be filled.
        /// </summary>
        public List<Step> Steps { get; set; }
        /// <summary>
        /// Recurrency; Recurrency object of the task; Based on the tasks_taskrecurrency table.
        /// </summary>
        public TaskRecurrency Recurrency { get; set; }
        /// <summary>
        /// RecurrencyType; Type of task recurrency; 
        /// - month
        /// - no recurrency
        /// - shifts
        /// - week
        /// DB: [tasks_taskrecurrency.type]
        /// </summary>
        public string RecurrencyType { get; set; }
        /// <summary>
        /// DeepLinkId; Linked object, checklist, audit; DB: [tasks_tasktemplate.deeplink_id]
        /// </summary>
        public int? DeepLinkId { get; set; }
        /// <summary>
        /// DeepLinkTo; Linked object, checklist, audit; DB: [tasks_tasktemplate.deeplink_to]
        /// </summary>
        public string DeepLinkTo { get; set; }
        /// <summary>
        /// DeepLinkCompletionIsRequired; Indicates if completion of the linked object (checklist or audit) is required before the task can be completed
        /// </summary>
        public bool? DeepLinkCompletionIsRequired { get; set; }
        /// <summary>
        /// AreaPath; AreaPath, consists of the path of areas based on the AreaId with the template. e.g. "Some Area 1 -> Some Area 1.1 -> Some Area 1.1.1"; Used for display purposes in the apps. 
        /// </summary>
        public string AreaPath { get; set; }
        /// <summary>
        /// AreaPathIds; AreaPathIds, consists of the ids of areas based on the AreaId with the template. e.g. "1 -> 2 -> 3"; Used for filter purposes in the apps
        /// </summary>
        public string AreaPathIds { get; set; }
        /// <summary>
        /// Properties; List of properties that can be filled in with a task. 
        /// </summary>
        public List<PropertyTaskTemplate> Properties { get; set; }
        /// <summary>
        /// PropertyUserValues; List of values that are filled in by a user based on the property value structure.
        /// </summary>
        public List<PropertyUserValue> PropertyValues { get; set; }
        /// <summary>
        /// PropertiesGen4 is a list of simplified properties for easier use in the frontend of the ezgo platform.
        /// </summary>
        public List<PropertyDTO> PropertiesGen4 { get; set; }

        /// <summary>
        /// HasDerivedItems; Has derived items (tasks) that already have executed. 
        /// </summary>
        public bool? HasDerivedItems { get; set; }
        /// <summary>
        /// HasPictureProof; Has picture proof that is required to complete the task. 
        /// </summary>
        public bool HasPictureProof { get; set; }
        /// <summary>
        /// ModifiedAt; DateTime the UserProfile was last modified. DB: [tasks_tasktemplate.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// WorkInstructions; WorkInstructions
        /// </summary>
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<TaskTemplateRelationWorkInstructionTemplate> WorkInstructionRelations { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this task template
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// Attachments; Attachments that are added to this task template. JSON in the database [tasks_tasktemplate.attachments]
        /// </summary>
        public List<Attachment> Attachments { get; set; }
        /// <summary>
        /// SharedTemplateId; Used in new templates when they are based on a shared template.
        /// </summary>
        public int? SharedTemplateId { get; set; }
        /// <summary>
        /// Version; The version of this instance of the object. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
        #endregion

        #region - constructor(s) -
        public TaskTemplate()
        {

        }
        #endregion
    }
}
