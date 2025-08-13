using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Enumerations
{
    public enum ActionStatusEnum
    {
        /// <summary>
        /// basic status
        /// </summary>
        Unsolved = 0,
        /// <summary>
        /// Bool IsResolved == true
        /// </summary>
        Solved = 1,
        /// <summary>
        /// Date due passed and bool IsResolved == false
        /// </summary>
        PastDue = 2
    }
}
