using System;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// Represents a customizable skill level entry in the Skills Matrix Legend.
    /// Supports internationalization and custom color schemes.
    /// </summary>
    public class SkillMatrixLegendItem
    {
        /// <summary>
        /// Unique identifier for the legend item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Skill level identifier (1-5 for operational, 1-3 for mandatory)
        /// </summary>
        public int SkillLevelId { get; set; }

        /// <summary>
        /// Type of skill: "mandatory" or "operational"
        /// </summary>
        public string SkillType { get; set; }

        /// <summary>
        /// Editable text label for the skill level (supports i18n)
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Editable description for the skill level (supports org-specific terminology)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Color of the number/icon (6-digit HEX, e.g., #FF8800)
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// Background color (6-digit HEX, e.g., #FFFFFF)
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Display order integer (ascending)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Score value for operational skills (1-5) or null for mandatory
        /// </summary>
        public int? ScoreValue { get; set; }

        /// <summary>
        /// Icon class name (e.g., "thumbsup", "thumbsdown", "warning")
        /// </summary>
        public string IconClass { get; set; }

        /// <summary>
        /// Whether this is a default system-provided legend item
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Company ID this item belongs to
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Timestamp when the item was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the item was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User ID who created this item
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// User ID who last updated this item
        /// </summary>
        public int? UpdatedBy { get; set; }
    }
}
