using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using WebApp.Models.Properties;
using WebApp.ViewModels;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstructionTemplateChangesNotificationModel : BaseViewModel
    {
        public int Id { get; set; }
        public int WorkInstructionTemplateId { get; set; }
        public int CompanyId { get; set; }
        public string NotificationComment { get; set; }
        public List<WorkInstructionTemplateChange> NotificationData { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int ModifiedById { get; set; }
        public string ModifiedBy { get; set; }
        public List<WorkInstructionTemplateChangeNotificationViewedModel> NotificationViewedStats { get; set; }
        public bool EnableInBrowserPdfPrint { get; set; }
        //while 5145 is still in development, this setting is used to determine the environments it should be available on
        public bool EnablePropertyTiles { get; set; }
    }
}
