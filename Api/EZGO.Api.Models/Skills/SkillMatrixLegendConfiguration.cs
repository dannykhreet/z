using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// Contains the complete legend configuration for a company's Skills Matrix.
    /// Groups legend items by skill type.
    /// </summary>
    public class SkillMatrixLegendConfiguration
    {
        /// <summary>
        /// Company ID this configuration belongs to
        /// </summary>
        [JsonPropertyName("companyId")]
        public int CompanyId { get; set; }

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
        /// Initialize with default collections
        /// </summary>
        public SkillMatrixLegendConfiguration()
        {
            MandatorySkills = new List<SkillMatrixLegendItem>();
            OperationalSkills = new List<SkillMatrixLegendItem>();
        }
    }
}
