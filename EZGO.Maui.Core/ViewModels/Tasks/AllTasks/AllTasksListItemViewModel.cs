using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.ViewModels.Tasks.AllTasks
{
    public class AllTasksListItemViewModel : NotifyPropertyChanged, IItemFilter<TaskStatusEnum>
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Picture { get; set; }

        public string TopMostAreaName { get; set; }

        public string AssignedAreaName { get; set; }

        public string AssignedAreaFullName { get; set; }

        public int AssignedAreaId { get; set; }

        public string RecurrencyType { get; set; }

        public TaskTemplateModel UnderlyingModel { get; private set; }

        public TaskStatusEnum FilterStatus { get; set; }

        public List<Tag> Tags { get; set; }

        public AllTasksListItemViewModel(TaskTemplateModel source)
        {
            Id = source.Id;
            Name = source.Name;
            Picture = source.Picture;
            Picture ??= source.VideoThumbnail;
            AssignedAreaId = source.AreaId ?? -1;
            RecurrencyType = source.RecurrencyType ?? "";
            UnderlyingModel = source;
            Tags = source.Tags;
        }

        public BasicTaskModel ToBasicTask()
        {
            var taskTemplate = UnderlyingModel;

            var result = new BasicTaskModel()
            {
                Id = taskTemplate.Id,
                Index = taskTemplate.Index,
                Name = taskTemplate.Name,
                Picture = taskTemplate.Picture,
                FilterStatus = Api.Models.Enumerations.TaskStatusEnum.Todo,
                TimeRealizedBy = taskTemplate.PlannedTime.ToString(),
                Description = taskTemplate.Description,
                Steps = taskTemplate.Steps,
                DescriptionFile = taskTemplate.DescriptionFile,
                WorkInstructionRelations = taskTemplate.WorkInstructionRelations,
                VideoThumbnail = taskTemplate.VideoThumbnail,
                Video = taskTemplate.Video,
                HasPictureProof = taskTemplate.HasPictureProof,
                DueAt = null,
                AreaPath = AssignedAreaFullName,
                RecurrencyType = taskTemplate.RecurrencyType,
                Tags = taskTemplate.Tags,
                Attachments = taskTemplate.Attachments
            };

            return result;
        }
    }
}
