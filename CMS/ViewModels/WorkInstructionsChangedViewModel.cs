using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Action;
using WebApp.Models.WorkInstructions;

namespace WebApp.ViewModels
{
    public class WorkInstructionsChangedViewModel : BaseViewModel
    {
        public List<Area> Areas { get; set; }
        public EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate OldTemplate { get; set; }
        public EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate NewTemplate { get; set; }
        public List<WorkInstructionTemplateChange> NotificationData { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
