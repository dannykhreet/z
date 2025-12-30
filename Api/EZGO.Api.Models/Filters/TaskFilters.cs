using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// TaskFilters; TaskFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The TaskFilters are used for Task functionality. This means TaskRecurrencies, TaskTemplates and Tasks
    /// </summary>
    public struct TaskFilters
    {
        public int? AreaId;
        public int[] AreaIds; //for multi select area ids
        public FilterAreaTypeEnum? FilterAreaType;
        public MonthRecurrencyTypeEnum? MonthRecurrencyType;
        public RecurrencyTypeEnum? RecurrencyType;
        public TaskTypeEnum? TaskType;
        public RoleTypeEnum? Role;
        public TaskStatusEnum? Status;
        public List<TaskStatusEnum> Statuses; //for multi select statuses
        public int? TemplateId;
        public int? RecurrencyId;
        public bool? IsSigned;
        public int? ShiftId;
        /// <summary>
        /// Weekdays; List of weekday numbers, weekdays start on day 0 and end on day 6.
        /// E.g. if the week starts on Sunday the list would be:
        /// 0 = Sunday
        /// 1 = Monday
        /// 2 = Tuesday
        /// 3 = Wednesday
        /// 4 = Thursday
        /// 5 = Friday
        /// 6 = Saturday
        /// If you in the case above want the recurrencies for the weekend you would supply 0,6.
        /// </summary>
        public List<int> Weekdays;
        public DateTime? Timestamp;
        public DateTime? StartTimestamp; 
        public DateTime? EndTimestamp; 
        public TimespanTypeEnum? TimespanType;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;
        public bool? IsCompleted;
        public int[] TagIds;
        public string FilterText;
        public List<RoleTypeEnum> Roles;
        public bool? InstructionsAdded; //used within CMS filters
        public bool? ImagesAdded; //used within CMS filters
        public bool? VideosAdded; //used within CMS filters
        public List<RecurrencyTypeEnum> RecurrencyTypes; //for multi select recurrency types
        public bool DeduplicatedForDisplay;
        public bool RemoveOverDueTasks;

        public SortColumnTypeEnum? SortColumn { get; set; }
        public SortColumnDirectionTypeEnum? SortDirection { get; set; }

        public bool HasFilters()
        {
            return (AreaId.HasValue || MonthRecurrencyType.HasValue || RecurrencyType.HasValue || TemplateId.HasValue || ShiftId.HasValue || Role.HasValue ||  TaskType.HasValue || RecurrencyId.HasValue || IsSigned.HasValue || Timestamp.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue  || Status.HasValue || RecurrencyType.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || (Weekdays != null && Weekdays.Count > 0) || TimespanType.HasValue || (TagIds != null && TagIds.Length > 0) || InstructionsAdded.HasValue || ImagesAdded.HasValue || VideosAdded.HasValue);
        }

    }
}
