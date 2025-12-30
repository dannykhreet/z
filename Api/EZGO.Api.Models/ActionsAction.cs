using EZGO.Api.Models.Basic;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// ActionAction; Action object (named ActionAction due to usage of the System.Action object within .Net)
    /// A action can have one or more texts (description/comment) and one or more media items attached. Depending on other settings a action can be linked to assigned users or areas.
    /// A action can be stored as a spot on action; Which is a standalone action (no direct link to a task or tasktemplate.
    /// A action can be stored as a action for a task, tasks can be normal tasks (linked with the task or tasktemplate) or linked to a task that is part of a checklist or audit.
    /// Database location: [actions_action].
    /// </summary>
    public class ActionsAction
    {
        #region - fields -
        /// <summary>
        /// Primary key, in other variables and objects usually named as ActionId.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [actions_action.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// CreatedById; Id of the user that created the action; 
        /// </summary>
        public int CreatedById { get; set; }
        /// <summary>
        /// CreatedBy; Name of user, based on CreatedById.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// CreatedByUser; Add extra information containing user inforamtion (based on include information)
        /// </summary>
        public UserBasic CreatedByUser { get; set; }
        /// <summary>
        /// TaskId; TaskId where the action is linked to. DB: [actions_action.task_id]
        /// </summary>
        public int? TaskId { get; set; }
        /// <summary>
        /// TaskTemplateId; TaskTemplateId where the action is linked to. DB: [actions_action.tasktemplate_id] 
        /// </summary>
        public int? TaskTemplateId { get; set; }
        /// <summary>
        /// Comments; Collection of action comments that are linked to this action based on the DG: [actions_actioncomment.action_id].
        /// </summary>
        public List<ActionComment> Comments { get; set; }
        /// <summary>
        /// LastCommentDate; Last date where a comment has been posted.
        /// </summary>
        public DateTime? LastCommentDate { get; set; }
        /// <summary>
        /// DueDate; Due date where a action needs to be finished. This is user input. DB: [actions_action.due_date]
        /// </summary>
        public DateTime? DueDate { get; set; }
        /// <summary>
        /// Description; Description of action. DB: [actions_action.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Comment; Comment of action. DB: [actions_action.comment]
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// CommentCount; Number of actions comments that are linked to this action based on the DG: [actions_actioncomment.action_id].
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// Images; Collection of images (uri parts). References the image columns in the database. For extendability a array is chosen so we can add more in the future without breaking the structure. 
        /// </summary>
        public List<string> Images { get; set; }
        /// <summary>
        /// ResolvedAt; Resolved at data; User input; DB: [actions_action.resolved_at]
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
        /// <summary>
        /// Videos; Collection of videos (uri parts). References the image columns in the database. For extendability a array is chosen so we can add more in the future without breaking the structure. 
        /// </summary>
        public List<string> Videos { get; set; }
        /// <summary>
        /// VideoThumbNails; Collection of video thumbnails (uri parts). References the image columns in the database. For extendability a array is chosen so we can add more in the future without breaking the structure. 
        /// </summary>
        public List<string> VideoThumbNails { get; set; }
        /// <summary>
        /// AssignedUsers; Collection of basic user items that are linked to this action based on the actions_action_assigned_users table.
        /// </summary>
        public List<UserBasic> AssignedUsers { get; set; }
        /// <summary>
        /// AssignedAreas; Collection of basic area items that are linked to this action based on the actions_action_assigned_areas table.
        /// </summary>
        public List<AreaBasic> AssignedAreas { get; set; }
        /// <summary>
        /// IsResolved; Boolean if a action is resolved. When true, ResolvedAt should also be filled. DB: [actions_action.is_resolved]
        /// </summary>
        public bool? IsResolved { get; set; }
        /// <summary>
        /// CreatedAt; Technical created at date time. DB: [actions_action.created_at]
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; Technical modified at date time. DB: [actions_action.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// Parent; Parent contains a action parent basic object. This is based on the TaskId and/or TaskTemplateId. 
        /// </summary>
        public ActionParentBasic Parent { get; set; }
        /// <summary>
        /// UnviewedCommentNr; Comments that are un-viewed for a specific user. 
        /// </summary>
        public int? UnviewedCommentNr { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this action
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// SendToUltimo; True if the action needs to be sent to Ultimo (not in database)
        /// </summary>
        public bool SendToUltimo { get; set; }
        /// <summary>
        /// SendToSapPm; True if the action needs to be sent to SAP Pm
        /// </summary>
        public bool SendToSapPm { get; set; }

        /// <summary>
        /// SapPmNotificationConfig; Configuration for sending an action as a sap notification to SAP Pm
        /// </summary>
        public SapPmNotificationConfig SapPmNotificationConfig { get; set; }

        /// <summary>
        /// UltimoStatus; can be "NONE", "READY_TO_BE_SENT", "SENT" or "ERROR"
        /// </summary>
        public string UltimoStatus { get; set; } = "NONE";
        /// <summary>
        /// UltimoStatusDateTime; A DateTime containing when the action was (attempted to be) sent to Ultimo
        /// </summary>
        public DateTime? UltimoStatusDateTime { get; set; }
        #endregion

        #region - constructor(s) -
        /// <summary>
        /// ActionsAction; Action contructor
        /// </summary>
        public ActionsAction()
        {
        }

        /// <summary>
        /// ActionAction; contructor including pre-fill of certain objects if needed
        /// </summary>
        /// <param name="preFill">prefill true/false</param>
        public ActionsAction(bool preFill)
        {
            if (preFill)
            {
                Comments = new List<ActionComment>();
                Images = new List<string>();
                Videos = new List<string>();
                VideoThumbNails = new List<string>();
            }
        }
        #endregion

    }
}
