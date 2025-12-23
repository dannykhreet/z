using System.Collections.Generic;
using System.Linq;

namespace WebApp.Models.Skills
{
    public class SkillsMatrix : EZGO.Api.Models.Skills.SkillsMatrix
    {
        public List<SkillMatrixLegendItem> LegendEntries { get; set; } = new();

        public int LegendVersion { get; set; }

        public bool HasLegend => LegendEntries != null && LegendEntries.Any();

        public void EnsureLegendDefaults()
        {
            if (LegendEntries != null && LegendEntries.Any())
            {
                return;
            }

            LegendEntries = new List<SkillMatrixLegendItem>
            {
                new SkillMatrixLegendItem { SkillLevelId = "mandatory_master", Label = "Masters the skill", Description = "Certified or self-confirmed mastery", IconColor = "#52AC52", BackgroundColor = "#E5F3E5", Order = 1, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "mandatory_warning", Label = "Almost expired", Description = "Expiring soon", IconColor = "#F2A541", BackgroundColor = "#FFF4E4", Order = 2, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "mandatory_expired", Label = "Expired", Description = "Certification expired", IconColor = "#D9534F", BackgroundColor = "#FDEDEC", Order = 3, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_1", Label = "Doesn't know the theory", Description = "Needs training", IconColor = "#D9534F", BackgroundColor = "#FDEDEC", Order = 4, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_2", Label = "Knows the theory", Description = "Can explain basics", IconColor = "#F28E2B", BackgroundColor = "#FFF4E4", Order = 5, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_3", Label = "Applies in standard situations", Description = "Works independently", IconColor = "#F5B041", BackgroundColor = "#FFF8E1", Order = 6, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_4", Label = "Applies in non-standard conditions", Description = "Handles deviations", IconColor = "#8BC34A", BackgroundColor = "#EAF4DC", Order = 7, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_5", Label = "Can educate others", Description = "Coaches colleagues", IconColor = "#52AC52", BackgroundColor = "#E5F3E5", Order = 8, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_expired", Label = "Operational skill expired", Description = "Reassessment needed", IconColor = "#C0392B", BackgroundColor = "#FDEDEC", Order = 9, Version = LegendVersion },
                new SkillMatrixLegendItem { SkillLevelId = "operational_warning", Label = "Operational almost expired", Description = "Expiry warning", IconColor = "#F28E2B", BackgroundColor = "#FFF4E4", Order = 10, Version = LegendVersion }
            };
        }
    }
}
