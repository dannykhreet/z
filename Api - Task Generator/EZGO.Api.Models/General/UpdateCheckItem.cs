using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.General
{
    /// <summary>
    /// UpdateCheckItem; Check item for use with the update check functionality. General update check item. 
    /// </summary>
    public class UpdateCheckItem
    {
        public UpdateCheckTypeEnum UpdateCheckType { get; set; }
        public int NumberOfItems { get; set; }
        public List<int> Ids { get; set; }
    }
}
