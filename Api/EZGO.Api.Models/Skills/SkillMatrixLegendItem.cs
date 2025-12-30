using System;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Skill level identifier (1-5 for operational, 1-3 for mandatory)
        /// </summary>
        [JsonPropertyName("skillLevelId")]
        public int SkillLevelId { get; set; }

        /// <summary>
        /// Type of skill: "mandatory" or "operational"
        /// </summary>
        [JsonPropertyName("skillType")]
        public string SkillType { get; set; }

        /// <summary>
        /// Editable text label for the skill level (supports i18n)
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }

        /// <summary>
        /// Editable description for the skill level (supports org-specific terminology)
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Color of the number/icon (6-digit HEX, e.g., #FF8800)
        /// </summary>
        [JsonPropertyName("iconColor")]
        public string IconColor { get; set; }

        /// <summary>
        /// Background color (6-digit HEX, e.g., #FFFFFF)
        /// </summary>
        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Display order integer (ascending)
        /// </summary>
        [JsonPropertyName("order")]
        public int Order { get; set; }

        /// <summary>
        /// Score value for operational skills (1-5) or null for mandatory
        /// </summary>
        [JsonPropertyName("scoreValue")]
        public int? ScoreValue { get; set; }

        /// <summary>
        /// Icon class name (e.g., "thumbsup", "thumbsdown", "warning")
        /// </summary>
        [JsonPropertyName("iconClass")]
        public string IconClass { get; set; }

        /// <summary>
        /// Whether this is a default system-provided legend item
        /// </summary>
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Reference to the parent configuration
        /// </summary>
        [JsonPropertyName("configurationId")]
        public int ConfigurationId { get; set; }

        /// <summary>
        /// Timestamp when the item was created
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the item was last updated
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
