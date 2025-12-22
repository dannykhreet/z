using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Company;
using WebApp.Models.Task;
using WebApp.Models.TaskManagement;

namespace WebApp.ViewModels
{
    public class TaskGenerationManagerViewModel : BaseViewModel
    {
        //public List<Area> Areas { get; set; }
        //public List<Shift> Shifts { get; set; }
        //public KeyValuePair<Area, List<Shift>> AreasComplete { get; set; }
        public string Reason { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        //public TaskDowntimePlanningModel TaskDowntimePlanning { get; set; }
        //public Dictionary<Area, TaskDowntimeAreaSubItems> AreaShiftTask { get; set; }
        public List<Area> Areas { get; set; }
        public List<Area> AreasFlattend { get; set; }
        public List<DowntimePlanning> DowntimePlannings { get; set; }
        public List<TaskGenerationShiftViewModel> Shifts { get; set; }
        public List<TaskTemplateModel> Tasks { get; set; }
        public bool MoreGenerationOptionsEnabled { get; set; }
    }
}
