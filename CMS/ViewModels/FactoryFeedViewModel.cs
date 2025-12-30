using System;
using System.Collections.Generic;
using WebApp.Models.FactoryFeed;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class FactoryFeedViewModel : BaseViewModel
    {
        public List<FactoryFeedModel> Feed { get; set; }

        public UserProfile CurrentUser { get; set; }
        public List<UserProfile> Users { get; set; }

        public int LikesTotal { get; set; }
        public int PostsTotal { get; set; }
        public int CommentsTotal { get; set; }
        public int AuditsTotal { get; set; }
        public int ChecklistsTotal { get; set; }
        public int TasksTotal { get; set; }
        public bool AdvancedStatsEnabled { get; set; }
    }
}
