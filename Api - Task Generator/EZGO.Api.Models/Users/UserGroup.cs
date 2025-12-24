using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Users
{
    public class UserGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GroupTypeEnum GroupType { get; set; }
        public List<int> UserIds { get; set; }
        public List<UserProfile> Users { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this user group
        /// </summary>
        public List<Tag> Tags { get; set; }
        public bool InUseInMatrix { get; set; }
    }
}
