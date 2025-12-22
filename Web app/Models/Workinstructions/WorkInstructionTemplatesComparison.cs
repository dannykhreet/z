using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.WorkInstructions;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstructionTemplatesComparison
    {
        public WorkInstructionTemplate OldTemplate { get; set; }
        public WorkInstructionTemplate NewTemplate { get; set; }

    }
}
