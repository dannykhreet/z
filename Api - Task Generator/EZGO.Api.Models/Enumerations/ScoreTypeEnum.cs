using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// ScoreTypeEnum; is used within Audits. ScoreType is a lowercase string in the database. In the API we will use this Enum.
    /// If we need to check something against the database data, use ToString and ToLowercase within the logic to emulate this behavior.
    /// When using for submitting to the EZGO API, always use the value (int) for posting.
    /// </summary>
    public enum ScoreTypeEnum
    {
        /// <summary>
        /// Score; in database 'score'.
        /// </summary>
        Score = 0,
        /// <summary>
        /// Thumbs; in database 'thumbs'.
        /// </summary>
        Thumbs = 1
    }

}
