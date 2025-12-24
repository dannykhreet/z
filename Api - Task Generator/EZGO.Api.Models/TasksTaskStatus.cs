using System;
using System.Collections.Generic;
using System.Text;
using EZGO.Api.Models.Enumerations;

namespace EZGO.Api.Models
{
    public class TasksTaskStatus
    {
        public TaskStatusEnum Status { get; set; }
        public DateTime SignedAt { get; set; }
        public int SignedById { get; set; }
        public string SignedBy { get; set; }
    }
}
