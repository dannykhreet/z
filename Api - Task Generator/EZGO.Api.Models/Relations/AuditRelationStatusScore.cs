using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// Basic score mechanism as used within the database. Currently we only have to update the Score.
    /// -- TASKS
    /// -- Type:Score
    /// -----------------------------------------------------------
    /// -- Score -> Has the score that the user selected
    /// -- TotalScore -> 1 (ignored? is set to MinTaskScore)
    /// -- MaxScore -> MaxTaskScore * Weight
    /// -- MinTaskScore -> minimal value that can be selected
    /// -- MaxTaskScore -> max value that can be selected
    /// -- Type: THUMBS
    /// -----------------------------------------------------------
    /// -- Score -> NULL
    /// -- TotalScore -> MinTaskScore score(Thumb Down) * Weight, MaxTaskScore (Thumb up) * Weight
    /// -- MaxScore -> MaxTaskScore* Weight
    /// -- MinTaskScore -> minimal value that can be with thumb down
    /// -- MaxTaskScore -> max value that can be with thumb up
    /// </summary>
    public class AuditRelationStatusScore : AuditRelationStatus
    {
        /// <summary>
        /// Type Score:
        /// - Has the score that the user selected
        /// Type Thumbs:
        /// - OPTIONAL NOT NEEDED.
        /// </summary>
        public int? Score { get; set; }
        /// <summary>
        /// Type Score:
        /// - 1 (ignored? is set to MinTaskScore or maxTaskScore...or random?) TODO figure out, data is inconstant.
        /// Type Thumbs:
        /// - MinTaskScore score (Thumb Down) * Weight, MaxTaskScore (Thumb up) * Weight.
        /// </summary>
        public int? TotalScore { get; set; }
        /// <summary>
        /// Type Score:
        /// -
        /// Type Thumb:
        /// -
        /// </summary>
        public int? MaxScore { get; set; }

    }
}
