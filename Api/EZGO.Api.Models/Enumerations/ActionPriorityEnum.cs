using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// ActionPriorityEnum; Added 4 level action priority type.
    /// Retrieve description with .GetCustomAttributes(typeof(DescriptionAttribute) functionality.
    /// </summary>
    public enum ActionPriorityEnum
    {
        [Description("Critical")]
        Critical = 1,
        [Description("High")]
        Important = 2,
        [Description("Normal")]
        Normal = 3,
        [Description("Low")]
        Low = 4
    }
}
