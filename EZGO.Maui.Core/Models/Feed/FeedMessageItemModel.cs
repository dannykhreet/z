using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Users;
using NodaTime;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EZGO.Maui.Core.Models.Feed
{
    public class FeedMessageItemModel : FeedMessageItem, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public LocalDateTime ItemLocalDate => Settings.ConvertDateTimeToLocal(ItemDate.ToLocalTime());
        public bool IsLikedByCurrentUser { get; set; }
        public bool AreCommentsVisible { get; set; } = false;
        public string AvatarUrl { get; set; }
        public string Username { get; set; }
        public int LikeCount { get; set; }
        public FeedTypeEnum FeedType { get; set; }
        public new ObservableCollection<FeedMessageItemModel> Comments { get; set; }
        public new int CommentCount { get; set; }
        public new string Title { get; set; }
        public new string Description { get; set; }
        public new bool IsSticky { get; set; }
        public string ModifiedByUsername { get; set; }
        public new int UserId { get; set; }
        public new int ModifiedById { get; set; }
        public bool IsModified => UserId != ModifiedById;
        public bool CanModifyPost => UserSettings.Id == UserId || UserSettings.RoleType == RoleTypeEnum.Manager;
        public List<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
        public ObservableCollection<UserProfileModel> LikedByUsers { get; set; } = new ObservableCollection<UserProfileModel>();
        public bool HasAnyMediaItems => MediaItems.Any(x => !x.IsEmpty);

        public void SetData()
        {
            LikeCount = LikesUserIds?.Count ?? 0;
            IsLikedByCurrentUser = LikesUserIds?.Contains(UserSettings.Id) ?? false;

            Media?.ForEach(m =>
            {
                if (m != null)
                {
                    var item = new MediaItem
                    {
                        PictureUrl = m.Uri,
                        FileUrl = m.Uri,
                    };
                    if ((m.Uri?.ToLower().Contains("mp4") ?? false) || (m.Uri?.ToLower().Contains("mov") ?? false))
                    {
                        item.IsVideo = true;
                        item.VideoUrl = m.Uri;
                        item.PictureUrl = m.VideoThumbnailUri;
                    }
                    MediaItems.Add(item);
                }
            });
        }

        public void ConvertMediaItemsToAttachmentsAndMedia()
        {
            ConvertMediaItemsToAttachments();
            ConvertMediaItemsToMedia();
        }

        private void ConvertMediaItemsToAttachments()
        {
            Attachments = new List<string>();

            MediaItems.ForEach(x =>
            {
                var attachment = x.PictureUrl;

                if (x.IsVideo)
                {
                    attachment = x.VideoUrl;
                }
                else if (x.IsFile)
                    attachment = x.FileUrl;

                Attachments.Add(attachment);
            });
        }

        private void ConvertMediaItemsToMedia()
        {
            Media = new List<Api.Models.Attachment>();

            MediaItems.ForEach(x =>
            {
                var media = new Api.Models.Attachment() { Uri = x.PictureUrl };

                if (x.IsVideo)
                {
                    media.Uri = x.VideoUrl;
                    media.VideoThumbnailUri = x.PictureUrl;
                }
                else if (x.IsFile)
                    media.Uri = x.FileUrl;

                Media.Add(media);
            });
        }
    }
}