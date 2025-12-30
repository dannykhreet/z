using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// TaskStatusEnum; Statuses that are available for a Task.
    /// When using for submitting to the EZGO Api, always use the value (int) for posting.
    /// </summary>
    public enum TaskStatusEnum
    {
        /// <summary>
        /// Skipped; in database 'skipped'.
        /// </summary>
        Skipped = 0,
        /// <summary>
        /// NotOk; in database 'not ok'
        /// </summary>
        NotOk = 1,
        /// <summary>
        /// Ok; in database 'ok'
        /// </summary>
        Ok = 2,
        /// <summary>
        /// [Todo]; in database 'todo'
        /// </summary>
        Todo = 3
    }

}
