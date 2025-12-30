using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.WorkInstructions;
using Microsoft.CodeAnalysis.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.ViewModels
{
    public class PdfChecklistTemplateViewModel
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
        public IMediaService MediaService { get; set; }
        public Area ChecklistArea { get; set; }
        public EZGO.Api.Models.ChecklistTemplate ChecklistTemplate { get; set; }
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
    }
}
