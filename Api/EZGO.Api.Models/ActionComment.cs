using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// ActionComment; Comment attached to a certain action. Actions can have one or more comments. Depending on implementation a comment can have a text and one or more media files. 
    /// Database location: [actions_actioncomment]
    /// </summary>
    public class ActionComment
    {
        #region - fields -
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as ActionCommentId. DB: [actions_actioncomment.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ActionId; Id of linked action object. DB: [actions_action.id]
        /// </summary>
        public int ActionId { get; set; }
        /// <summary>
        /// Comment; Contains user input 'the comment'. DB: [actions_actioncomment.comment.id]
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action comment belongs to. DB: [actions_actioncomment.company_id] , depending on implementation it can be the linked action's company id; DB: [actions_action.company_id] 
        /// </summary>
        public int? CompanyId { get; set; }
        /// <summary>
        /// Images; Collection of images (uri parts). References the image columns in the database. For extendability a array is chosen so we can add more in the future without breaking the structure. 
        /// </summary>
        public List<string> Images { get; set; }
        /// <summary>
        /// Video; Video uri; containing a linked video. 
        /// </summary>
        public string Video { get; set; }
        /// <summary>
        /// VideoThumbnail; Video thumbnail of the linked video (this.Video)
        /// </summary>
        public string VideoThumbnail { get; set; }
        /// <summary>
        /// CreatedBy; Based on UserId.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// CreatedAt; Technical created at date time. DB: [actions_actioncomment.created_at]
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; Technical modified at date time. DB: [actions_actioncomment.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// UserModifiedUtcAt; Specific date for posting / creating / updating. If supplied this date will be used for the modified_at data in the database. Will be a own column in future development.
        /// </summary>
        public DateTime? UserModifiedUtcAt { get; set; }
        /// <summary>
        /// UserId; UserId that creates the comment. DB: [actions_actioncomment.user_id]
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// CreatedByUser; Add extra information containing user inforamtion (based on include information)
        /// </summary>
        public UserBasic CreatedByUser { get; set; }

        /// <summary>
        /// ViewedByUsers; List of user basic objects containing all users that are linked through the [actions_commentviewed] table. 
        /// </summary>
        public List<UserBasic> ViewedByUsers { get; set; }

        #endregion

        #region - constructor(s) -
        /// <summary>
        /// ActionComment; Action comment contructor
        /// </summary>
        public ActionComment()
        {
        }

        /// <summary>
        /// ActionComment; Action comment contructor, including pre-init of certain variables. 
        /// </summary>
        /// <param name="preInit">pre-init yes/no (true/false)</param>
        public ActionComment(bool preInit)
        {
            if(preInit)
            {
                Images = new List<string>();
            }
        }
        #endregion
    }
}
