using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Operations
{
    public enum OperationTypes { Task = 1, Checklist = 2 }

    public class Batch
    {
        public List<Operation> Operations { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
    }

    public class Operation
    {
        public string Parameters { get; set; }
        public int Type { get; set; }
        public string StringType { get; set; }
        public string Body { get; set; }
    }
}