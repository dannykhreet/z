using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebApp.Models.Skills
{
    public class SkillMatrixLegendPayload
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("entries")]
        public List<SkillMatrixLegendItem> Entries { get; set; } = new();
    }
}
