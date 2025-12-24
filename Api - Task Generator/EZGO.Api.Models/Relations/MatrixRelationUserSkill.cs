using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class MatrixRelationUserSkill
    {
        public int Id { get; set; }
        public int MatrixId { get; set; }
        public int UserSkillId { get; set; }
        public int Index { get; set; }
    }
}
