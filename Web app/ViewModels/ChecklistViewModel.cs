using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.WorkInstructions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApp.Models.Checklist;
using WebApp.Models.Shared;

namespace WebApp.ViewModels
{
    public partial class ChecklistViewModel : BaseViewModel
    {
        public List<ChecklistTemplateModel> ChecklistTemplates { get; set; }
        public ChecklistTemplateModel CurrentChecklistTemplate { get; set; }
        public List<Checklist> CompletedChecklists { get; set; }
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        public List<Area> Areas { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        public int SharedTemplateId { get; set; }
        public List<CompanyBasic> CompaniesInHolding { get; set; }
        public bool TaskTemplateAttachmentsEnabled { get; set; }
        public bool EnableStageTemplateShiftNotesAndSignatures { get; set; }
        public List<int> ConnectedTaskTemplateIds { get; set; }
        public bool IsNewTemplate { get; set; }
        public ExtractionModel ExtractionData { get; set; }
        public ChecklistViewModel()
        {
        }
    }
}
