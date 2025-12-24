using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.General
{
    /// <summary>
    /// UpdateCheckCommentItem; Check item for use with the update check functionality. Specifically for Comment items. 
    /// </summary>
    public class UpdateCheckCommentItem
    {
        public int ActionId { get; set; }
        public int NumberOfItems { get; set; }
    }
}
