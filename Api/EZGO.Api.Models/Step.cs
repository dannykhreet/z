using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Step; Template step used within a Task and or TaskTemplate.
    /// Database location: [tasks_templatestep].
    /// </summary>
    public class Step
    {
        #region - fields -
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as StepId or TaskTemplateStepId. DB: [tasks_tasktemplatestep.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Description; Description of a step. DB: [tasks_tasktemplatestep.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Picture; Picture of a step. Uri part. DB: [tasks_tasktemplatestep.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// TaskTemplateId; TemplateId of the task template. DB: [tasks_tasktemplatestep.template_id]
        /// </summary>
        public int TaskTemplateId { get; set; }
        /// <summary>
        /// Index; Index number (sort order of steps). DB: [tasks_tasktemplatestep.index]
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Video; Video uri DB: [tasks_tasktemplatestep.video]
        /// </summary>
        public string Video { get; set; }
        /// <summary>
        /// VideoThumbnail; VideoThumbnail uri DB: [tasks_tasktemplatestep.video_thumbnail]
        /// </summary>
        public string VideoThumbnail { get; set; }
        #endregion

        #region - constructor(s) -
        public Step()
        {

        }
        #endregion
    }
}
