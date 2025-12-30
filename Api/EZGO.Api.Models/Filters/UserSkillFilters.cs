using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    public struct UserSkillFilters
    {
        public SkillTypeEnum? SkillType;
        public int? UserSkillId; //for retrieval of values
        public int? Limit;
        public int? Offset;


        public bool HasFilters()
        {
            return (SkillType.HasValue || Limit.HasValue || Offset.HasValue || UserSkillId.HasValue);
        }
    }
}
