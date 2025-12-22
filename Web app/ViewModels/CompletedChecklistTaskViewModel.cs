using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using WebApp.Models.Checklist;
using WebApp.Models.Properties;

namespace WebApp.ViewModels
{
    public class CompletedChecklistTaskViewModel : BaseViewModel
    {
        public CompletedChecklistModel Checklist { get; set; }

        //while 5145 is still in development, this setting is used to determine the environments it should be available on
        public bool EnablePropertyTiles { get; set; }
        public bool EnableInBrowserPdfPrint { get; set; }
    }
}
