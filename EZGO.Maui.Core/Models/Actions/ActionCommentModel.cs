using EZGO.Api.Models;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.ModelInterfaces;
using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Actions
{
    public class ActionCommentModel : ActionComment, IBase<BasicActionCommentModel>
    {
        public List<MediaItem> LocalMediaItems { get; set; }

        public MediaItem VideoMediaItem { get; set; }

        public List<MediaItem> ImageMediaItems { get; set; }

        public bool UnPosted { get; set; }

        public string LocalId { get; set; } = Guid.NewGuid().ToString("N");

        public new int CompanyId { get; set; }

        public int? LocalActionId { get; set; }

        public BasicActionCommentModel ToBasic()
        {
            return new BasicActionCommentModel
            {
                Id = this.Id,
                ModifiedAt = Settings.ConvertDateTimeToLocal(this.ModifiedAt ?? DateTime.Now),
                CreatedBy = this.CreatedBy,
                UserId = this.UserId,
                Comment = this.Comment,
                LocalMediaItems = this.LocalMediaItems,
                VideoMediaItem = this.VideoMediaItem,
                ImageMediaItems = this.ImageMediaItems,
                UnPosted = this.UnPosted,
                LocalId = this.LocalId,
                LocalActionId = this.LocalActionId
            };

        }

        public ActionComment ToActionComment()
        {
            return new ActionComment()
            {
                Id = this.Id,
                ActionId = this.ActionId,
                ModifiedAt = this.ModifiedAt,
                Comment = this.Comment,
                CompanyId = this.CompanyId,
                CreatedAt = this.CreatedAt,
                CreatedBy = this.CreatedBy,
                Images = this.Images,
                UserId = this.UserId,
                VideoThumbnail = this.VideoThumbnail,
                UserModifiedUtcAt = this.UserModifiedUtcAt,
                Video = this.Video,
                ViewedByUsers = this.ViewedByUsers
            };
        }
    }

    public class ActionCommentComparer : IEqualityComparer<ActionCommentModel>
    {
        public bool Equals(ActionCommentModel x, ActionCommentModel y)
        {
            if (x.Id != 0 && y.Id != 0 && x.Id == y.Id)
            {
                return true;
            }
            else
                return false;
        }

        public int GetHashCode(ActionCommentModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
