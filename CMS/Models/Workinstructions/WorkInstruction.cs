using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.WorkInstructions;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstruction : EZGO.Api.Models.WorkInstructions.WorkInstruction
    {
        public new List<WorkInstructionItem> InstructionItems { get; set; } //hide base implementation, use custom implementation for webapp
    }
}
