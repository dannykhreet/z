using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    public class WorkInstructionTemplateChange
    {
        public string PropertyName { get; set; }
        public string TranslationKey { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
