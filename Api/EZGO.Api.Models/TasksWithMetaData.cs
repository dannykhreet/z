using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class TasksWithMetaData
    {
        public TasksMetaData Meta { get; set; }
        public List<TasksTask> Data { get; set; }
    }
}
