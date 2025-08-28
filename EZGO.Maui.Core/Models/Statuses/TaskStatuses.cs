using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class TaskStatuses : NotifyPropertyChanged, IStatus<TaskStatusEnum>
    {
        public TaskStatuses()
        {
            StatusModels = Microsoft.Maui.Devices.DeviceInfo.Idiom == DeviceIdiom.Phone ? GetPhoneStatusesOrder() : GetTabletStatusesOrder();
        }

        public TaskStatusEnum CurrentStatus { get; set; }

        public List<StatusModel<TaskStatusEnum>> StatusModels { get; set; }

        private List<StatusModel<TaskStatusEnum>> GetPhoneStatusesOrder()
        {
            return new List<StatusModel<TaskStatusEnum>>
            {
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.NotOk,"RedColor"),
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.Todo, "DarkerGreyColor"),
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.Skipped, "SkippedColor"),
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.Ok, "GreenColor"),
            };
        }

        private List<StatusModel<TaskStatusEnum>> GetTabletStatusesOrder()
        {
            return new List<StatusModel<TaskStatusEnum>>
            {
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.Ok, "GreenColor"),
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.NotOk,"RedColor"),
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.Skipped, "SkippedColor"),
                new StatusModel<TaskStatusEnum>(TaskStatusEnum.Todo, "DarkerGreyColor"),
            };
        }
    }
}
