using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// StageTemplate; A stage in a checklist contains a subset of the checklist items and can be completed within the checklist.
    /// </summary>
    public class StageTemplate
    {
        /// <summary>
        /// Stage template id. Unique per stage.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Company id of the company owning this object.
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Id of the parent checklist template of this stage template.
        /// </summary>
        public int ChecklistTemplateId { get; set; }
        /// <summary>
        /// Name of the stage.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the stage.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// When true, the stages after this are blocked and items in those stages cannot be tapped until this stage is completed.
        /// </summary>
        public bool BlockNextStagesUntilCompletion { get; set; }
        /// <summary>
        /// When true, the items in this stage cannot be altered once this stage has been completed.
        /// </summary>
        public bool LockStageAfterCompletion { get; set; }
        /// <summary>
        /// When true, shift notes can be added when completing this stage. These shift notes can be read by users who continue the checklist afterwards.
        /// </summary>
        public bool UseShiftNotes { get; set; }
        /// <summary>
        /// The number of signatures that is required to complete this stage. When set to 0, no signatures are required.
        /// </summary>
        public int NumberOfSignaturesRequired { get; set; }
        /// <summary>
        /// Index of the stage determines the order in which the stages appear in a checklist, from low to high.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Collection of task template ids of the tasktemplates that are part of this stage.
        /// </summary>
        public List<int> TaskTemplateIds { get; set; }

        /// <summary>
        /// Tags; Tags that are added to this stage template
        /// </summary>
        public List<Tag> Tags { get; set; }
    }
}
