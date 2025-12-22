using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class CompletedTaskViewModel : BaseViewModel
    {
        public CompletedTaskViewModel()
        {
        }

        public List<TasksTask> CompletedTasks { get; set; }
        public DateTime ReferenceDate { get; set; }
        public string FilterType { get; set; }
        public int AreaId { get; set; }
        public int TemplateId { get; set; }
        public DateTime? Timestamp { get; set; }

        //while 5145 is still in development, this setting is used to determine the environments it should be available on
        public bool EnablePropertyTiles { get; set; }
    }
}
