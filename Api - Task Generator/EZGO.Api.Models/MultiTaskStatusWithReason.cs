using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class MultiTaskStatusWithReason
    {
        public List<int> TaskIds { get; set; }
        public int Status { get; set; }
        public DateTime SignedAtUtc { get; set; }
        public string Comment { get; set; }
    }
}
