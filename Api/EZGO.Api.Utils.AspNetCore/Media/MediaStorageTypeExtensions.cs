using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Media
{
    public static class MediaStorageTypeExtensions
    {
        /// <summary>
        /// ToStorageLocation; Get specific storage location for certain media storage type,
        /// </summary>
        /// <param name="mediaStorageType">MediaStorageTypeEnum value</param>
        /// <returns>string containing the correct storage location, will return "" when non found. </returns>
        public static string ToStorageLocation(this MediaStorageTypeEnum mediaStorageType)
        {
            string output = "";

            switch(mediaStorageType)
            {
                case MediaStorageTypeEnum.AuditDescriptions: output = Settings.MediaSettings.TYPE_AUDIT_DESCRIPTIONS; break;
                case MediaStorageTypeEnum.AuditItems: output = Settings.MediaSettings.TYPE_AUDIT_ITEMS; break;
                case MediaStorageTypeEnum.Audits: output = Settings.MediaSettings.TYPE_AUDITS; break;
                case MediaStorageTypeEnum.AuditSignatures: output = Settings.MediaSettings.TYPE_SIGNATURE; break;
                case MediaStorageTypeEnum.AuditSteps: output = Settings.MediaSettings.TYPE_AUDIT_STEPS; break;
                case MediaStorageTypeEnum.ChecklistDescriptions: output = Settings.MediaSettings.TYPE_CHECKLIST_DESCRIPTIONS; break;
                case MediaStorageTypeEnum.ChecklistItems: output = Settings.MediaSettings.TYPE_CHECKLIST_ITEMS; break;
                case MediaStorageTypeEnum.Checklists: output = Settings.MediaSettings.TYPE_CHECKLISTS; break;
                case MediaStorageTypeEnum.ChecklistSignatures: output = Settings.MediaSettings.TYPE_SIGNATURE; break;
                case MediaStorageTypeEnum.ChecklistSteps: output = Settings.MediaSettings.TYPE_CHECKLIST_STEPS; break;
                case MediaStorageTypeEnum.TaskDescriptions: output = Settings.MediaSettings.TYPE_TASK_DESCRIPTIONS; break;
                case MediaStorageTypeEnum.Tasks: output = Settings.MediaSettings.TYPE_TASKS; break;
                case MediaStorageTypeEnum.TaskSteps: output = Settings.MediaSettings.TYPE_TASK_STEPS; break;
                case MediaStorageTypeEnum.ProfileImage: output = Settings.MediaSettings.TYPE_PROFILEIMAGE; break;
                case MediaStorageTypeEnum.Actions: output = Settings.MediaSettings.TYPE_ACTIONS; break;
                case MediaStorageTypeEnum.ActionComments: output = Settings.MediaSettings.TYPE_ACTIONCOMMENTS; break;
                case MediaStorageTypeEnum.Area: output = Settings.MediaSettings.TYPE_AREAS; break;
                case MediaStorageTypeEnum.Comments: output = Settings.MediaSettings.TYPE_COMMENTS; break;
                case MediaStorageTypeEnum.FactoryFeed: output = Settings.MediaSettings.TYPE_FACTORYFEED; break;
                case MediaStorageTypeEnum.FactoryFeedMessages: output = Settings.MediaSettings.TYPE_FACTORYFEEDMESSAGES; break;
                case MediaStorageTypeEnum.Company: output = Settings.MediaSettings.TYPE_COMPANY; break;
                case MediaStorageTypeEnum.WorkInstruction: output = Settings.MediaSettings.TYPE_WORKINSTRUCTION; break;
                case MediaStorageTypeEnum.WorkInstructionItem: output = Settings.MediaSettings.TYPE_WORKINSTRUCTION; break;
                case MediaStorageTypeEnum.AssessmentTemplate: output = Settings.MediaSettings.TYPE_ASSESSMENT; break;
                case MediaStorageTypeEnum.AssessmentSignature: output = Settings.MediaSettings.TYPE_SIGNATURE; break;
                case MediaStorageTypeEnum.Announcement: output = Settings.MediaSettings.TYPE_ANNOUNCEMENT; break;
                case MediaStorageTypeEnum.PictureProof: output = Settings.MediaSettings.TYPE_PICTUREPROOF; break;
            }
            return output;
        }

    }
}
