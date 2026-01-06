using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// ScoringMethodEnum is used to indicate the method that is used to get the final score of a user skill value
    /// </summary>
    public enum ScoringMethodEnum
    {
        /// <summary>
        /// The score is a manual input by a user and was not calculated by the system
        /// </summary>
        Manual = 0,
        /// <summary>
        /// The score is the result of an assessment and was calculated by the system as the average score of all assessment item scores
        /// </summary>
        Assessment = 1
    }
}
