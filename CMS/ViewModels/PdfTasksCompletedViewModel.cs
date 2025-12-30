using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Pdf;
using WebApp.Models.Shared;

namespace WebApp.ViewModels
{
    public class PdfTasksCompletedViewModel : BaseViewModel
    {
        public List<SharedTaskModel> Tasks { get; set; }
        public List<PdfActionModel> Actions { get; set; }
        public List<PdfCommentModel> Comments { get; set; }
        public IMediaService MediaService { get; set; }
        public CompletedTasksPdfHeaderModel HeaderInfo { get; set; }
    }
}
