using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class ActionStatuses : IStatus<ActionStatusEnum>
    {
        private ActionStatusEnum resolved = ActionStatusEnum.Solved;
        private ActionStatusEnum pastDue = ActionStatusEnum.PastDue;
        private ActionStatusEnum unresolved = ActionStatusEnum.Unsolved;

        public ActionStatuses()
        {
            StatusModels = new List<StatusModel<ActionStatusEnum>>
            {
                new StatusModel<ActionStatusEnum>(resolved, "GreenColor"),
                new StatusModel<ActionStatusEnum>(pastDue, "RedColor"),
                new StatusModel<ActionStatusEnum>(unresolved, "DarkerGreyColor"),
            };
        }

        public ActionStatusEnum CurrentStatus { get; set; }

        public List<StatusModel<ActionStatusEnum>> StatusModels { get; set; }
    }
}
