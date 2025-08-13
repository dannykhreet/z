using EZGO.Api.Models.Enumerations;
using System;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class LocalTaskStatusModel
    {
        public long TaskId { get; set; }

        public int Status { get; set; }

        public int SignedById { get; set; }

        public DateTime SignedAtUtc { get; set; }

        public bool Posted { get; set; }
    }
}
