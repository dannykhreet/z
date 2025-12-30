using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// ShiftFilters; ShiftFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The ShiftFilters are used for Shifts functionality.
    /// </summary>
    public struct ShiftFilters
    {
        public int? Day;
        public int? AreaId;
        public FilterAreaTypeEnum? FilterAreaType;
        public bool HasFilters() {
            return (Day.HasValue || AreaId.HasValue);
        }
    }
}
