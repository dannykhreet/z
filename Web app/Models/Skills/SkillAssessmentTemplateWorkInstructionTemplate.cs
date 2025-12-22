using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.WorkInstructions;

namespace WebApp.Models.Skills
{
    public class SkillAssessmentTemplateWorkInstructionTemplate : EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate
    {
        public new string Role
        {
            get { return base.Role.HasValue ? base.Role.ToString().ToLower() : string.Empty; }
        }

        public int WorkInstructionTemplateId
        {
            get { return base.Id; }
        }
    }
}
