using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Comment; Comment is a specific item used for commenting on certain situations. This can also be used as burden of proof.
    /// DB: [comments]
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Id; Primary key; DB: [comments.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Description; Description of a comment; DB: [comments.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Attachments; Attachments list of partial uri's containing media.; DB: [comments.attachments] JSON as string
        /// </summary>
        public List<string> Attachments { get; set; }
        /// <summary>
        /// Media; A list of Attachment(s) that contain more information about the attachment than just the URI (DB: [comments.attachments])
        /// </summary>
        public List<Attachment> Media { get; set; }
        /// <summary>
        /// CommentDate; Date when comment is made. DB: [comments.comment_date]
        /// </summary>
        public DateTime? CommentDate { get; set; }
        /// <summary>
        /// CommentText; Comment text of a comment; DB: [comments.comment]
        /// </summary>
        public string CommentText { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [comments.company_id] 
        /// </summary>
        public int? CompanyId { get; set; }
        /// <summary>
        /// CreatedBy; Name of the user based on the  UserId of the user who created the comment. 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// UserId; UserId of the user who created the comment. DB: [comments.user_id]
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// CreatedAt; Technical created at date time. DB: [comments.created_at]
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; Technical modified at date time. DB: [comments.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// IsBurdenOfProof; Is burden of proof comment (unimplemented feature)
        /// </summary>
        public bool? IsBurdenOfProof { get; set; }
        /// <summary>
        /// TaskId; TaskId where the action is linked to. DB: [comments.task_id]
        /// </summary>
        public int? TaskId { get; set; }
        /// <summary>
        /// TaskTemplateId; TaskTemplateId where the action is linked to. DB: [comments.tasktemplate_id] 
        /// </summary>
        public int? TaskTemplateId { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this comment
        /// </summary>
        public List<Tag> Tags { get; set; }
    }
}
