using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models.Feed;

namespace EZGO.Maui.Core.Interfaces.Feed
{
    public interface IFeedService
    {
        Task<List<FeedMessageItemModel>> GetMainFeedMessages(bool refresh = false, bool isFromSyncService = false, int limit = 0, int offset = 0);
        Task<List<FeedMessageItemModel>> GetFactoryUpdatesMessages(bool refresh = false, bool isFromSyncService = false, int limit = 0, int offset = 0);
        Task<FeedDataModel> GetFeedDataAsync(bool refresh = false, bool isFromSyncService = false, int limit = 0, int offset = 0);
        Task PostSetMessageLiked(FeedMessageItemModel message, bool liked);
        Task<List<FeedMessageItemModel>> GetComments(int feedId, int messageId, bool refresh = false);
        Task<int> PostFeedItemAsync(FeedMessageItemModel item);
        Task<int> GetFeedId(FeedTypeEnum feedType, bool refresh = false, bool isFromSyncService = false, int limit = 0, int offset = 0);
        Task<bool> DeleteItem(FeedMessageItemModel item);
        Task<bool> EditFeedItemAsync(FeedMessageItemModel item);
    }
}

