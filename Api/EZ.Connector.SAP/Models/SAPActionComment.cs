using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.SAP.Models
{
    /// <summary>
    /// SAPActionComment; Object posted to a SAP connector; This will probably be converted to JSON before posting; 
    /// NOTE! Object may change depending on which 3rd party connector to SAP we will be using.
    /// </summary>
    public class SAPActionComment
    {
        /// <summary>
        /// Id: (int)Internal Id as used within the EZGO database structure, will be created on insertion, will never change.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Comment: User inputted text; Comments are part of a chat based structure; Can contain a conversation or extra information.
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// CreatedById: (int)Internal user_id based on it's profile as used within the EZGO database structure , will be added on insertion, will never change.
        /// </summary>
        public int CreatedById { get; set; }
        /// <summary>
        /// CreatedBy: (varchar)Name of created user, will change on name change. Change frequency unknown.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// CreatedAt; (DateTime UTC format YYYY-MM-ddTHH:mm:ss) Creation date and time of action. Can not change.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; (DateTime UTC format YYYY-MM-ddTHH:mm:ss) Modification date and time of action. Can change. Change frequency every time something changes with action.
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}
