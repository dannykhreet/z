using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Users
{
    public class UserAppPreferencesWithMetadata
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserAppPreferences UserAppPreferences { get; set; }
    }
}
