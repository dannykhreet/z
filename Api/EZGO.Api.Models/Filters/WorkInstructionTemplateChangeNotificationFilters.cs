using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// WorkInstructionFilters; WorkInstruction filters for filtering work instruction collections.
    /// </summary>
    public struct WorkInstructionTemplateChangeNotificationFilters
    {
        public int? WorkInstructionTemplateId { get; set; }
        public DateTime? StartTimestamp { get; set; }
        public DateTime? EndTimeStamp { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public int? AreaId { get; set; }
        public int? UserId { get; set; }
        public bool? Confirmed { get; set; }

        public bool HasFilters()
        {
            return WorkInstructionTemplateId.HasValue || StartTimestamp.HasValue || EndTimeStamp.HasValue;
        }
    }
}
