using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models.Companies;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    internal class TagManager : BaseManager<ITagManager>, ITagManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _databaseAccessHelper;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;
        private readonly ICompanyManager _companyManager;
        private readonly IToolsManager _toolsManager;

        //Index in dictionary is based on group guid in db
        //TODO: a better way to manage default configs for system tags
        private readonly Dictionary<string, TagConfiguration> _defaultTagConfigs;
        private readonly Dictionary<string, string> _tagLanguageKeys;
        #endregion

        #region - properties -
        private string culture;
        public string Culture { 
            get { return culture ?? "en_us"; } 
            set { culture = value; GetSystemTagTranslations(); }
        }

        Dictionary<string, string> _systemTagTranslations;
        #endregion

        #region - constructor(s) -
        public TagManager(IToolsManager toolsManager, IDatabaseAccessHelper databaseAccessHelper, IConfigurationHelper configurationHelper, IDataAuditing dataAuditing, ICompanyManager companyManager, ILogger<ITagManager> logger) : base(logger)
        {
            _databaseAccessHelper = databaseAccessHelper;
            _configurationHelper = configurationHelper;
            _dataAuditing = dataAuditing;
            _companyManager = companyManager;
            _toolsManager = toolsManager;

            //the default configuration for tags in each group
            _defaultTagConfigs = new Dictionary<string, TagConfiguration>
            {
                { "b2ce6e074ef240558b951b20c8aa232b", new TagConfiguration { ColorCode = "#ed1c24", IconName = "shield-halved", IconStyle = "solid" } },            //Safety
                { "c2fdb4ba02894a6ea7d499a34939b994", new TagConfiguration { ColorCode = "#ec008c", IconName = "gem", IconStyle = "regular" } },                    //Quality
                { "91cf310a89354cbc9a97502279bd2bc1", new TagConfiguration { ColorCode = "#608B58", IconName = "dollar-sign", IconStyle = "solid" } },              //Cost
                { "f4d78b286bb744a6a61c18ee7ab2a33b", new TagConfiguration { ColorCode = "#daa520", IconName = "cogs", IconStyle = "solid" } },                     //Maintenance
                { "6e771dc81ddc44f49393074177e2d442", new TagConfiguration { ColorCode = "#39b54a", IconName = "leaf", IconStyle = "solid" } },                     //Sustainability
                { "c929a51a292d4c33a5410cf2586cd6e0", new TagConfiguration { ColorCode = "#B9794B", IconName = "users", IconStyle = "solid" } },                    //People
                { "9C2C09F5EED943CA9BB9C4312FD6BC85", new TagConfiguration { ColorCode = "#f7931e", IconName = "chart-line", IconStyle = "solid" } },               //Continuous Improvement
                { "A4F6C688EE17432EA645A2D380C3F82E", new TagConfiguration { ColorCode = "#11247F", IconName = "hashtag", IconStyle = "solid" } },                  //General
                { "224A926BC8B2438E9578BEEB370A91A8", new TagConfiguration { ColorCode = "#D7DF23", IconName = "user-clock", IconStyle = "solid" } },               //Productivity
                { "A9CC94545E9F482CB33D9B93AD503F4F", new TagConfiguration { ColorCode = "#4EAAA2", IconName = "chart-simple", IconStyle = "solid" } },             //Efficiency
                { "8CA707C185694011A209F8B75F45AEB4", new TagConfiguration { ColorCode = "#A2E583", IconName = "right-from-bracket", IconStyle = "solid" } },       //Output
                { "A7F6844C6E62408587CE3B9C29FAAFCA", new TagConfiguration { ColorCode = "#E0C862", IconName = "truck-arrow-right", IconStyle = "solid" } },        //Delivery
                { "B2CC971F5C3B45BC97CBE165DB892C2F", new TagConfiguration { ColorCode = "#D38086", IconName = "user-tag", IconStyle = "solid" } }                  //Customer
            };

            _tagLanguageKeys = new()
            {
                { "b2ce6e074ef240558b951b20c8aa232b", "CMS_TAGS_SAFETY"},                    //Safety
                { "c2fdb4ba02894a6ea7d499a34939b994", "CMS_TAGS_QUALITY"},                   //Quality
                { "91cf310a89354cbc9a97502279bd2bc1", "CMS_TAGS_COST"},                      //Cost
                { "f4d78b286bb744a6a61c18ee7ab2a33b", "CMS_TAGS_MAINTENANCE"},               //Maintenance
                { "6e771dc81ddc44f49393074177e2d442", "CMS_TAGS_SUSTAINABILITY"},            //Sustainability
                { "c929a51a292d4c33a5410cf2586cd6e0", "CMS_TAGS_PEOPLE"},                    //People
                { "9C2C09F5EED943CA9BB9C4312FD6BC85", "CMS_TAGS_CONTINUOUS_IMPROVEMENT"},    //Continuous Improvement
                { "A4F6C688EE17432EA645A2D380C3F82E", "CMS_TAGS_GENERAL"},                   //General
                { "224A926BC8B2438E9578BEEB370A91A8", "CMS_TAGS_PRODUCTIVITY"},              //Productivity
                { "A9CC94545E9F482CB33D9B93AD503F4F", "CMS_TAGS_EFFICIENCY"},                //Efficiency
                { "8CA707C185694011A209F8B75F45AEB4", "CMS_TAGS_OUTPUT"},                    //Output
                { "A7F6844C6E62408587CE3B9C29FAAFCA", "CMS_TAGS_DELIVERY"},                  //Delivery
                { "B2CC971F5C3B45BC97CBE165DB892C2F", "CMS_TAGS_CUSTOMER"}                   //Customer
            };
        }
        #endregion

        #region - public methods tags -
        /// <summary>
        /// Add a tag for a company or a holding.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<int> AddTagAsync(int companyId, int userId, Tag tag)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_userid", userId)
            };

            parameters.AddRange(GetNpgsqlParametersFromTag(tag: tag));

            var possibleId = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_tag", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                tag.Id = possibleId;

                List<NpgsqlParameter> groupParameters = new()
                {
                    new NpgsqlParameter("@_groupguid", tag.GroupGuid),
                    new NpgsqlParameter("@_tagid", possibleId)
                };
                var rowcount2 = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_tag_taggroup_relation", parameters: groupParameters, commandType: System.Data.CommandType.StoredProcedure));

                var holdingId = 0;
                if (tag.IsHoldingTag == true)
                {
                    holdingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
                }
                if (holdingId > 0)
                {
                    //holding logic
                    await AddTagHoldingRelation(holdingId, possibleId);

                    var rowcount = await AddTagToAllCompaniesInHolding(holdingId, possibleId);
                    var rowcountConfig = await AddChangeHoldingTagConfiguration(holdingId, tag, userId, companyId);

                    var companyIdsInHolding = await _companyManager.GetCompanyIdsInHolding(holdingId);

                    //add data auditing for each company in the holding
                    foreach (int companyIdInHolding in companyIdsInHolding)
                    {
                        var mutatedRelation = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_company.ToString(), fieldName: Models.Enumerations.TableFields.tag_id.ToString(), id: possibleId, fieldname2: Models.Enumerations.TableFields.company_id.ToString(), id2: companyIdInHolding);
                        await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutatedRelation, Models.Enumerations.TableNames.tags_tag_company.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added tag company relation.");
                    }
                }
                else
                {
                    //single company logic
                    var configurationId = await AddChangeTagConfiguration(companyId, tag, userId);

                    var tagCompanyRelationId = await AddTagCompanyRelation(companyId, userId, possibleId);
                }

                var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.tags_tag.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added tag.");
            }
            return possibleId;
        }

        /// <summary>
        /// Updates tag with the data provided by the Tag object.
        /// If the tag is a holding tag, a relation will be added to all companies in the holding.
        /// A tag configuration will be saved for each company in the holding.
        /// If the tag is not a holding tag, any existing relations with other companies will be removed.
        /// </summary>
        /// <param name="companyId">Id of the company</param>
        /// <param name="userId">Id of the user</param>
        /// <param name="tagId">Id of the tag</param>
        /// <param name="tag">Tag object containing the new data</param>
        /// <returns></returns>
        public async Task<bool> ChangeTagAsync(int companyId, int userId, int tagId, Tag tag)
        {
            //data auditing for tag
            var original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag.ToString(), tagId);

            var rowsaffected = 0;

            //if system tag, only update the configuration
            if (tag.IsSystemTag == true)
            {
                int holdingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
                if (holdingId > 0)
                {
                    var rowcountConfig = await AddChangeHoldingTagConfiguration(holdingId, tag, userId, companyId);
                }
                else
                {
                    var configId = await AddChangeTagConfiguration(companyId, tag, userId);
                }
            }
            else
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("_companyid", companyId),
                    new NpgsqlParameter("_userid", userId)
                };
                parameters.AddRange(GetNpgsqlParametersFromTag(tag, tagId));

                rowsaffected = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("change_tag", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                //update tag company relations for companies in holding
                int holdingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
                if (holdingId > 0)
                {
                    var otherCompanyIdsInHolding = await _companyManager.GetCompanyIdsInHolding(holdingId);
                    otherCompanyIdsInHolding.Remove(companyId);

                    Dictionary<int, string> companyAuditingDataOriginal = new();
                    foreach (int companyIdInHolding in otherCompanyIdsInHolding)
                    {
                        string originalRelation = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_company.ToString(), fieldName: Models.Enumerations.TableFields.tag_id.ToString(), id: tagId, fieldname2: Models.Enumerations.TableFields.company_id.ToString(), id2: companyIdInHolding);
                        companyAuditingDataOriginal.Add(companyIdInHolding, originalRelation);
                    }

                    int rowCount = 0;
                    if (tag.IsHoldingTag == true)
                    {
                        await AddTagHoldingRelation(holdingId, tagId);
                        rowCount = await AddTagToAllCompaniesInHolding(holdingId: holdingId, tagId: tagId);
                        var rowcountConfig = await AddChangeHoldingTagConfiguration(holdingId, tag, userId, companyId);
                    }
                    else if (tag.IsHoldingTag == false)
                    {
                        rowCount = await RemoveTagFromOtherCompaniesInHolding(holdingId: holdingId, tagId: tagId, companyId: companyId);
                        await RemoveTagHoldingRelation(holdingId, tagId);
                        var configId = await AddChangeTagConfiguration(companyId, tag, userId);
                    }


                    if (rowCount > 0)
                    {
                        foreach (int otherCompanyId in otherCompanyIdsInHolding)
                        {
                            var mutatedRelation = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_company.ToString(), fieldName: Models.Enumerations.TableFields.tag_id.ToString(), id: tagId, fieldname2: Models.Enumerations.TableFields.company_id.ToString(), id2: otherCompanyId);
                            await _dataAuditing.WriteDataAudit(original: companyAuditingDataOriginal[otherCompanyId], mutated: mutatedRelation, Models.Enumerations.TableNames.tags_tag_company.ToString(), objectId: tagId, userId: userId, companyId: companyId, description: "Changed tag company relation.");
                        }
                    }
                }
                else
                {
                    //company is not in a holding, only update the tag configuration
                    var configId = await AddChangeTagConfiguration(companyId, tag, userId);
                }
            }

            //data auditing for tag
            var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag.ToString(), tagId);
            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tags_tag.ToString(), objectId: tagId, userId: userId, companyId: companyId, description: "Changed tag.");

            return rowsaffected > 0;
        }

        /// <summary>
        /// Get tag based on tag id
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="tagId">tag id</param>
        /// <returns>tag object with given id</returns>
        public async Task<Tag> GetTagAsync(int companyId, int tagId)
        {
            var tag = new Tag();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_id", tagId)
                };

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tag", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    CreateOrFillTagFromReader(dr, tag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            if (tag.Id > 0)
            {
                return tag;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all active tags for the company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="include">'usage' can be included, this will add data about where each tag has been applied (eg. checklists, audits, tasks, etc.)</param>
        /// <returns>A list of tag objects</returns>
        public async Task<List<Tag>> GetTagsAsync(int companyId, Features features = null, TagsFilters? filters = null, string include = null)
        {
            var output = new List<Tag>();
            string databaseFunctionName = "get_tags";
            if (include != null && include.Split(",").Contains(IncludesEnum.Usage.ToString().ToLower())) databaseFunctionName = "get_tags_with_usage";

            List<NpgsqlParameter> parameters = new();
            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                
                if (filters.HasValue)
                {
                    if (!string.IsNullOrEmpty(filters.Value.Type))
                    {
                        parameters.Add(new NpgsqlParameter("@_type", filters.Value.Type));

                        if (filters.Value.AreaId.HasValue)
                        {
                            parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                        }
                    }
                }

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader(databaseFunctionName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    var tag = CreateOrFillTagFromReader(dr, tag: null, features: features);
                    if (tag != null && tag.Id > 0)
                    {
                        if (tag.GroupGuid != null)
                            output.Add(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        /// <summary>
        /// Get tag names based on tag ids
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="tagIds">tag ids to get the names for</param>
        /// <returns>dictionary of tag ids and tag names</returns>
        public async Task<Dictionary<int, string>> GetTagNamesAsync(int companyId, List<int> tagIds)
        {
            Dictionary<int, string> idsNames = new();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_tagids", tagIds)
                };

                using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tag_names", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    int id = Convert.ToInt32(dr["id"]);
                    string name = dr["name"].ToString();
                    idsNames.Add(id, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagNames(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return idsNames;
        }

        /// <summary>
        /// Gets active tags by tag group for a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="groupId">tag group id</param>
        /// <returns>List of tag objects</returns>
        public async Task<List<Tag>> GetTagsByGroupAsync(int companyId, string groupId)
        {
            var output = new List<Tag>();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_taggroupid", groupId)
                };

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tags_by_taggroup_id", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    var tag = CreateOrFillTagFromReader(dr);
                    if (tag != null && tag.Id > 0)
                    {
                        output.Add(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return output;
        }

        public async Task<Dictionary<string, string>> GetTagGroupTranslationsAsync()
        {
            Dictionary<string, string> output = new();
            try
            {
                if (string.IsNullOrEmpty(Culture) || Culture == "en_en")
                {
                    Culture = "en_us";
                }
                else
                {
                    string activeCultures = await _toolsManager.GetSupportedLanguages();
                    if (!activeCultures.Contains(Culture))
                    {
                        Culture = "en_us"; //default to english if language is not active.
                    }
                }

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_culture", Culture)
                };

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_resource_language_tag_groups", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    output.Add(dr["resource_key"].ToString(), dr["resource_value"].ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GetTagGroupTranslationsAsync.GetTagsAsync(): ", ex.Message));
            }
            return output;
        }

        /// <summary>
        /// Gets tag groups including a list of tags per group for a company.
        /// Can return all groups for management purposes.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="returnAllGroups">If true will return all tag groups. Check the IsSelected bool to see wich ones are selected for the company</param>
        /// <param name="include">'usage' can be included. This will use the IsInUse bool to indicate if a group has tags in it that are in use. It will also add data about where each tag has been applied (eg. checklists, audits, tasks, etc.)</param>
        /// <returns>A list of TagGroup objects</returns>
        public async Task<List<TagGroup>> GetTagGroupsAsync(int companyId, bool returnAllGroups = false, string include = null, Features features = null)
        {
            var tagGroups = new List<TagGroup>();
            var tags = await GetTagsAsync(companyId: companyId, features: features, include: include);

            try
            {
                int holdingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
                List<NpgsqlParameter> parameters = new();
                string databaseFunctionName;

                if (holdingId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                    databaseFunctionName = "get_taggroups";
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    databaseFunctionName = "get_taggroups_for_company";
                }

                await using (NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader(databaseFunctionName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var tagGroup = CreateOrFillTagGroupFromReader(dr);
                        if (tagGroup != null && tagGroup.Id > 0)
                        {
                            if (returnAllGroups || tagGroup.IsSelected == true)
                                tagGroups.Add(tagGroup);
                        }
                    }
                }

                foreach (var tagGroup in tagGroups)
                {
                    tagGroup.Tags = tags.Where(tag => tag.GroupGuid == tagGroup.Guid).OrderBy(tag => tag.Id).ToList();
                    Tag systemTag = tagGroup.Tags.Find(t => t.IsSystemTag == true);
                    if (systemTag?.UseTranslation == true)
                    {
                        tagGroup.Name = systemTag.Name;
                        foreach (var item in tagGroup.Tags)
                        {
                            item.GroupName = tagGroup.Name;
                        }
                    }

                    if (include != null && include.Split(",").Contains(IncludesEnum.Usage.ToString().ToLower()))
                        foreach (Tag tag in tagGroup.Tags)
                        {
                            tagGroup.IsInUse = false;
                            if (tag.IsInUse == true)
                            {
                                tagGroup.IsInUse = true;
                                break;
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagGroupsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return tagGroups;
        }

        /// <summary>
        /// Sets the tag groups that are in use by the holding or the company.
        /// If the company is in a holding, the groups will be selected for all companies in the holding.
        /// Tag groups with IsSelected true will be selected, while tag groups with IsSelected false will be deselected.
        /// </summary>
        /// <param name="companyId">comapny id</param>
        /// <param name="tagGroups">a list of tag groups</param>
        /// <returns>number of tag group relations</returns>
        public async Task<int> SetTagGroupsAsync(int companyId, List<TagGroup> tagGroups)
        {
            List<TagGroup> currentTagGroups = new();
            int relationsChangedCount = 0;

            try
            {
                string databaseFunctionName;
                List<NpgsqlParameter> parameters = new();
                int holdingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
                if (holdingId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                    databaseFunctionName = "get_taggroups";
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    databaseFunctionName = "get_taggroups_for_company";
                }

                await using (NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader(databaseFunctionName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var tagGroup = CreateOrFillTagGroupFromReader(dr);
                        if (tagGroup != null && tagGroup.Id > 0)
                        {
                            currentTagGroups.Add(tagGroup);
                        }
                    }
                }

                List<TagGroup> currentSelected = currentTagGroups.Where(tg => tg.IsSelected == true).ToList();
                List<TagGroup> newSelected = tagGroups.Where(tg => tg.IsSelected == true).ToList();

                List<TagGroup> groupsToRemove = currentSelected.Where(x => !newSelected.Select(y => y.Id).ToList().Contains(x.Id)).ToList();
                List<TagGroup> groupsToAdd = newSelected.Where(x => !currentSelected.Select(y => y.Id).ToList().Contains(x.Id)).ToList();

                if (groupsToRemove != null && groupsToRemove.Count > 0)
                {
                    foreach (var taggroup in groupsToRemove)
                    {
                        int rowncount = 0;

                        if (holdingId > 0)
                            rowncount = await RemoveTagGroupHoldingRelationAsync(holdingId: holdingId, taggroupId: taggroup.Id);
                        else
                            rowncount = await RemoveTagGroupCompanyRelationAsync(taggroupId: taggroup.Id, companyId: companyId);

                        if (rowncount > 0) relationsChangedCount++;
                    }
                }

                //Add tags (not if relation in db already exists it will be ignored on execution within the SP)
                if (groupsToAdd != null && groupsToAdd.Count > 0)
                {
                    foreach (var taggroup in groupsToAdd)
                    {
                        int id = 0;

                        if (holdingId > 0)
                            id = await this.AddTagGroupHoldingRelationAsync(holdingId: holdingId, taggroupId: taggroup.Id);
                        else
                            id = await AddTagGroupCompanyRelationAsync(taggroupId: taggroup.Id, companyId: companyId);

                        if (id > 0) relationsChangedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagGroupsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return relationsChangedCount;
        }

        /// <summary>
        /// sets the tag to active or inactive
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="userId">user id</param>
        /// <param name="tagId">tag id</param>
        /// <param name="isActive">if tag should be active or inactive</param>
        /// <returns>true if a record was changed in the database</returns>
        public async Task<bool> SetTagActiveAsync(int companyId, int userId, int tagId, bool isActive = true)
        {
            var original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag.ToString(), tagId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_userid", userId),
                new NpgsqlParameter("@_tagid", tagId),
                new NpgsqlParameter("@_active", isActive)
            };
            var rowseffected = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("set_tag_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag.ToString(), tagId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tags_tag.ToString(), objectId: tagId, userId: userId, companyId: companyId, description: "Changed tag active state.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// GetTagsWithActionAsync; Gets a list of tags related to the object of which the id was given.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="objectType">Object type to get the tags for</param>
        /// <param name="id">Id of the object to get the tags for</param>
        /// <returns>A list of tag objects. Empty list if no tags are found.</returns>
        public async Task<List<Tag>> GetTagsWithObjectAsync(int companyId, ObjectTypeEnum objectType, int id, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var tagsFromObject = new List<Tag>();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId)
                };

                string idFieldName = "";

                switch (objectType)
                {
                    case ObjectTypeEnum.Action:
                        idFieldName = Models.Enumerations.TableFields.action_id.ToString();
                        break;
                    case ObjectTypeEnum.Comment:
                        idFieldName = Models.Enumerations.TableFields.comment_id.ToString();
                        break;
                    case ObjectTypeEnum.WorkInstructionTemplate:
                        idFieldName = Models.Enumerations.TableFields.workinstruction_template_id.ToString();
                        break;
                    case ObjectTypeEnum.ChecklistTemplate:
                        idFieldName = Models.Enumerations.TableFields.checklisttemplate_id.ToString();
                        break;
                    case ObjectTypeEnum.AssessmentTemplate:
                        idFieldName = Models.Enumerations.TableFields.assessment_template_id.ToString();
                        break;
                    case ObjectTypeEnum.AuditTemplate:
                        idFieldName = Models.Enumerations.TableFields.audittemplate_id.ToString();
                        break;
                    case ObjectTypeEnum.TaskTemplate:
                        idFieldName = Models.Enumerations.TableFields.tasktemplate_id.ToString();
                        break;
                    case ObjectTypeEnum.Checklist:
                        idFieldName = Models.Enumerations.TableFields.checklist_id.ToString();
                        break;
                    case ObjectTypeEnum.Task:
                        idFieldName = Models.Enumerations.TableFields.task_id.ToString();
                        break;
                    case ObjectTypeEnum.Audit:
                        idFieldName = Models.Enumerations.TableFields.audit_id.ToString();
                        break;
                    case ObjectTypeEnum.Assessment:
                        idFieldName = Models.Enumerations.TableFields.assessment_id.ToString();
                        break;
                    case ObjectTypeEnum.ChecklistTemplateStage:
                        idFieldName = Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString();
                        break;
                    case ObjectTypeEnum.ChecklistStage:
                        idFieldName = Models.Enumerations.TableFields.checklist_stage_id.ToString();
                        break;
                }
                parameters.Add(new NpgsqlParameter("@_foreignkeyfieldname", idFieldName));
                parameters.Add(new NpgsqlParameter("@_id", id));

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tags_by_object_id_alt", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind);
                while (await dr.ReadAsync())
                {
                    Tag tag = CreateOrFillTagFromReader(dr);

                    tagsFromObject.Add(tag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagsWithObjectAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return tagsFromObject.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Update the tags that are attached to an object.
        /// </summary>
        /// <param name="objectType">The object type of the object the tags are attached to</param>
        /// <param name="id">The id of the object the tags are attached to</param>
        /// <param name="tags">The list of tags that should be attached to the object</param>
        /// <param name="companyId">company id</param>
        /// <param name="userId">user id</param>
        /// <returns>returns true after completing</returns>
        public async Task<bool> UpdateTagsOnObjectAsync(ObjectTypeEnum objectType, int id, List<Tag> tags, int companyId, int userId)
        {
            string original = "";
            string idFieldName = ""; //field name of the fk in db, for data auditing purposes

            List<Tag> currentTags = new();

            switch (objectType)
            {
                case ObjectTypeEnum.Action:
                    idFieldName = Models.Enumerations.TableFields.action_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Action, id: id);
                    break;
                case ObjectTypeEnum.Comment:
                    idFieldName = Models.Enumerations.TableFields.comment_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Comment, id: id);
                    break;
                case ObjectTypeEnum.WorkInstructionTemplate:
                    idFieldName = Models.Enumerations.TableFields.workinstruction_template_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.WorkInstructionTemplate, id: id);
                    break;
                case ObjectTypeEnum.ChecklistTemplate:
                    idFieldName = Models.Enumerations.TableFields.checklisttemplate_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistTemplate, id: id);
                    break;
                case ObjectTypeEnum.AssessmentTemplate:
                    idFieldName = Models.Enumerations.TableFields.assessment_template_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.AssessmentTemplate, id: id);
                    break;
                case ObjectTypeEnum.AuditTemplate:
                    idFieldName = Models.Enumerations.TableFields.audittemplate_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.AuditTemplate, id: id);
                    break;
                case ObjectTypeEnum.TaskTemplate:
                    idFieldName = Models.Enumerations.TableFields.tasktemplate_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.TaskTemplate, id: id);
                    break;
                case ObjectTypeEnum.Checklist:
                    idFieldName = Models.Enumerations.TableFields.checklist_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Checklist, id: id);
                    break;
                case ObjectTypeEnum.Task:
                    idFieldName = Models.Enumerations.TableFields.task_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Task, id: id);
                    break;
                case ObjectTypeEnum.Audit:
                    idFieldName = Models.Enumerations.TableFields.audit_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Audit, id: id);
                    break;
                case ObjectTypeEnum.Assessment:
                    idFieldName = Models.Enumerations.TableFields.assessment_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Assessment, id: id);
                    break;
                case ObjectTypeEnum.ChecklistTemplateStage:
                    idFieldName = Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistTemplateStage, id: id);
                    break;
                case ObjectTypeEnum.ChecklistStage:
                    idFieldName = Models.Enumerations.TableFields.checklist_stage_id.ToString();
                    currentTags = await GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistStage, id: id);
                    break;
            }

            original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_relation.ToString(), idFieldName, id);

            var tagsToBeRemoved = currentTags.Where(x => !tags.Select(y => y.Id).ToList().Contains(x.Id)).ToList();
            //Remove tags, that are not in the new list of tags anymore.
            if (tagsToBeRemoved != null && tagsToBeRemoved.Count > 0)
            {
                foreach (var tag in tagsToBeRemoved)
                {
                    await this.RemoveTagFromObjectAsync(companyId: companyId, tagId: tag.Id, objectType: objectType, id: id);
                }
            }

            //Add tags (not if relation in db already exists it will be ignored on execution within the SP)
            if (tags.Count > 0)
            {
                foreach (var tag in tags)
                {
                    await this.AddTagToObjectAsync(companyId: companyId, tagId: tag.Id, objectType: objectType, id: id);
                }
            }

            var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_relation.ToString(), idFieldName, id);
            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tags_tag_relation.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Changed object tag relation collection.");

            return true;
        }

        /// <summary>
        /// Gets a list of relations between objects of a given type and tags
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="objectType">type of objects for wich to get the tag relations</param>
        /// <returns>a list of tag relations</returns>
        public async Task<List<TagRelation>> GetTagRelationsByObjectTypeAsync(int companyId, ObjectTypeEnum objectType, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<TagRelation>();
            string fkFieldName = "";

            if (objectType == ObjectTypeEnum.Action)
            {
                fkFieldName = Models.Enumerations.TableFields.action_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.Comment)
            {
                fkFieldName = Models.Enumerations.TableFields.comment_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.WorkInstructionTemplate)
            {
                fkFieldName = Models.Enumerations.TableFields.workinstruction_template_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.ChecklistTemplate)
            {
                fkFieldName = Models.Enumerations.TableFields.checklisttemplate_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.AssessmentTemplate)
            {
                fkFieldName = Models.Enumerations.TableFields.assessment_template_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.AuditTemplate)
            {
                fkFieldName = Models.Enumerations.TableFields.audittemplate_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.TaskTemplate)
            {
                fkFieldName = Models.Enumerations.TableFields.tasktemplate_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.Checklist)
            {
                fkFieldName = Models.Enumerations.TableFields.checklist_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.Task)
            {
                fkFieldName = Models.Enumerations.TableFields.task_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.Audit)
            {
                fkFieldName = Models.Enumerations.TableFields.audit_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.Assessment)
            {
                fkFieldName = Models.Enumerations.TableFields.assessment_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.ChecklistTemplateStage)
            {
                fkFieldName = Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString();
            }
            else if (objectType == ObjectTypeEnum.ChecklistStage)
            {
                fkFieldName = Models.Enumerations.TableFields.checklist_stage_id.ToString();
            }

            if (!string.IsNullOrEmpty(fkFieldName))
            {
                try
                {
                    List<NpgsqlParameter> parameters = new()
                    {
                        new NpgsqlParameter("@_companyid", companyId),
                        new NpgsqlParameter("@_foreignkeyfieldname", fkFieldName)
                    };

                    using (NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tags_on_object_type", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                    {
                        while (await dr.ReadAsync())
                        {
                            var tag = CreateOrFillTagRelationFromReader(dr, companyId);
                            if (tag != null && tag.Id > 0)
                            {
                                output.Add(tag);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagRelationsByObjectTypeAsync(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

                }
            }

            return output.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Gets the tag relations for assessment template.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="assassmentTemplateId">assessment template id. If null, tag relations for all assessment templates will be returned</param>
        /// <returns>list of tag relations for assessment templates</returns>
        public async Task<List<TagRelation>> GetTagsOnAssessmentTemplateSkillInstructionsAsync(int companyId, int? assassmentTemplateId = null)
        {
            var output = new List<TagRelation>();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
            };

            if (assassmentTemplateId != null)
            {
                parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", assassmentTemplateId));
            }

            try
            {
                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tags_on_assessmenttemplate_skillinstructions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    var tag = CreateOrFillTagRelationFromReader(dr, companyId);

                    if (tag != null && tag.Id > 0)
                    {
                        output.Add(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagsOnAssessmentTemplateSkillInstructionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return output;
        }

        /// <summary>
        /// Returns a list of tag relations for one or all assessment skill instructions
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="assassmentId">assessment id. If null, will return tag relations for all assessment instructions</param>
        /// <returns>list of tag relations for one or all assessment instructions</returns>
        public async Task<List<TagRelation>> GetTagsOnAssessmentSkillInstructions(int companyId, int? assassmentId = null)
        {
            var output = new List<TagRelation>();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
            };

            if (assassmentId != null)
            {
                parameters.Add(new NpgsqlParameter("@_assessmentid", assassmentId));
            }

            try
            {
                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tags_on_assessment_skillinstructions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    var tag = CreateOrFillTagRelationFromReader(dr, companyId);

                    if (tag != null && tag.Id > 0)
                    {
                        output.Add(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagsOnAssessmentTemplateSkillInstructions(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return output;
        }

        /// <summary>
        /// Get the number of active tags a company has
        /// </summary>
        /// <param name="companyId">company id of the company to count tags for</param>
        /// <returns>Number of active tags for the given company</returns>
        public async Task<int> GetTagsCountCompany(int companyId)
        {
            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId)
            };
            var count = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("get_tags_count_company", parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));
            return count;
        }

        /// <summary>
        /// Get the number of active holding tags that a company has
        /// </summary>
        /// <param name="companyId">company id of the company to count holding tags for</param>
        /// <returns>Number of active holding tags for the given company</returns>
        public async Task<int> GetTagsCountHolding(int companyId)
        {
            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId)
            };
            var count = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("get_tags_count_holding", parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));
            return count;
        }
        #endregion

        #region - private methods tags -
        private Dictionary<string, string> GetSystemTagTranslations()
        {  
            return _systemTagTranslations ??= GetTagGroupTranslationsAsync().Result;
        }

        /// <summary>
        /// RemoveTagsFromObjectAsync; Remove a tag from an object.
        /// Following stored procedures will be used: "remove_tag_from_object"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="tagId">TagId (DB: companies_area.id)</param>
        /// <param name="objectType">The type of object the tag will be removed from</param>
        /// <param name="id">id of the object</param>
        /// <returns>The count of the deleted record.</returns>
        private async Task<int> RemoveTagFromObjectAsync(int companyId, int tagId, ObjectTypeEnum objectType, int id)
        {

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_tagid", tagId),
                new NpgsqlParameter("@_companyid", companyId)
            };
            if (objectType == ObjectTypeEnum.Action) parameters.Add(new NpgsqlParameter("@_actionid", id));
            else if (objectType == ObjectTypeEnum.Comment) parameters.Add(new NpgsqlParameter("@_commentid", id));
            else if (objectType == ObjectTypeEnum.WorkInstructionTemplate) parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", id));
            else if (objectType == ObjectTypeEnum.ChecklistTemplate) parameters.Add(new NpgsqlParameter("@_checklisttemplateid", id));
            else if (objectType == ObjectTypeEnum.AssessmentTemplate) parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", id));
            else if (objectType == ObjectTypeEnum.AuditTemplate) parameters.Add(new NpgsqlParameter("@_audittemplateid", id));
            else if (objectType == ObjectTypeEnum.TaskTemplate) parameters.Add(new NpgsqlParameter("@_tasktemplateid", id));
            //TODO
            //else if (objectType == ObjectTypeEnum.WorkInstructionTemplateItem) parameters.Add(new NpgsqlParameter("@_workinstructiontemplateitemid", id));
            else if (objectType == ObjectTypeEnum.ChecklistTemplateStage) parameters.Add(new NpgsqlParameter("@_checklisttemplatestageid", id));

            var count = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("remove_tag_from_object", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return count;
        }

        /// <summary>
        /// AddTagToObjectAsync; Add tag to an object. If the tag is already added to the object, the insert will be ignored. The id will still be returned.
        /// Following stored procedures will be used: "add_tag_relation_v2"
        /// </summary>
        /// <param name="companyId">ActionId (DB: actions_action.id)</param>
        /// <param name="tagId">TagId (DB: profile_user.id)</param>
        /// <param name="objectType">The type of object the tag will be added to</param>
        /// <param name="id">id of the object</param>
        /// <returns>The Id of the inserted record.</returns>
        private async Task<int> AddTagToObjectAsync(int companyId, int tagId, ObjectTypeEnum objectType, int id)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_tagid", tagId)
            };
            if (objectType == ObjectTypeEnum.Action) parameters.Add(new NpgsqlParameter("@_actionid", id));
            else if (objectType == ObjectTypeEnum.Comment) parameters.Add(new NpgsqlParameter("@_commentid", id));
            else if (objectType == ObjectTypeEnum.WorkInstructionTemplate) parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", id));
            else if (objectType == ObjectTypeEnum.ChecklistTemplate) parameters.Add(new NpgsqlParameter("@_checklisttemplateid", id));
            else if (objectType == ObjectTypeEnum.AssessmentTemplate) parameters.Add(new NpgsqlParameter("@_assessmenttemplateid", id));
            else if (objectType == ObjectTypeEnum.AuditTemplate) parameters.Add(new NpgsqlParameter("@_audittemplateid", id));
            else if (objectType == ObjectTypeEnum.TaskTemplate) parameters.Add(new NpgsqlParameter("@_tasktemplateid", id));
            else if (objectType == ObjectTypeEnum.Assessment) parameters.Add(new NpgsqlParameter("@_assessmentid", id));
            else if (objectType == ObjectTypeEnum.Audit) parameters.Add(new NpgsqlParameter("@_auditid", id));
            else if (objectType == ObjectTypeEnum.Checklist) parameters.Add(new NpgsqlParameter("@_checklistid", id));
            else if (objectType == ObjectTypeEnum.Task) parameters.Add(new NpgsqlParameter("@_taskid", id));
            //TODO
            //else if (objectType == ObjectTypeEnum.WorkInstructionTemplateItem) parameters.Add(new NpgsqlParameter("@_workinstructiontemplateitemid", id));
            else if (objectType == ObjectTypeEnum.ChecklistTemplateStage) parameters.Add(new NpgsqlParameter("@_checklisttemplatestageid", id));
            else if (objectType == ObjectTypeEnum.ChecklistStage) parameters.Add(new NpgsqlParameter("@_checkliststageid", id));

            var relationId = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_tag_relation_v2", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return relationId;
        }

        /// <summary>
        /// Adds or changes the configuration of a tag.
        /// Configuration can include icon, color and/or where the tag can be used.
        /// For a holding tag use AddChangeHoldingTagConfiguration
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="tag">tag object</param>
        /// <param name="userId">user id</param>
        /// <returns>configuration id</returns>
        private async Task<int> AddChangeTagConfiguration(int companyId, Tag tag, int userId)
        {
            string original = string.Empty;
            string databaseFunction = "add_tag_configuration";
            string existingTagConfiguration = await GetTagConfiguration(companyId, tag.Id);

            if (!string.IsNullOrEmpty(existingTagConfiguration))
            {
                original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_configuration.ToString(), Models.Enumerations.TableFields.tag_id.ToString(), tag.Id, Models.Enumerations.TableFields.company_id.ToString(), companyId);
                databaseFunction = "change_tag_configuration";
            }

            TagConfiguration tagConfiguration = GetTagConfigurationFromTag(tag);

            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_tagid", tag.Id),
                new NpgsqlParameter("@_configuration", tagConfiguration.ToJsonFromObject())
            };

            var id = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync(databaseFunction, parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));

            if (id > 0)
            {
                var mutatedConfiguration = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_configuration.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutatedConfiguration, Models.Enumerations.TableNames.tags_tag_configuration.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Added or changed tag configuration relation.");
            }

            return id;
        }

        /// <summary>
        /// Adds or changes the configuration of a holding tag.
        /// Configuration can include icon, color and/or where the tag can be used.
        /// For a company tag, use AddChangeTagConfiguration.
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <param name="tag">tag id</param>
        /// <param name="userId">user id</param>
        /// <param name="companyId">company id</param>
        /// <returns>rowcount</returns>
        private async Task<int> AddChangeHoldingTagConfiguration(int holdingId, Tag tag, int userId, int companyId)
        {
            string original = string.Empty;
            original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_configuration.ToString(), Models.Enumerations.TableFields.tag_id.ToString(), tag.Id, Models.Enumerations.TableFields.company_id.ToString(), companyId);

            TagConfiguration tagConfiguration = GetTagConfigurationFromTag(tag);

            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_holdingid", holdingId),
                new NpgsqlParameter("@_tagid", tag.Id),
                new NpgsqlParameter("@_configuration", tagConfiguration.ToJsonFromObject())
            };

            var rowcount = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("set_holding_tag_configuration_for_companies", parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowcount > 0)
            {
                var mutatedConfiguration = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_configuration.ToString(), Models.Enumerations.TableFields.tag_id.ToString(), tag.Id, Models.Enumerations.TableFields.company_id.ToString(), companyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutatedConfiguration, Models.Enumerations.TableNames.tags_tag_configuration.ToString(), objectId: tag.Id, userId: userId, companyId: companyId, description: "Added or changed tag configuration relation for holding tag.");
            }

            return rowcount;
        }

        /// <summary>
        /// Gets the configuration for a tag in json
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="tagId">tag id</param>
        /// <returns>tag configuration in form of a json string</returns>
        private async Task<string> GetTagConfiguration(int companyId, int tagId)
        {
            string tagConfigurationJson = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_tagid", tagId)
                };

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_tag_configuration", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    if (dr.HasColumn("configuration") && dr["configuration"] != DBNull.Value)
                        tagConfigurationJson = dr["configuration"].ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TagManager.GetTagConfiguration(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return tagConfigurationJson;
        }

        private TagConfiguration GetTagConfigurationFromTag(Tag tag)
        {
            return new()
            {
                ColorCode = tag.ColorCode,
                IconName = tag.IconName,
                IconStyle = tag.IconStyle,
                AllowedOnObjectTypes = tag.AllowedOnObjectTypes,
                UseTranslation = tag.UseTranslation
            };
        }

        /// <summary>
        /// Establish a relation between a tag and a company.
        /// A company only has access to tags that have a relation with the company.
        /// A tag can have multiple related companies.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="userId">user id</param>
        /// <param name="tagId">tag id</param>
        /// <returns>relation id</returns>
        private async Task<int> AddTagCompanyRelation(int companyId, int userId, int tagId)
        {
            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_tagid", tagId)
            };
            var id = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_tag_company_relation", parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));

            if (id > 0)
            {
                var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.tags_tag_company.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.tags_tag_company.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Added tag company relation.");
            }

            return id;
        }

        /// <summary>
        /// Adds a relation between a tag and a holding.
        /// Holding tags require this relation.
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <param name="tagId">tag id</param>
        /// <returns>relation id</returns>
        private async Task<int> AddTagHoldingRelation(int holdingId, int tagId)
        {
            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_holdingid", holdingId),
                new NpgsqlParameter("@_tagid", tagId)
            };
            var id = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_tag_holding_relation", parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));
            return id;
        }

        /// <summary>
        /// Remove a holding tag relation.
        /// Used when a tag is edited to no longer be a holding tag
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <param name="tagId">tag id</param>
        /// <returns>row count of deleted records</returns>
        private async Task<int> RemoveTagHoldingRelation(int holdingId, int tagId)
        {
            List<NpgsqlParameter> relationParameters = new()
            {
                new NpgsqlParameter("@_holdingid", holdingId),
                new NpgsqlParameter("@_tagid", tagId)
            };
            var rowcount = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("remove_tag_holding_relation", parameters: relationParameters, commandType: System.Data.CommandType.StoredProcedure));
            return rowcount;
        }

        /// <summary>
        /// Adds a relation between the tag and all companies within a holding.
        /// For a relation between a tag and a single company use AddTagCompanyRelation.
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <param name="tagId">tag id</param>
        /// <returns>row count</returns>
        private async Task<int> AddTagToAllCompaniesInHolding(int holdingId, int tagId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_holdingid", holdingId),
                new NpgsqlParameter("@_tagid", tagId)
            };
            var rowCount = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_tag_to_all_companies_in_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return rowCount;
        }

        /// <summary>
        /// Remove the relations between the tag and all companies in a holding except for one.
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <param name="tagId">tag id</param>
        /// <param name="companyId">company id of company that keeps the relation</param>
        /// <returns>row count</returns>
        private async Task<int> RemoveTagFromOtherCompaniesInHolding(int holdingId, int tagId, int companyId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_holdingid", holdingId),
                new NpgsqlParameter("@_tagid", tagId),
                new NpgsqlParameter("@_companyid", companyId)
            };
            var rowCount = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("delete_tag_from_all_other_companies_in_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return rowCount;
        }

        /// <summary>
        /// Add a relation between tag group and holding to indicate that a tag group is selected for that holding
        /// </summary>
        /// <param name="taggroupId">taggroup id</param>
        /// <param name="holdingId">holding id</param>
        /// <returns>id of the relation</returns>
        private async Task<int> AddTagGroupHoldingRelationAsync(int taggroupId, int holdingId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_taggroupid", taggroupId),
                new NpgsqlParameter("@_holdingid", holdingId)
            };
            var id = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_taggroup_holding_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return id;
        }

        /// <summary>
        /// Add a relation between a tag group and a company to indicate that the tag group is selected for that company
        /// </summary>
        /// <param name="taggroupId">taggroup id</param>
        /// <param name="companyId">company id</param>
        /// <returns></returns>
        private async Task<int> AddTagGroupCompanyRelationAsync(int taggroupId, int companyId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_taggroupid", taggroupId),
                new NpgsqlParameter("@_companyid", companyId)
            };
            var id = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_taggroup_company_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return id;
        }

        /// <summary>
        /// Removes the relation between a taggroup and a holding to indicate that the taggroup is not selected anymore
        /// </summary>
        /// <param name="taggroupId">taggroup id</param>
        /// <param name="holdingId">holding id</param>
        /// <returns>rowcount of deleted records</returns>
        private async Task<int> RemoveTagGroupHoldingRelationAsync(int taggroupId, int holdingId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_taggroupid", taggroupId),
                new NpgsqlParameter("@_holdingid", holdingId)
            };
            var rowcount = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("remove_taggroup_holding_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return rowcount;
        }

        /// <summary>
        /// Remove relation between tag group and company to indicate the tag group is not selected by the company
        /// </summary>
        /// <param name="taggroupId">taggroup id</param>
        /// <param name="companyId">company id</param>
        /// <returns>rowcount of deleted records</returns>
        private async Task<int> RemoveTagGroupCompanyRelationAsync(int taggroupId, int companyId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_taggroupid", taggroupId),
                new NpgsqlParameter("@_companyid", companyId)
            };
            var rowcount = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("remove_taggroup_company_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return rowcount;
        }

        /// <summary>
        /// Creates a tag relation object from a NpgsqlDataReader
        /// </summary>
        /// <param name="dr">NpgsqlDataReader</param>
        /// <param name="tagRelation">Optional existing TagRelation object, use if one needs to be updated. Default null.</param>
        /// <returns>TagRelation object</returns>
        private TagRelation CreateOrFillTagRelationFromReader(NpgsqlDataReader dr, int companyId, TagRelation tagRelation = null)
        {
            tagRelation ??= new TagRelation();

            tagRelation.ObjectId = Convert.ToInt32(dr["object_id"]);
            CreateOrFillTagFromReader(dr, tagRelation);

            return tagRelation;
        }

        /// <summary>
        /// Creates a tag object from a NpgsqlDataReader
        /// </summary>
        /// <param name="dr">NpgsqlDataReader</param>
        /// <param name="tag">Optional tag object, use to update existsing tag object. Default null.</param>
        /// <returns>Tag object</returns>
        private Tag CreateOrFillTagFromReader(NpgsqlDataReader dr, Tag tag = null, Features features = null)
        {
            tag ??= new Tag();
            tag.Id = Convert.ToInt32(dr["id"]);
            tag.Guid = dr["guid"].ToString();
            tag.Name = dr["name"].ToString();

            if (dr.HasColumn("group_guid") && dr["group_guid"] != DBNull.Value)
                tag.GroupName = dr["group_name"].ToString();

            if (dr.HasColumn("group_guid") && dr["group_guid"] != DBNull.Value)
                tag.GroupGuid = dr["group_guid"].ToString();

            if (dr.HasColumn("created_at") && dr["created_at"] != DBNull.Value)
                tag.CreatedAt = Convert.ToDateTime(dr["created_at"]);

            if (dr.HasColumn("modified_at") && dr["modified_at"] != DBNull.Value)
                tag.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);

            if (dr.HasColumn("created_by_id") && dr["created_by_id"] != DBNull.Value)
                tag.CreatedBy = dr["created_by_id"].ToString();

            if (dr.HasColumn("modified_by_id") && dr["modified_by_id"] != DBNull.Value)
                tag.ModifiedBy = dr["modified_by_id"].ToString();

            if (dr.HasColumn("is_searchable"))
                tag.IsSeachable = Convert.ToBoolean(dr["is_searchable"]);

            if (dr.HasColumn("is_system_tag"))
                tag.IsSystemTag = Convert.ToBoolean(dr["is_system_tag"]);

            if (dr.HasColumn("is_holding_tag"))
                tag.IsHoldingTag = Convert.ToBoolean(dr["is_holding_tag"]);

            if (dr.HasColumn("is_used_in_checklist_template"))
            {
                if (Convert.ToBoolean(dr["is_used_in_checklist_template"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.Checklist);
                }
            }

            if (dr.HasColumn("is_used_in_workinstruction_template") && features?.WorkInstructions == true)
            {
                if (Convert.ToBoolean(dr["is_used_in_workinstruction_template"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.WorkInstruction);
                }
            }

            if (dr.HasColumn("is_used_in_assessment_template") && features?.SkillAssessments == true)
            {
                if (Convert.ToBoolean(dr["is_used_in_assessment_template"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.Assessment);
                }
            }

            if (dr.HasColumn("is_used_in_audit_template"))
            {
                if (Convert.ToBoolean(dr["is_used_in_audit_template"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.Audit);
                }
            }

            if (dr.HasColumn("is_used_in_task_template"))
            {
                if (Convert.ToBoolean(dr["is_used_in_task_template"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.Task);
                }
            }

            if (dr.HasColumn("is_used_in_action"))
            {
                if (Convert.ToBoolean(dr["is_used_in_action"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.Action);
                }
            }

            if (dr.HasColumn("is_used_in_comment"))
            {
                if (Convert.ToBoolean(dr["is_used_in_comment"]))
                {
                    tag.UsedInTemplateTypes ??= new();
                    tag.UsedInTemplateTypes.Add(TagableObjectEnum.Comment);
                }
            }

            //if tag is in use on any of these types, the tag will be considered as 'in use'
            List<TagableObjectEnum> taggableObjectsToConsiderTagInUse = new() {
                TagableObjectEnum.Assessment,
                TagableObjectEnum.Audit,
                TagableObjectEnum.Checklist,
                TagableObjectEnum.Task,
                TagableObjectEnum.WorkInstruction
            };

            tag.IsInUse = (tag.UsedInTemplateTypes != null && tag.UsedInTemplateTypes.Any(taggableObjectsToConsiderTagInUse.Contains));

            if (dr.HasColumn("configuration") && dr["configuration"] != DBNull.Value)
            {
                TagConfiguration tagConfiguration = dr["configuration"].ToString().ToObjectFromJson<TagConfiguration>();
                tag.IconName = tagConfiguration.IconName;
                tag.IconStyle = tagConfiguration.IconStyle;
                tag.ColorCode = tagConfiguration.ColorCode;
                tag.AllowedOnObjectTypes = tagConfiguration.AllowedOnObjectTypes;
                tag.UseTranslation = tagConfiguration.UseTranslation;
            }

            if (tag.IsSystemTag == true)
                tag.UseTranslation ??= true; // default to true

            if (!string.IsNullOrEmpty(tag.GroupGuid) && (string.IsNullOrEmpty(tag.IconName) || string.IsNullOrEmpty(tag.ColorCode) || string.IsNullOrEmpty(tag.IconStyle)))
            {
                if (_defaultTagConfigs.ContainsKey(tag.GroupGuid))
                {
                    TagConfiguration tc = _defaultTagConfigs[tag.GroupGuid];
                    if (string.IsNullOrEmpty(tag.IconName))
                        tag.IconName = tc.IconName;
                    if (string.IsNullOrEmpty(tag.IconStyle))
                        tag.IconStyle = tc.IconStyle;
                    if (string.IsNullOrEmpty(tag.ColorCode))
                        tag.ColorCode = tc.ColorCode;
                }
            }
            if (tag.AllowedOnObjectTypes == null)
            {
                tag.AllowedOnObjectTypes = new();
                foreach (TagableObjectEnum item in Enum.GetValues(typeof(TagableObjectEnum)))
                {
                    tag.AllowedOnObjectTypes.Add(item);
                }
            }

            if (tag.UseTranslation == true && tag.IsSystemTag == true && _systemTagTranslations != null && _systemTagTranslations.Count > 0 && _configurationHelper.GetValueAsBool("AppSettings:EnableTagTranslations"))
            {
                string translatedTagName = _systemTagTranslations[_tagLanguageKeys[tag.GroupGuid]];
                tag.Name = string.IsNullOrEmpty(translatedTagName) ? tag.Name : translatedTagName;
            }

            return tag;
        }

        /// <summary>
        /// Creates a TagGroup object from a NpgsqlDataReader
        /// </summary>
        /// <param name="dr">NpgsqlDataReader</param>
        /// <param name="tagGroup">Optional TagGroup, provide existing TagGroup object to update</param>
        /// <returns></returns>
        private TagGroup CreateOrFillTagGroupFromReader(NpgsqlDataReader dr, TagGroup tagGroup = null)
        {
            tagGroup ??= new TagGroup();

            tagGroup.Id = Convert.ToInt32(dr["id"]);
            tagGroup.Guid = dr["guid"].ToString();
            tagGroup.Name = dr["name"].ToString();
            if (dr.HasColumn("is_selected") && dr["is_selected"] != DBNull.Value)
                tagGroup.IsSelected = Convert.ToBoolean(dr["is_selected"]);

            return tagGroup;
        }

        /// <summary>
        /// GetNpgsqlParametersFromTag; Creates a list of NpgsqlParameters, and fills it based on the supplied Tag object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="tag">The supplied Tag object, containing all data.</param>
        /// <param name="tagId">TagId (DB: tags_tag.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromTag(Tag tag, int tagId = 0)
        {
            List<NpgsqlParameter> parameters = new();
            string guid;

            if (tagId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_id", tagId));
                guid = tag.Guid;
            }
            else
            {
                guid = Guid.NewGuid().ToString("N");
            }

            parameters.Add(new NpgsqlParameter("@_name", tag.Name));
            parameters.Add(new NpgsqlParameter("@_guid", guid));
            parameters.Add(new NpgsqlParameter("@_issearchable", tag.IsSeachable));
            parameters.Add(new NpgsqlParameter("@_isholdingtag", tag.IsHoldingTag));

            return parameters;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_companyManager.GetPossibleExceptions());
                listEx.AddRange(_toolsManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion
    }
}