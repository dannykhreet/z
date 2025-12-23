using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApp.Models.Skills
{
    public class SkillMatrixLegendItem
    {
        private const string HexPattern = "^#([A-Fa-f0-9]{6})$";

        [Required]
        [JsonProperty("skill_level_id")]
        public string SkillLevelId { get; set; }

        [Required]
        [MinLength(1)]
        [JsonProperty("label")]
        public string Label { get; set; }

        [Required]
        [MinLength(1)]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Required]
        [RegularExpression(HexPattern)]
        [JsonProperty("icon_color")]
        public string IconColor { get; set; }

        [Required]
        [RegularExpression(HexPattern)]
        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [Range(0, int.MaxValue)]
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Version is incremented every time the legend set is changed to support concurrency control.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }

        public IEnumerable<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SkillLevelId))
            {
                errors.Add("skill_level_id is required");
            }

            if (string.IsNullOrWhiteSpace(Label))
            {
                errors.Add("label is required");
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                errors.Add("description is required");
            }

            if (!string.IsNullOrEmpty(IconColor) && !Regex.IsMatch(IconColor, HexPattern))
            {
                errors.Add("icon_color must be a 6-digit hex value prefixed with '#'");
            }

            if (!string.IsNullOrEmpty(BackgroundColor) && !Regex.IsMatch(BackgroundColor, HexPattern))
            {
                errors.Add("background_color must be a 6-digit hex value prefixed with '#'");
            }

            if (Order < 0)
            {
                errors.Add("order must be zero or higher");
            }

            return errors.Distinct();
        }
    }
}
