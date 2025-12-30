using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    public struct SearchFilters
    {
        public int? AreaId { get; set; }
        public List<string> Types { get; set; }
        public List<int> Roles { get; set; }
        public bool? HasItemsAttached { get; set; }
        public bool? HasSubItemsAttached { get; set; }
        public bool? HasVideoAttached { get; set; }
        public bool? HasPictureAttached { get; set; }
        public bool? HasChildren { get; set; }
        public bool? HasSignedChildren { get; set; }
        public int UserId { get; set; }
        public bool? MyItems { get; set; }
        public bool? AssignedToMe { get; set; }
        public string SearchValue { get; set; }
        public int? Limit { get; set; }
        public int? OffSet { get; set; }
        public SortColumnTypeEnum? SortColumn { get; set; }
        public SortColumnDirectionTypeEnum? SortDirection { get; set; }

    }
}
