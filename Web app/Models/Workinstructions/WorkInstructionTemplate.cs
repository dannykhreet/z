using EZGO.Api.Models.Enumerations;
using System.Collections.Generic;

namespace WebApp.Models.WorkInstructions
{
    public class WorkInstructionTemplate : EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate
    {
        public new List<WorkInstructionItem> InstructionItems { get; set; } //hide base implementation, use custom implementation for webapp

        ///// <summary>
        ///// For script compatibility;
        ///// </summary>
        public List<WorkInstructionItem> TaskTemplates { get; set; }

        ///// <summary>
        ///// For script compatibility;
        ///// </summary>
        public new string Role { get; set; }

        public RoleTypeEnum BaseRole
        {
            get { return base.Role ?? RoleTypeEnum.Basic; }
        }
        public int? SharedTemplateId { get; set; }
    }
}
