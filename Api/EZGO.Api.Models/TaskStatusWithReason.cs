using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class TaskStatusWithReason
    {
        public int TaskId { get; set; }
        public int Status { get; set; }
        public DateTime SignedAtUtc { get; set; }
        public string Comment { get; set; }
    }
}
