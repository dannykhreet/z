using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Action;

namespace WebApp.Models.User
{
    public class UserProfile
    {
        public int Id { get; set; }
        public CompanyModel Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UPN { get; set; }
        public string Picture { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public string UserName { get; set; }
        public int? SuccessorId { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsServiceAccount { get; set; }
        public bool? IsTagManager { get; set; }
        public List<AllowedAreaModel> AllowedAreas { get; set; }
        public List<AllowedAreaModel> DisplayAreas { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public string ValidationKey { get; set; }
        public string UserGUID { get; set; }
        public string SapPmUsername { get; set; }
        public UserExtendedDetails UserExtendedDetails { get; set; }
        public List<EZGO.Api.Models.Enumerations.RoleTypeEnum> Roles {get; set;}
        public UserProfile()
        {
        }

        public UserBasicModel ToBasic()
        {
            return new UserBasicModel
            {
                Id = this.Id,
                Name = string.Format("{0} {1}", FirstName, LastName),
                Picture = Picture
            };
        }
    }
}
