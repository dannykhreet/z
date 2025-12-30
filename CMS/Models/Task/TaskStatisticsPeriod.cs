using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Task
{
    public class TaskStatisticsPeriod
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
    }
}
