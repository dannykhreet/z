using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class CompanyStatistics
    {
        public List<StatisticTypeItem> CompanyBasicStatistics { get; set; }

        public List<StatisticMonthYearItem> ChecklistStatistics { get; set; }
        public List<StatisticMonthYearItem> AuditsStatistics { get; set; }
        public List<StatisticMonthYearItem> TasksStatistics { get; set; }
        public List<StatisticMonthYearItem> TasksOkStatistics { get; set; }
        public List<StatisticMonthYearItem> TasksSkippedStatistics { get; set; }
        public List<StatisticMonthYearItem> TasksNotOkStatistics { get; set; }
        public List<StatisticMonthYearItem> AssessmentsStatistics { get; set; }
        public List<StatisticMonthYearItem> ActionCreatedStatistics { get; set; }
        public List<StatisticMonthYearItem> ActionDueAtStatistics { get; set; }
        public List<StatisticMonthYearItem> CommentCreatedStatistics { get; set; }
        public List<StatisticMonthYearItem> TaskTemplateStatistics { get; set; }
        public List<StatisticMonthYearItem> ChecklistTemplateStatistics { get; set; }
        public List<StatisticMonthYearItem> AuditTemplateStatistics { get; set; }
        public List<StatisticMonthYearItem> AssessmentTemplateStatistics { get; set; }
        public List<StatisticMonthYearItem> WorkInstructionTemplateStatistics { get; set; }

    }
}
