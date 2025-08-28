using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Actions
{
    public class ActionCommentUpdateModel
    {
        public List<string> PostedMessageIds { get; set; }
        public List<ActionCommentModel> NewComments { get; set; }
    }
}
