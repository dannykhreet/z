using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class ObjectTasksCounters
    {
        public int ParentObjectId { get; set; }
        public int TaskId { get; set; }
        public int ActionNr { get; set; }
        public int CommentNr { get; set; }
        public int CompanyId { get; set; }
    }
}
