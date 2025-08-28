using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class BasicTaskStatusModel : NotifyPropertyChanged
    {
        //"TaskId": 2813631,
        //"Status": "not ok",
        //"SignedById": 1815,
        //"SignedAt": "2020-04-30T14:12:11+00:00",
        //"TaskTemplateId": 30928

        public bool HasChanged { get; set; }
        public long TaskId { get; set; }
        public string Status { get; set; }
        public int? SignedById { get; set; }
        public DateTime? SignedAt { get; set; }
        public string SignedBy { get; set; }
        public string SignedByName { get; set; }
        public string Comment { get; set; }
        public int TaskTemplateId { get; set; }
        public bool HasPictureProof { get; set; }
        public PictureProof PictureProof { get; set; }

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