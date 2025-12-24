using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations.Base
{
    public class BaseRelationWorkInstruction
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public List<string> Media { get; set; }
    }
}
