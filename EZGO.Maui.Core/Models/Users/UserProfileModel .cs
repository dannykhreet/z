using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Utils;
using System;

namespace EZGO.Maui.Core.Models.Users
{
    public class UserProfileModel : UserProfile
    {
        public string UserName { get; set; }
        public string FullName { get => String.Format("{0} {1}", FirstName, LastName); }

        private string _picture = "profile.png";
        public new string Picture
        {
            get => _picture;
            set { if (value != null) _picture = value; }
        }

        public RoleTypeEnum RoleEnum
        {
            //Basic = 0,
            //Manager = 1,
            //ShiftLeader = 2,
            //Staff = 98,
            //SuperUser = 99,
            get
            {
                try
                {
                    string role = this.Role.Replace("_", " ");
                    role = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(role.ToLower());
                    role = role.Replace(" ", string.Empty);

                    if (Enum.TryParse(role, out RoleTypeEnum result))
                        return result;
                    else 
                        return RoleTypeEnum.Basic;
                }
                catch
                {
                    return RoleTypeEnum.Basic;
                }
            }
        }
    }
}