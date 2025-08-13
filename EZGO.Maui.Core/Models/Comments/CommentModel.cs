using EZGO.Api.Models;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Comments
{
    public class CommentModel : NotifyPropertyChanged
    {
        public string InternalId { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public List<MediaItem> Attachments { get; set; }
        public DateTime CommentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public LocalDateTime LocalCommentDate => Settings.ConvertDateTimeToLocal(CommentDate.ToLocalTime());
        public string CommentText { get; set; }
        public int? CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public int? UserId { get; set; }
        public bool? IsBurdenOfProof { get; set; }
        public int TaskId { get; set; }
        public int TaskTemplateId { get; set; }

        public bool HasAttachments => Attachments?.Any() ?? false;
        public List<Tag> Tags { get; set; }
        public List<Attachment> Media { get; set; }
        public bool IsPosted { get; set; }

        public static CommentModel FromModel(Api.Models.Comment another)
        {
            if (another == null)
                return null;

            return new CommentModel()
            {
                Id = another.Id,
                Description = another.Description,
                Attachments = another.Media?.Select(MediaItem.FromApiAttachment).ToList() ?? new List<MediaItem>(),
                CommentDate = another.CommentDate ?? DateTime.Now,
                CommentText = another.CommentText,
                CompanyId = another.CompanyId,
                CreatedBy = another.CreatedBy,
                UserId = another.UserId,
                IsBurdenOfProof = another.IsBurdenOfProof,
                TaskId = another.TaskId ?? 0,
                TaskTemplateId = another.TaskTemplateId ?? 0,
                CreatedAt = another.CreatedAt ?? DateTime.Now,
                Tags = another.Tags,
                Media = another.Media,
            };
        }

        public Comment ToApiModel()
        {
            return new Comment()
            {
                Attachments = Attachments.Where(x => !x.IsEmpty && !x.IsVideo).Select(x => x.PictureUrl).ToList(),
                CommentDate = CommentDate,
                CommentText = CommentText,
                CompanyId = CompanyId,
                CreatedBy = CreatedBy,
                Description = Description,
                Id = Id,
                IsBurdenOfProof = IsBurdenOfProof,
                TaskTemplateId = TaskTemplateId,
                UserId = UserId,
                TaskId = TaskId,
                Tags = Tags,
            };
        }
    }
}
