using EZGO.Api.Models.Enumerations;

namespace EZGO.Api.Helper
{
    public static class SortParameterHelper
    {
        public static SortColumnTypeEnum? ParseSortColumn(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return null;

            return sort.ToLower() switch
            {
                "id" => SortColumnTypeEnum.Id,
                "name" => SortColumnTypeEnum.Name,
                "duedate" => SortColumnTypeEnum.DueDate,
                "startdate" => SortColumnTypeEnum.StartDate,
                "modifiedat" => SortColumnTypeEnum.ModifiedAt,
                "areaname" => SortColumnTypeEnum.AreaName,
                "username" => SortColumnTypeEnum.UserName,
                "lastcommentdate" => SortColumnTypeEnum.LastCommentDate,
                "priority" => SortColumnTypeEnum.Priority,
                _ => null
            };
        }

        public static SortColumnDirectionTypeEnum? ParseSortDirection(string direction)
        {
            if (string.IsNullOrWhiteSpace(direction))
                return null;

            return direction.ToLower() switch
            {
                "asc" => SortColumnDirectionTypeEnum.Ascending,
                "ascending" => SortColumnDirectionTypeEnum.Ascending,
                "desc" => SortColumnDirectionTypeEnum.Descending,
                "descending" => SortColumnDirectionTypeEnum.Descending,
                _ => null
            };
        }

        public static string ToSortString(this SortColumnTypeEnum sortColumn)
        {
            return sortColumn.ToString().ToLower();
        }

        public static string ToSortString(this SortColumnDirectionTypeEnum sortDirection)
        {
            return sortDirection == SortColumnDirectionTypeEnum.Ascending ? "asc" : "desc";
        }
    }
}
