using EZGO.Api.Models.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    /// <summary>
    /// FilterConverterExtensions; Convert filters from one source to another source.
    /// </summary>
    public static class FilterConverterExtensions
    {
        public static TaskFilters ToTaskFilters(this AuditFilters? auditFilter)
        {
            var output = new TaskFilters();

            if(auditFilter.HasValue && auditFilter.Value.HasFilters())
            {
                output.AllowedOnly = auditFilter.Value.AllowedOnly;
                output.TemplateId = auditFilter.Value.TemplateId;
                output.AreaId = auditFilter.Value.AreaId;
                output.FilterAreaType = auditFilter.Value.FilterAreaType;
                output.IsCompleted = auditFilter.Value.IsCompleted;
                output.Limit = auditFilter.Value.Limit;
                output.Offset = auditFilter.Value.Offset;
                output.Timestamp = auditFilter.Value.Timestamp;
                output.StartTimestamp = auditFilter.Value.StartTimestamp;
                output.EndTimestamp = auditFilter.Value.EndTimestamp;
                output.TimespanType = auditFilter.Value.TimespanType;
            }

            return output;
        }

        public static TaskFilters ToTaskFilters(this ChecklistFilters? checklistFilter)
        {
            var output = new TaskFilters();

            if (checklistFilter.HasValue && checklistFilter.Value.HasFilters())
            {
                output.AllowedOnly = checklistFilter.Value.AllowedOnly;
                output.TemplateId = checklistFilter.Value.TemplateId;
                output.AreaId = checklistFilter.Value.AreaId;
                output.FilterAreaType = checklistFilter.Value.FilterAreaType;
                output.IsCompleted = checklistFilter.Value.IsCompleted;
                output.Limit = checklistFilter.Value.Limit;
                output.Offset = checklistFilter.Value.Offset;
                output.Timestamp = checklistFilter.Value.Timestamp;
                output.StartTimestamp = checklistFilter.Value.StartTimestamp;
                output.EndTimestamp = checklistFilter.Value.EndTimestamp;
                output.TimespanType = checklistFilter.Value.TimespanType;
            }

            return output;
        }
    }
}
