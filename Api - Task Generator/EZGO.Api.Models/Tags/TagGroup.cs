using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tags
{
    public class TagGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public List<Tag> Tags { get; set; }
        public bool? IsSelected { get; set; }
        public bool? IsInUse { get; set; }
    }
}
