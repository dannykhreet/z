using EZGO.Api.Models.Tags;
using EZGO.CMS.LIB.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Action;

namespace WebApp.Models.Comment
{
    public class CommentModel //: EZGO.Api.Models.Comment
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
        public DateTime CommentDate { get { return CreatedAt; } private set { } }
        public string CommentText { get; set; }
        public int CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsBurdenOfProof { get; set; }
        public int? TaskId { get; set; }
        public int? TaskTemplateId { get; set; }
        public List<Tag> Tags { get; set; }

        public ActionModel ToAction()
        {
            return new ActionModel
            {
                ActionType = ActionTypeEnum.comment,
                Id = Id,
                Description = Description,
                Images = Attachments,
                Comment = CommentText,
                CreatedAt = CreatedAt,
                CreatedById = UserId,
                CreatedBy = CreatedBy,
                ModifiedAt = ModifiedAt,
                DueDate = CommentDate,
                Tags = Tags
            };
        }
    }
}
