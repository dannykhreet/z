using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApp.Models.Checklist;

namespace WebApp.ViewModels
{
    public class CompletedChecklistSingleViewModel : CompletedChecklistModel
    {
        public string CompletedTextValue { get; set; } = "Ended";
        public string ByTextValue { get; set; } = "by";
        public string ModifiedTextValue { get; set; } = "Last modified at";
        public string OngoingTextValue { get; set; } = "ONGOING";
        public TimeZoneInfo Timezone { get; set; }

    }
}
