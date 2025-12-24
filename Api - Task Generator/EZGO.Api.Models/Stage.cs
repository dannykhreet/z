using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Stage; A stage in a checklist contains a subset of the checklist items and can be completed within the checklist.
    /// </summary>
    public class Stage
    {
        /// <summary>
        /// Id; The Stage Id. Unique per stage.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// CompanyId; The Id of the Company related to this stage
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Signatures; the signatures of the stage
        /// </summary>
        public List<Signature> Signatures { get; set; }
        /// <summary>
        /// Name; Name of the stage template.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description; Description of the stage template.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Status; the values can be 'todo' or 'done'
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// ShiftNotes; The Shift notes for the stage
        /// </summary>
        public string ShiftNotes { get; set; }
        /// <summary>
        /// StageTemplateId; the id of the stage in the template
        /// </summary>
        public int StageTemplateId { get; set; }
        /// <summary>
        /// ChecklistId; the Id of the checklist
        /// </summary>
        public int ChecklistId { get; set; }
        /// <summary>
        /// CreatedAt; The DateTime when the stage was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// CreatedById; The User Id of the user that created the stage
        /// </summary>
        public int CreatedById { get; set; }
        /// <summary>
        /// ModifiedAt; The DateTime when the stage was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }
        /// <summary>
        /// ModifiedById; The User Id of the user that last modified the stage
        /// </summary>
        public int ModifiedById { get; set; }
        /// <summary>
        /// IsActive; Indicates whether the stage was deleted or not
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// BlockNextStagesUntilCompletion; Indicates whether next stages shouldn't be tappable yet before this stage has been completed.
        /// </summary>
        public bool BlockNextStagesUntilCompletion { get; set; }
        /// <summary>
        /// LockStageAfterCompletion; Indicates whether after this stage get the status "done", tasks in this stage or the stage itself cannot be modified anymore.
        /// </summary>
        public bool LockStageAfterCompletion { get; set; }
        /// <summary>
        /// UseShiftNotes; Indicates whether shift notes are required for this stage.
        /// </summary>
        public bool UseShiftNotes { get; set; }
        /// <summary>
        /// NumberOfSignatures; Indicates how many signatures are needed for completing the stage.
        /// </summary>
        [Obsolete("NumberOfSignatures will be removed in the future, please use NumberOfSignaturesRequired.")]
        public int NumberOfSignatures { get; set; }
        /// <summary>
        /// The number of signatures that is required to complete this stage. When set to 0, no signatures are required.
        /// </summary>
        public int NumberOfSignaturesRequired { get; set; }

        /// <summary>
        /// TaskTemplateIds; A list of task template id's coupled to this stage
        /// </summary>
        public List<int> TaskTemplateIds { get; set; }
        /// <summary>
        /// TaskIds; A list of task id's coupled to this stage
        /// </summary>
        public List<int> TaskIds { get; set; }

        /// <summary>
        /// Index; Index, sort order, located with template; DB: [checklists_checklisttemplate_stage.index]
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Tags; Tags that are added to this stage's template
        /// </summary>
        public List<Tag> Tags { get; set; }
    }
}
