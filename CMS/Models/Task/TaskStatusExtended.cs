using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Task
{
    public class TaskStatusExtended : TaskStatusBasic
    {
        public TaskStatusExtended(TaskStatusBasic taskStatusBasic, DateTime timeStamp)
        {
            TaskId = taskStatusBasic.TaskId;
            Status = taskStatusBasic.Status;
            SignedById = taskStatusBasic.SignedById;
            SignedBy = taskStatusBasic.SignedBy;
            SignedAt = taskStatusBasic.SignedAt;
            TaskTemplateId = taskStatusBasic.TaskTemplateId;
            TimeStamp = timeStamp;
        }

        public DateTime TimeStamp { get; set; }
    }
}
