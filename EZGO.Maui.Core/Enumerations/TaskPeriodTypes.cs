using System;
namespace EZGO.Maui.Core.Enumerations
{
    [Flags]
    public enum TaskPeriodTypes
    {
        Shift = 1,

        Today = 2,

        Week = 8,

        OverDue = 16
    }
}
