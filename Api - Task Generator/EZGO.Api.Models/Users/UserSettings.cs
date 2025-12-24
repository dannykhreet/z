using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Users
{
    /// <summary>
    /// UserSettings; Specific user settings (roles etc) that are stored with the extended user details. 
    /// These settings can be extended for specific settings that can be used for specific users. 
    /// NOTE! this are not 'APP SETTINGS' which users can set them selves. These will be handled by seperate logic. 
    /// </summary>
    public class UserSettings
    {
        public int UserId { get; set; }
        public List<RoleTypeEnum> Roles { get; set; }
    }
}
