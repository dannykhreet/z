using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using System.Collections.Generic;
using WebApp.Models.Shared;
using WebApp.Models.WorkInstructions;

namespace WebApp.ViewModels
{
    public class WorkInstructionsViewModel : BaseViewModel
    {
        public WorkInstruction CurrentWorkInstruction { get; set; }
        public List<WorkInstruction> WorkInstructions { get; set; }
        public WorkInstructionTemplate CurrentWorkInstructionTemplate { get; set; }
        public List<WorkInstructionTemplate> WorkInstructionTemplates { get; set; }
        public List<Area> Areas { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        public string WorkInstructionTypeFilter { get; set; }
        public bool ShowAvailableForAllAreasToggle { get; set; }
        public int SharedTemplateId { get; set; }
        public List<CompanyBasic> CompaniesInHolding { get; set; }
        public bool IsNewTemplate { get; set; }
        public ExtractionModel ExtractionData { get; set; }
    }
}
