using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class AreaFunctionalLocationRelation
    {
        public int Id { get; set; }
        public int AreaId { get; set; }
        public int LocationId { get; set; }
    }
}
