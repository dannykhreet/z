using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApp.Models.Audit;

namespace WebApp.ViewModels
{
    public class CompletedAuditSingleViewModel : CompletedAuditModel
    {

        public string CompletedTextValue { get; set; }
        public TimeZoneInfo Timezone { get; set; }
        public string Locale { get; set; }
    }
}
