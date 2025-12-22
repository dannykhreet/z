using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Feed;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Stats;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.FactoryFeed;
using WebApp.Models.User;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
    public class FactoryFeedController : BaseController
    {

        private readonly ILogger<FactoryFeedController> _logger;
        private readonly IApiConnector _connector;

        public FactoryFeedController(
                        ILogger<FactoryFeedController> logger,
                        IApiConnector connector,
                        ILanguageService language,
                        IHttpContextAccessor httpContextAccessor,
                        IConfigurationHelper configurationHelper,
                        IApplicationSettingsHelper applicationSettingsHelper,
                        IInboxService inboxService
                    ) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed")]
        public async Task<IActionResult> Index()
        {


            FactoryFeedViewModel output = new FactoryFeedViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Factory feed";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.FACTORYFEED;
            output.Locale = _locale;
            output.NewComments = 0;
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();

            var userresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (userresult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Users = JsonConvert.DeserializeObject<List<UserProfile>>(userresult.Message);
                output.Users ??= new List<UserProfile>();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();

            var endpoint = string.Format(Logic.Constants.FactoryFeed.GetFactoryFeed, 2, 10, 0);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Feed = JsonConvert.DeserializeObject<List<FactoryFeedModel>>(result.Message);
            }

            foreach (var feed in output.Feed)
            {
                if (feed.Items != null)
                {
                    foreach (var feeditem in feed.Items)
                    {
                        feeditem.PostUser = output.Users.Where(u => u.Id == feeditem.UserId).FirstOrDefault();
                        feeditem.CurrentUser = output.CurrentUser;
                        feeditem.LikesUsers ??= new List<UserBasic>();
                        feeditem.LikesUserIds ??= new List<int>();
                        feeditem.CommentUsers ??= new List<UserProfile>();
                        if (feeditem.Comments != null)
                        {
                            foreach (var comment in feeditem.Comments)
                            {
                                var user = output.Users.Where(u => u.Id == comment.UserId).FirstOrDefault();
                                if (user != null)
                                {
                                    feeditem.CommentUsers.Add(user);
                                }
                            }
                        }
                        feeditem.ModifiedByUser = output.Users.Where(u => u.Id == feeditem.ModifiedById).FirstOrDefault();

                        feeditem.ApplicationSettings = output.ApplicationSettings;
                        feeditem.CmsLanguage = output.CmsLanguage;
                    }
                }
            }
            output.AdvancedStatsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableEZFeedStatistics");
            var statisticsEndpoint = string.Format(Logic.Constants.FactoryFeed.GetMyEZFeedStatistics);
            if (!output.AdvancedStatsEnabled)
            {
                statisticsEndpoint = string.Format(Logic.Constants.FactoryFeed.GetMyStatistics);
            }
            var statisticsResult = await _connector.GetCall(statisticsEndpoint);

            if (statisticsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var statistics = JsonConvert.DeserializeObject<List<StatisticGenericItem>>(statisticsResult.Message);

                if (output.AdvancedStatsEnabled)
                {
                    output.LikesTotal = statistics.Where(s => s.Name.ToLower().Contains("my likes total")).FirstOrDefault().CountNr ?? 0;
                    output.PostsTotal = statistics.Where(s => s.Name.ToLower().Contains("my posts total")).FirstOrDefault().CountNr ?? 0;
                    output.CommentsTotal = statistics.Where(s => s.Name.ToLower().Contains("my comments total")).FirstOrDefault().CountNr ?? 0;
                }
                else
                {
                    output.AuditsTotal = statistics.Where(s => s.Name.ToLower().Contains("my audits total")).FirstOrDefault().CountNr ?? 0;
                    output.ChecklistsTotal = statistics.Where(s => s.Name.ToLower().Contains("my checklists total")).FirstOrDefault().CountNr ?? 0;
                    output.TasksTotal = statistics.Where(s => s.Name.ToLower().Contains("my tasks total")).FirstOrDefault().CountNr ?? 0;
                }
            }

            await Task.CompletedTask;

            return View(output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/getfeeditems/{feedId}")]
        [HttpGet]
        public async Task<IActionResult> GetFeedItems(int feedId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
        {


            var output = "";

            var userresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            var users = new List<UserProfile>();
            var currentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            var cmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            var applicationSettings = await this.GetApplicationSettings();

            if (userresult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                users = JsonConvert.DeserializeObject<List<UserProfile>>(userresult.Message);
                users ??= new List<UserProfile>();
            }

            var items = new List<FactoryFeedItemModel>();
            /*
             * 
                public List<UserProfile> LikesUsers { get; set; }
                public List<UserProfile> CommentUsers { get; set; }
        
                public UserProfile PostUser { get; set; }
                public UserProfile CurrentUser { get; set; }
             */
            var itemsResult = await _connector.GetCall(string.Format(Logic.Constants.FactoryFeed.GetFactoryFeedItems, feedId, limit, offset));
            if (itemsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                items = JsonConvert.DeserializeObject<List<FactoryFeedItemModel>>(itemsResult.Message);
                items ??= new List<FactoryFeedItemModel>();

                foreach (var item in items)
                {
                    item.PostUser = users.Where(u => u.Id == item.UserId).FirstOrDefault();
                    item.CurrentUser = currentUser;
                    item.LikesUsers ??= new List<UserBasic>();
                    item.CommentUsers ??= new List<UserProfile>();
                    item.ModifiedByUser = users.Where(u => u.Id == item.ModifiedById).FirstOrDefault();

                    item.ApplicationSettings = applicationSettings;
                    item.CmsLanguage = cmsLanguage;

                    var strHtml = await this.RenderViewAsyncUsingGetView(@"~/Views/FactoryFeed/_contentcard.cshtml", item, true);
                    output += strHtml;
                }
            }

            return Ok(output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/getfactoryupdates/{feedId}")]
        [HttpGet]
        public async Task<IActionResult> GetFactoryUpdates(int feedId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
        {


            var output = "";

            var userresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            var users = new List<UserProfile>();
            var currentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            var cmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            var applicationSettings = await this.GetApplicationSettings();

            if (userresult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                users = JsonConvert.DeserializeObject<List<UserProfile>>(userresult.Message);
                users ??= new List<UserProfile>();
            }

            var items = new List<FactoryFeedItemModel>();
            var itemsResult = await _connector.GetCall(string.Format(Logic.Constants.FactoryFeed.GetFactoryFeedItems, feedId, limit, offset));
            if (itemsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                items = JsonConvert.DeserializeObject<List<FactoryFeedItemModel>>(itemsResult.Message);
                items ??= new List<FactoryFeedItemModel>();

                foreach (var item in items)
                {
                    item.PostUser = users.Where(u => u.Id == item.UserId).FirstOrDefault();
                    item.CurrentUser = currentUser;
                    item.LikesUsers ??= new List<UserBasic>();
                    item.CommentUsers ??= new List<UserProfile>();
                    item.ModifiedByUser = users.Where(u => u.Id == item.ModifiedById).FirstOrDefault();

                    item.ApplicationSettings = applicationSettings;
                    item.CmsLanguage = cmsLanguage;

                    var strHtml = await this.RenderViewAsyncUsingGetView(@"~/Views/FactoryFeed/_factoryupdate.cshtml", item, true);
                    output += strHtml;
                }
            }

            return Ok(output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/getfeeditemcomments/{feedId}/{feedItemId}")]
        [HttpGet]
        public async Task<IActionResult> GetFeedItemComments(int feedId, int feedItemId, int limit = 0, int offset = 0)
        {


            string output = "";

            var userresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            var users = new List<UserProfile>();
            if (userresult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                users = JsonConvert.DeserializeObject<List<UserProfile>>(userresult.Message);
                users ??= new List<UserProfile>();
            }

            var comments = new List<FactoryFeedItemCommentModel>();
            var cmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            var applicationSettings = await this.GetApplicationSettings();
            var itemsResult = await _connector.GetCall(string.Format(Logic.Constants.FactoryFeed.GetFactoryFeedItemComments, feedId, feedItemId, limit, offset));
            if (itemsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                comments = JsonConvert.DeserializeObject<List<FactoryFeedItemCommentModel>>(itemsResult.Message);
                comments ??= new List<FactoryFeedItemCommentModel>();
                foreach (var comment in comments)
                {
                    comment.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                    comment.PostUser = users.Where(u => u.Id == comment.UserId).FirstOrDefault();
                    comment.ModifiedByUser = users.Where(u => u.Id == comment.ModifiedById).FirstOrDefault();
                    comment.CmsLanguage = cmsLanguage;
                    comment.ApplicationSettings = applicationSettings;

                    var strHtml = await this.RenderViewAsyncUsingGetView(@"~/Views/FactoryFeed/_comment.cshtml", comment, true);
                    output += strHtml;
                }
                return Ok(output);
            }
            return BadRequest();

        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/addfeedmessage")]
        [HttpPost]
        public async Task<IActionResult> AddFeedMessage(string title, string description, int feedId, EZGO.Api.Models.Attachment[] attachments)
        {
            var newItem = new FeedMessageItem()
            {
                Description = description,
                ItemType = EZGO.Api.Models.Enumerations.FeedItemTypeEnum.Person,
                ItemDate = new DateTime(DateTime.UtcNow.Ticks),
                Title = title.Length > 250 ? title.Substring(0, 250) : title,
                FeedId = feedId,
                IsSticky = false,
                IsHighlighted = false,
                IsLiked = false,
                Media = new List<EZGO.Api.Models.Attachment>()
            };
            if (attachments != null)
            {
                newItem.Media = attachments.ToList();
            }
            var response = await _connector.PostCall(Logic.Constants.FactoryFeed.AddFeedMessage, newItem.ToJsonFromObject());

            var newItemDisplay = new FactoryFeedItemModel()
            {
                Description = newItem.Description,
                ItemType = (int)newItem.ItemType,
                ItemDate = newItem.ItemDate,
                Title = newItem.Title,
                FeedId = newItem.FeedId,
                IsSticky = newItem.IsSticky,
                IsHighlighted = newItem.IsHighlighted,
                IsLiked = newItem.IsLiked,
                Attachments = newItem.Media.Select(m => m.Uri).ToList(),
                Media = newItem.Media
            };
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //fill in data needed for the _contentcard partial view
                var id = 0;
                if (int.TryParse(response.Message, out id))
                {
                    newItemDisplay.Id = id;
                    newItemDisplay.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<Models.User.UserProfile>();
                    newItemDisplay.PostUser = newItemDisplay.CurrentUser;
                    newItemDisplay.ModifiedByUser = newItemDisplay.CurrentUser;
                    newItemDisplay.UserId = newItemDisplay.CurrentUser.Id;
                    newItemDisplay.LikesUserIds = new List<int>();
                    newItemDisplay.LikesUsers = new List<UserBasic>();
                    newItemDisplay.ApplicationSettings = await this.GetApplicationSettings();

                    newItemDisplay.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
                    return PartialView("_contentcard", newItemDisplay);
                }
                else
                {
                    //if for some reason the response message is not an id, return ok because the post was still successful on the backend
                    return Ok();
                }
            }
            else
            {
                return BadRequest(response.Message);
            }

        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/addfactoryupdatemessage")]
        [HttpPost]
        public async Task<IActionResult> AddFactoryUpdateMessage(string title, string description, int feedId, bool isSticky, EZGO.Api.Models.Attachment attachment)
        {
            var factoryUpdate = new FeedMessageItem()
            {
                Description = description,
                ItemType = EZGO.Api.Models.Enumerations.FeedItemTypeEnum.Person,
                ItemDate = new DateTime(DateTime.UtcNow.Ticks),
                Title = title.Length > 250 ? title.Substring(0, 250) : title,
                FeedId = feedId,
                IsSticky = isSticky,
                IsHighlighted = false,
                IsLiked = false,
                Media = new List<EZGO.Api.Models.Attachment>()
            };

            if (attachment != null && attachment.Uri != null)
            {
                factoryUpdate.Media.Add(attachment);
            }

            var response = await _connector.PostCall(Logic.Constants.FactoryFeed.AddFeedMessage, factoryUpdate.ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok();
            }
            else
            {
                return BadRequest(response.Message);
            }

        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/likefeeditem/{id}")]
        [HttpPost]
        public async Task<IActionResult> LikeFeedItem(int id, bool isLiked)
        {
            var endpoint = string.Format(Logic.Constants.FactoryFeed.SetItemLiked, id);

            var response = await _connector.PostCall(endpoint, isLiked.ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok();
            else
                return BadRequest();
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/addcomment")]
        [HttpPost]
        public async Task<IActionResult> AddComment(int id, int companyId, int feedId, string comment, EZGO.Api.Models.Attachment attachment)
        {
            if (comment == null)
                comment = string.Empty;

            var newItem = new FeedMessageItem()
            {
                ParentId = id,
                Description = comment,
                ItemType = EZGO.Api.Models.Enumerations.FeedItemTypeEnum.Person,
                Title = comment.Length > 250 ? comment.Substring(0, 250) : comment,
                CompanyId = companyId,
                FeedId = feedId,
                ItemDate = new DateTime(DateTime.UtcNow.Ticks),
                Media = new List<EZGO.Api.Models.Attachment>()
            };

            if (attachment != null && attachment.Uri != null)
            {
                newItem.Media.Add(attachment);
            }
            var endpoint = string.Format(Logic.Constants.FactoryFeed.AddFeedMessage);
            var response = await _connector.PostCall(endpoint, newItem.ToJsonFromObject());

            var newItemDisplay = new FactoryFeedItemCommentModel()
            {
                ParentId = id,
                Description = newItem.Description,
                ItemType = (int)newItem.ItemType,
                Title = newItem.Title,
                CompanyId = companyId,
                FeedId = feedId,
                ItemDate = newItem.ItemDate,
                Attachments = newItem.Media.Select(m => m.Uri).ToList(),
                Media = newItem.Media
            };

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                newItemDisplay.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
                newItemDisplay.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                newItemDisplay.PostUser = newItemDisplay.CurrentUser;
                newItemDisplay.ModifiedByUser = newItemDisplay.CurrentUser;
                newItemDisplay.UserId = newItemDisplay.CurrentUser.Id;
                newItemDisplay.ApplicationSettings = await this.GetApplicationSettings();
                newItemDisplay.Id = Convert.ToInt32(response.Message);
                return PartialView("_comment", newItemDisplay);
            }
            else
            {
                return BadRequest(response.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/addmainfeed")]
        [HttpPost]
        public async Task<IActionResult> AddMainFeed()
        {


            var feed = new FactoryFeed()
            {
                Name = "Main Feed",
                Description = "Main Feed",
                Attachments = new List<string>(),
                CompanyId = User.GetProfile().Company.Id,
                DataJson = "",
                FeedType = EZGO.Api.Models.Enumerations.FeedTypeEnum.MainFeed,
                Items = new List<FeedMessageItem>(),
            };

            var endpoint = string.Format(Logic.Constants.FactoryFeed.AddFactoryFeed);
            var response = await _connector.PostCall(endpoint, feed.ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response.Message);
            else
                return BadRequest();
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/addfactoryupdatesfeed")]
        [HttpPost]
        public async Task<IActionResult> AddFactoryUpdatesFeed()
        {


            var feed = new FactoryFeed()
            {
                Name = "Factory Updates",
                Description = "Factory Updates",
                Attachments = new List<string>(),
                CompanyId = User.GetProfile().Company.Id,
                DataJson = "",
                FeedType = EZGO.Api.Models.Enumerations.FeedTypeEnum.FactoryUpdates,
                Items = new List<FeedMessageItem>(),
            };

            var endpoint = string.Format(Logic.Constants.FactoryFeed.AddFactoryFeed);
            var response = await _connector.PostCall(endpoint, feed.ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response.Message);
            else
                return BadRequest();
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/changefeedmessage")]
        [HttpPost]
        public async Task<IActionResult> ChangeFeedMessage(int id, int companyId, int feedId, string message, EZGO.Api.Models.Attachment[] attachments, int originalUserId)
        {
            var existingItem = new FeedMessageItem()
            {
                Id = id,
                Description = message,
                ItemType = EZGO.Api.Models.Enumerations.FeedItemTypeEnum.Person,
                Title = message.Length > 250 ? message.Substring(0, 250) : message,
                CompanyId = companyId,
                FeedId = feedId,
                ItemDate = new DateTime(DateTime.UtcNow.Ticks),
                Attachments = new List<string>()
            };

            existingItem.Media = attachments.ToList();

            var endpoint = string.Format(Logic.Constants.FactoryFeed.ChangeFeedMessage, id);
            var response = await _connector.PostCall(endpoint, existingItem.ToJsonFromObject());

            var existingItemDisplay = new FactoryFeedItemModel()
            {
                Id = id,
                Description = message,
                ItemType = (int)existingItem.ItemType,
                ItemDate = existingItem.ItemDate,
                Title = existingItem.Title,
                FeedId = feedId,
                IsSticky = existingItem.IsSticky,
                IsHighlighted = existingItem.IsHighlighted,
                IsLiked = existingItem.IsLiked,
                CompanyId = companyId,
                Attachments = existingItem.Media.Select(m => m.Uri).ToList(),
                Media = existingItem.Media
            };

            var postUserResponse = await _connector.GetCall($"/v1/userprofile/{originalUserId}");
            UserProfile postUser = (postUserResponse.StatusCode == System.Net.HttpStatusCode.OK) ? JsonConvert.DeserializeObject<UserProfile>(postUserResponse.Message) : null;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                existingItemDisplay.UserId = originalUserId;
                existingItemDisplay.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                existingItemDisplay.PostUser = postUser;
                existingItemDisplay.ModifiedByUser = existingItemDisplay.CurrentUser;
                existingItemDisplay.ModifiedById = existingItemDisplay.CurrentUser.Id;
                existingItemDisplay.LikesUserIds = new List<int>();
                existingItemDisplay.LikesUsers = new List<UserBasic>();
                existingItemDisplay.ApplicationSettings = await this.GetApplicationSettings();

                existingItemDisplay.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
                existingItemDisplay.ApplicationSettings = await this.GetApplicationSettings();
                return PartialView("_contentcard", existingItemDisplay);
            }
            else
            {
                return BadRequest();
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/changefactoryupdatemessage")]
        [HttpPost]
        public async Task<IActionResult> ChangeFactoryUpdateMessage(int id, int companyId, int feedId, string message, string description, EZGO.Api.Models.Attachment attachment, bool isSticky)
        {
            var existingItem = new FeedMessageItem()
            {
                Id = id,
                Description = description,
                ItemType = EZGO.Api.Models.Enumerations.FeedItemTypeEnum.Person,
                Title = message.Length > 250 ? message.Substring(0, 250) : message,
                CompanyId = companyId,
                FeedId = feedId,
                ItemDate = new DateTime(DateTime.UtcNow.Ticks),
                Attachments = new List<string>(),
                Media = new List<EZGO.Api.Models.Attachment>(),
                IsSticky = isSticky
            };

            if (attachment != null && attachment.Uri != null)
            {
                existingItem.Media.Add(attachment);
            }

            var endpoint = string.Format(Logic.Constants.FactoryFeed.ChangeFeedMessage, id);
            var response = await _connector.PostCall(endpoint, existingItem.ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response.Message);
            else
                return BadRequest();
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/changefeedcomment")]
        [HttpPost]
        public async Task<IActionResult> ChangeFeedComment(int id, int companyId, EZGO.Api.Models.Attachment attachment, int feedId, string message, int parentId, int originalUserId)
        {
            var existingItem = new FeedMessageItem()
            {
                Id = id,
                ParentId = parentId,
                Description = message,
                ItemType = EZGO.Api.Models.Enumerations.FeedItemTypeEnum.Person,
                Title = message.Length > 250 ? message.Substring(0, 250) : message,
                CompanyId = companyId,
                FeedId = feedId,
                ItemDate = new DateTime(DateTime.UtcNow.Ticks),
                Attachments = new List<string>(),
                Media = new List<EZGO.Api.Models.Attachment>()
            };

            if (attachment != null && attachment.Uri != null)
            {
                existingItem.Media.Add(attachment);
            }
            var endpoint = string.Format(Logic.Constants.FactoryFeed.ChangeFeedMessage, id);
            var response = await _connector.PostCall(endpoint, existingItem.ToJsonFromObject());

            var existingItemDisplay = new FactoryFeedItemCommentModel()
            {
                Id = existingItem.Id,
                ParentId = parentId,
                Description = existingItem.Description,
                ItemType = (int)existingItem.ItemType,
                Title = existingItem.Title,
                CompanyId = companyId,
                FeedId = existingItem.FeedId,
                Attachments = existingItem.Media.Select(m => m.Uri).ToList(),
                Media = existingItem.Media,
                ItemDate = existingItem.ItemDate
            };

            var commentUserResponse = await _connector.GetCall($"/v1/userprofile/{originalUserId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK && commentUserResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                existingItemDisplay.UserId = originalUserId;
                existingItemDisplay.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
                existingItemDisplay.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                existingItemDisplay.PostUser = JsonConvert.DeserializeObject<UserProfile>(commentUserResponse.Message);
                existingItemDisplay.ModifiedByUser = existingItemDisplay.CurrentUser;
                existingItemDisplay.ModifiedById = existingItemDisplay.CurrentUser.Id;
                existingItemDisplay.ApplicationSettings = await this.GetApplicationSettings();
                return PartialView("_comment", existingItemDisplay);
            }
            else
            {
                return BadRequest();
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/deleteitem/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var endpoint = string.Format(Logic.Constants.FactoryFeed.RemoveFeedMessage, id);
            var response = await _connector.PostCall(endpoint, false.ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok();
            else
                return BadRequest();
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/updatecheck/{fromDateUtc}")]
        public async Task<IActionResult> UpdateCheck(DateTime fromDateUtc)
        {
            fromDateUtc = fromDateUtc.ToUniversalTime();
            var updateCheckResult = await _connector.GetCall(string.Format(Logic.Constants.FactoryFeed.UpdateCheck, fromDateUtc.ToString("MM-dd-yyyy HH:mm:ss")));
            if (updateCheckResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var updates = JsonConvert.DeserializeObject<List<UpdateCheckItem>>(updateCheckResult.Message);

                foreach(var update in updates)
                {
                    if(update.UpdateCheckType == EZGO.Api.Models.Enumerations.UpdateCheckTypeEnum.EzFeed)
                    {
                        return Ok(true);
                    }
                }

            }
            return Ok(false);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
        [Route("/factoryfeed/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var output = new FactoryFeedViewModel();
            
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.AdvancedStatsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableEZFeedStatistics");
            
            var statisticsEndpoint = string.Format(Logic.Constants.FactoryFeed.GetMyEZFeedStatistics);
            if (!output.AdvancedStatsEnabled)
            {
                statisticsEndpoint = string.Format(Logic.Constants.FactoryFeed.GetMyStatistics);
            }
            
            var statisticsResult = await _connector.GetCall(statisticsEndpoint);
            if (statisticsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var statistics = JsonConvert.DeserializeObject<List<StatisticGenericItem>>(statisticsResult.Message);

                if (output.AdvancedStatsEnabled)
                {
                    output.LikesTotal = statistics.Where(s => s.Name.ToLower().Contains("my likes total")).FirstOrDefault().CountNr ?? 0;
                    output.PostsTotal = statistics.Where(s => s.Name.ToLower().Contains("my posts total")).FirstOrDefault().CountNr ?? 0;
                    output.CommentsTotal = statistics.Where(s => s.Name.ToLower().Contains("my comments total")).FirstOrDefault().CountNr ?? 0;
                }
                else
                {
                    output.AuditsTotal = statistics.Where(s => s.Name.ToLower().Contains("my audits total")).FirstOrDefault().CountNr ?? 0;
                    output.ChecklistsTotal = statistics.Where(s => s.Name.ToLower().Contains("my checklists total")).FirstOrDefault().CountNr ?? 0;
                    output.TasksTotal = statistics.Where(s => s.Name.ToLower().Contains("my tasks total")).FirstOrDefault().CountNr ?? 0;
                }
                
                return View("~/Views/FactoryFeed/_profilecard.cshtml", output);
            }

            return BadRequest();
        }

        [Route("/factoryfeed/upload")]
        [HttpPost]
        [RequestSizeLimit(52428800)]
        public async Task<string> upload(IFormCollection data)
        {

            foreach (IFormFile item in data.Files)
            {
                //var fileContent = item;
                if (item != null && item.Length > 0)
                {
                    // get a stream
                    using (var ms = new MemoryStream())
                    {

                        item.CopyTo(ms);
                        var fileBytes = ms.ToArray();

                        using var form = new MultipartFormDataContent();
                        using var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        form.Add(fileContent, "file", Path.GetFileName(data["filename"]));

                        var endpoint = string.Format(Logic.Constants.FactoryFeed.UploadPictureUrlItem);
                        
                        var itemType = data["itemtype"].ToString().ToLower();
                        if (itemType.StartsWith("video"))
                        {
                            endpoint = string.Format(Logic.Constants.FactoryFeed.UploadVideoUrlItem);
                        }
                        else if (itemType.StartsWith("application/pdf"))
                        {
                            endpoint = string.Format(Logic.Constants.FactoryFeed.UploadDocsUrlItem);
                        }

                        ApiResponse filepath = await _connector.PostCall(endpoint, form);
                        string output = filepath.Message;
                        if (itemType.StartsWith("video"))
                        {
                            output = filepath.Message.Replace("media/", "");
                        }
                        return output;

                    }

                }
                else
                {
                    return string.Empty;
                }
            }

            return string.Empty;

        }
    }
}
