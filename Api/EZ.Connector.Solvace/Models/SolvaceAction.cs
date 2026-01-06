using EZ.Connector.Solvace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Solvace.Models
{
    /// <summary>
    /// SolvaceAction; Object posted to a SolvaceAction connector; This will probably be converted to JSON before posting; 
    /// NOTE! Object may change depending on which 3rd party connector to SolvaceAction we will be using.
    /// </summary>
    public class SolvaceAction
    {
        /// <summary>
        /// Id: (int)Internal Id as used within the EZGO database structure, will be created on insertion, will never change.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// CompanyId: (int)Internal company Id as used within the EZGO database structure , will be added on insertion, will never change.
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// CreatedById: (int)Internal user_id based on it's profile as used within the EZGO database structure , will be added on insertion, will never change.
        /// </summary>
        public int CreatedById { get; set; }
        /// <summary>
        /// CreatedBy: (varchar)Name of created user, will change on name change. Change frequency unknown.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// DueDate: (DateTime format YYYY-MM-ddTHH:mm:ss) Date and time when action must be completed. Currently mostly filled with complete dates. Will contain time in futuren. Can change. Change frequency unknown.
        /// </summary>
        public DateTime DueDate { get; set; }
        /// <summary>
        /// Description: (text)Description of the action that should be taken. Can change. Change frequency unknown.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Comment: (text)Comment of the issue thats occurring and way action should be taken. Can change. Change frequency unknown.
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// (bool)Boolean if action is resolved or not, changes after resolving the action. Can change. Change frequency once, after resolving action.
        /// </summary>
        public bool IsResolved { get; set; }
        /// <summary>
        /// CreatedAt; (DateTime UTC format YYYY-MM-ddTHH:mm:ss) Creation date and time of action. Can not change.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt; (DateTime UTC format YYYY-MM-ddTHH:mm:ss) Modification date and time of action. Can change. Change frequency every time something changes with action.
        /// </summary>
        public DateTime ModifiedAt { get; set; }
        /// <summary>
        /// Media; List of strings containing URLs to specific media (images,videos) that were added to this actions directly.
        /// </summary>
        public List<string> Media { get; set; }
        /// <summary>
        /// Comments; List of comments; Comments are used as a chat-like structure and extra information.
        /// </summary>
        public List<SolvaceActionComment> Comments { get; set; }

        /// <summary>
        /// SolvaceAction; Constructor, auto creates the properties that are lists.
        /// </summary>
        public SolvaceAction()
        {
            Comments = new List<SolvaceActionComment>();
            Media = new List<string>();
        }
    }
}


