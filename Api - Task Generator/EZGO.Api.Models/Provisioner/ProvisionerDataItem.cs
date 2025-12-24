using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Provisioner
{
    /// <summary>
    /// ProvisionerDataItem; Data item, contains 1 row of data. 
    /// </summary>
    public class ProvisionerDataItem
    {
        /// <summary>
        /// Array of data, for processing purposes. 
        /// </summary>
        public string[] DataContentItem { get; set; }
    }
}
