using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Collection of deep links from a task to checklists or audits
    /// </summary>
    public class DeepLinkConfiguration
    {
        /// <summary>
        /// List of deeplinks 
        /// </summary>
        public List<DeepLink> DeepLinks { get; set; }
    }
}
