using System;
using System.Collections.Generic;
using System.Text;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;

namespace EZGO.Api.Models.Filters
{
    public struct Gen4TaskFilters
    {
        public DateTime? StartTimestamp;
        public DateTime? EndTimestamp;

        public TaskTimeSpanEnum? Timespan;
        public int TimespanOffset;
        public int AreaId;

        public List<TaskStatusEnum> Statuses;
        public int[] TagIds;
        public string FilterText;
        public List<RecurrencyTypeEnum> RecurrencyTypes;

        public int? Limit;
        public int? Offset;

        // Time related filters
        public ShiftDayWeekTimestamps ShiftDayWeekTimestamps;
        public ShiftTimestamps ShiftTimestamps;

        // Display filters
        public bool? AllowDuplicateTaskInstances;
    }
}
