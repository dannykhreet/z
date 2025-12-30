using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// RoleTypeEnum; Roles in the current EZGO application are based on 3 levels. These levels are saved in the DB as a lowercased string.
    /// For the API we will use the RoleTypeEnum to make sure everything is more or less set in stone.
    /// If we need to check something against the database data, use ToString and ToLowercase within the logic to emulate this behavior.
    /// When using for submitting to the EZGO Api, always use the value (int) for posting.
    /// In the future when the types are extended, extend this enum.
    /// </summary>
    public enum RoleTypeEnum
    {
        /// <summary>
        /// Basic; in database 'basic'. Is a user role.
        /// </summary>
        Basic = 0,
        /// <summary>
        /// Manager; in database 'manager'. Is a user role.
        /// </summary>
        Manager = 1,
        /// <summary>
        /// Manager; in database 'shift_leader'. Is a user role.
        /// </summary>
        ShiftLeader = 2,
        /// <summary>
        /// Viewer, can view CMS, can only retrieve data. Is a system role. Can not post data. Maps to user roles in user settings in DB;
        /// </summary>
        Viewer = 10,
        /// <summary>
        /// ServiceAccount; Is a service account. EZF only. Is a system role. Maps to IsServiceAccount bit in DB; 
        /// </summary>
        ServiceAccount = 97,
        /// <summary>
        /// Staff, is and administrative user. Is a system role. For now EZFactory Only. Maps to IsStaff bit in DB;
        /// </summary>
        Staff = 98,
        /// <summary>
        /// SuperUser, is an administrative user. Is a system role. EZFactory ONLY; Maps to IsSuperUser bit in DB;
        /// </summary>
        SuperUser = 99,
        /// <summary>
        /// TagManager, can magement tags for company/holding. Is a system role. Maps to IsTagManager bit in DB; 
        /// </summary>
        TagManager = 20,
        /// <summary>
        /// RoleManager; Can and may manage roles with users information. Is a system role. Maps to user roles in user settings in DB;
        /// </summary>
        RoleManager = 21,
        /// <summary>
        /// 
        /// </summary>
        UserManager = 22,
        /// <summary>
        /// ExtendedUserManager; Can and may manage extended user information. Is a system role. Maps to user roles in user settings in DB;
        /// </summary>
        ExtendedUserManager = 23,





    }
    //TODO add custom attribute for DatabaseName or extension
}
