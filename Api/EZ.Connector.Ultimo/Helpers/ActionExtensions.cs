using EZ.Connector.Ultimo.Models;
using EZGO.Api.Models;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZ.Connector.Ultimo.Helpers
{
    public static class ActionExtensions
    {
        /// <summary>
        /// ToUltimoAction; Converts a action (ActionAction) to a Ultimo action for further processing the the Ultimo connector.
        /// </summary>
        /// <param name="action">ActionsAction object.</param>
        /// <returns>UltimoAction, containing only the converted data.</returns>
        public static UltimoAction ToUltimoAction(this ActionsAction action)
        {
            /* action structure:
             * public int Id { get; set; }
             * public int CompanyId { get; set; }
             * public int CreatedById { get; set; }
             * public string CreatedBy { get; set; }
             * public UserBasic CreatedByUser { get; set; }
             * public int? TaskId { get; set; }
             * public int? TaskTemplateId { get; set; }
             * public List<ActionComment> Comments { get; set; }
             * public DateTime? LastCommentDate { get; set; }
             * public DateTime? DueDate { get; set; }
             * public string Description { get; set; }
             * public string Comment { get; set; }
             * public int CommentCount { get; set; }
             * public List<string> Images { get; set; }
             * public DateTime? ResolvedAt { get; set; }
             * public List<string> Videos { get; set; }
             * public List<string> VideoThumbNails { get; set; }
             * public List<UserBasic> AssignedUsers { get; set; }
             * public List<AreaBasic> AssignedAreas { get; set; }
             * public bool? IsResolved { get; set; }
             * public DateTime? CreatedAt { get; set; }
             * public DateTime? ModifiedAt { get; set; }
             * public ActionParentBasic Parent { get; set; }
             * public int? UnviewedCommentNr { get; set; }
             * public List<Tag> Tags { get; set; }
             * public bool SendToUltimo { get; set; }
             */
            UltimoAction ultimoAction = new UltimoAction();
            if (action != null && action.Id > 0)
            {
                if (action.Description != null && action.Description.Length > 195)
                {
                    ultimoAction.Description = action.Description.Substring(0, 195) + "...";
                }
                else if (action.Description != null)
                {
                    ultimoAction.Description = action.Description;
                }
                else
                {
                    ultimoAction.Description = "";
                }

                var actionComments = "null";
                var actionAssignedUsers = "null";
                var actionAssignedAreas = "null";
                var actionTags = "null";

                if (action.Comments != null && action.Comments.Count > 0)
                {
                    actionComments = string.Join(", <br />", action.Comments.OrderBy(c => c.ModifiedAt).Select(c => string.Format("{0}, {1}, {2}", c.CreatedAt, c.CreatedBy, c.Comment)));
                }
                if (action.AssignedUsers != null && action.AssignedUsers.Count > 0)
                {
                    actionAssignedUsers = string.Join(", ", action.AssignedUsers.Select(u => u.Name));
                }
                if (action.AssignedAreas != null && action.AssignedAreas.Count > 0)
                {
                    actionAssignedAreas = string.Join(", ", action.AssignedAreas.Select(a => a.NamePath));
                }
                if (action.Tags != null && action.Tags.Count > 0)
                {
                    actionTags = string.Join(", ", action.Tags.Select(t => t.Name));
                }

                var resolvedAt = action.ResolvedAt?.ToString() ?? "null";

                var actionStatus = string.Format("{0}{1}", 
                    ((action.IsResolved ?? false) ? "resolved" : (!(action.IsResolved ?? false) && action.DueDate < DateTime.Today.AddDays(1)) ? "overdue" : "unresolved"), 
                    (action.UnviewedCommentNr > 0) ? ",unviewed" : null);

                //ultimoAction.ReportText = action.Comment;
                ultimoAction.ReportText =
                    $"ACTION: {action.Description}; <br />" +
                    $"SITUATION: {action.Comment}; <br />" +
                    $"TAGS: {actionTags}; <br />" +
                    $"<br />" +
                    $"AUTHOR: {action.CreatedBy}; <br />" +
                    $"INVOLVED USERS: {actionAssignedUsers}; <br />" +
                    $"ASSIGNED AREAS: {actionAssignedAreas}; <br />" +
                    $"<br />" +
                    $"ACTION STATUS: {actionStatus} <br />" +
                    $"CREATION DATE: {action.CreatedAt}; <br />" +
                    $"MODIFICATION DATE: {action.ModifiedAt}; <br />" +
                    $"DUE DATE: {action.DueDate}; <br />" +
                    $"RESOLUTION DATE: {resolvedAt}; <br />" +
                    $"<br />" +
                    $"CHAT REMARKS: <br /> " +
                    $"{actionComments};";

                //$"Parent: {action.Parent} \n" +
                //$"Videos: {action.Videos} \n" +
                //$"VideoThumbNails: {action.VideoThumbNails} \n" +
                //$"Images: {action.Images} \n" +
                //$"CreatedByUser: {action.CreatedByUser.Name} \n" +
                //$"TaskId: {action.TaskId} \n" +
                //$"TaskTemplateId: {action.TaskTemplateId} \n" +
                //$"Id: {action.Id} \n" +
                //$"CompanyId: {action.CompanyId} \n" +
                //$"CreatedById: {action.CreatedById} \n" +
                //$"SendToUltimo: {action.SendToUltimo} \n" +


                if (action.CreatedAt.HasValue)
                {
                    ultimoAction.StatusCreatedReportDate = action.CreatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                ultimoAction.ExternalId = action.Id;
                return ultimoAction;
            }

            return null;
        }

        /// <summary>
        /// IsValidUltimoAction; Check if action is valid for conversion to a UltimoAction.
        /// </summary>
        /// <param name="action">EZGO API action.</param>
        /// <returns>true/false depending on outcome.</returns>
        public static bool IsValidUltimoAction(this ActionsAction action)
        {
           return (action != null && action.Id > 0 && !string.IsNullOrEmpty(action.Description) && action.CreatedAt.HasValue);
        }

    }



}
