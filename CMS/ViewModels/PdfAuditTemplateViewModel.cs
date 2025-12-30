using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.ViewModels
{
    public class PdfAuditTemplateViewModel
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
        public IMediaService MediaService { get; set; }
        public Area AuditArea { get; set; }
        public EZGO.Api.Models.AuditTemplate AuditTemplate { get; set; }
        public List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate> WorkInstructions { get; set; }
    }
}
