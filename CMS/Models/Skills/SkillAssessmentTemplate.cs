using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Skills
{
    public class SkillAssessmentTemplate : EZGO.Api.Models.Skills.AssessmentTemplate
    {
        ///// <summary>
        ///// For script compatibility;
        ///// </summary>
        public bool IsSignatureRequired { get; set; }
        ///// <summary>
        ///// For script compatibility;
        ///// </summary>
        public bool IsDoubleSignatureRequired { get; set; }
        ///// <summary>
        ///// For script compatibility;
        ///// </summary>
        public List<SkillAssessmentTemplateSkillInstruction> TaskTemplates { get; set; }

    }

    public class SkillAssessmentTemplateSkillInstruction : EZGO.Api.Models.Skills.AssessmentTemplateSkillInstruction
    {
        ///// <summary>
        ///// For script compatibility;
        ///// </summary>
        public new string Role { get; set; }

        public EZGO.Api.Models.Enumerations.RoleTypeEnum BaseRole
        {
            get { return base.Role ?? EZGO.Api.Models.Enumerations.RoleTypeEnum.Basic; }
        }

        public bool isNew { get; set; }

    }
}
  