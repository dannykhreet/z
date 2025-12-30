using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// StatusEnum; Statuses that are available for certain item.
    /// When using for submitting to the EZGO Api, always use the value (int) for posting.
    /// </summary>
    public enum StatusTypeEnum
    {
        /// <summary>
        /// Skipped; 
        /// </summary>
        Skipped = 0,
        /// <summary>
        /// NotOk; 
        /// </summary>
        NotOk = 1,
        /// <summary>
        /// Ok; 
        /// </summary>
        Ok = 2,
        /// <summary>
        /// [Todo];
        /// </summary>
        Todo = 3
    }

}
