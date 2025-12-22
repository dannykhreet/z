using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.ViewModels
{
    public class PdfTaskTemplateViewModel
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
        public IMediaService MediaService { get; set; }
        public Area TaskArea { get; set; }
        public EZGO.Api.Models.TaskTemplate TaskTemplate { get; set; }
        public Dictionary<int, EZGO.Api.Models.Shift> TaskShifts { get; set; }
    }
}
