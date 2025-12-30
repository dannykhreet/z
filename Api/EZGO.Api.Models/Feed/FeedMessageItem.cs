using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Feed
{
    /// <summary>
    /// FeedMessageItem; Feed message item, part of a FactoryFeedCollection.Items.
    /// </summary>
    public class FeedMessageItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int? CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public FeedItemTypeEnum ItemType { get; set; }
        public bool IsSticky { get; set; }
        public bool IsHighlighted { get; set; }
        public List<string> Attachments { get; set; }
        public List<Attachment> Media { get; set; }
        public int FeedId { get; set; }
        public int? UserId { get; set; } //only with personal
        public UserBasic PostUser { get; set; }
        public string DataJson { get; set; } //dynamic field can be added by CMS for extra information if needed.
        public bool IsLiked { get; set; }
        public List<int> LikesUserIds { get; set; }
        public List<UserBasic> LikesUsers { get; set; }
        public List<FeedMessageItem> Comments { get; set; }
        public DateTime ItemDate { get; set; }
        public int CommentCount { get; set; }
        public int ModifiedById { get; set; }
        public UserBasic ModifiedByUser { get; set; }
    }
}
