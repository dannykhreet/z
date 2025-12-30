using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using Microsoft.CodeAnalysis.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.WorkInstructions;

namespace WebApp.ViewModels
{
    public class PdfWorkInstructionChangeNotificationViewModel
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
        public IMediaService MediaService { get; set; }

        public WorkInstructionTemplateChangesNotificationModel ChangeNotification { get; set; }
        public EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate WorkInstruction { get; set; }
        public List<Area> Areas { get; set; }
        public List<Tag> Tags { get; set; }

        public string Locale { get; set; }

    }
}
