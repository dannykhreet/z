using DocumentFormat.OpenXml.Vml.Office;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    /// <summary>
    /// UserRelationConverters convert from and to userrelation area objects.
    /// </summary>
    public static class UserRelationConverters
    {
        public static AreaBasic ToBasicArea(this UserRelationArea relationarea)
        {
            if(relationarea != null)
            {
                var area = new AreaBasic();
                area.Id = relationarea.AreaId;
                if (!string.IsNullOrEmpty (relationarea.AreaName))
                {
                    area.Name = relationarea.AreaName;
                }
                if (!string.IsNullOrEmpty(relationarea.AreaNamePath))
                {
                    area.NamePath = relationarea.AreaNamePath;
                }
                return area;
            }

            return null;
        }
    }
}
