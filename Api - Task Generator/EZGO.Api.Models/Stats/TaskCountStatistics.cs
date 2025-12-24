using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class TaskCountStatistics
    {
        public int TotalCount { get; set; }
        public int TodoCount { get; set; }
        public int SkippedCount { get; set; }
        public int NotOkCount { get; set; }
        public int OkCount { get; set; }
        public int WeeklyTasksCount { get; set; }
        public int MonthlyTaskCount { get; set; }
        public int OneTimeOnlyCount { get; set; }
        public int ShiftCount { get; set; }
        public int DailyDynamicCount { get; set; }
        public int DailyPeriodCount { get; set; }
        //public int IsActionOnTheSpotCount { get; set; }
        //public int HasOpenActionsCount { get; set; }
        //public int HasClosedActionCount { get; set; }
        //public int HasCommentCount { get; set; }
        //public int HasDeeplinkCount { get; set; }
        //public int StartTodayCount { get; set; }
        //public int SignedByMeCount { get; set; }
    }
}
