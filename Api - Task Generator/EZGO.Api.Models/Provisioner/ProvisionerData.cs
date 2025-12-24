using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Provisioner
{
    /// <summary>
    /// ProvisionerData; Provisioner data, will be filled from either file or content text.
    /// Effectively a CSV like data set. 
    /// </summary>
    public class ProvisionerData
    {
        /// <summary>
        /// DataType; Specific type, currently determined processing. Types: EZGO, ATOSS
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// Content, text. Content will be split per line, per line items will be split per separator. 
        /// </summary>
        public string DataContent { get; set; }
        /// <summary>
        /// ContentItems, contains items, split per line, containing array of content. 
        /// </summary>
        public List<ProvisionerDataItem> DataContentItems { get; set; }
        /// <summary>
        /// Users, provision users, contains parsed data content items that have been converted to users.
        /// </summary>
        public List<ProvisionerUser> Users { get; set; }
        /// <summary>
        /// SystemUsers; SystemUsers that can be used for inserting/updating data if use with holding or not supplied.
        /// </summary>
        public List<ProvisionerSystemUser> SystemUsers { get; set; }
        /// <summary>
        /// ExternalCompanyMapping; Mapping with external company ids (which can be part of the data) and internal mappings.
        /// </summary>
        public List<ProvisionerCompanyMapper> ExternalCompanyMapping { get; set; }
        /// <summary>
        /// AreaMapping; Contains mapping for coupling default area to an user if needed.
        /// </summary>
        public List<ProvisionerAreaMapper> AreaMapping { get; set; }
    }
}
