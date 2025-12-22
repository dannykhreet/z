using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Skills
{
    public class SkillsMatrix : EZGO.Api.Models.Skills.SkillsMatrix
    {
        public List<SkillMatrixLegendItem> LegendItems { get; set; } = new List<SkillMatrixLegendItem>();
    }
}
