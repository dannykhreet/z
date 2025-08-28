using System.Collections.Generic;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Interfaces.Tasks
{
    public interface ITaskTemplateModel
    {
        public string DisplayPicture { get; }
        public int ActionBubbleCount { get; }
        public bool ShowPlannedTimeButton { get; }
        public TaskStatusEnum FilterStatus { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public bool HasComment { get; }
        public string CommentString { get; }
        public int? PlannedTime { get; set; }
        public MachineStatusEnum TaskMachineStatus { get; }
        public int? DeepLinkId { get; set; }
        public bool HasVideo { get; }
        public string TaskMarkedString { get; }
        public bool IsTaskMarked { get; }
        public string DueDateString { get; }
        public string OverDueString { get; }
        public IReadOnlyList<BasicTaskPropertyModel> PropertyList { get; }
        public bool HasFeatures { get; }
        public string PropertyValuesString { get; }
        public List<Tag> Tags { get; }
        public List<Attachment> Attachments { get; set; }
    }
}
