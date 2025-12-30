using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class WorkInstructionManager : BaseManager<WorkInstructionManager>, IWorkInstructionManager
    {
        #region - properties -
        private string culture;
        public string Culture
        {
            get { return culture; }
            set { culture = _tagManager.Culture = value; }
        }
        #endregion

        #region - private(s) -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IAreaManager _areaManager;
        private readonly ITagManager _tagManager;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor(s) -
        public WorkInstructionManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, IAreaManager areaManager, ITagManager tagManager, IDataAuditing dataAuditing, ILogger<WorkInstructionManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _areaManager = areaManager;
            _tagManager = tagManager;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods WorkInstructions
        public Task<List<WorkInstruction>> GetWorkInstructionsAsync(int companyId, int? userId = null, WorkInstructionFilters? filters = null, string include = null, bool useStatic = false)
        {
            throw new NotImplementedException();
        }

        public Task<WorkInstruction> GetWorkInstructionAsync(int companyId, int workInstructionId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddWorkInstructionAsync(int companyId, int userId, WorkInstruction workInstruction)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeWorkInstructionAsync(int companyId, int userId, int workInstructionId, WorkInstruction workInstruction)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetWorkInstructionActiveAsync(int companyId, int userId, int workInstructionId, bool isActive = true)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetWorkInstructionCompletedAsync(int companyId, int userId, int workInstructionId, bool isCompleted = true)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region - public methods WorkInstructionTemplates
        public async Task<List<WorkInstructionTemplate>> GetWorkInstructionTemplatesAsync(int companyId, int? userId = null, WorkInstructionFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<WorkInstructionTemplate>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (userId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                }
                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_role", filters.Value.RoleType.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_allowed_only_user_id", userId.Value));
                    }

                    if (filters.Value.InstructionType.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_instructiontype", Convert.ToInt32(filters.Value.InstructionType.Value)));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (filters.Value.IncludeAvailableForAllAreas.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_include_workinstructiontemplates_available_for_all_areas", filters.Value.IncludeAvailableForAllAreas.Value));
                    }

                    if(!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }
                }

                using (dr = await _manager.GetDataReader("get_workinstructiontemplates", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var workInstructionTemplate = CreateOrFillWorkInstructionTemplateFromReader(dr);
                        output.Add(workInstructionTemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.GetWorkInstructionTemplatesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) output = await AppendTagsToWorkInstructionTemplatesAsync(companyId: companyId, workInstructionTemplates: output, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Items.ToString().ToLower())) output = await AppendInstructionTemplatesItemsAsync(companyId: companyId, workInstructionTemplates: output, filters: filters, userId: userId, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToWorkInstructionTemplatesAsync(companyId: companyId, workInstructionTemplates: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
            }

            foreach (var template in output)
            {
                if (template.InstructionItems != null && template.InstructionItems.Count > 1)
                    template.InstructionItems = template.InstructionItems.OrderBy(i => i.Index).ToList();
            }

            return output;
        }

        public async Task<Dictionary<int, string>> GetWorkInstructionsTemplateNames(int companyId, List<int> workinstructionIds)
        {
            Dictionary<int, string> idsNames = new();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_workinstructionids", workinstructionIds)
                };

                using NpgsqlDataReader dr = await _manager.GetDataReader("get_workinstructiontemplate_names", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    int id = Convert.ToInt32(dr["id"]);
                    string name = dr["name"].ToString();
                    idsNames.Add(id, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.GetWorkInstructionsTemplateNames(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return idsNames;
        }

        public async Task<WorkInstructionTemplate> GetWorkInstructionTemplateAsync(int companyId, int workInstructionTemplateId, int userId = 0, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var workInstructionTemplate = new WorkInstructionTemplate();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (userId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId));
                }

                parameters.Add(new NpgsqlParameter("@_id", workInstructionTemplateId));

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillWorkInstructionTemplateFromReader(dr, workInstructionTemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.GetWorkInstructionTemplateAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (workInstructionTemplate.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Items.ToString().ToLower())) workInstructionTemplate = await AppendInstructionTemplateItemsAsync(companyId: companyId, workInstructionTemplate: workInstructionTemplate, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) workInstructionTemplate = await AppendAreaPathsToWorkInstructionTemplateAsync(companyId: companyId, workInstructionTemplate: workInstructionTemplate, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Parents.ToString().ToLower())) workInstructionTemplate = await AppendWorkInstructionParentsAsync(companyId: companyId, workInstructionTemplate: workInstructionTemplate, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) workInstructionTemplate.Tags = await GetTagsWithWorkInstructionTemplateAsync(companyId: companyId, workInstructionTemplateId: workInstructionTemplate.Id);

                if (workInstructionTemplate.InstructionItems != null && workInstructionTemplate.InstructionItems.Count > 1)
                    workInstructionTemplate.InstructionItems = workInstructionTemplate.InstructionItems.OrderBy(i => i.Index).ToList();

                return workInstructionTemplate;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> AddWorkInstructionTemplateAsync(int companyId, int userId, WorkInstructionTemplate workInstructionTemplate)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            //use temporary list 
            var tempInstructionItems = workInstructionTemplate?.InstructionItems.ToList() ?? new List<InstructionItemTemplate>();
            foreach (var tempInstructionItem in tempInstructionItems)
            {
                var index = tempInstructionItems.IndexOf(tempInstructionItem);
                workInstructionTemplate.InstructionItems[index].Index = index + 1;
            }

            parameters.AddRange(GetNpgsqlParametersFromWorkInstructionTemplate(workInstructionTemplate: workInstructionTemplate, companyId: companyId, workInstructionTemplateId: workInstructionTemplate.Id, userId: userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_workinstructiontemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                if (workInstructionTemplate.Tags != null && workInstructionTemplate.Tags.Count > 0)
                {
                    await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.WorkInstructionTemplate, possibleId, workInstructionTemplate.Tags, companyId, userId);
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_templates.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.workinstruction_templates.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added workinstructiontemplate.");
            }

            if (workInstructionTemplate.InstructionItems == null)
                return possibleId;


            foreach (var item in workInstructionTemplate?.InstructionItems)
            {
                parameters = GetNpgsqlParametersFromInstructionItemTemplate(item, companyId, workInstructionTemplateId: possibleId, userId: userId, update: false);
                try
                {
                    var possibleItemId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_workinstructiontemplate_item", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                    if (possibleItemId > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_template_items.ToString(), possibleItemId);
                        await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.workinstruction_template_items.ToString(), objectId: possibleItemId, userId: userId, companyId: companyId, description: "Added workinstructiontemplate item.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.AddWorkInstructionTemplateAsync(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
            }
            return possibleId;
        }

        public async Task<int> AddWorkInstructionTemplateChangesNotification(int companyId, int userId, WorkInstructionTemplate oldTemplate, WorkInstructionTemplate newTemplate, string notificationComment = null)
        {
            if (oldTemplate == null)
            {
                oldTemplate = new WorkInstructionTemplate();
            }

            if (newTemplate == null)
            {
                newTemplate = new WorkInstructionTemplate();
            }

            var workInstructionChanges = new List<WorkInstructionTemplateChange>();
            //gather changes with oldvalue not null
            if (oldTemplate.Name != newTemplate.Name)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "Name",
                    OldValue = oldTemplate.Name,
                    NewValue = newTemplate.Name,
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_NAME"
                });
            }

            if (oldTemplate.Description != newTemplate.Description)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "Description",
                    OldValue = oldTemplate.Description,
                    NewValue = newTemplate.Description,
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_DESCRIPTION"
                });
            }

            if (oldTemplate.AreaId != newTemplate.AreaId)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "AreaId",
                    OldValue = oldTemplate.AreaId.ToString(),
                    NewValue = newTemplate.AreaId.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_AREA_ID"
                });
            }

            if (oldTemplate.Media != null || newTemplate.Media != null)
            {
                if (oldTemplate.Media == null)
                    oldTemplate.Media = new List<string>();
                if (newTemplate.Media == null)
                    newTemplate.Media = new List<string>();

                if (oldTemplate?.Media?.ToJsonFromObject() != newTemplate?.Media?.ToJsonFromObject())
                {
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    var oldMedia = JsonSerializer.Serialize(oldTemplate.Media, options);
                    var newMedia = JsonSerializer.Serialize(newTemplate.Media, options);

                    workInstructionChanges.Add(new WorkInstructionTemplateChange()
                    {
                        PropertyName = "Media",
                        OldValue = oldMedia,
                        NewValue = newMedia,
                        TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_MEDIA"
                    });
                }
            }

            if (oldTemplate.WorkInstructionType != newTemplate.WorkInstructionType)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "WorkInstructionType",
                    OldValue = oldTemplate.WorkInstructionType.ToString(),
                    NewValue = newTemplate.WorkInstructionType.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_WORK_INSTRUCTION_TYPE"
                });
            }

            if (oldTemplate.Role != newTemplate.Role)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "Role",
                    OldValue = oldTemplate.Role.ToString(),
                    NewValue = newTemplate.Role.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_ROLE"
                });
            }

            if (oldTemplate.Tags != null || newTemplate.Tags != null)
            {
                if (oldTemplate.Tags == null)
                    oldTemplate.Tags = new List<Tag>();
                if (newTemplate.Tags == null)
                    newTemplate.Tags = new List<Tag>();

                var oldIds = oldTemplate.Tags.Select(x => x.Id).OrderBy(x => x).ToList();
                var newIds = newTemplate.Tags.Select(x => x.Id).OrderBy(x => x).ToList();

                if (!oldIds.SequenceEqual(newIds))
                {
                    workInstructionChanges.Add(new WorkInstructionTemplateChange()
                    {
                        PropertyName = "Tags",
                        OldValue = oldIds.ToJsonFromObject(),
                        NewValue = newIds.ToJsonFromObject(),
                        TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_TAGS"
                    });
                }
            }

            if (oldTemplate.IsAvailableForAllAreas != newTemplate.IsAvailableForAllAreas)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "IsAvailableForAllAreas",
                    OldValue = oldTemplate.IsAvailableForAllAreas.ToString(),
                    NewValue = newTemplate.IsAvailableForAllAreas.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_IS_AVAILABLE_FOR_ALL_AREAS"
                });
            }

            if (oldTemplate.NumberOfInstructionItems != newTemplate.NumberOfInstructionItems)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "NumberOfInstructionItems",
                    OldValue = oldTemplate.NumberOfInstructionItems.ToString(),
                    NewValue = newTemplate.NumberOfInstructionItems.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_NUMBER_OF_INSTRUCTION_ITEMS"
                });
            }

            var itemChanges = await GetChangesForWorkInstructionTemplateInstructionItems(oldItems: oldTemplate.InstructionItems, newItems: newTemplate.InstructionItems);
            if (itemChanges != null && itemChanges.Count > 0)
            {
                workInstructionChanges.AddRange(itemChanges);
            }

            var workInstructionTemplateChangesNotification = new WorkInstructionTemplateChangeNotification()
            {
                WorkInstructionTemplateId = newTemplate.Id,
                CompanyId = companyId,
                NotificationComment = string.IsNullOrEmpty(notificationComment) ? null : notificationComment,
                NotificationData = workInstructionChanges
            };

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromWorkInstructionTemplateChangesNotification(notification: workInstructionTemplateChangesNotification, companyId: companyId, userId: userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_workinstructiontemplate_changes_notification", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                return possibleId;
            }

            return 0;
        }

        public async Task<List<WorkInstructionTemplateChange>> GetChangesForWorkInstructionTemplateInstructionItems(List<InstructionItemTemplate> oldItems, List<InstructionItemTemplate> newItems)
        {
            var output = new List<WorkInstructionTemplateChange>();

            var oldIds = new List<int>();
            var newIds = new List<int>();

            if (oldItems != null)
            {
                oldIds = oldItems.Select(i => i.Id).ToList();
            }
            if (newItems != null)
            {
                newIds = newItems.Select(i => i.Id).ToList();
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            foreach (var oldId in oldIds)
            {
                if(newIds.Contains(oldId))
                {
                    var oldWiItemOnlyChanges = new InstructionItemTemplate();
                    var newWiItemOnlyChanges = new InstructionItemTemplate();

                    var oldWiItem = oldItems.Where(i => i.Id == oldId).FirstOrDefault();
                    var newWiItem = newItems.Where(i => i.Id == oldId).FirstOrDefault();

                    bool oldWiChanged = false;
                    //determine changed properties
                    //skip id, companyid, createdat, modifiedat, createdbyid, modifiedbyid for comparison
                    if(oldWiItem != null && newWiItem != null)
                    {
                        if (oldWiItem.InstructionTemplateId != newWiItem.InstructionTemplateId)
                        {
                            oldWiItemOnlyChanges.InstructionTemplateId = oldWiItem.InstructionTemplateId;
                            newWiItemOnlyChanges.InstructionTemplateId = newWiItem.InstructionTemplateId;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.AssessmentTemplateId != newWiItem.AssessmentTemplateId)
                        {
                            oldWiItemOnlyChanges.AssessmentTemplateId = oldWiItem.AssessmentTemplateId;
                            newWiItemOnlyChanges.AssessmentTemplateId = newWiItem.AssessmentTemplateId;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Name != newWiItem.Name)
                        {
                            oldWiItemOnlyChanges.Name = oldWiItem.Name;
                            newWiItemOnlyChanges.Name = newWiItem.Name;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Description != newWiItem.Description)
                        {
                            oldWiItemOnlyChanges.Description = oldWiItem.Description;
                            newWiItemOnlyChanges.Description = newWiItem.Description;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Picture != newWiItem.Picture)
                        {
                            oldWiItemOnlyChanges.Picture = oldWiItem.Picture;
                            newWiItemOnlyChanges.Picture = newWiItem.Picture;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Video != newWiItem.Video)
                        {
                            oldWiItemOnlyChanges.Video = oldWiItem.Video;
                            newWiItemOnlyChanges.Video = newWiItem.Video;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.VideoThumbnail != newWiItem.VideoThumbnail)
                        {
                            oldWiItemOnlyChanges.VideoThumbnail = oldWiItem.VideoThumbnail;
                            newWiItemOnlyChanges.VideoThumbnail = newWiItem.VideoThumbnail;
                            oldWiChanged = true;
                        }

                        if (oldWiItem?.Media?.ToJsonFromObject() != newWiItem?.Media?.ToJsonFromObject())
                        {
                            oldWiItemOnlyChanges.Media = oldWiItem.Media;
                            newWiItemOnlyChanges.Media = newWiItem.Media;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Index != newWiItem.Index)
                        {
                            oldWiItemOnlyChanges.Index = oldWiItem.Index;
                            newWiItemOnlyChanges.Index = newWiItem.Index;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Tags != null && newWiItem.Tags != null && oldWiItem.Tags.ToJsonFromObject() != newWiItem.Tags.ToJsonFromObject())
                        {
                            oldWiItemOnlyChanges.Tags = oldWiItem.Tags;
                            newWiItemOnlyChanges.Tags = newWiItem.Tags;
                            oldWiChanged = true;
                        }

                        if (oldWiItem?.Attachments?.ToJsonFromObject() != newWiItem?.Attachments?.ToJsonFromObject())
                        {
                            oldWiItemOnlyChanges.Attachments = oldWiItem.Attachments;
                            newWiItemOnlyChanges.Attachments = newWiItem.Attachments;
                            oldWiChanged = true;
                        }

                        //store changed properties of item as json in one workinstructiontemplatechange
                        if (oldWiChanged)
                        {
                            output.Add(new WorkInstructionTemplateChange()
                            {
                                PropertyName = $"InstructionItem{oldWiItem.Id}",
                                OldValue = JsonSerializer.Serialize(oldWiItemOnlyChanges, options),
                                NewValue = JsonSerializer.Serialize(newWiItemOnlyChanges, options),
                                TranslationKey = ""
                            });
                        }
                    }
                }
                else
                {
                    var oldWiItem = oldItems.Where(i => i.Id == oldId).FirstOrDefault();
                    //item doesnt exist anymore
                    //new is empty, old is old wi template item
                    if (oldWiItem != null)
                    {
                        output.Add(new WorkInstructionTemplateChange()
                        {
                            PropertyName = $"InstructionItem{oldWiItem.Id}",
                            OldValue = JsonSerializer.Serialize(oldWiItem, options),
                            NewValue = null,
                            TranslationKey = ""
                        });
                    }
                }
            }
            foreach(var newId in newIds.Except(oldIds))
            {
                //new added item
                //new is new wi template item
                var newWiItem = newItems.Where(i => i.Id == newId).FirstOrDefault();

                //old is empty
                if (newWiItem != null)
                {
                    output.Add(new WorkInstructionTemplateChange()
                    {
                        PropertyName = $"InstructionItem{newWiItem.Id}",
                        OldValue = null,
                        NewValue = JsonSerializer.Serialize(newWiItem, options),
                        TranslationKey = ""
                    });
                }
            }

            return output;
        }

        public async Task<int> ChangeWorkInstructionTemplateAsync(int companyId, int userId, int workInstructionTemplateId, WorkInstructionTemplate workInstructionTemplate)
        {
            workInstructionTemplate.Id = workInstructionTemplateId;
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_templates.ToString(), workInstructionTemplateId);
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            var tempInstructionItems = workInstructionTemplate.InstructionItems.ToList();
            foreach (var tempInstructionItem in tempInstructionItems)
            {
                var index = tempInstructionItems.IndexOf(tempInstructionItem);
                workInstructionTemplate.InstructionItems[index].Index = index + 1;
            }

            parameters.AddRange(GetNpgsqlParametersFromWorkInstructionTemplate(workInstructionTemplate: workInstructionTemplate, companyId: companyId, workInstructionTemplateId: workInstructionTemplateId, userId: userId, update: true));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_workinstructiontemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                workInstructionTemplate.Tags ??= new();
                await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.WorkInstructionTemplate, possibleId, workInstructionTemplate.Tags, companyId, userId);

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_templates.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.workinstruction_templates.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Changed workinstructiontemplate.");
            }
            if (workInstructionTemplate.InstructionItems == null)
                workInstructionTemplate.InstructionItems = new List<InstructionItemTemplate>();


            bool success = await UpdateInstructionTemplateItems(workInstructionTemplate, userId, companyId);
            if (success)
            {
                return possibleId;
            }
            else
            {
                return 0;
            }
        }

        public async Task<bool> SetWorkInstructionTemplateActiveAsync(int companyId, int userId, int workInstructionTemplateId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_templates.ToString(), workInstructionTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", workInstructionTemplateId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_workinstructiontemplate_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_templates.ToString(), workInstructionTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.workinstruction_templates.ToString(), objectId: workInstructionTemplateId, userId: userId, companyId: companyId, description: "Changed workinstruction template active state.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - public methods WorkInstructionTemplateChangesNotifications 
        public async Task<List<WorkInstructionTemplateChangeNotification>> GetWorkInstructionTemplateChangesNotificationsAsync(int companyId, int? userId = null, WorkInstructionTemplateChangeNotificationFilters? filters = null)
        {
            var output = new List<WorkInstructionTemplateChangeNotification>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.WorkInstructionTemplateId.HasValue && filters.Value.WorkInstructionTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_workinstruction_template_id", filters.Value.WorkInstructionTemplateId.Value));
                    }

                    if (filters.Value.UserId.HasValue && filters.Value.UserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.StartTimestamp.HasValue && filters.Value.StartTimestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_starttimestamp", filters.Value.StartTimestamp.Value));
                    }

                    if (filters.Value.EndTimeStamp.HasValue && filters.Value.EndTimeStamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_endtimestamp", filters.Value.EndTimeStamp.Value));
                    }

                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if(filters.Value.Confirmed.HasValue && userId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_confirmed", filters.Value.Confirmed.Value));
                        parameters.Add(new NpgsqlParameter("@_confirmedbyuserid", userId.Value));
                    }
                }

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate_change_notifications", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var workInstructionTemplateChangesNotification = CreateOrFillWorkInstructionTemplateChangesNotificationFromReader(dr);
                        output.Add(workInstructionTemplateChangesNotification);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.GetWorkInstructionTemplatesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<WorkInstructionTemplateChangeNotification> GetWorkInstructionTemplateChangeNotificationAsync(int id, int companyId, string include = null)
        {
            var output = new WorkInstructionTemplateChangeNotification();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_id", id));

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate_change_notification", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var workInstructionTemplateChangesNotification = CreateOrFillWorkInstructionTemplateChangesNotificationFromReader(dr);
                        if (workInstructionTemplateChangesNotification != null && workInstructionTemplateChangesNotification.Id > 0)
                        {
                            output = workInstructionTemplateChangesNotification;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.GetWorkInstructionTemplateChangeNotification(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower())) output = await AppendViewedStatsToChangeNotification(companyId: companyId, change: output);

            return output;
        }

        public async Task<bool> ConfirmWorkInstructionTemplateChangesNotifications(int companyId, int userId, int workInstructionTemplateId)
        {
            try
            {
                //_companyid integer, _workinstruction_template_id integer, _userid integer
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_workinstruction_template_id", workInstructionTemplateId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_workinstruction_template_changes_notifications_confirmed", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                return (rowseffected > 0);
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex) //swallow error for now.
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }
        }
        #endregion

        #region - private methods WorkInstructions

        #endregion

        #region - private methods WorkInstructionTemplates
        private WorkInstructionTemplate CreateOrFillWorkInstructionTemplateFromReader(NpgsqlDataReader dr, WorkInstructionTemplate workInstructionTemplate = null)
        {
            if (workInstructionTemplate == null) workInstructionTemplate = new WorkInstructionTemplate();

            workInstructionTemplate.Id = Convert.ToInt32(dr["id"]);
            workInstructionTemplate.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                workInstructionTemplate.Description = dr["description"].ToString();
            }

            workInstructionTemplate.AreaId = Convert.ToInt32(dr["area_id"]);
            workInstructionTemplate.CompanyId = Convert.ToInt32(dr["company_id"]);

            if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
            {
                workInstructionTemplate.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
                workInstructionTemplate.Picture = workInstructionTemplate.Media.FirstOrDefault();
            }

            if (Enum.TryParse(dr["instruction_type"].ToString(), out InstructionTypeEnum instructionType) &&
                Enum.IsDefined(typeof(InstructionTypeEnum), instructionType))
            {
                workInstructionTemplate.WorkInstructionType = instructionType;
            }

            if (Enum.TryParse(dr["role"].ToString(), out RoleTypeEnum roleType) &&
                Enum.IsDefined(typeof(RoleTypeEnum), roleType))
            {
                workInstructionTemplate.Role = roleType;
            }

            if (dr["created_at"] != DBNull.Value)
            {
                workInstructionTemplate.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                workInstructionTemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }
            workInstructionTemplate.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            workInstructionTemplate.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);

            workInstructionTemplate.CreatedBy = Convert.ToString(dr["created_by"]);
            workInstructionTemplate.ModifiedBy = Convert.ToString(dr["modified_by"]);

            if (dr["is_available_for_all_areas"] != DBNull.Value)
            {
                workInstructionTemplate.IsAvailableForAllAreas = Convert.ToBoolean(dr["is_available_for_all_areas"]);
            }

            if (dr.HasColumn("unread_changes_notifications_count") && dr["unread_changes_notifications_count"] != DBNull.Value)
            {
                workInstructionTemplate.UnreadChangesNotificationsCount = Convert.ToInt32(dr["unread_changes_notifications_count"]);
            }

            if (dr.HasColumn("skillinstructions_active") && dr["skillinstructions_active"] != DBNull.Value)
            {
                workInstructionTemplate.IsWITemplateLinkedToAssessment = Convert.ToBoolean(dr["skillinstructions_active"]);
            }
            if (dr.HasColumn("version"))
            {
                if (dr["version"] != DBNull.Value)
                {
                    workInstructionTemplate.Version = Convert.ToString(dr["version"]);
                }
            }

            return workInstructionTemplate;
        }

        private InstructionItemTemplate CreateOrFillInstructionTemplateItemFromReader(NpgsqlDataReader dr, InstructionItemTemplate instructionItemTemplate = null)
        {
            if (instructionItemTemplate == null) instructionItemTemplate = new InstructionItemTemplate();

            instructionItemTemplate.Id = Convert.ToInt32(dr["id"]);
            instructionItemTemplate.Index = Convert.ToInt32(dr["index"]);
            instructionItemTemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            instructionItemTemplate.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                instructionItemTemplate.Description = dr["description"].ToString();
            }

            if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
            {
                instructionItemTemplate.Media = dr["media"].ToString().ToObjectFromJson<List<string>>();
                if (instructionItemTemplate.Media.Count == 2)
                {
                    instructionItemTemplate.VideoThumbnail = instructionItemTemplate.Media[0];
                    instructionItemTemplate.Video = instructionItemTemplate.Media[1];
                }
                else
                {
                    instructionItemTemplate.Picture = instructionItemTemplate.Media.FirstOrDefault();
                }
            }

            if (dr["created_at"] != DBNull.Value)
            {
                instructionItemTemplate.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                instructionItemTemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }
            instructionItemTemplate.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            instructionItemTemplate.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
            instructionItemTemplate.CreatedBy = Convert.ToString(dr["created_by"]);
            instructionItemTemplate.ModifiedBy = Convert.ToString(dr["modified_by"]);
            if (dr.HasColumn("workinstruction_template_id"))
            {
                instructionItemTemplate.InstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);
            }

            if (dr.HasColumn("attachments"))
            {
                var attachmentsJson = Convert.ToString(dr["attachments"] ?? "");
                if (attachmentsJson == "")
                {
                    instructionItemTemplate.Attachments = new List<Attachment>();
                }
                else
                {
                    instructionItemTemplate.Attachments = Convert.ToString(dr["attachments"] ?? "").ToObjectFromJson<List<Attachment>>();
                }
            }

            return instructionItemTemplate;
        }

        private WorkInstructionTemplateChangeNotificationViewed CreateOrFillChangeNotificationViewedStatFromReader(NpgsqlDataReader dr,  WorkInstructionTemplateChangeNotificationViewed stat = null)
        {
            if (stat == null) stat = new WorkInstructionTemplateChangeNotificationViewed();

            stat.Id = Convert.ToInt32(dr["id"]);

            stat.WorkInstructionTemplateChangeNotificationId = Convert.ToInt32(dr["workinstruction_template_change_notification_id"]);
            stat.WorkInstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);

            if (dr["viewed_at"] != DBNull.Value)
            {
                stat.ViewedAt = Convert.ToDateTime(dr["viewed_at"]);
            }

            stat.ViewedUser = new Models.Basic.UserBasic()
            {
                Id = Convert.ToInt32(dr["user_id"]),
                Name = Convert.ToString(dr["user_fullname"]),
                Picture = Convert.ToString(dr["user_picture"])
            };

            return stat;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromWorkInstructionTemplate(WorkInstructionTemplate workInstructionTemplate, int companyId, int workInstructionTemplateId = 0, int userId = 0, bool update = false)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (workInstructionTemplateId > 0 && update) parameters.Add(new NpgsqlParameter("@_id", workInstructionTemplateId));

            parameters.Add(new NpgsqlParameter("@_name", workInstructionTemplate.Name));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            parameters.Add(new NpgsqlParameter("@_areaid", workInstructionTemplate.AreaId));

            if (string.IsNullOrEmpty(workInstructionTemplate.Picture))
                workInstructionTemplate.Media = null;
            else
                workInstructionTemplate.Media = new List<string> { workInstructionTemplate.Picture };

            if (workInstructionTemplate.Media != null && workInstructionTemplate.Media.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_media", workInstructionTemplate.Media.ToJsonFromObject()));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_media", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(workInstructionTemplate.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", workInstructionTemplate.Description));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }

            if (workInstructionTemplate.Role != null && Enum.IsDefined(typeof(RoleTypeEnum), workInstructionTemplate.Role))
            {
                parameters.Add(new NpgsqlParameter("@_role", (int)workInstructionTemplate.Role));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_role", DBNull.Value));
            }

            if (Enum.IsDefined(typeof(InstructionTypeEnum), workInstructionTemplate.WorkInstructionType))
            {
                parameters.Add(new NpgsqlParameter("@_instruction_type", (int)workInstructionTemplate.WorkInstructionType));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_instruction_type", (int)InstructionTypeEnum.SkillInstruction));
            }

            if (userId != 0 && !update)
            {
                parameters.Add(new NpgsqlParameter("@_created_by_id", userId));
            }
            else if (workInstructionTemplate.CreatedById != 0 && !update)
            {
                parameters.Add(new NpgsqlParameter("@_created_by_id", Convert.ToInt32(workInstructionTemplate.CreatedById)));
            }
            if (userId != 0)
            {
                parameters.Add(new NpgsqlParameter("@_modified_by_id", userId));
            }
            else if (workInstructionTemplate.ModifiedById != 0)
            {
                parameters.Add(new NpgsqlParameter("@_modified_by_id", Convert.ToInt32(workInstructionTemplate.ModifiedById)));
            }
            if (workInstructionTemplate.IsAvailableForAllAreas.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_is_available_for_all_areas", workInstructionTemplate.IsAvailableForAllAreas.Value));
            }

            return parameters;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromInstructionItemTemplate(InstructionItemTemplate instructionItemTemplate, int companyId, int workInstructionTemplateId = 0, int userId = 0, bool update = false)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (instructionItemTemplate?.Id > 0 && update) parameters.Add(new NpgsqlParameter("@_id", instructionItemTemplate.Id));
            parameters.Add(new NpgsqlParameter("@_workinstruction_template_id", workInstructionTemplateId));
            parameters.Add(new NpgsqlParameter("@_name", instructionItemTemplate.Name));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_index", instructionItemTemplate.Index));

            if (string.IsNullOrEmpty(instructionItemTemplate.Picture) && string.IsNullOrEmpty(instructionItemTemplate.Video) && string.IsNullOrEmpty(instructionItemTemplate.VideoThumbnail))
            {
                instructionItemTemplate.Media = null;
            }
            else if (!string.IsNullOrEmpty(instructionItemTemplate.Video) && !string.IsNullOrEmpty(instructionItemTemplate.VideoThumbnail))
            {
                instructionItemTemplate.Media = new List<string> { instructionItemTemplate.VideoThumbnail, instructionItemTemplate.Video };
            }
            else if (!string.IsNullOrEmpty(instructionItemTemplate.Picture))
            {
                instructionItemTemplate.Media = new List<string> { instructionItemTemplate.Picture };
            }

            if (instructionItemTemplate.Media != null && instructionItemTemplate.Media.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_media", instructionItemTemplate.Media.ToJsonFromObject()));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_media", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(instructionItemTemplate.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", instructionItemTemplate.Description));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }

            if (userId != 0 && !update)
            {
                parameters.Add(new NpgsqlParameter("@_created_by_id", userId));
            }
            else if (instructionItemTemplate.CreatedById != 0 && !update)
            {
                parameters.Add(new NpgsqlParameter("@_created_by_id", instructionItemTemplate.CreatedById));
            }
            if (userId != 0)
            {
                parameters.Add(new NpgsqlParameter("@_modified_by_id", userId));
            }
            else if (instructionItemTemplate.ModifiedById != 0)
            {
                parameters.Add(new NpgsqlParameter("@_modified_by_id", instructionItemTemplate.ModifiedById));
            }
            if (instructionItemTemplate.Attachments != null && instructionItemTemplate.Attachments.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_attachments", instructionItemTemplate.Attachments.ToJsonFromObject()));
            }
            return parameters;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromWorkInstructionTemplateChangesNotification(WorkInstructionTemplateChangeNotification notification, int companyId, int userId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (notification.WorkInstructionTemplateId != 0)
            {
                parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", notification.WorkInstructionTemplateId));
            }

            if (companyId != 0)
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            }

            if (!string.IsNullOrEmpty(notification.NotificationComment))
            {
                parameters.Add(new NpgsqlParameter("@_notificationcomment", notification.NotificationComment));
            }

            if (notification.NotificationData != null)
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var notificationData = JsonSerializer.Serialize(notification.NotificationData, options);
                parameters.Add(new NpgsqlParameter("@_notificationdata", notificationData));
            }

            if (userId != 0)
            {
                parameters.Add(new NpgsqlParameter("@_userid", userId));
            }

            return parameters;
        }

        /// <summary>
        /// GetTagsWithWorkInstructionTemplateAsync; Get Tags with an Action based on ActionId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="workInstructionTemplateId">WorkInstructionTemplateId (DB: workinstruction_templates.id)</param>
        /// <returns>List of Tags.</returns>
        private async Task<List<Tag>> GetTagsWithWorkInstructionTemplateAsync(int companyId, int workInstructionTemplateId)
        {
            var output = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.WorkInstructionTemplate, id: workInstructionTemplateId);
            if (output != null && output.Count > 0)
            {
                return output;
            }
            return null;
        }

        /// <summary>
        /// AppendTagsToWorkInstructionTemplatesAsync; append tags to WorkInstructionTemplate collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="workInstructionTemplates">Collection of WorkInstructionTemplate</param>
        /// <returns>Collection of WorkInstructionTemplate</returns>
        private async Task<List<WorkInstructionTemplate>> AppendTagsToWorkInstructionTemplatesAsync(int companyId, List<WorkInstructionTemplate> workInstructionTemplates, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var allTagsOnWorkInstructionTemplates = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.WorkInstructionTemplate, connectionKind: connectionKind);
            if (allTagsOnWorkInstructionTemplates != null)
            {
                foreach (var WorkInstructionTemplate in workInstructionTemplates)
                {
                    var tagsOnThisWorkInstruction = allTagsOnWorkInstructionTemplates.Where(t => t.ObjectId == WorkInstructionTemplate.Id).ToList();
                    if (tagsOnThisWorkInstruction != null && tagsOnThisWorkInstruction.Count > 0)
                    {
                        WorkInstructionTemplate.Tags ??= new List<Models.Tags.Tag>();
                        WorkInstructionTemplate.Tags.AddRange(tagsOnThisWorkInstruction);
                    }

                }
            }

            return workInstructionTemplates;
        }

        private async Task<WorkInstructionTemplate> AppendAreaPathsToWorkInstructionTemplateAsync(int companyId, WorkInstructionTemplate workInstructionTemplate, bool addAreaPath = true, bool addAreaPathIds = false)
        {
            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                var area = areas?.Where(x => x.Id == workInstructionTemplate.AreaId)?.FirstOrDefault();
                if (area != null)
                {
                    if (addAreaPath) workInstructionTemplate.AreaPath = area?.FullDisplayName;
                    if (addAreaPathIds) workInstructionTemplate.AreaPathIds = area?.FullDisplayIds;
                }
            }
            return workInstructionTemplate;
        }

        private async Task<List<WorkInstructionTemplate>> AppendAreaPathsToWorkInstructionTemplatesAsync(int companyId, List<WorkInstructionTemplate> workInstructionTemplates, bool addAreaPath = true, bool addAreaPathIds = false)
        {
            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var workInstructionTemplate in workInstructionTemplates)
                {
                    var area = areas?.Where(x => x.Id == workInstructionTemplate.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        if (addAreaPath) workInstructionTemplate.AreaPath = area.FullDisplayName;
                        if (addAreaPathIds) workInstructionTemplate.AreaPathIds = area.FullDisplayIds;
                    }

                }
            }
            return workInstructionTemplates;
        }

        private async Task<WorkInstructionTemplate> AppendInstructionTemplateItemsAsync(int companyId, WorkInstructionTemplate workInstructionTemplate, WorkInstructionFilters? filters = null, int? userId = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;
            if (workInstructionTemplate.InstructionItems == null) workInstructionTemplate.InstructionItems = new List<InstructionItemTemplate>();

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_template_id", workInstructionTemplate.Id));

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate_items", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        workInstructionTemplate.InstructionItems.Add(CreateOrFillInstructionTemplateItemFromReader(dr, new InstructionItemTemplate()));
                    }

                    workInstructionTemplate.NumberOfInstructionItems = workInstructionTemplate.InstructionItems?.Count ?? 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.AppendWorkInstructionTemplateItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return workInstructionTemplate;
        }

        private async Task<List<WorkInstructionTemplate>> AppendInstructionTemplatesItemsAsync(int companyId, List<WorkInstructionTemplate> workInstructionTemplates, WorkInstructionFilters? filters = null, int? userId = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;
            List<InstructionItemTemplate> instructionItems = new List<InstructionItemTemplate>();

            if (workInstructionTemplates == null || workInstructionTemplates.Count == 0)
                return workInstructionTemplates;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_template_ids", workInstructionTemplates.Select(w => w.Id).ToList()));

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate_items_v2", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        instructionItems.Add(CreateOrFillInstructionTemplateItemFromReader(dr, new InstructionItemTemplate()));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.AppendWorkInstructionTemplateItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            foreach (var workInstructionTemplate in workInstructionTemplates)
            {
                if (workInstructionTemplate.InstructionItems == null) workInstructionTemplate.InstructionItems = new List<InstructionItemTemplate>();

                var relevantInstructionItems = instructionItems.Where(i => i.InstructionTemplateId == workInstructionTemplate.Id).ToList();

                workInstructionTemplate.InstructionItems.AddRange(relevantInstructionItems);

                workInstructionTemplate.NumberOfInstructionItems = workInstructionTemplate.InstructionItems?.Count ?? 0;
            }

            return workInstructionTemplates;
        }

        private async Task<WorkInstructionTemplateChangeNotification> AppendViewedStatsToChangeNotification(int companyId, WorkInstructionTemplateChangeNotification change, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;
            if (change.NotificationViewedStats == null)
            {
                change.NotificationViewedStats = new List<WorkInstructionTemplateChangeNotificationViewed>();
            }

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_change_id", change.Id));

                using (dr = await _manager.GetDataReader("get_workinstructiontemplate_change_notification_viewed_stats", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        change.NotificationViewedStats.Add(CreateOrFillChangeNotificationViewedStatFromReader(dr, new WorkInstructionTemplateChangeNotificationViewed()));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.AppendViewedStatsToChangeNotification(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return change;
        }

        private async Task<bool> UpdateInstructionTemplateItems(WorkInstructionTemplate workInstructionTemplate, int userId, int companyId)
        {
            if (workInstructionTemplate?.InstructionItems == null)
                workInstructionTemplate.InstructionItems = new List<InstructionItemTemplate>();

            var existingWorkInstructionTemplate = new WorkInstructionTemplate() { Id = workInstructionTemplate.Id, CompanyId = workInstructionTemplate.CompanyId, InstructionItems = new List<InstructionItemTemplate>() };

            //get existing instruction template items by companyid and template id
            await AppendInstructionTemplateItemsAsync(existingWorkInstructionTemplate.CompanyId, existingWorkInstructionTemplate);

            //check which ones are not in current list and delete them
            var toDelete = existingWorkInstructionTemplate?.InstructionItems
                .Where(old => !workInstructionTemplate.InstructionItems.Select(current => current.Id).Contains(old.Id)).ToList();

            foreach (var item in toDelete)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_template_items.ToString(), item.Id);

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_id", item.Id));
                parameters.Add(new NpgsqlParameter("@_company_id", workInstructionTemplate.CompanyId));
                parameters.Add(new NpgsqlParameter("@_active", false));
                try
                {
                    var rowcount = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_workinstructiontemplate_item_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                    if (rowcount > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_template_items.ToString(), item.Id);
                        await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.workinstruction_template_items.ToString(), objectId: item.Id, userId: userId, companyId: companyId, description: "Changed workinstruction template item active state.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.UpdateInstructionTemplateItems(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
            }

            //insert new steps
            var toAdd = workInstructionTemplate.InstructionItems.Where(current => !existingWorkInstructionTemplate.InstructionItems.Select(i => i.Id).Contains(current.Id)).ToList();
            foreach (var item in toAdd)
            {
                List<NpgsqlParameter> parameters = GetNpgsqlParametersFromInstructionItemTemplate(item, workInstructionTemplate.CompanyId, workInstructionTemplateId: workInstructionTemplate.Id, userId: userId, update: false);
                try
                {
                    var possibleItemId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_workinstructiontemplate_item", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                    if (possibleItemId > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_template_items.ToString(), possibleItemId);
                        await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.workinstruction_template_items.ToString(), objectId: possibleItemId, userId: userId, companyId: companyId, description: "Added workinstruction template item.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.UpdateInstructionTemplateItems(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
            }

            //update existing steps
            var toUpdate = workInstructionTemplate.InstructionItems.Where(current => existingWorkInstructionTemplate.InstructionItems.Select(i => i.Id).Contains(current.Id)).ToList();
            foreach (var item in toUpdate)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_template_items.ToString(), item.Id);

                List<NpgsqlParameter> parameters = GetNpgsqlParametersFromInstructionItemTemplate(item, workInstructionTemplate.CompanyId, workInstructionTemplateId: workInstructionTemplate.Id, userId: userId, update: true);
                try
                {
                    var possibleItemId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_workinstructiontemplate_item", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                    if (possibleItemId > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.workinstruction_template_items.ToString(), possibleItemId);
                        await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.workinstruction_template_items.ToString(), objectId: possibleItemId, userId: userId, companyId: companyId, description: "Changed workinstruction template item.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.UpdateInstructionTemplateItems(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
            }

            return true;
        }
        #endregion

        #region - private methods WorkInstructionTemplateChangesNotifications 
        private WorkInstructionTemplateChangeNotification CreateOrFillWorkInstructionTemplateChangesNotificationFromReader(NpgsqlDataReader dr, WorkInstructionTemplateChangeNotification workInstructionTemplateChangesNotification = null)
        {
            /*
             * id integer, 
             * workinstruction_template_id integer, 
             * company_id integer, 
             * notification_comment text, 
             * notification_data text, 
             * created_at timestamp without time zone, 
             * created_by_id integer, 
             * created_by character varying, 
             * modified_at timestamp without time zone, 
             * modified_by_id integer, 
             * modified_by character varying
             */
            if (workInstructionTemplateChangesNotification == null) workInstructionTemplateChangesNotification = new WorkInstructionTemplateChangeNotification();

            workInstructionTemplateChangesNotification.Id = Convert.ToInt32(dr["id"]);
            workInstructionTemplateChangesNotification.WorkInstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);
            workInstructionTemplateChangesNotification.CompanyId = Convert.ToInt32(dr["company_id"]);
            workInstructionTemplateChangesNotification.NotificationComment = dr["notification_comment"].ToString();

            if (dr["notification_data"] != DBNull.Value && !string.IsNullOrEmpty(dr["notification_data"].ToString()))
            {
                workInstructionTemplateChangesNotification.NotificationData = dr["notification_data"].ToString().ToObjectFromJson<List<WorkInstructionTemplateChange>>();
            }

            if (dr["created_at"] != DBNull.Value)
            {
                workInstructionTemplateChangesNotification.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                workInstructionTemplateChangesNotification.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }
            workInstructionTemplateChangesNotification.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            workInstructionTemplateChangesNotification.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);

            workInstructionTemplateChangesNotification.CreatedBy = Convert.ToString(dr["created_by"]);
            workInstructionTemplateChangesNotification.ModifiedBy = Convert.ToString(dr["modified_by"]);

            return workInstructionTemplateChangesNotification;
        }

        #endregion

        #region - relations -
        public async Task<bool> RemoveTaskTemplateWorkInstructionRelation(int companyId, int taskTemplateWorkInstructionRelationId, int taskTemplateId, int? auditTemplateId = null, int? checklistTemplateId = null)
        {
            var storedProcedure = "remove_workinstruction_tasktemplate_relation";
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", taskTemplateWorkInstructionRelationId));
            parameters.Add(new NpgsqlParameter("@_tasktemplateid", taskTemplateId));

            if (auditTemplateId.HasValue && auditTemplateId.Value > 0)
            {
                parameters.Add(new NpgsqlParameter("@_audittemplateid", auditTemplateId));
                storedProcedure = "remove_workinstruction_audittemplate_item_relation";
            }

            if (checklistTemplateId.HasValue && checklistTemplateId.Value > 0)
            {
                parameters.Add(new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId));
                storedProcedure = "remove_workinstruction_checklisttemplate_item_relation";
            }

            //"remove_workinstruction_tasktemplate_relation"("_companyid" int4, "_id" int4, "_tasktemplateid" int4)
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);
        }

        public async Task<bool> ChangeTaskTemplateWorkInstructionRelationAsync(int companyId, int userId, int workInstructionRelationId, TaskTemplateRelationWorkInstructionTemplate workInstructionRelation)
        {
            var storedProcedure = "change_workinstruction_tasktemplate_relation";
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", workInstructionRelationId));
            parameters.Add(new NpgsqlParameter("@_index", workInstructionRelation.Index));
            parameters.Add(new NpgsqlParameter("@_tasktemplateid", workInstructionRelation.TaskTemplateId));

            if (workInstructionRelation.AuditTemplateId.HasValue && workInstructionRelation.AuditTemplateId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_audittemplateid", workInstructionRelation.AuditTemplateId.Value));
                storedProcedure = "change_workinstruction_audittemplate_item_relation";
            }

            if (workInstructionRelation.ChecklistTemplateId.HasValue && workInstructionRelation.ChecklistTemplateId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_checklisttemplateid", workInstructionRelation.ChecklistTemplateId.Value));
                storedProcedure = "change_workinstruction_checklisttemplate_item_relation";
            }

            //"change_workinstruction_tasktemplate_relation"("_companyid" int4, "_id" int4, "_tasktemplateid" int4, "_index" int4)
            //"change_workinstruction_audittemplate_item_relation"("_companyid" int4, "_id" int4, "_tasktemplateid" int4, "_audittemplateid" int4, "_index" int4)
            //"change_workinstruction_checklisttemplate_item_relation"("_companyid" int4, "_id" int4, "_tasktemplateid" int4, "_checklisttemplateid" int4, "_index" int4)
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);
        }

        public async Task<int> AddTaskTemplateWorkInstructionRelationAsync(int companyId, int userId, TaskTemplateRelationWorkInstructionTemplate workInstructionRelation)
        {
            var storedProcedure = "add_workinstruction_tasktemplate_relation";
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_tasktemplateid", workInstructionRelation.TaskTemplateId));
            parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", workInstructionRelation.WorkInstructionTemplateId));
            parameters.Add(new NpgsqlParameter("@_index", workInstructionRelation.Index));

            if (workInstructionRelation.AuditTemplateId.HasValue && workInstructionRelation.AuditTemplateId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_audittemplateid", workInstructionRelation.AuditTemplateId.Value));
                storedProcedure = "add_workinstruction_audittemplate_item_relation";
            }

            if (workInstructionRelation.ChecklistTemplateId.HasValue && workInstructionRelation.ChecklistTemplateId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_checklisttemplateid", workInstructionRelation.ChecklistTemplateId.Value));
                storedProcedure = "add_workinstruction_checklisttemplate_item_relation";
            }
            //"add_workinstruction_tasktemplate_relation"("_companyid" int4, "_tasktemplateid" int4, "_workinstructiontemplateid" int4, "_index" int4)
            //"add_workinstruction_audittemplate_item_relation"("_companyid" int4, "_tasktemplateid" int4, "_workinstructiontemplateid" int4, "_audittemplateid" int4, "_index" int4)
            //"add_workinstruction_checklisttemplate_item_relation"("_companyid" int4, "_tasktemplateid" int4, "_workinstructiontemplateid" int4, "_checklisttemplateid" int4, "_index" int4)
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync(storedProcedure, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return possibleId;
        }

        public async Task<WorkInstructionTemplate> AppendWorkInstructionParentsAsync(int companyId, WorkInstructionTemplate workInstructionTemplate, ConnectionKind connectionKind = ConnectionKind.Reader)
        {

            NpgsqlDataReader dr = null;
            workInstructionTemplate.ParentRelations = new List<WorkInstructionRelationParent>();

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_workinstructiontemplateid", workInstructionTemplate.Id));

                //"public"."get_workinstruction_parent_relations"("_companyid" int4, "_workinstructiontemplateid" int4)
                //"company_id" int4, "workinstruction_template_id" int4, "tasktemplate_id" int4, "audittemplate_id" int4, "checklisttemplate_id" int4, "name" varchar, "media" text, "object_type" varchar
                using (dr = await _manager.GetDataReader("get_workinstruction_parent_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {

                        WorkInstructionRelationParent workInstructionParent = new WorkInstructionRelationParent();

                        if (dr["audittemplate_id"] != DBNull.Value) workInstructionParent.AuditTemplateId = Convert.ToInt32(dr["audittemplate_id"]);
                        if (dr["checklisttemplate_id"] != DBNull.Value) workInstructionParent.ChecklistTemplateId = Convert.ToInt32(dr["checklisttemplate_id"]);
                        workInstructionParent.Name = dr["name"].ToString();
                        if (dr["tasktemplate_id"] != DBNull.Value) workInstructionParent.TaskTemplateId = Convert.ToInt32(dr["tasktemplate_id"]);
                        if (dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
                        {
                            if (dr["media"].ToString().Contains("[") || dr["media"].ToString().Contains("{")) //make sure it contains json
                            {
                                var list = dr["media"].ToString().ToObjectFromJson<List<string>>();
                                workInstructionParent.Picture = list.FirstOrDefault();
                            }
                            else
                            { //if not; handle as single string
                                workInstructionParent.Picture = dr["media"].ToString();
                            }

                        }

                        workInstructionTemplate.ParentRelations.Add(workInstructionParent);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("WorkInstructionManager.AppendWorkInstructionTemplateItemsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }


            return workInstructionTemplate;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_areaManager.GetPossibleExceptions());
                listEx.AddRange(_tagManager.GetPossibleExceptions());
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
