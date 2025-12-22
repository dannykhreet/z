using EZGO.Api.Models;
using EZGO.Api.Models.Users;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class UserProfileViewModel : BaseViewModel
    {
        #region - constructor(s) -
        public UserProfileViewModel()
        {
        }

        #endregion
        public List<Area> Areas { get; set; }
        public List<Models.User.UserProfile> UserProfiles { get; set; }
        public Models.User.UserProfile UserProfile { get; set; }
        public UserExtendedDetails UserExtendedDetails { get; set; }
        public bool CurrentUserProfileHasActions { get; set; }
        public Models.User.UserProfile CurrentUser { get; set; }
        public TfaSetup TwoFactorySetup { get; set; }
        public bool EnableRoleManagement { get; set; }
        public bool EnableExtendedUserManagement { get; set; }
    }
}
