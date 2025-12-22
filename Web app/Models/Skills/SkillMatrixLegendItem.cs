using System.Collections.Generic;

namespace WebApp.Models.Skills
{
    public class SkillMatrixLegendItem
    {
        public const string TypeMandatory = "mandatory";
        public const string TypeOperational = "operational";
        public const string TypeOperationalStatus = "operational-status";

        public string Key { get; set; }
        public string Type { get; set; }
        public int? Score { get; set; }
        public string Label { get; set; }
        public string IconText { get; set; }
        public string IconClass { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public int DisplayOrder { get; set; }

        public static List<SkillMatrixLegendItem> CreateDefaultLegend()
        {
            return new List<SkillMatrixLegendItem>
            {
                new SkillMatrixLegendItem
                {
                    Key = "mandatory-master",
                    Type = TypeMandatory,
                    Score = 2,
                    IconClass = "thumbsup",
                    Label = "Masters the skill",
                    BackgroundColor = "#ddf7dd",
                    TextColor = "#5eaf5e",
                    DisplayOrder = 1
                },
                new SkillMatrixLegendItem
                {
                    Key = "mandatory-almost-expired",
                    Type = TypeMandatory,
                    Score = 5,
                    IconClass = "warning",
                    Label = "Almost expired",
                    BackgroundColor = "#fff0d4",
                    TextColor = "#ffa500",
                    DisplayOrder = 2
                },
                new SkillMatrixLegendItem
                {
                    Key = "mandatory-expired",
                    Type = TypeMandatory,
                    Score = 1,
                    IconClass = "thumbsdown",
                    Label = "Expired",
                    BackgroundColor = "#ffeaea",
                    TextColor = "#cb0000",
                    DisplayOrder = 3
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-5",
                    Type = TypeOperational,
                    Score = 5,
                    IconText = "5",
                    Label = "Can educate others",
                    BackgroundColor = "#ddf7dd",
                    TextColor = "#008000",
                    DisplayOrder = 1
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-4",
                    Type = TypeOperational,
                    Score = 4,
                    IconText = "4",
                    Label = "Is able to apply it in non-standard conditions",
                    BackgroundColor = "#f2f5dd",
                    TextColor = "#8da304",
                    DisplayOrder = 2
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-3",
                    Type = TypeOperational,
                    Score = 3,
                    IconText = "3",
                    Label = "Is able to apply it in standard situations",
                    BackgroundColor = "#fff0d4",
                    TextColor = "#ffa500",
                    DisplayOrder = 3
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-2",
                    Type = TypeOperational,
                    Score = 2,
                    IconText = "2",
                    Label = "Knows the theory",
                    BackgroundColor = "#ffe4da",
                    TextColor = "#ff4500",
                    DisplayOrder = 4
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-1",
                    Type = TypeOperational,
                    Score = 1,
                    IconText = "1",
                    Label = "Doesn't know the theory",
                    BackgroundColor = "#ffeaea",
                    TextColor = "#cb0000",
                    DisplayOrder = 5
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-expired",
                    Type = TypeOperationalStatus,
                    IconClass = "warning-icon",
                    Label = "Operational skill expired",
                    BackgroundColor = "#ffeaea",
                    TextColor = "#cb0000",
                    DisplayOrder = 6
                },
                new SkillMatrixLegendItem
                {
                    Key = "operational-almost-expired",
                    Type = TypeOperationalStatus,
                    IconClass = "warning",
                    Label = "Operational almost expired",
                    BackgroundColor = "#fff0d4",
                    TextColor = "#ffa500",
                    DisplayOrder = 7
                }
            };
        }
    }
}
