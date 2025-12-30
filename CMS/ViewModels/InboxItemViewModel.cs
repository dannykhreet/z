using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class InboxItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public object Item { get; set; }
        public ObjectTypeEnum Type { get; set; }
        
        private string templateType;
        public string TemplateType
        {
            get { return templateType ?? Type.ToString().Replace("Template", " Template"); }

            set { templateType = value; }
        }

        /// <summary>
        /// ezgolist type
        /// </summary>
        public string ListType
        {
            get
            {
                return Type switch
                {
                    ObjectTypeEnum.ChecklistTemplate => "checklist",
                    ObjectTypeEnum.TaskTemplate => "task",
                    ObjectTypeEnum.AuditTemplate => "audit",
                    ObjectTypeEnum.WorkInstructionTemplate => "workinstruction",
                    ObjectTypeEnum.AssessmentTemplate => "assessment",
                    _ => "unknown",//should never happen
                };
            }
        }
        public string FromCompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string TimeAgo
        {
            get
            {
                TimeSpan timeSpanUntillNow = (DateTime.UtcNow - ModifiedAt);
                if (timeSpanUntillNow.Days > 0)
                    return timeSpanUntillNow.Days + " days ago";
                else if (timeSpanUntillNow.Hours > 0)
                    return timeSpanUntillNow.Hours + " hours ago";
                else return timeSpanUntillNow.Minutes + " minutes ago";
            }
        }
        public InboxItemViewModel()
        {
        }
    }
}
