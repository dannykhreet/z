using System;
using EZGO.Maui.Core.Enumerations;

namespace EZGO.Maui.Core.Models.Instructions
{
    public class InstructionViewedTimeModel
    {
        public int InstructionId { get; set; }
        public long TimeSpend { get; set; }
        public InstructionsTimeEnum TimeType { get; set; }

        public InstructionViewedTimeModel(int instructionId, long timeSpend, InstructionsTimeEnum timeType = InstructionsTimeEnum.Miliseconds)
        {
            InstructionId = instructionId;
            TimeSpend = timeSpend;
            TimeType = timeType;
        }
    }
}
