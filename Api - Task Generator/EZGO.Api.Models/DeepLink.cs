using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Deep link from a task to a checklist or audit
    /// </summary>
    public class DeepLink
    {
        /// <summary>
        /// Id of the linked template
        /// </summary>
        public int DeepLinkId { get; set; }
        /// <summary>
        /// Type of template that is linked to. Can be 'checklist' or 'audit'
        /// </summary>
        public string DeepLinkTo { get; set; }
        /// <summary>
        /// If true, linked checklist or audit must be copleted before the task it is linked to can be completed
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
