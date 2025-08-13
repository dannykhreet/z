using System;
using System.Collections.Generic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class SkillStatuses : IStatus<SkillTypeEnum>
    {
        public SkillStatuses()
        {
            StatusModels = new List<StatusModel<SkillTypeEnum>>
            {
                new StatusModel<SkillTypeEnum>(SkillTypeEnum.Mandatory, "GreenColor"),
            };
        }

        public SkillTypeEnum CurrentStatus { get; set; }
        public List<StatusModel<SkillTypeEnum>> StatusModels { get; set; }
    }
}
