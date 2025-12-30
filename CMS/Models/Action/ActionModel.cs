using EZGO.Api.Models;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.CMS.LIB.Enumerators;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApp.Attributes;

namespace WebApp.Models.Action
{
    public class ActionModel //: ActionsAction
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int TaskTemplateId { get; set; }
        public int TaskId { get; set; }
        public bool IsResolved { get; set; }
        public bool IsOverdue
        {
            get => !IsResolved && DueDate < DateTime.Today.AddDays(1);
        }
        [Required]
        public string Comment { get; set; }
        [Required]
        public string Description { get; set; }
        public string Picture { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public string CreatedBy { get; set; }
        public int CreatedById { get; set; }
        //TODO add errormessages more consistent across other required properties aswell
        //TODO use languageservice to resolve errormessages so they won't always be in english
        [Required(ErrorMessage ="A future date is required"), EnsureFutureDate(ErrorMessage = "Please ensure the duedate lies in the future")]
        public DateTime DueDate { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ActionCommentModel> Comments { get; set; } = new List<ActionCommentModel>();
        public int UnviewedCommentNr { get; set; }
        public bool HasUnviewedComments => UnviewedCommentNr > 0;
        public bool HasComments => Comments.Any();
        public ActionParentModel Parent { get; set; }
        public bool IsOntheSpotAction => Parent?.ActionId == Id && !(Parent?.TaskId.HasValue ?? false) && !(Parent?.TaskTemplateId.HasValue ?? false);
        public List<UserBasicModel> AssignedUsers { get; set; } = new List<UserBasicModel>();
        public string AssignedUserIds { get => AssignedUsers?.Select(x => string.Format("r{0}", x.Id))?.DefaultIfEmpty().Aggregate((a, b) => a + ',' + b); }
        public List<AreaBasicModel> AssignedAreas { get; set; } = new List<AreaBasicModel>();
        public List<string> VideoThumbNails { get; set; } = new List<string>();
        public List<string> Videos { get; set; } = new List<string>();
        public List<Tag> Tags { get; set; }
        public DateTime ResolvedAt { get; set; }
        public ActionTypeEnum ActionType { get; set; } = 0;
        public ApplicationSettings ApplicationSettings { get; set; }
        public bool SendToUltimo { get; set; }
        public string UltimoStatus { get; set; } = "NONE";
        public DateTime? UltimoStatusDateTime { get; set; }
        public bool SendToSapPm { get; set; }
        public SapPmNotificationConfig SapPmNotificationConfig { get; set; }
        /// <summary>
        /// To validate if controls should be disabled when reloading page
        /// </summary>
        public bool AlreadySentToSapPm { get; set; }
        public ActionsAction ToApiModel()
        {
            return new ActionsAction
            {
                Id = Id,
                AssignedAreas = AssignedAreas?.Select(x => new EZGO.Api.Models.Basic.AreaBasic { Id = x.Id, Name = x.Name, NamePath = x.NamePath}).ToList(),
                AssignedUsers = AssignedUsers?.Select(x => new EZGO.Api.Models.Basic.UserBasic { Id = x.Id, Name = x.Name, Picture = x.Picture }).ToList(),
                Comment = Comment,
                CompanyId = CompanyId,
                CreatedById = CreatedById,
                Description = Description,
                DueDate = DueDate,
                TaskTemplateId = TaskTemplateId > 0 ? TaskTemplateId : new Nullable<int>(), 
                Images = Images ?? new List<string>(),
                Videos = Videos ?? new List<string>(),
                VideoThumbNails = VideoThumbNails ?? new List<string>(),
                IsResolved = IsResolved,
                Tags = Tags,
                SendToUltimo = SendToUltimo,
                UltimoStatus = UltimoStatus,
                UltimoStatusDateTime = UltimoStatusDateTime,
                SendToSapPm = SendToSapPm,
                SapPmNotificationConfig = SapPmNotificationConfig
            };
        }
    }
}
