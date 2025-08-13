using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Messages
{
    public class ActionChangedMessageArgs
    {
        public int TaskTemplateId { get; set; }
        public int TaskId { get; set; }
        public ChangeType TypeOfChange { get; set; }

        public enum ChangeType
        {
            None,
            Created,
            SetToResolved,
        }
    }

}
