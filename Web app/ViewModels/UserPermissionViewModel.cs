using EZGO.Api.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class UserPermissionViewModel : BaseViewModel
    {
        #region - constructor(s) -
        public UserPermissionViewModel()
        {
        }
        #endregion


        #region - -
        public List<Models.User.UserProfile> UserProfiles { get; set; }

        public List<Area> Areas { get; set; }

        public Models.User.UserProfile CurrentUser { get; set; }

        #endregion
    }
}
