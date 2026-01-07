using System.Collections.Generic;

namespace WebApp.Models.Skills
{
    /// <summary>
    /// Legend configuration with custom labels and colors per skill level.
    /// </summary>
    public class SkillMatrixLegendConfiguration
    {
        public List<SkillMatrixLegendItem> MandatorySkills { get; set; } = new List<SkillMatrixLegendItem>();
        public List<SkillMatrixLegendItem> OperationalSkills { get; set; } = new List<SkillMatrixLegendItem>();

        /// <summary>
        /// Get mandatory skill item by level, or null if not configured.
        /// </summary>
        public SkillMatrixLegendItem GetMandatory(int skillLevelId)
        {
            return MandatorySkills?.Find(x => x.SkillLevelId == skillLevelId);
        }

        /// <summary>
        /// Get operational skill item by level, or null if not configured.
        /// </summary>
        public SkillMatrixLegendItem GetOperational(int skillLevelId)
        {
            return OperationalSkills?.Find(x => x.SkillLevelId == skillLevelId);
        }
    }

    public class SkillMatrixLegendItem
    {
        public int SkillLevelId { get; set; }
        public string Label { get; set; }
        public string IconColor { get; set; }
        public string BackgroundColor { get; set; }
    }
}
