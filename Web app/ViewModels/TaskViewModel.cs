using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.WorkInstructions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using WebApp.Models.Shared;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        public List<TaskTemplateModel> TaskTemplates { get; set; }
        public TaskTemplateModel CurrentTaskTemplate { get; set; }
        public List<TasksTask> CompletedTasks { get; set; } = new List<TasksTask>();
        public RecurrencyViewModel Recurrency { get; set; }
        public List<Area> Areas { get; set; }
        public List<WorkInstructionTemplate> WorkInstructions { get; set; }
        public int PropertyStructureVersion { get; set; }
        public List<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
        public bool EnableTaskIndexingButtons { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        public int SharedTemplateId { get; set; }
        public List<CompanyBasic> CompaniesInHolding { get; set; }
        public bool TaskTemplateAttachmentsEnabled { get; set; }
        public int PreviousTasksCount { get; set; }
        public bool IsNewTemplate { get; set; }
        public ExtractionModel ExtractionData { get; set; }
        public TaskViewModel()
        {
            Recurrency = new RecurrencyViewModel();
        }
    }
}
