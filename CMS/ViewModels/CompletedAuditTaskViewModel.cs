using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using WebApp.Models.Audit;
using WebApp.Models.Properties;

namespace WebApp.ViewModels
{
    public class CompletedAuditTaskViewModel : BaseViewModel
    {
        public CompletedAuditModel Audit { get; set; }

        //while 5145 is still in development, this setting is used to determine the environments it should be available on
        public bool EnablePropertyTiles { get; set; }
        public bool EnableInBrowserPdfPrint { get; set; }
    }
}
