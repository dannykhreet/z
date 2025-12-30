using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApp.Models.Checklist;

namespace WebApp.ViewModels
{
    public class CompletedChecklistViewModel : BaseViewModel
    {
        public CompletedChecklistViewModel()
        {
        }

        public List<CompletedChecklistModel> CompletedChecklists { get; set; }
        public int TemplateId { get; set; }
        public int ChecklistId { get; set; }
    }
}
