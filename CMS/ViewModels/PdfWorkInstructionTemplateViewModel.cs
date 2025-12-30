using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.ViewModels
{
    public class PdfWorkInstructionTemplateViewModel
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
        public IMediaService MediaService { get; set; }
        public Area WorkInstructionTemplateArea { get; set; }
        public EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate WorkInstructionTemplate { get; set; }
    }
}
