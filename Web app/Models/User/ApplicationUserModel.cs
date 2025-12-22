using System;
using System.Collections.Generic;
using EZGO.Api.Models.Settings;
using Newtonsoft.Json;

namespace WebApp.Models.User
{
    public class ApplicationUserModel
    {

        [JsonProperty(PropertyName = "Id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "UserName")]
        public string username { get; set; }

        [JsonProperty(PropertyName = "Company")]
        public CompanyModel company { get; set; }

        [JsonProperty(PropertyName = "FirstName")]
        public string first_name { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string last_name { get; set; }

        [JsonProperty(PropertyName = "Email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "UPN")]
        public string upn { get; set; }

        [JsonProperty(PropertyName = "Picture")]
        public string picture { get; set; }

        [JsonProperty(PropertyName = "Role")]
        public string role { get; set; }

        public List<AllowedAreaModel> AllowedAreas { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public ApplicationUserModel()
        {
        }
    }
}
