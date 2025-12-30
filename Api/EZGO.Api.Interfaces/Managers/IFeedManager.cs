using EZGO.Api.Models.Feed;
using EZGO.Api.Models.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IFeedManager
    {
        Task<List<FactoryFeed>> GetFeedAsync(int companyId, int? userId = null, bool? useTreeView = true, FeedFilters? filters = null, string include = null);
        Task<List<FeedMessageItem>> GetFeedItemsAsync(int companyId, int? userId = null, bool? useTreeView = true, FeedFilters? filters = null, string include = null);
        Task<List<FeedMessageItem>> GetFeedItemCommentsAsync(int companyId, int? userId = null, bool? useTreeView = true, FeedFilters? filters = null, string include = null);
        Task<FeedMessageItem> GetFeedItemAsync(int companyId,int feedItemId,int? userId = null,bool? useTreeView = true,string include = null);
        Task<int> AddFeedAsync(int companyId, int userId, FactoryFeed feed);
        Task<bool> ChangeFeedAsync(int companyId, int userId, int feedId, FactoryFeed feed);
        Task<int> AddFeedItemAsync(int companyId, int userId, FeedMessageItem feedItem);
        Task<bool> ChangeFeedItemAsync(int companyId, int userId, int feedItemId, FeedMessageItem feedItem);
        Task<bool> SetFeedItemActiveAsync(int companyId, int userId, int feedItemId, bool isActive = true);
        Task<bool> SetFeedItemLikedAsync(int companyId, int userId, int feedItemId, bool isLiked = true);
        Task<bool> SetFeedItemViewedAsync(int companyId, int feedItemId, int userId);
        List<Exception> GetPossibleExceptions();
    }
}
