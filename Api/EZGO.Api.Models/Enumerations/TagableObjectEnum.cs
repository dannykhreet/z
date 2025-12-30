using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    //NOTE: LEAVE THE NUMBERS
    //AllowedOnObjectTypes in the DB are numbers, so the number must correspond to a TagableObject
    public enum TagableObjectEnum
    {
        Action = 0,
        Assessment = 1,
        Audit = 2,
        Checklist = 3,
        Comment = 4,
        Task = 5,
        WorkInstruction = 6,
        StageTemplate = 7,
        Stage = 8
    }
}
