using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    public enum PropertyFieldKindTypeEnum
    {
        Custom = 0, //can not be used for general reporting.
        Uri = 1,
        Temperature = 10,
        Pressure = 11,
        Distance = 12,
        Time = 13
    }
}
