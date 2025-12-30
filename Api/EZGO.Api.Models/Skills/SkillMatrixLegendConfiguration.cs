using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// Contains the complete legend configuration for a company's Skills Matrix.
    /// Supports version tracking for configuration updates.
    /// </summary>
    public class SkillMatrixLegendConfiguration
    {
        /// <summary>
        /// Unique identifier for the configuration
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Company ID this configuration belongs to
        /// </summary>
        [JsonPropertyName("companyId")]
        public int CompanyId { get; set; }

        /// <summary>
        /// Version number that increments on updates for tracking changes
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; }

        /// <summary>
        /// List of mandatory skill legend items
        /// </summary>
        [JsonPropertyName("mandatorySkills")]
        public List<SkillMatrixLegendItem> MandatorySkills { get; set; }

        /// <summary>
        /// List of operational skill legend items
        /// </summary>
        [JsonPropertyName("operationalSkills")]
        public List<SkillMatrixLegendItem> OperationalSkills { get; set; }

        /// <summary>
        /// Timestamp when the configuration was created
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the configuration was last updated
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User ID who created this configuration
        /// </summary>
        [JsonPropertyName("createdBy")]
        public int? CreatedBy { get; set; }

        /// <summary>
        /// User ID who last updated this configuration
        /// </summary>
        [JsonPropertyName("updatedBy")]
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Initialize with default collections
        /// </summary>
        public SkillMatrixLegendConfiguration()
        {
            MandatorySkills = new List<SkillMatrixLegendItem>();
            OperationalSkills = new List<SkillMatrixLegendItem>();
        }

        /// <summary>
        /// Creates default legend configuration with standard colors and labels
        /// </summary>
        /// <param name="companyId">The company ID for this configuration</param>
        /// <returns>A new configuration with default values</returns>
        public static SkillMatrixLegendConfiguration CreateDefault(int companyId)
        {
            var config = new SkillMatrixLegendConfiguration
            {
                CompanyId = companyId,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                MandatorySkills = new List<SkillMatrixLegendItem>
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
                    }
                },
                OperationalSkills = new List<SkillMatrixLegendItem>
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
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
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            return config;
        }
    }
}
