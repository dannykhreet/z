using System;
using System.Collections.Generic;
using System.ComponentModel;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Instructions
{
    //TODO Add ToBase
    public class InstructionsModel : WorkInstructionTemplate, IItemFilter<InstructionTypeEnum>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new List<InstructionItem> InstructionItems { get; set; }

        public int WorkInstructionTemplateId { get; set; }

        private InstructionTypeEnum instructionTypeEnum;

        public InstructionTypeEnum FilterStatus { get => WorkInstructionType; set => instructionTypeEnum = value; }

        public bool HasUnreadChanges => UnreadChangesNotificationsCount > 0;
    }
}
