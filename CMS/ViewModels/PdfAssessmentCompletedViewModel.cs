using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Pdf;
using WebApp.Models.Shared;

namespace WebApp.ViewModels
{
    public class PdfAssessmentCompletedViewModel : BaseViewModel
    {
        public Assessment Assessment { get; set; }
        public IMediaService MediaService { get; set; }
        public CompletedAssessmentPdfHeaderModel HeaderInfo { get; set; }
    }
}
