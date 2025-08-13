using System;
using System.Collections.Generic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Statuses
{
    public class InstructionStatuses : NotifyPropertyChanged, IStatus<InstructionTypeEnum>
    {
        public InstructionStatuses()
        {
            StatusModels = new List<StatusModel<InstructionTypeEnum>>
            {
                new StatusModel<InstructionTypeEnum>(InstructionTypeEnum.BasicInstruction, "GreenColor"),
                new StatusModel<InstructionTypeEnum>(InstructionTypeEnum.SkillInstruction, "OrangeColor"),
            };
        }

        public InstructionTypeEnum CurrentStatus { get; set; }
        public List<StatusModel<InstructionTypeEnum>> StatusModels { get; set; }
    }
}
