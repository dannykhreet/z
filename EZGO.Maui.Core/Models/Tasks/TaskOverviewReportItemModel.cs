using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using System;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TaskOverviewReportItemModel : NotifyPropertyChanged
    {
        private string status;

        public string Status
        {
            get => status;
            set
            {
                status = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(TaskStatus));
            }
        }

        public string Type { get; set; }

        public int NrOfItems { get; set; }

        private TaskStatusEnum? taskStatus;

        public TaskStatusEnum TaskStatus
        {
            get
            {
                TaskStatusEnum result;

                if (!taskStatus.HasValue)
                    result = (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), Status?.Replace(" ", string.Empty) ?? "skipped", true);
                else
                    result = taskStatus.Value;

                return result;
            }
            set
            {
                taskStatus = value;

                OnPropertyChanged();
            }
        }
    }
}
