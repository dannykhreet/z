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
    }
}
