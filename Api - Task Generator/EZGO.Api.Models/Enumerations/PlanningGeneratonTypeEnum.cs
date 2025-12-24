using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    public enum PlanningGeneratonTypeEnum
    {
        [Description("Area")]
        Area = 1,
        [Description("Shift")]
        Shift = 2,
        [Description("Period")]
        Period = 3
    }
}
