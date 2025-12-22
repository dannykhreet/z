using System;
using System.Collections.Generic;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Logic.Interfaces;
using WebApp.Models.Audit;
using WebApp.Models.Shared;

namespace WebApp.ViewModels
{
    public class AuditViewModel : BaseViewModel
    {
        public List<AuditTemplateModel> AuditTemplates { get; set; }
        public AuditTemplateModel CurrentAudit { get; set; }
        public List<Audit> CompletedAudits { get; set; }
        public IScoreColorCalculator PercentageScoreColorCalculator { get; set; }
        public List<Area> Areas { get; set; }
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        public List<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
        public int SharedTemplateId { get; set; }
        public List<CompanyBasic> CompaniesInHolding { get; set; }
        public bool TaskTemplateAttachmentsEnabled { get; set; }
        public List<int> ConnectedTaskTemplateIds { get; set; }
        public ExtractionModel ExtractionData { get; set; }
        public bool IsNewTemplate { get; set; }
        public AuditViewModel()
        {
        }
    }
}
