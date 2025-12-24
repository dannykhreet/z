using EZGO.Api.Models.Basic;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models
{

    /// <summary>
    /// TaskTasks; Task object (named TasksTask due to usage of the Threading.Task object within .Net)
    /// Database location: [tasks_task].
    /// </summary>
    public class TasksTask
    {
        #region - fields -
        /// <summary>
        /// Primary key, in other variables and objects usually named as TaskId. DB: [tasks_task.id]
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// ChecklistId; Possible linked Checklists (if task is a checklist item)
        /// </summary>
        public int? ChecklistId { get; set; }
        /// <summary>
        /// AuditId; Possible linked Audit (if task is a audit item)
        /// </summary>
        public int? AuditId { get; set; }
        /// <summary>
        /// AreaId; Task is linked to this area through its template. DB: [tasks_task.area_id]
        /// </summary>
        public int? AreaId { get; set; }
        /// <summary>
        /// Status; Task status
        /// - todo
        /// - done
        /// - skipped
        /// - not ok
        /// DB: [tasks_task.status]
        /// </summary>
        public string Status { get; set; } //enum
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [tasks_task.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// RecurrencyId; RecurrencyId if the task has a recurrency (only for tasks, not for audit/checklist items). DB: [tasks_task.recurrency_id]
        /// </summary>
        public int? RecurrencyId { get; set; }
        /// <summary>
        /// TemplateId; TemplateId of the template where the task is based on.  DB: [tasks_task.template_id]
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// Name; Name of task. Name is located with the template. DB: [tasks_tasktemplate.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Comment; Comment of task.  DB: [tasks_tasks.comment]
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Description; Description of audit. Description is located with the template. DB: [tasks_tasktemplate.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// DescriptionFile; DescriptionFile uri to a pdf.  DB: [tasks_tasktemplate.description_file]
        /// </summary>
        public string DescriptionFile { get; set; }
        /// <summary>
        /// Score; Score filled in by user; DB: [tasks_task.score]
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// EndDate; EndDate of the task; DB: [tasks_task.end_date]
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// StartDate; StartDate of the task; DB: [tasks_task.start_date]
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// DueAt; DueAt DateTime of the task; DB: [tasks_task.due_at]
        /// </summary>
        public DateTime? DueAt { get; set; }
        /// <summary>
        /// StartAt; StartAt DateTime of the task; DB: [tasks_task.start_at]
        /// </summary>
        public DateTime? StartAt { get; set; }
        /// <summary>
        /// shift_id; Linked shift id of task; DB: [tasks_task.shift_id]
        /// </summary>
        public int? ShiftId { get; set; }
        /// <summary>
        /// Deviance; Deviance for audit task score system;  DB: [tasks_task.diviance]
        /// </summary>
        public int? Deviance { get; set; }
        /// <summary>
        /// MaxScore; MaxScore for audit task score system;  DB: [tasks_task.maxscore]
        /// </summary>
        public int? MaxScore { get; set; }
        /// <summary>
        /// TotalScore; TotalScore for audit task score system;  DB: [tasks_task.totalscore]
        /// </summary>
        public int? TotalScore { get; set; }
        /// <summary>
        /// Picture; Picture of task. Uri part. Picture is located with the template. DB: [tasks_tasktemplate.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// TaskType; TaskType of task. Uri part. TaskType is located with the template. 
        /// - checklist
        /// - audit
        /// - task
        /// DB: [tasks_tasktemplate.type]
        /// </summary>
        public string TaskType { get; set; }
        /// <summary>
        /// RecurrencyType; Type of task recurrency; RecurrenceType is located in the recurrency. 
        /// - month
        /// - no recurrency
        /// - shifts
        /// - week
        /// DB: [tasks_taskrecurrency.type]
        /// </summary>
        public string RecurrencyType { get; set; }
        /// <summary>
        /// Signatures; Signature objects, filled when a task is completed. Based on the signature fields in the db with a task record.
        /// </summary>
        public Signature Signature { get; set; }
        /// <summary>
        /// Template; Template object of the Task; Based on the tasks_tasktemplate table.
        /// </summary>
        public TaskTemplate Template { get; set; }
        /// <summary>
        /// Recurrency; Recurrency object of the task; Based on the tasks_taskrecurrency table.
        /// </summary>
        public TaskRecurrency Recurrency { get; set; }
        /// <summary>
        /// TaskSteps; TasksSteps collection of tasks; Based on the tasks_tasktemplatesteps table.
        /// </summary>
        public List<Step> TaskSteps { get; set; }
        /// <summary>
        /// CreatedAt; Technical created at date time. DB: [tasks_task.created_at]
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; Technical modified at date time. DB: [tasks_task.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// AreaPath; AreaPath, consists of the path of areas based on the AreaId with the template. e.g. "Some Area 1 -> Some Area 1.1 -> Some Area 1.1.1"; Used for display purposes in the apps. 
        /// </summary>
        public string AreaPath { get; set; }
        /// <summary>
        /// AreaPathIds; AreaPathIds, consists of the ids of areas based on the AreaId with the template. e.g. "1 -> 2 -> 3"; Used for filter purposes in the apps
        /// </summary>
        public string AreaPathIds { get; set; }
        /// <summary>
        /// ActionsCount; Number of linked actions with this task.
        /// </summary>
        public int? ActionsCount { get; set; }
        /// <summary>
        /// OpenTemplateActionsCount; Number of linked actions with this tasktemplate that are unresolved.
        /// </summary>
        public int? OpenTemplateActionsCount { get; set; }
        /// <summary>
        /// ActionCollection; Actions linked to this task.
        /// </summary>
        public List<ActionsAction> Actions { get; set; }
        /// <summary>
        /// Steps; Steps are part of the template of a Task, if a Template has not Steps this will not be filled.
        /// </summary>
        public List<Step> Steps { get; set; }
        /// <summary>
        /// Index; Index, sort order, located with template; DB: [tasks_tasktemplate.index]
        /// </summary>
        public int? Index { get; set; }
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
        /// CompletedDeeplinkId; Id of the completed checklist or audit;
        /// </summary>
        public int? CompletedDeeplinkId { get; set; }
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
        /// TimeRealizedById; Time relized by id, UserId. DB: [tasks_task.realized_by_id]
        /// </summary>
        public int? TimeRealizedById { get; set; }
        /// <summary>
        /// TimeRealizedById; Name of the realized by id.
        /// </summary>
        public string TimeRealizedBy { get; set; }
        /// <summary>
        /// TimeTaken; Time taken in minutes. 
        /// </summary>
        public int? TimeTaken { get; set; }
        /// <summary>
        /// Video; Video of task. Uri part. Picture is located with the template. DB: [tasks_tasktemplate.video]
        /// </summary>
        public string Video { get; set; }
        /// <summary>
        /// VideoThumbnail; VideoThumbnail of task. Uri part. VideoThumbnail is located with the template. DB: [tasks_tasktemplate.video_thumbnail]
        /// </summary>
        public string VideoThumbnail { get; set; }
        /// <summary>
        /// Properties; List of properties that can be filled in with a task. 
        /// </summary>
        public List<PropertyTaskTemplate> Properties { get; set; }
        /// <summary>
        /// PropertyUserValues; List of values that are filled in by a user based on the property value structure.
        /// </summary>
        public List<PropertyUserValue> PropertyUserValues { get; set; }
        /// <summary>
        /// PropertiesGen4 is a list of simplified properties for easier use in the frontend of the ezgo platform.
        /// </summary>
        public List<PropertyDTO> PropertiesGen4 { get; set; }
        /// <summary>
        /// EditedByUsers; Contains a list of users that have worked on the checklist item.
        /// </summary>
        public List<UserBasic> EditedByUsers { get; set; }
        /// <summary>
        /// PictureProof; The PictureProof object with info about picture proof
        /// </summary>
        public PictureProof PictureProof { get; set; }
        /// <summary>
        /// CommentCount; Number of attached comments.
        /// </summary>
        public int? CommentCount { get; set; }
        /// <summary>
        /// Comments; Comments collection attached to task.
        /// </summary>
        public List<Comment> Comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<TaskTemplateRelationWorkInstructionTemplate> WorkInstructionRelations { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }

        /// <summary>
        /// This boolean indicates whether picture proof is configured for this task
        /// </summary>
        public bool HasPictureProof { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this task
        /// </summary>
        public List<Tag> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }
        /// <summary>
        /// Version; The version of this instance of the object. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
        #endregion

        #region - constructor(s) -
        public TasksTask()
        {

        }
        #endregion

    }
}
