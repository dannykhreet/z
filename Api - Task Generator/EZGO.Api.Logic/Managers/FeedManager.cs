using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Api.Models.Filters;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// FeedManager; The FeedManager contains all logic for retrieving and setting feeds.
    /// Feeds contain a list of items, which can contain text, images, merged data, statistics etc.
    /// Feeds will be partly generated and partly based on direct user input. Feed items can be liked or viewed. 
    /// Feeds are currently not displayed or used, this feature was build but stored for later implementation. 
    /// </summary>
    public class FeedManager : BaseManager<FeedManager>, IFeedManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;

        #endregion

        #region - constructor(s) -
        public FeedManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, IDataAuditing dataAuditing, ILogger<FeedManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _dataAuditing = dataAuditing;

        }
        #endregion

        /// <summary>
        /// GetFeedAsync; Get a list of factory feeds.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that can be used for filtering the data.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>A list of factory feed items.</returns>
        public async Task<List<FactoryFeed>> GetFeedAsync(int companyId, int? userId = null, bool? useTreeView = true, FeedFilters? filters = null, string include = null)
        {
            var output = new List<FactoryFeed>();

            NpgsqlDataReader dr = null;

            try
            {
                var storedProcedure = "get_factoryfeeds";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue && filters.Value.FeedType.HasValue)
                {
                    storedProcedure = "get_factoryfeeds_by_type";
                    parameters.Add(new NpgsqlParameter("@_feedtype", Convert.ToInt32(filters.Value.FeedType.Value)));
                }

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var feed = CreateOrFillFeedFromReader(dr);
                        output.Add(feed);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("FeedManager.GetFeedAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }

            }

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.FeedItems.ToString().ToLower())) output = await GetFactoryFeedMessagesAsync(factoryfeeds: output, companyId: companyId, userId: userId.Value, filters: filters, include: include);
            }

            if (useTreeView.HasValue && useTreeView.Value)
            {
                output = await CreateTreeViews(output);
            }

            return output;
        }

        /// <summary>
        /// GetFeedItemsAsync, get a list of feed items, usually filtered on a specific feed for display.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that can be used for filtering the data.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of feed message items</returns>
        public async Task<List<FeedMessageItem>> GetFeedItemsAsync(int companyId, int? userId = null, bool? useTreeView = true, FeedFilters? filters = null, string include = null)
        {
            var output = new List<FeedMessageItem>();
            //"_companyid" int4, "_factoryfeedid" int4=NULL::integer, "_user" int4=NULL::integer, "_timestamp" timestamp=NULL::timestamp without time zone, "_limit" int4=0, "_offset" int4=0

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters != null)
                {
                    if (filters.Value.FactoryFeedId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_factoryfeedid", filters.Value.FactoryFeedId.Value));
                    }

                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.UserId.HasValue && filters.Value.UserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_user", filters.Value.UserId.Value));
                    }
                }

                var storedProcedure = "get_factoryfeedmessages";

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        //get the likes from "factoryfeed_messageliked" by "factoryfeedmessage_id" and get "user_id"s and populate LikesUserIds
                        var feeditem = CreateOrFillFeedItemFromReader(dr);
                        output.Add(feeditem);
                    }
                }
                foreach (var factoryFeedMessageItem in output)
                {
                    factoryFeedMessageItem.LikesUsers = await GetFactoryFeedLikesUsersAsync(factoryFeedMessageItem.Id);
                    if (factoryFeedMessageItem.LikesUsers != null && factoryFeedMessageItem.LikesUsers.Count > 0)
                    {
                        factoryFeedMessageItem.LikesUserIds = factoryFeedMessageItem.LikesUsers.Select(u => u.Id).ToList();
                        factoryFeedMessageItem.IsLiked = true;
                    }

                    if(factoryFeedMessageItem.LikesUserIds == null)
                    {
                        factoryFeedMessageItem.LikesUserIds = new List<int>();
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("FeedManager.GetFeedItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }

            }

            return output;
        }

        /// <summary>
        /// GetFeedItemCommesAsync, get a list of comments belonging to a feed item on a feed.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that can be used for filtering the data.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of feed message items</returns>
        public async Task<List<FeedMessageItem>> GetFeedItemCommentsAsync(int companyId, int? userId = null, bool? useTreeView = true, FeedFilters? filters = null, string include = null)
        {
            var output = new List<FeedMessageItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters != null)
                {
                    if (filters.Value.FactoryFeedId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_factoryfeedid", filters.Value.FactoryFeedId.Value));
                    }
                    if (filters.Value.FeedMessageId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_factoryfeeditemid", filters.Value.FeedMessageId.Value));
                    }

                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }
                }

                var storedProcedure = "get_factoryfeedmessagecomments";

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var feeditem = CreateOrFillFeedItemFromReader(dr);
                        output.Add(feeditem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("FeedManager.GetFeedItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// GetFeedItemAsync; get a single feed item by its unique identifier.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="feedItemId">FeedItemId (DB: factoryfeed_messages.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="useTreeView">Indicates whether a tree view structure should be applied (reserved for future use).</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>Single feed message item matching the supplied identifier, or null when no item is found.</returns>
        public async Task<FeedMessageItem> GetFeedItemAsync(int companyId,int feedItemId,int? userId = null,bool? useTreeView = true,string include = null)
        {
            FeedMessageItem output = null;
            NpgsqlDataReader dr = null;

            try
            {
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_id", feedItemId)
                };

                var storedProcedure = "get_factoryfeedmessage_by_id";

                using (dr = await _manager.GetDataReader(storedProcedure,commandType: CommandType.StoredProcedure,parameters: parameters))
                {
                    if (await dr.ReadAsync())
                    {
                        output = CreateOrFillFeedItemFromReader(dr);
                    }
                }

                if (output != null)
                {
                    output.LikesUsers = await GetFactoryFeedLikesUsersAsync(output.Id);

                    if (output.LikesUsers != null && output.LikesUsers.Count > 0)
                    {
                        output.LikesUserIds = output.LikesUsers.Select(u => u.Id).ToList();
                        output.IsLiked = true;
                    }

                    if (output.LikesUserIds == null)
                    {
                        output.LikesUserIds = new List<int>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("FeedManager.GetFeedItemAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY))
                {
                    this.Exceptions.Add(ex);
                }
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }
            }

            return output;
        }

        /// <summary>
        /// AddFeedAsync; Add a new factory feed to the database
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feed">Feed item containing all data</param>
        /// <returns>Id of the new feed.</returns>
        public async Task<int> AddFeedAsync(int companyId, int userId, FactoryFeed feed)
        {
            List<NpgsqlParameter> parameters = GetNpgsqlParametersFromFeed(companyId: companyId, feed);
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_factoryfeed", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeeds.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.factoryfeeds.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Added EZ Feed.");
            }
            return id;
        }

        /// <summary>
        /// ChangeFeedAsync; Change an existing feed.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feedId">Factoryfeeds.id</param>
        /// <param name="feed">Feed containing all data</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ChangeFeedAsync(int companyId, int userId, int feedId, FactoryFeed feed)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeeds.ToString(), feedId);
            List<NpgsqlParameter> parameters = GetNpgsqlParametersFromFeed(companyId: companyId, feed);
            var resultCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_factoryfeed", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (resultCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeeds.ToString(), feedId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.factoryfeeds.ToString(), objectId: feedId, userId: userId, companyId: companyId, description: "Changed EZ Feed.");

            }
            return resultCount > 0;
        }

        /// <summary>
        /// AddFeedItemAsync; Add new feed item. Feed items are added to a feed.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feedItem">FeedItem containing all data</param>
        /// <returns>Id of the new item.</returns>
        public async Task<int> AddFeedItemAsync(int companyId, int userId, FeedMessageItem feedItem)
        {
            List<NpgsqlParameter> parameters = GetNpgsqlParametersFromFeedItem(companyId: companyId, feedItem: feedItem, userId: userId);
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_factoryfeed_message", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeed_messages.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.factoryfeed_messages.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Added feed message item.");
            }
            return id;

        }

        /// <summary>
        /// ChangeFeedItemAsync; Change an existing feed item. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feedItemId">Id of the item that need to be changed.</param>
        /// <param name="feedItem">FeedItem containing data.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ChangeFeedItemAsync(int companyId, int userId, int feedItemId, FeedMessageItem feedItem)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeed_messages.ToString(), feedItemId);
            List<NpgsqlParameter> parameters = GetNpgsqlParametersFromFeedItem(companyId: companyId, feedItem: feedItem, userId: userId);
            var resultCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_factoryfeed_message", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (resultCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeed_messages.ToString(), feedItemId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.factoryfeed_messages.ToString(), objectId: feedItemId, userId: userId, companyId: companyId, description: "Changed feed message item.");

            }
            return resultCount > 0;
        }

        /// <summary>
        /// SetFeedItemActiveAsync; Set a item active/inactive
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feedItemId">FeedItem Id which need to be set inactive.</param>
        /// <param name="isActive">true/false depending on what needs to be done.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SetFeedItemActiveAsync(int companyId, int userId, int feedItemId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeed_messages.ToString(), feedItemId);
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", feedItemId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var resultCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_factoryfeedmessage_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (resultCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.factoryfeed_messages.ToString(), feedItemId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.factoryfeed_messages.ToString(), objectId: feedItemId, userId: userId, companyId: companyId, description: "Changed feed message item active state.");
            }
            return resultCount > 0;
        }

        /// <summary>
        /// SetFeedItemLikedAsync; Set a item liked for a specific user.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feedItemId">FeedItemId (DB: factoryfeed_messages.id)</param>
        /// <param name="isLiked">liked, true/false</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SetFeedItemLikedAsync(int companyId, int userId, int feedItemId, bool isLiked = true)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", feedItemId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            var storedProcedure = isLiked ? "set_factoryfeedmessage_liked" : "set_factoryfeedmessage_unliked";
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);
        }

        /// <summary>
        /// SetFeedItemViewedAsync; Set a item viewed for a specific user.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="feedItemId">FeedItemId (DB: factoryfeed_messages.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SetFeedItemViewedAsync(int companyId, int feedItemId, int userId)
        {

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", feedItemId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_factoryfeedmessage_viewed", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);

        }

        /// <summary>
        /// CreateOrFillFeedFromReader; creates and fills a feed object from a DataReader.
        /// NOTE! intended for use with the feed stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="feed">Feed object, if not supplied will be created.</param>
        /// <returns>Factory feed object.</returns>
        private FactoryFeed CreateOrFillFeedFromReader(NpgsqlDataReader dr, FactoryFeed feed = null)
        {
            if (feed == null) feed = new FactoryFeed();

            feed.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                feed.Description = dr["description"].ToString();
            }
            feed.Name = dr["name"].ToString();
            feed.CompanyId = Convert.ToInt32(dr["company_id"]);
            feed.Attachments = dr["attachments"].ToString().ToObjectFromJson<List<string>>();
            feed.FeedType = (FeedTypeEnum)dr["feed_type"];

            //handle data


            return feed;
        }

        /// <summary>
        /// GetNpgsqlParametersFromFeed; Creates a list of NpgsqlParameters, and fills it based on the supplied feed object.
        /// NOTE! intended for use with the feed stored procedures within the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="feed">Feed object containing all data.</param>
        /// <returns>List of NpgsqlParameter, filled with data.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromFeed(int companyId, FactoryFeed feed)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (feed.Id > 0)
            {
                parameters.Add(new NpgsqlParameter("@_id", feed.Id));

            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_feedtype", Convert.ToInt32(feed.FeedType)));
            }

            parameters.Add(new NpgsqlParameter("@_name", feed.Name));
            parameters.Add(new NpgsqlParameter("@_description", feed.Description));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_attachments", feed.Attachments.ToJsonFromObject()));

            //data
            parameters.Add(new NpgsqlParameter("@_data", DBNull.Value));

            return parameters;
        }

        /// <summary>
        /// CreateOrFillFeedItemFromReader; Creates a lFeedMessageItem and fills it based on the supplied dr object.
        /// NOTE! intended for use with the feeditem stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="feedItem">FeedMessageItem, if not supplied will be created</param>
        /// <returns>FeedMessageItem item containing all relevant data.</returns>
        private FeedMessageItem CreateOrFillFeedItemFromReader(NpgsqlDataReader dr, FeedMessageItem feedItem = null)
        {
            if (feedItem == null) feedItem = new FeedMessageItem();

            feedItem.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                feedItem.Description = dr["description"].ToString();
            }
            feedItem.CompanyId = Convert.ToInt32(dr["company_id"]);
            feedItem.Title = dr["title"].ToString();
            feedItem.FeedId = Convert.ToInt32(dr["factoryfeed_id"]);
            feedItem.ItemDate = Convert.ToDateTime(dr["message_date"]);
            feedItem.IsSticky = (bool)dr["is_sticky"];
            feedItem.IsHighlighted = (bool)dr["is_highlighted"];
            feedItem.ItemType = (FeedItemTypeEnum)dr["message_type"];

            if (dr.HasColumn("attachments") && dr["attachments"] != DBNull.Value && !string.IsNullOrEmpty(dr["attachments"].ToString()))
            {
                var attachmentsBasic = new List<string>();
                var attachmentsExtended = new List<Attachment>();
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    attachmentsBasic = dr["attachments"].ToString().ToObjectFromJson<List<string>>();
                }
                catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    attachmentsExtended = dr["attachments"].ToString().ToObjectFromJson<List<Attachment>>();
                }
                catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
                if (attachmentsBasic != null && attachmentsBasic.Count > 0)
                {
                    feedItem.Attachments = attachmentsBasic;
                    feedItem.Media = attachmentsBasic.Select(a => new Attachment() { Uri = a }).ToList();
                }
                else if (attachmentsBasic != null && attachmentsExtended.Count > 0)
                {
                    feedItem.Media = attachmentsExtended;
                    feedItem.Attachments = attachmentsExtended.Select(a => a.Uri).ToList();
                }
            }

            feedItem.UserId = Convert.ToInt32(dr["user_id"]);

            if (dr.HasColumn("comment_count") && dr["comment_count"] != DBNull.Value)
            {
                feedItem.CommentCount = Convert.ToInt32(dr["comment_count"]);
            }

            if (dr.HasColumn("parent_id") && dr["parent_id"] != DBNull.Value)
            {
                feedItem.ParentId = Convert.ToInt32(dr["parent_id"]);
            }

            if (dr.HasColumn("modified_by_id") && dr["modified_by_id"] != DBNull.Value)
            {
                feedItem.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
            }

            if (feedItem.UserId.HasValue && feedItem.UserId.Value > 0 &&
                dr.HasColumn("created_by") && dr["created_by"] != DBNull.Value)
            {
                feedItem.PostUser = new Models.Basic.UserBasic()
                {
                    Id = feedItem.UserId.Value,
                    Name = Convert.ToString(dr["created_by"])
                };
                if (dr.HasColumn("created_by_picture") && dr["created_by_picture"] != DBNull.Value)
                {
                    feedItem.PostUser.Picture = Convert.ToString(dr["created_by_picture"]);
                }
            }

            if (feedItem.ModifiedById > 0 &&
                dr.HasColumn("modified_by") && dr["modified_by"] != DBNull.Value)
            {
                feedItem.ModifiedByUser = new Models.Basic.UserBasic()
                {
                    Id = feedItem.ModifiedById,
                    Name = Convert.ToString(dr["modified_by"]),
                };
                if (dr.HasColumn("modified_by_picture") && dr["modified_by_picture"] != DBNull.Value)
                {
                    feedItem.ModifiedByUser.Picture = Convert.ToString(dr["modified_by_picture"]);
                }
            }

            return feedItem;
        }

        /// <summary>
        /// GetNpgsqlParametersFromFeedItem; Creates a list of NpgsqlParameters, and fills it based on the supplied feed item object.
        /// NOTE! intended for use with the feed item stored procedures within the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="feedItem">FeedMessageItem containing all relevant data.</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromFeedItem(int companyId, int userId, FeedMessageItem feedItem)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (feedItem.Id > 0) parameters.Add(new NpgsqlParameter("@_id", feedItem.Id));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_factoryfeedid", feedItem.FeedId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            parameters.Add(new NpgsqlParameter("@_title", feedItem.Title));
            parameters.Add(new NpgsqlParameter("@_description", feedItem.Description));

            if (feedItem.Media != null && feedItem.Media.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_attachments", feedItem.Media.ToJsonFromObject()));
            }
            else if (feedItem.Attachments != null && feedItem.Attachments.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_attachments", feedItem.Attachments.ToJsonFromObject()));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_attachments", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_issticky", feedItem.IsSticky));
            parameters.Add(new NpgsqlParameter("@_ishighlighted", feedItem.IsHighlighted));

            parameters.Add(new NpgsqlParameter("@_messagetype", Convert.ToInt32(feedItem.ItemType)));

            // parameters.Add(new NpgsqlParameter("@_userid", userId));

            parameters.Add(new NpgsqlParameter("@_data", DBNull.Value) { DbType = DbType.String });

            parameters.Add(new NpgsqlParameter("@_messagedate", new DateTime(feedItem.ItemDate.Ticks)));

            if (feedItem.ParentId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_parentid", feedItem.ParentId.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_parentid", DBNull.Value) { DbType=DbType.Int32 });
            }

            return parameters;
        }

        /// <summary>
        /// GetFactoryFeedMessagesAsync; Get a list of FactoryFeed items.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="factoryfeeds">List of feeds where messages need to be added.</param>
        /// <param name="filters">Filters where the data needs to be filtered.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of factory feeds containing messages.</returns>
        private async Task<List<FactoryFeed>> GetFactoryFeedMessagesAsync(int companyId, int userId, List<FactoryFeed> factoryfeeds, FeedFilters? filters = null, string include = null)
        {
            foreach (var feed in factoryfeeds)
            {
                var newFilters = filters.Value;
                newFilters.FactoryFeedId = feed.Id;
                filters = newFilters;

                var items = await GetFeedItemsAsync(companyId: companyId, userId: userId, filters: filters, include: include);
                feed.Items = items.Where(x => x.FeedId == feed.Id).ToList();
            }
            return factoryfeeds;
        }

        /// <summary>
        /// CreateTreeView; create a tree view of a factory feed for display purposes.
        /// </summary>
        /// <param name="factoryFeeds">FactoryFeeds to be tree-viewed</param>
        /// <returns>Object tree containing a factory feed tree.</returns>
        private async Task<List<FactoryFeed>> CreateTreeViews(List<FactoryFeed> factoryFeeds)
        {
            foreach (var factoryFeed in factoryFeeds)
            {
                if (factoryFeed.Items != null && factoryFeed.Items.Count > 0)
                {
                    factoryFeed.Items = await CreateTreeView(factoryFeed.Items);
                }

            }

            return factoryFeeds;
        }

        /// <summary>
        /// CreateTreeView; Create a tree view of a list of message items, used within the CreateTreeViews methods and it self (recursive).
        /// </summary>
        /// <param name="feedMessageItems">Messages to be tree viewed.</param>
        /// <param name="parentid">ParentId of new tree node</param>
        /// <returns>List of treeview items.</returns>
        private async Task<List<FeedMessageItem>> CreateTreeView(List<FeedMessageItem> feedMessageItems, int parentid = 0)
        {
            var output = new List<FeedMessageItem>();

            foreach (var item in feedMessageItems)
            {
                var currentItem = item;
                if (currentItem.ParentId.HasValue == false || currentItem.ParentId.Value == parentid)
                {
                    var foundItems = await CreateTreeView(feedMessageItems.Where(x => x.ParentId.HasValue == true).ToList(), currentItem.Id);
                    if (foundItems != null && foundItems.Count > 0)
                    {
                        currentItem.Comments = foundItems;
                    }
                    output.Add(currentItem);
                }
            }

            await Task.CompletedTask;

            return output;
        }

        private async Task<List<int>> GetFactoryFeedLikesAsync(int factoryFeedMessageItemId)
        {

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_factoryfeedmessage_id", factoryFeedMessageItemId));

            var storedProcedure = "get_factoryfeedlikes";
            List<int> likes = new List<int>();
            using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
            {
                while (await dr.ReadAsync())
                {

                    likes.Add(Convert.ToInt32(dr["user_id"]));
                }
            }

            return likes;
        }

        private async Task<List<UserBasic>> GetFactoryFeedLikesUsersAsync(int factoryFeedMessageItemId)
        {

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_factoryfeedmessage_id", factoryFeedMessageItemId));

            var storedProcedure = "get_factoryfeedlikes";
            List<UserBasic> likes = new List<UserBasic>();
            using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
            {
                while (await dr.ReadAsync())
                {
                    if (dr.HasColumn("user_id") && dr["user_id"] != DBNull.Value &&
                        dr.HasColumn("created_by") && dr["created_by"] != DBNull.Value)
                    {
                        var user = new UserBasic()
                        {
                            Id = Convert.ToInt32(dr["user_id"]),
                            Name = Convert.ToString(dr["created_by"])
                        };

                        if (dr.HasColumn("created_by_picture") && dr["created_by_picture"] != DBNull.Value)
                        {
                            user.Picture = Convert.ToString(dr["created_by_picture"]);
                        }
                        
                        likes.Add(user);
                    }
                }
            }

            return likes;
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
