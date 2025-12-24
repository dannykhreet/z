namespace WebApp.Models.Skills
{
    /// <summary>
    /// Represents a customizable skill level entry in the Skills Matrix Legend.
    /// Supports internationalization and custom color schemes.
    /// </summary>
    public class SkillMatrixLegendItem
    {
        /// <summary>
        /// Unique identifier for the skill level
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
    }

    /// <summary>
    /// Contains the complete legend configuration for a company
    /// </summary>
    public class SkillMatrixLegendConfiguration
    {
        /// <summary>
        /// Unique identifier for the configuration
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Company ID this configuration belongs to
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Version number that increments on updates
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// List of mandatory skill legend items
        /// </summary>
        public System.Collections.Generic.List<SkillMatrixLegendItem> MandatorySkills { get; set; }

        /// <summary>
        /// List of operational skill legend items
        /// </summary>
        public System.Collections.Generic.List<SkillMatrixLegendItem> OperationalSkills { get; set; }

        /// <summary>
        /// Initialize with default collections
        /// </summary>
        public SkillMatrixLegendConfiguration()
        {
            MandatorySkills = new System.Collections.Generic.List<SkillMatrixLegendItem>();
            OperationalSkills = new System.Collections.Generic.List<SkillMatrixLegendItem>();
        }

        /// <summary>
        /// Creates default legend configuration with standard colors and labels
        /// </summary>
        public static SkillMatrixLegendConfiguration CreateDefault()
        {
            return new SkillMatrixLegendConfiguration
            {
                Version = 1,
                MandatorySkills = new System.Collections.Generic.List<SkillMatrixLegendItem>
                {
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 1,
                        SkillType = "mandatory",
                        Label = "Masters the skill",
                        Description = "User has mastered this mandatory skill",
                        IconColor = "#008000",
                        BackgroundColor = "#DDF7DD",
                        Order = 1,
                        IconClass = "thumbsup",
                        IsDefault = true
                    },
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 2,
                        SkillType = "mandatory",
                        Label = "Almost expired",
                        Description = "Skill certification is about to expire",
                        IconColor = "#FFA500",
                        BackgroundColor = "#FFF0D4",
                        Order = 2,
                        IconClass = "warning",
                        IsDefault = true
                    },
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 3,
                        SkillType = "mandatory",
                        Label = "Expired",
                        Description = "Skill certification has expired",
                        IconColor = "#CB0000",
                        BackgroundColor = "#FFEAEA",
                        Order = 3,
                        IconClass = "thumbsdown",
                        IsDefault = true
                    }
                },
                OperationalSkills = new System.Collections.Generic.List<SkillMatrixLegendItem>
                {
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 1,
                        SkillType = "operational",
                        Label = "Doesn't know the theory",
                        Description = "User does not have theoretical knowledge",
                        IconColor = "#CB0000",
                        BackgroundColor = "#FFEAEA",
                        Order = 1,
                        ScoreValue = 1,
                        IsDefault = true
                    },
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 2,
                        SkillType = "operational",
                        Label = "Knows the theory",
                        Description = "User has theoretical knowledge",
                        IconColor = "#FF4500",
                        BackgroundColor = "#FFE4DA",
                        Order = 2,
                        ScoreValue = 2,
                        IsDefault = true
                    },
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 3,
                        SkillType = "operational",
                        Label = "Is able to apply this in the standard situations",
                        Description = "User can apply skill in standard conditions",
                        IconColor = "#FFA500",
                        BackgroundColor = "#FFF0D4",
                        Order = 3,
                        ScoreValue = 3,
                        IsDefault = true
                    },
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 4,
                        SkillType = "operational",
                        Label = "Is able to apply this in the non-standard conditions",
                        Description = "User can apply skill in non-standard conditions",
                        IconColor = "#8DA304",
                        BackgroundColor = "#F2F5DD",
                        Order = 4,
                        ScoreValue = 4,
                        IsDefault = true
                    },
                    new SkillMatrixLegendItem
                    {
                        SkillLevelId = 5,
                        SkillType = "operational",
                        Label = "Can educate others",
                        Description = "User can train and educate other team members",
                        IconColor = "#008000",
                        BackgroundColor = "#DDF7DD",
                        Order = 5,
                        ScoreValue = 5,
                        IsDefault = true
                    }
                }
            };
        }
    }
}
