using EZGO.Api.Models;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using WebApp.Models.WorkInstructions;

namespace WebApp.ViewModels
{
    public class WorkInstructionChangeNotificationViewModel : BaseViewModel
    {
        public WorkInstructionTemplateChangesNotificationModel ChangeNotification { get; set; }
        public EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate WorkInstruction { get; set; }
        public List<Area> Areas { get; set; }
        public List<Tag> Tags { get; set; }

        public TimeZoneInfo Timezone { get; set; }
        public bool EnableInBrowserPdfPrint { get; set; }
        //while 5145 is still in development, this setting is used to determine the environments it should be available on
        public bool EnablePropertyTiles { get; set; }

        public string ModifiedTextValue { get; set; } = "Modified by";
        public string ByTextValue { get; set; } = "by";
        public string UsersThatHaveConfirmedThisChangeValue { get; set; } = "Users that have confirmed viewing these changes";
    }
}
