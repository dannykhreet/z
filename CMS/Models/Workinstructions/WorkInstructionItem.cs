using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstructionItem : EZGO.Api.Models.WorkInstructions.InstructionItemTemplate
    {
        public bool isNew { get; set; }
    }
}
