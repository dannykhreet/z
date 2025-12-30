using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// UserProfile; UserProfile contains the basic data of a user. The UserName, Password and security/authentication properties are handled through the ApplicationUser.
    /// Database location: profiles_user
    /// </summary>
    public class UserProfile
    {
        #region - fields -
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as UserId or UserProfileId. DB: [profiles_user.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// UserGUID; for user guid identifier.
        /// </summary>
        public string UserGUID { get; set; }
        /// <summary>
        /// Email; Email address of user. DB: [profiles_user.email]
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// FirstName; First name of user. DB: [profiles_user.first_name]
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// LastName; Last name of user. DB: [profiles_user.last_name]
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Picture; Picture of user. Uri part. DB: [profiles_user.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// IsStaff; IsStaff is a boolean if it is a EZfactory user. DB: [profiles_user.is_staff]
        /// </summary>
        public bool IsStaff { get; set; }
        /// <summary>
        /// IsSuperUser; IsSuperUser is a boolean if it is a EZfactory super user. DB: [profiles_user.is_susperuser]
        /// </summary>
        public bool IsSuperUser { get; set; }
        /// <summary>
        /// IsServiceAccount; IsServiceAccount is a boolean if it is an account that was created when the company was created
        /// </summary>
        public bool IsServiceAccount { get; set; }
        /// <summary>
        /// IsHoldingManager; IsHoldingManager is a boolean that determines if an account can operate on holding level. DB: [profiles_user.is_tag_manager]
        /// </summary>
        public bool? IsTagManager { get; set; }
        /// <summary>
        /// Role; Role of user.  DB: [profiles_user.role]
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Company; Basic company object containing company data; Based on CompanyId.
        /// </summary>
        public CompanyBasic Company { get; set; }
        /// <summary>
        /// AllowedAreas; Allowed areas of this user (note for basic/shift_leader users)
        /// </summary>
        public List<AreaBasic> AllowedAreas { get; set; }
        /// <summary>
        /// DisplayAreas; Display areas of this user (note for basic/shift_leader users)
        /// </summary>
        public List<AreaBasic> DisplayAreas { get; set; }
        /// <summary>
        /// Rank; NOT USED;
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// UPN; UPN key used for 3rd party login systems.  DB: [profiles_user.upn]
        /// </summary>
        public string UPN { get; set; }
        /// <summary>
        /// UserName; Username of user.  DB: [profiles_user.username]
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// SuccessorId; SuccessorId of the user. DB: [profiles_user.successor_id]
        /// </summary>
        public int? SuccessorId { get; set; }
        /// <summary>
        /// SapPmUsername; A 12 character username that is used to communicate with SAP
        /// </summary>
        public string SapPmUsername { get; set; }
        /// <summary>
        /// ModifiedAt; DateTime the UserProfile was last modified. DB: [profiles_user.modified_at]
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this user profile
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// Timezone used by company of user
        /// </summary>
        public string CompanyTimezone { get; set; }
        /// <summary>
        /// Language culture used by company of user.
        /// </summary>
        public string CompanyLanguageCulture { get; set; }
        #endregion

        #region - setttings, rights and related -
        /// <summary>
        /// Roles; Collection of roles. Roles can be user roles (manager, basic, shift leader) and system roles (viewer, tag manager etc) 
        /// </summary>
        public List<RoleTypeEnum> Roles { get; set; }

        /// <summary>
        /// CurrentIps; Comma separated list of current ips that the user is logged in from.
        /// </summary>
        public string CurrentIps { get; set; }
        #endregion

        #region MFA 
        /// <summary>
        /// TimebasedOneTimePasswordEnabled time based one time password enabled true/false (for use with authentication app)
        /// </summary>
        public bool MfaTimebasedOneTimePasswordEnabled { get; set; }
        #endregion

        #region - sync -
        public string SyncGUID { get; set; }
        #endregion

        #region - constructor(s) -
        public UserProfile()
        {
            
        }
        #endregion
    }
}
