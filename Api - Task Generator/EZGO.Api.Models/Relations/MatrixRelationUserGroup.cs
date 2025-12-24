using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class MatrixRelationUserGroup
    {
        public int Id { get; set; }
        public int MatrixId { get; set; }
        public int UserGroupId { get; set; }
        public int Index { get; set; }
    }
}
