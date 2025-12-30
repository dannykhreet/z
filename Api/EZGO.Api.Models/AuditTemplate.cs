using EZGO.Api.Models.Enumerations;
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
    /// AuditTemplate; Audits are based on a audit template. A audit template has all the source information to generate a audit. Audit templates can be managed through the management portals. 
    /// Database location: [audits_audittemplate].
    /// </summary>
    public class AuditTemplate
    {
        #region - fields -
        /// <summary>
        /// Primary key, in other variables and objects usually named as AuditTemplateId. DB: [audits_audittemplate.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name; Name of audit. DB: [audits_audittemplate.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description; Description of audit. DB: [audits_audittemplate.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Picture; Picture of a audit, uri part. DB: [audits_audittemplate.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// HasIncompleteAudits; Bool, check if audit templates has incomplete audits. 
        /// </summary>
        public bool? HasIncompleteAudits { get; set; }
        /// <summary>
        /// IsDoubleSignatureRequired; Bool if a double signature is required. Note of double signature is required, signature required must also be filled with true. DB: [audits_audittemplate.double_signature_required]
        /// </summary>
        public bool IsDoubleSignatureRequired { get; set; }
        /// <summary>
        /// IsSignatureRequired; Bool if a signature is required, part of template. DB: [audits_audittemplate.signature_required]
        /// </summary>
        public bool IsSignatureRequired { get; set; }
        /// <summary>
        /// ScoreType; Type of scoring used with the tasks. DB: [audits_audittemplate.score_type]
        /// - thumbs
        /// - score
        /// Thumbs will use a thumb up, thumb down and skipped option
        /// Score will use a score choice based on the range of min/max task score values.
        /// </summary>
        public string ScoreType { get; set; } //create enum
        /// <summary>
        /// MinTaskScore; Minimal task score that can be used; DB: [audits_audittemplate.min_task_score]
        /// </summary>
        public int? MinScore { get; set; }
        /// <summary>
        /// MaxTaskScore; Maximal task score that can be used; DB: [audits_audittemplate.max_task_score]
        /// </summary>
        public int? MaxScore { get; set; }
        /// <summary>
        /// Score; Score of the template. This is a general score based on all underlying audits. DB: [audits_audittemplate.score]
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// LastSignedAt; When the most recent audit based on this tempalte was signed.
        /// </summary>
        public DateTime? LastSignedAt { get; set; }
        /// <summary>
        /// AreaId; AreaId linked to this template. DB: [audits_audittemplate.area_id]
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [audits_audittemplate.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Role; Role of the template. Used in display filtering. 
        /// - manager
        /// - basic
        /// - shift_leader
        /// </summary>
        public string Role { get; set; } //create enum
        /// <summary>
        /// AreaPath; AreaPath, consists of the path of areas based on the AreaId with the template. e.g. "Some Area 1 -> Some Area 1.1 -> Some Area 1.1.1"; Used for display purposes in the apps. 
        /// </summary>
        public string AreaPath { get; set; }
        /// <summary>
        /// AreaPathIds; AreaPathIds, consists of the ids of areas based on the AreaId with the template. e.g. "1 -> 2 -> 3"; Used for filter purposes in the apps
        /// </summary>
        public string AreaPathIds { get; set; }
        /// <summary>
        /// TaskTemplates; Collection of tasktemplates objects that must be executed with a new audit. 
        /// </summary>
        public List<TaskTemplate> TaskTemplates { get; set; }
        /// <summary>
        /// Properties; List of properties that can be filled in with a audit. 
        /// </summary>
        public List<PropertyAuditTemplate> Properties { get; set; }
        /// <summary>
        /// OpenFieldsProperties; List of open field properties that can be filled in with a audit. 
        /// </summary>
        public List<PropertyAuditTemplate> OpenFieldsProperties { get; set; }
        /// <summary>
        /// PropertiesGen4 is a list of simplified properties for easier use in the frontend of the ezgo platform.
        /// </summary>
        public List<PropertyDTO> OpenFieldsPropertiesGen4 { get; set; }

        /// <summary>
        /// HasDerivedItems; Has dirived items (audits) that already have executed. 
        /// </summary>
        /// 
        public bool? HasDerivedItems { get; set; }
        /// <summary>
        /// ModifiedAt; DateTime the UserProfile was last modified. DB: [audits_audittemplate.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<AuditTemplateRelationWorkInstructionTemplate> WorkInstructionRelations { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this audit template
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
        #endregion

        #region - constructor(s) -
        /// <summary>
        /// AuditTemplate constructor.
        /// </summary>
        public AuditTemplate()
        {

        }
        #endregion
    }
}
