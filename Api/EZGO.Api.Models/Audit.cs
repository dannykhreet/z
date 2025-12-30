using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Audit; Audit is executed by a user. Based on the tasks of a audit a score is calculated. Audits are based on AuditTemplates. 
    /// Database location: [audits_audit].
    /// </summary>
    public class Audit
    {
        #region - fields -
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as AuditId. DB: [audits_audit.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// AreaId; Audit is linked to this area through its template. DB: [audits_audittemplate.area_id]
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [audits_audit.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// IsCompleted; Boolean if audit is completed; DB: [audits_audit.is_completed]
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        ///  TotalScore; Based on the scores of all tasks items; DB: [audits_audit.total_score]
        /// </summary>
        public int TotalScore { get; set; }
        /// <summary>
        /// MinTaskScore; Minimal task score that can be used; This is stored with the AuditTemplate. DB: [audits_audittemplate.min_task_score]
        /// </summary>
        public int? MinTaskScore { get; set; } //part of template
        /// <summary>
        /// MaxTaskScore; Maximal task score that can be used; This is stored with the AuditTemplate. DB: [audits_audittemplate.max_task_score]
        /// </summary>
        public int? MaxTaskScore { get; set; } //part of template
        /// <summary>
        /// TemplateId; TemplateId of the AuditTemplate where this audit is based on. DB: [audits_audit.template_id]
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// Name; Name of audit. Name is located with the template. DB: [audits_audittemplate.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description; Description of audit. Description is located with the template. DB: [audits_audittemplate.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Picture; Picture of audit. Uri part. Picture is located with the template. DB: [audits_audittemplate.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// ScoreType; Type of scoring used with the tasks. DB: [audits_audittemplate.score_type]
        /// - thumbs
        /// - score
        /// Thumbs will use a thumb up, thumb down and skipped option
        /// Score will use a score choice based on the range of min/max task score values.
        /// </summary>
        public string ScoreType { get; set; }
        /// <summary>
        /// IsDoubleSignatureRequired; Bool if a double signature is required, part of template. Note of double signature is required, signature required must also be filled with true. DB: [audits_audittemplate.double_signature_required]
        /// </summary>
        public bool IsDoubleSignatureRequired { get; set; }
        /// <summary>
        /// IsSignatureRequired; Bool if a signature is required, part of template. DB: [audits_audittemplate.signature_required]
        /// </summary>
        public bool IsSignatureRequired { get; set; }
        /// <summary>
        /// Signatures; Signature objects, filled when a audit is completed. Based on the signature fields in the db with a audit record.
        /// </summary>
        public List<Signature> Signatures { get; set; }
        /// <summary>
        /// Tasks; Collection of task objects that are done with a audit. 
        /// </summary>
        public List<TasksTask> Tasks { get; set; }
        /// <summary>
        /// CreatedAt; Technical created at date time. DB: [audits_audit.created_at]
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; Technical modified at date time. DB: [audits_audit.modified_at]
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
        /// Properties; List of properties that can be filled in with a audit. 
        /// </summary>
        public List<PropertyAuditTemplate> Properties { get; set; }
        /// <summary>
        /// PropertyUserValues; List of values that are filled in by a user based on the property value structure.
        /// </summary>
        public List<PropertyUserValue> PropertyUserValues { get; set; }
        /// <summary>
        /// OpenFieldsProperties; List of open field properties that can be filled in with a audit. 
        /// </summary>
        public List<PropertyAuditTemplate> OpenFieldsProperties { get; set; }
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
        /// <summary>
        /// Tags; Tags that are added to this audit's template
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// LinkedTaskId; The task id of the task this audit is linked to
        /// </summary>
        public long? LinkedTaskId { get; set; }
        /// <summary>
        /// IsRequiredForLinkedTask; Indicates if the audit is required for completing the linked task at the time of completion
        /// </summary>
        public bool? IsRequiredForLinkedTask { get; set; }
        /// <summary>
        /// Version; The version of this instance of the object. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
        #endregion
        #region - constructor(s) - 
        /// <summary>
        /// Audit contructor.
        /// </summary>
        public Audit()
        {
            Signatures = new List<Signature>();

        }
        #endregion
    }
}
