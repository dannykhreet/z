using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    public enum PropertyFieldTypeEnum
    {
        Custom = 0,
        SingleValue = 1,
        Range = 2,
        UpperLimit = 3,
        LowerLimit = 4,
        EqualTo = 5,
        UpperLimitEqualTo = 6, //not yet implemented
        LowerLimitEqualTo = 7 //not yet implemented
    }
}
