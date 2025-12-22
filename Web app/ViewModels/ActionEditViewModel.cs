using EZGO.CMS.LIB.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Action;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class ActionEditViewModel : ActionModel
    {
        public ActionEditViewModel(ActionModel action = null)
        {
            if (action != null)
            {
                Id = action.Id;
                TaskTemplateId = action.TaskTemplateId;
                TaskId = action.TaskId;
                IsResolved = action.IsResolved;
                Comment = action.Comment;
                Description = action.Description;
                Picture = action.Picture;
                Images = action.Images;
                CreatedBy = action.CreatedBy;
                CreatedById = action.CreatedById;
                DueDate = action.DueDate;
                ModifiedAt = action.ModifiedAt;
                CreatedAt = action.CreatedAt;
                Comments = action.Comments;
                UnviewedCommentNr = action.UnviewedCommentNr;
                Parent = action.Parent;
                AssignedUsers = action.AssignedUsers;
                AssignedAreas = action.AssignedAreas;
                VideoThumbNails = action.VideoThumbNails;
                Videos = action.Videos;
                ResolvedAt = action.ResolvedAt;
                Tags = action.Tags;
                SendToUltimo = action.SendToUltimo;
                UltimoStatus = action.UltimoStatus;
                UltimoStatusDateTime = action.UltimoStatusDateTime;
                SendToSapPm = action.SendToSapPm;
                SapPmNotificationConfig = action.SapPmNotificationConfig;
            }
        }

        public Dictionary<string, string> CmsLanguage { get; set; }
        public List<UserBasicModel> ResourcesUsers { get; set; } = new List<UserBasicModel>();
        public List<AreaBasicModel> ResourcesAreas { get; set; } = new List<AreaBasicModel>();
        public string UsersJson => AssignedUsers?.Select(x => x.Id).ToJsonFromObject() ?? "[]";
        public string AreasJson => AssignedAreas?.Select(x => x.Id).ToJsonFromObject() ?? "[]";
        public bool CanAddMedia => (Images?.Count ?? 0) + (VideoThumbNails?.Count ?? 0) < 6;
    }
}
