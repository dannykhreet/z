using EZ.Connector.SAP.Models;
using EZGO.Api.Models;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZ.Connector.SAP.Helpers
{
    public static class ActionExtensions
    {
        /// <summary>
        /// ToSAPAction; Converts a action (ActionAction) to a SAP action for further processing the the SAP connector.
        /// </summary>
        /// <param name="action">ActionsAction object.</param>
        /// <returns>SAPAction, containing only the converted data.</returns>
        public static SAPAction ToSAPAction(this ActionsAction action)
        {
            if (action != null)
            {
                var sapAction = new SAPAction();

                sapAction.Id = action.Id;
                sapAction.Comment = action.Comment;
                sapAction.CompanyId = action.CompanyId;
                sapAction.CreatedAt = action.CreatedAt.Value;
                sapAction.CreatedBy = action.CreatedBy;
                sapAction.CreatedById = action.CreatedById;
                sapAction.Description = action.Description;
                sapAction.DueDate = action.DueDate.Value;
                sapAction.IsResolved = action.IsResolved.Value;
                sapAction.ModifiedAt = action.ModifiedAt.Value;

                if(action.Comments != null && action.Comments.Any())
                {
                    foreach(var item in action.Comments)
                    {
                        sapAction.Comments.Add(item.ToSAPActionComment());
                    }
                }

                if(action.Images != null && action.Images.Any())
                {
                    foreach (var item in action.Images)
                    {
                        sapAction.Media.Add(item); //TODO add URL converter
                    }
                }

                if (action.Videos != null && action.Videos.Any())
                {
                    foreach (var item in action.Videos)
                    {
                        sapAction.Media.Add(item); //TODO add URL converter
                    }
                }

                return sapAction;
            }

            return null;
        }

        /// <summary>
        /// ToSAPActionComment; Converts a ActionComment (ActionComment) to a SAP action for further processing in the SAP connector.
        /// </summary>
        /// <param name="action">ActionsAction object.</param>
        /// <returns>SAPActionComment, containing only the converted data.</returns>
        public static SAPActionComment ToSAPActionComment(this ActionComment actionComment)
        {
            if(actionComment !=null)
            {
                var sapActionComment = new SAPActionComment();

                sapActionComment.Comment = actionComment.Comment;
                if(actionComment.CreatedAt.HasValue) sapActionComment.CreatedAt = actionComment.CreatedAt.Value;
                sapActionComment.CreatedBy = actionComment.CreatedBy;
                sapActionComment.CreatedById = actionComment.UserId;
                sapActionComment.Id = actionComment.Id;
                if (actionComment.ModifiedAt.HasValue) sapActionComment.ModifiedAt = actionComment.ModifiedAt.Value;

                return sapActionComment;
            }

            return null;
        }


        public static SAPAariniContainer ToSAPAriniBTP(this ActionsAction action, string baseUrlMedia = "", string baseUrlVideo = "")
        {
            if (action != null)
            {
                SAPAariniContainer container = new SAPAariniContainer();
                SAPAariniBTP sapAction = new SAPAariniBTP();

                sapAction.AttachmentId = ""; //default
                sapAction.UserCanBeNotified = false; //default
                sapAction.NotificationPhase = "1";//default
                sapAction.PlantSection = "YOH"; //default
                sapAction.Completed = false;
                sapAction.Room = "";
                sapAction.ShortText = action.Comment;
                sapAction.NotificationTimezone = "UTC"; //default
                sapAction.NotificationDate = string.Format("\\/Date({0})\\/", (Int32)(action.CreatedAt.Value.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                sapAction.NotificationTime = string.Format("PT{0}H{1}M{2}S", action.CreatedAt.Value.ToString("HH"), action.CreatedAt.Value.ToString("mm"), action.CreatedAt.Value.ToString("ss"));
                sapAction.TecObjNoLeadingZeros = "000000000210100004"; //default
                sapAction.TechnicalObjectTypeDesc = "Equipment";
                sapAction.ReporterDisplay = "";
                sapAction.LastChangedTimestamp = string.Format("\\/Date({0})\\/", (Int32)(action.ModifiedAt.Value.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                sapAction.TechnicalObjectType = "EAMS_EQUI"; //TODO make dynamic when post works
                sapAction.Deleted = false;
                sapAction.Effect = "";
                sapAction.EffectText = "";
                sapAction.DateMonitor = "Y";
                sapAction.NotificationTimestamp = string.Format("\\/Date({0})\\/", (Int32)(action.CreatedAt.Value.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                sapAction.ReporterUserId = "PGOSWAMI"; //need to be replaced with user?
                sapAction.TechnicalObjectNumber = "210100004"; //TODO make dynamic when post works?
                sapAction.TechnicalObjectDescription = "Compressor Motor"; //TODO make dynamic when post works?
                sapAction.NotificationType = "M2"; //TODO make dynamic when post works?
                sapAction.NotificationTypeText = "Malfunction Report"; //TODO make dynamic when post works?
                sapAction.Priority = "2"; //TODO; Make dynamic, based on due date maybe?.
                sapAction.PriorityType = "PM";
                sapAction.PriorityText = "HIGH"; //TODO; Make dynamic, based on due date maybe?.
                sapAction.Location = "";
                sapAction.Reporter = "PGOSWAMI"; //need to be replaced with user?
                sapAction.Subscribed = false;
                sapAction.SystemStatus = "Outstanding";
                sapAction.MediaFiles = new List<string>();

                //TODO enable and add defaults when post works
                if (action.Images != null && action.Images.Any())
                {
                    foreach (var item in action.Images)
                    {
                        sapAction.MediaFiles.Add(string.Concat(baseUrlMedia,item)); //TODO add URL converter
                    }
                }

                if (action.Videos != null && action.Videos.Any())
                {
                    foreach (var item in action.Videos)
                    {
                        sapAction.MediaFiles.Add(string.Concat(baseUrlVideo,item)); //TODO add URL converter
                    }
                }


                container.d = sapAction;

                return container;
            }

            return null;
        }

        /// <summary>
        /// IsValidSAPAction; Check if action is valid for conversion to a SAPAction.
        /// </summary>
        /// <param name="action">EZGO API action.</param>
        /// <returns>true/false depending on outcome.</returns>
        public static bool IsValidSAPAction(this ActionsAction action)
        {
           return (action != null && action.Id > 0 && !string.IsNullOrEmpty(action.Comment) && action.CreatedAt.HasValue && action.ModifiedAt.HasValue && action.DueDate.HasValue && action.IsResolved.HasValue);
        }

    }



}
