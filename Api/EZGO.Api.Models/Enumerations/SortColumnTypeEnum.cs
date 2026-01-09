using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    public enum SortColumnTypeEnum
    {
        Id,
        Name,
        DueDate,
        StartDate,
        ModifiedAt,
        AreaName,
        UserName,
        LastCommentDate,
        Priority
    }

    public enum SortColumnDirectionTypeEnum
    {
        Ascending,
        Descending
    }
}
