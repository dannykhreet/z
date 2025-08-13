using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Local
{
    public class LocalActionModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskTemplateId { get; set; }
        public long? TaskId { get; set; }
    }
}
