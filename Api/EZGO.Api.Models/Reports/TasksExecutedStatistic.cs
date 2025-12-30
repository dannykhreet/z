using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class TasksExecutedStatistic
    {
        public string LabelText { get; set; }
        public int TodoCount { get; set; }
        public int ExecutedCount { get; set; }
    }
}
