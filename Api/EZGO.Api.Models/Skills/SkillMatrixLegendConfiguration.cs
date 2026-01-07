using System.Collections.Generic;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// Simple legend configuration containing custom labels and colors.
    /// </summary>
    public class SkillMatrixLegendConfiguration
    {
        /// <summary>
        /// Mandatory skill legend items (skillLevelId 1-3)
        /// </summary>
        public List<SkillMatrixLegendItem> MandatorySkills { get; set; }

        /// <summary>
        /// Operational skill legend items (skillLevelId 1-5)
        /// </summary>
        public List<SkillMatrixLegendItem> OperationalSkills { get; set; }

        public SkillMatrixLegendConfiguration()
        {
            MandatorySkills = new List<SkillMatrixLegendItem>();
            OperationalSkills = new List<SkillMatrixLegendItem>();
        }
    }
}
