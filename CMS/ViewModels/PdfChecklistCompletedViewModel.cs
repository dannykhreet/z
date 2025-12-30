using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Pdf;

namespace WebApp.ViewModels
{
    public class PdfChecklistCompletedViewModel : BaseViewModel
    {
        public Area ChecklistArea { get; set; }
        public string CompletedByUserName { get; set; }
        public DateTime CompletedAt { get; set; }
        public EZGO.Api.Models.Checklist Checklist { get; set; }
        public List<PdfActionModel> Actions { get; set; }
        public List<PdfCommentModel> Comments { get; set; }
        public IMediaService MediaService { get; set; }
    }
}
