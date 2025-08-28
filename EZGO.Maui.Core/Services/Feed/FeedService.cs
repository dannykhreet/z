using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Feed;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Feed;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Feed
{
    public class FeedService : IFeedService
    {
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly ICachingService _cachingService;
        private readonly IUserService _userService;
        private const int DEFAULT_PAGE_SIZE = 10;

        public FeedService(IApiRequestHandler apiRequestHandler, IUserService userService)
        {
            _apiRequestHandler = apiRequestHandler;
            _userService = userService;
            _cachingService = DependencyService.Get<ICachingService>();
        }

        private async Task<List<FactoryFeedModel>> GetFeed(bool refresh = false, bool isFromSyncService = false, int limit = DEFAULT_PAGE_SIZE, int offset = 0)
        {
            return await PerformanceTracker.TrackOperationAsync<List<FactoryFeedModel>>("GetFeed", async () =>
            {
                if (!CompanyFeatures.CompanyFeatSettings.FactoryFeedEnabled)
                    return new List<FactoryFeedModel>();

                try
                {
                    var parameters = new List<string>
                    {
                        $"offset={offset}",
                        $"limit={limit}",
                        "include=feeditems"
                    };
                    var action = "feeds?" + string.Join("&", parameters);
                    Debug.WriteLine($"[DEBUG_FEED_API] Calling feed API: {action}");
                    var feed = await _apiRequestHandler.HandleListRequest<FactoryFeedModel>(action, refresh, isFromSyncService);
                    return feed;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in GetFeed: {ex.Message}");
                    return new List<FactoryFeedModel>();
                }
            });
        }

        private async Task<List<FeedMessageItemModel>> GetMessages(FeedTypeEnum feedType, bool refresh = false, bool isFromSyncService = false, int limit = DEFAULT_PAGE_SIZE, int offset = 0)
        {
            Debug.WriteLine($"[DEBUG_FEED_MESSAGES] Getting messages for feed type: {feedType}, limit: {limit}, offset: {offset}, refresh: {refresh}, isFromSyncService: {isFromSyncService}");
            return await PerformanceTracker.TrackOperationAsync<List<FeedMessageItemModel>>($"GetMessages_{feedType}", async () =>
            {
                if (!CompanyFeatures.CompanyFeatSettings.FactoryFeedEnabled)
                    return new List<FeedMessageItemModel>();

                try
                {
                    var feed = await GetFeed(refresh, isFromSyncService, limit, offset);
                    var items = feed.FirstOrDefault(f => f.FeedType == feedType)?.Items ?? new List<FeedMessageItemModel>();

                    if (items.Any())
                        await SetItemsData(items, feedType);

                    return items;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in GetMessages: {ex.Message}");
                    return new List<FeedMessageItemModel>();
                }
            });
        }

        private async Task SetItemsData(List<FeedMessageItemModel> items, FeedTypeEnum feedType)
        {
            var users = await _userService.GetCompanyUsersAsync();
            var userDictionary = users.ToDictionary(u => u.Id);

            foreach (var item in items)
            {
                item.SetData();
                item.FeedType = feedType;

                if (userDictionary.TryGetValue(item.UserId, out var user))
                {
                    item.AvatarUrl = user.Picture;
                    item.Username = user.FullName;
                }

                if (item.ModifiedById != item.UserId && userDictionary.TryGetValue(item.ModifiedById, out var modifiedByUser))
                {
                    item.ModifiedByUsername = modifiedByUser.FullName;
                }

                if (item.LikesUserIds != null)
                {
                    item.LikedByUsers = new ObservableCollection<UserProfileModel>(
                        item.LikesUserIds
                            .Where(userDictionary.ContainsKey)
                            .Select(id => userDictionary[id]));
                }
            }
        }

        public async Task<List<FeedMessageItemModel>> GetFactoryUpdatesMessages(bool refresh = false, bool isFromSyncService = false, int limit = 10, int offset = 0)
        {
            return await GetMessages(FeedTypeEnum.FactoryUpdates, refresh, isFromSyncService, limit, offset);
        }


        public async Task<List<FeedMessageItemModel>> GetMainFeedMessages(bool refresh = false, bool isFromSyncService = false, int limit = 10, int offset = 0)
        {
            return await GetMessages(FeedTypeEnum.MainFeed, refresh, isFromSyncService, limit, offset);
        }

        public async Task PostSetMessageLiked(FeedMessageItemModel message, bool liked)
        {
            string action = $"feeds/item/setliked/{message.Id}";

            var response = await _apiRequestHandler.HandlePostRequest(action, liked);

            if (response.IsSuccessStatusCode)
            {
                await AlterCachedItemAsync(message);
                await AlterMyEzFeedStatsAsync(ReportsConstants.MyLikesTotal, 1);
            }
        }

        private async Task AlterMyEzFeedStatsAsync(string statName, int addCount)
        {
            static void alteringFunction(ReportsCount reportsCount, int addCount)
            {
                reportsCount.CountNr += addCount;
            }

            var uri = $"reporting/statistics/my/ezfeed";

            // Alter my ez feed stats cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: uri).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<ReportsCount>(uri, (reportsCount) => alteringFunction(reportsCount, addCount), (reportsCount) => reportsCount.Name == statName);
        }

        private async Task AlterCachedItemAsync(FeedMessageItemModel item)
        {
            static void alteringFunction(FactoryFeedModel factoryFeed, FeedMessageItemModel item)
            {
                var cachedMessage = factoryFeed.Items.FirstOrDefault(i => i.Id == item.Id);

                if (cachedMessage != null)
                {
                    cachedMessage.Title = item.Title;
                    cachedMessage.Description = item.Description;
                    cachedMessage.IsSticky = item.IsSticky;
                    cachedMessage.LikesUserIds = item.LikesUserIds;
                }
            }

            var uri = $"feeds?&include=feeditems&usetreeview=true&limit=0";

            // Alter feed cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: uri).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<FactoryFeedModel>(uri, (factoryFeed) => alteringFunction(factoryFeed, item), (factoryFeed) => factoryFeed.Id == item.FeedId);
        }

        private async Task AlterCommentCountListAsync(FeedMessageItemModel message)
        {
            static void alteringFunction(FactoryFeedModel factoryFeed, FeedMessageItemModel message)
            {
                var cachedMessage = factoryFeed.Items.FirstOrDefault(i => i.Id == message.ParentId);

                if (cachedMessage != null)
                    cachedMessage.CommentCount--;
            }

            var uri = $"feeds?&include=feeditems&usetreeview=true&limit=0";

            // Alter feed cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: uri).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<FactoryFeedModel>(uri, (factoryFeed) => alteringFunction(factoryFeed, message), (factoryFeed) => factoryFeed.Id == message.FeedId);
        }

        public async Task<List<FeedMessageItemModel>> GetComments(int feedId, int messageId, bool refresh = false)
        {
            var url = $"feeds/itemcomments/{feedId}/{messageId}";
            var comments = await _apiRequestHandler.HandleListRequest<FeedMessageItemModel>(url, refresh);

            if (comments.Any())
                await SetItemsData(comments, FeedTypeEnum.MainFeed);

            return comments;
        }

        public async Task<bool> EditFeedItemAsync(FeedMessageItemModel item)
        {
            var result = false;

            var message = new FeedMessageItem
            {
                CompanyId = item.CompanyId,
                Description = item.Description,
                FeedId = item.FeedId,
                ItemDate = item.ItemDate,
                Title = item.Title,
                UserId = item.UserId,
                ItemType = item.ItemType,
                IsSticky = item.IsSticky,
                IsHighlighted = item.IsHighlighted,
                ParentId = item.ParentId,
                Id = item.Id,
                Attachments = item.Attachments
            };

            var uri = $"feeds/item/change/{item.Id}";

            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, message))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    bool.TryParse(json, out result);
                    await AlterCachedItemAsync(item);
                }
            }
            return result;
        }

        public async Task<int> PostFeedItemAsync(FeedMessageItemModel item)
        {
            var message = new FeedMessageItem
            {
                CompanyId = item.CompanyId,
                Description = item.Description,
                FeedId = item.FeedId,
                ItemDate = item.ItemDate,
                Title = item.Title,
                UserId = item.UserId,
                ItemType = item.ItemType,
                IsSticky = item.IsSticky,
                IsHighlighted = item.IsHighlighted,
                ParentId = item.ParentId,
                Attachments = item.Attachments,
                Media = item.Media
            };

            var uri = "feeds/item/add";
            int result = -1;

            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, message))
            {
                if (response.IsSuccessStatusCode)
                {
                    //update feed cache
                    await GetFeed(true).ConfigureAwait(false);
                    string json = await response.Content.ReadAsStringAsync();
                    bool isNumber = int.TryParse(json, out result);
                    if (!isNumber) result = message.Id;

                    //alter my ez feed stats cache
                    if (item.ItemType == FeedItemTypeEnum.Person)
                        await AlterMyEzFeedStatsAsync(ReportsConstants.MyCommentsTotal, 1);
                    if (item.ItemType == FeedItemTypeEnum.Company || item.ItemType == FeedItemTypeEnum.EzFactory)
                        await AlterMyEzFeedStatsAsync(ReportsConstants.MyPostsTotal, 1);
                }
            }
            return result;
        }

        public async Task<int> GetFeedId(FeedTypeEnum feedType, bool refresh = false, bool isFromSyncService = false, int feedLimit = 0, int offset = 0)
        {
            var feed = await GetFeed(refresh, isFromSyncService, feedLimit, offset: offset);
            var id = feed?.FirstOrDefault(f => f.FeedType == feedType)?.Id ?? -1;
            return id;
        }

        public async Task<FeedDataModel> GetFeedDataAsync(bool refresh = false, bool isFromSyncService = false, int limit = DEFAULT_PAGE_SIZE, int offset = 0)
        {
            var feed = await GetFeed(refresh, isFromSyncService, limit, offset);

            var mainFeed = feed.FirstOrDefault(f => f.FeedType == FeedTypeEnum.MainFeed);
            var factoryUpdates = feed.FirstOrDefault(f => f.FeedType == FeedTypeEnum.FactoryUpdates);

            var mainItems = mainFeed?.Items ?? new List<FeedMessageItemModel>();
            if (mainItems.Any())
                await SetItemsData(mainItems, FeedTypeEnum.MainFeed);

            var factoryItems = factoryUpdates?.Items ?? new List<FeedMessageItemModel>();
            if (factoryItems.Any())
                await SetItemsData(factoryItems, FeedTypeEnum.FactoryUpdates);

            return new FeedDataModel
            {
                MainFeedId = mainFeed?.Id ?? -1,
                FactoryUpdatesFeedId = factoryUpdates?.Id ?? -1,
                MainFeedMessages = mainItems,
                FactoryUpdatesMessages = factoryItems
            };
        }

        public async Task<bool> DeleteItem(FeedMessageItemModel item)
        {
            bool result = false;
            var uri = $"feeds/item/setactive/{item.Id}";

            using (HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest(uri, false))
            {
                if (response.IsSuccessStatusCode)
                {
                    await AlterCommentCountListAsync(item);
                    string json = await response.Content.ReadAsStringAsync();
                    bool.TryParse(json, out result);
                }
            }
            return result;
        }
    }
}