namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// Simple legend item with only the customizable properties.
    /// </summary>
    public class SkillMatrixLegendItem
    {
        /// <summary>
        /// Skill level identifier (1-5 for operational, 1-3 for mandatory)
        /// </summary>
        public int SkillLevelId { get; set; }

        /// <summary>
        /// Custom text label for this skill level
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Color of the icon/number (HEX, e.g., #FF8800)
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// Background color (HEX, e.g., #FFFFFF)
        /// </summary>
        public string BackgroundColor { get; set; }
    }
}
