using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;
using WebApp.Models.User;
using WebApp.ViewModels;

namespace WebApp.Models.FactoryFeed
{
    public class FactoryFeedItemModel : BaseViewModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ItemType { get; set; }
        public bool IsSticky { get; set; }
        public bool IsHighlighted { get; set; }
        public List<string> Attachments { get; set; }
        public List<EZGO.Api.Models.Attachment> Media { get; set; }
        public int FeedId { get; set; }
        public int UserId { get; set; }
        public bool IsLiked { get; set; }
        public List<int> LikesUserIds { get; set; }
        public DateTime ItemDate { get; set; }
        public List<FactoryFeedItemCommentModel> Comments { get; set; }
        public int CommentCount { get; set; }
        public int ModifiedById { get; set; }


        public List<UserBasic> LikesUsers { get; set; }
        public List<UserProfile> CommentUsers { get; set; }
        
        public UserProfile PostUser { get; set; }
        public UserProfile CurrentUser { get; set; }
        public UserProfile ModifiedByUser { get; set; }
    }
}
