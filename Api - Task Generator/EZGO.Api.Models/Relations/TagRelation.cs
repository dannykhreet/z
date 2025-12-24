using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class TagRelation : Tag
    {
        public int ObjectId { get; set; }
    }
}
