using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class ActionAreaRelation
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public int AreaId { get; set; }
    }
}
