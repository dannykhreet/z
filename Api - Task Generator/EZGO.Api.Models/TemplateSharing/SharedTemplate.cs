using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.TemplateSharing
{
    public class SharedTemplate
    {
        /// <summary>
        /// Id in shared_template table
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the template when it was shared
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Object type that was shared (should be a template)
        /// </summary>
        public ObjectTypeEnum Type { get; set; }
        /// <summary>
        /// Name of the company that shared this tempalte
        /// </summary>
        public string FromCompanyName { get; set; }
        /// <summary>
        /// Date the template was shared
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Last time this object was changed, e.g. accepted or declined
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}
