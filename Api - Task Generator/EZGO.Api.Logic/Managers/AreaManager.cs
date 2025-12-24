using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.SapPm;
using Microsoft.Extensions.Caching.Memory;
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
    /// AreaManager; The AreaManager contains all logic for retrieving and setting Areas.
    /// A area is used as a collection group. Areas are created with a company and mostly setup as a tree (e.g. parent -> child 1 -> child 1.1 etc).
    /// Most of the primary objects (Task/TaskTemplate, Checklist/ChecklistTemplate, Audit/AuditTemplate etc.) are directly linked to a area. 
    /// Area's are also the primary filters for displaying data in the client apps (users choose a specific area before actually using the other functionalities).
    /// Depending on the user, one are more areas are coupled to a user. These are treaded as 'rights', so a user has rights to view a certain area. 
    /// These rights are display only though. Seeing depending on which functionality a user is using they can view objects that are linked to areas where they don't have any rights to. 
    /// </summary>
    public class AreaManager : BaseManager<AreaManager>, IAreaManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IUserAccessManager _userAccessManager;
        private readonly IGeneralManager _generalManager;
        private readonly IAreaBasicManager _areaBasicManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;
        private readonly ISapPmManager _sapPmManager;
        private readonly IUserManager _userManager;
        #endregion

        #region - constructor(s) -
        public AreaManager(IDatabaseAccessHelper manager, IUserManager userManager, IConfigurationHelper configurationHelper, IDataAuditing dataAuditing, ILogger<AreaManager> logger, IUserAccessManager userAccessManager, ISapPmManager sapPmManager, IGeneralManager generalManager, IAreaBasicManager areaBasicManager) : base(logger)
        {
            _manager = manager;
            _userAccessManager = userAccessManager;
            _configurationHelper = configurationHelper;
            _dataAuditing = dataAuditing;
            _generalManager = generalManager;
            _sapPmManager = sapPmManager;
            _areaBasicManager = areaBasicManager;
            _userManager = userManager;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetAreasAsync; Get area's of a company. Area's are based on the [companies_area] table in the database.
        /// The items are loaded from the database based on a MaxLevel (default to 2, so level 0 and 1) and are rendered as a TreeView.
        /// So the actual output will be a List of items, containing sub-items, that contain sub-items etc.
        /// If a flat collection is needed, UseTreeview can be set to false and the entire list will be returned.
        /// Following stored procedures will be used for database data retrieval: "get_areas"
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <param name="maxLevel">MaxLevel (levels companies_company.level) that must be queried.</param>
        /// <param name="useTreeview">UseTreeView, true/false, return the list as flat collection or as dynamic tree view.</param>
        /// <param name="userId">UserId (DB: user_profiles.id)</param>
        /// <param name="filters">Filters object with filters that need to be done;</param>
        /// <param name="include">Include items (extra data to be retrieved after main set of areas are retrieved)</param>
        /// <returns>List of Area objects.</returns>
        public async Task<List<Area>> GetAreasAsync(int companyId, int maxLevel = 2, bool useTreeview = true, int? userId = null, AreaFilters? filters = null, string include = null)
        {
            var output = new List<Area>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_maxlevel", maxLevel));

                using (dr = await _manager.GetDataReader("get_areas", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var area = CreateOrFillAreaFromReader(dr);
                        output.Add(area);
                    }
                }

            } catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreasAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            } finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && (userId != null))
            {
                var currentUser = await _userManager.GetUserProfileAsync(companyId: companyId, userId: userId.Value);
                if (currentUser != null && currentUser.Role != "manager")
                {
                    //NOTE: When adding more filters, if they apply to manager, this needs to change
                    //but currently we only the AllowedOnly filter so this solution currently works fine
                    output = (await FilterAreas(companyId: companyId, filters: filters.Value, nonFilteredCollection: output, userId: userId)).ToList();
                }
            }

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.CompanyRoot.ToString().ToLower())) output = await AppendCompanyRoot(areas: output, companyId: companyId);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.SapPmFunctionalLocations.ToString().ToLower())) output = await AppendFunctionalLocationsToAreas(areas: output, companyId: companyId);

            }

            if (useTreeview) output = CreateTreeView(output);

            return output;
        }

        /// <summary>
        /// GetAreaAsync; Get a single area object based on the AreaId parameter. Based on the [companies_area] table in the database.
        /// Following stored procedures will be used for database data retrieval: "get_area"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="include">Include parameter</param>
        /// <returns>Area object.</returns>
        public async Task<Area> GetAreaAsync(int companyId, int areaId, ConnectionKind connectionKind = ConnectionKind.Reader, string include = null)
        {
            var area = new Area();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", areaId));

                using (dr = await _manager.GetDataReader("get_area", commandType: System.Data.CommandType.StoredProcedure, connectionKind: connectionKind, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillAreaFromReader(dr, area: area);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreaAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            //Get system information

            if(area.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Areas.ToString().ToLower())) area.Children = CreateTreeView((await _areaBasicManager.GetAreasBasicByStartAreaAsync(companyId: companyId, areaId: area.Id, areaFilterType: FilterAreaTypeEnum.RecursiveRootToLeaf, connectionKind: connectionKind)).Select(x => new Area() { Id =  x.Id, Name = x.Name, ParentId = x.ParentId, FullDisplayName = x.NamePath}).ToList());
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.SapPmFunctionalLocations.ToString().ToLower())) area = await AppendFunctionalLocationToArea(area: area, companyId: companyId, connectionKind: connectionKind);
                return area;
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Get area names based on area ids
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="areaIds">area ids to get the names for</param>
        /// <returns>dictionary of area ids and area names</returns>
        public async Task<Dictionary<int, string>> GetAreaNamesAsync(int companyId, List<int> areaIds)
        {
            Dictionary<int, string> idsNames = new();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_areaids", areaIds)
                };

                using NpgsqlDataReader dr = await _manager.GetDataReader("get_area_names", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    int id = Convert.ToInt32(dr["id"]);
                    string name = dr["name"].ToString();
                    idsNames.Add(id, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreaNamesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return idsNames;
        }

        /// <summary>
        /// AddAreaAsync; Adds a area to the database.
        /// Following stored procedures will be used for database data retrieval: "add_area"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="area">area object (DB: companies_area)</param>
        /// <returns>The identity of the table (DB: companies_area.id)</returns>
        public async Task<int> AddAreaAsync(int companyId, int userId, Area area)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            NpgsqlDataReader dr = null;

            parameters.AddRange(GetNpgsqlParametersFromArea(area: area, companyId: area.CompanyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_area", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.companies_area.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added area.");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_USER_AREA_SYNC_CONFIG_KEY))
                {
                    //update underlying users
                    if (area.ParentId.HasValue && area.ParentId.Value != 0)
                    {
                        List<NpgsqlParameter> areaParameters = new List<NpgsqlParameter>();
                        areaParameters.Add(new NpgsqlParameter("@_areaid", area.ParentId.Value));
                        areaParameters.Add(new NpgsqlParameter("@_companyid", companyId));

                        List<int> userIds = new List<int>();
                        using (dr = await _manager.GetDataReader("get_users_connected_to_allowed_area", commandType: System.Data.CommandType.StoredProcedure, parameters: areaParameters))
                        {
                            while (await dr.ReadAsync())
                            {
                                userIds.Add(Convert.ToInt32(dr["user_id"]));
                            }
                        }
                        foreach (var userid in userIds)
                        {
                            //update allowed areas
                            List<NpgsqlParameter> allowedAreaParameters = new List<NpgsqlParameter>();
                            allowedAreaParameters.Add(new NpgsqlParameter("@_userid", userid));
                            allowedAreaParameters.Add(new NpgsqlParameter("@_companyid", companyId));
                            Convert.ToInt32(await _manager.ExecuteScalarAsync("set_allowed_areas", parameters: allowedAreaParameters, commandType: System.Data.CommandType.StoredProcedure));
                        }
                    }
                }

                //update sap info if feature on
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP"))
                {
                    if (area.SapPmFunctionalLocationId != null && area.SapPmFunctionalLocationId.Value > 0)
                    {
                        SapPmLocation sapPmLocation = await _sapPmManager.GetSapPmFunctionalLocationAsync(companyId: companyId, functionalLocationId: area.SapPmFunctionalLocationId.Value);
                        if (!sapPmLocation.MarkedForDeletion)
                        {
                            var relationId = await _sapPmManager.AddAreaSapPmLocationRelationAsync(companyId: companyId, areaId: possibleId, locationId: area.SapPmFunctionalLocationId.Value, userId: userId);
                        }
                    }
                }
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeAreaAsync; Change a Area.
        /// Following stored procedures will be used for database data retrieval: "change_area"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="area">Area object (DB: companies_area)</param>
        /// <param name="areaId">AreaId, id of the object in the database that needs to be updated. (DB: companies_area.id)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeAreaAsync(int companyId, int userId, int areaId, Area area)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area.ToString(), areaId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromArea(area: area, companyId: companyId, areaId: areaId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_area", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area.ToString(), areaId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_area.ToString(), objectId: areaId, userId: userId, companyId: companyId, description: "Changed area.");
            }

            //update sap info if feature on
            if(await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP"))
            {
                bool relationAlreadyExists = false;
                var functionalLocationRelations = await _sapPmManager.GetAreaFunctionalLocationRelationsAsync(companyId: companyId, areaId: areaId);
                if(functionalLocationRelations != null && functionalLocationRelations.Count > 0)
                {
                    foreach(var relation in functionalLocationRelations)
                    {
                        if (relation.LocationId != area.SapPmFunctionalLocationId)
                        {
                            _ = await _sapPmManager.RemoveAreaSapPmLocationRelationAsync(id: relation.Id, areaId: relation.AreaId, locationId: relation.LocationId, companyId: companyId, userId: userId);
                        }
                        else
                        {
                            relationAlreadyExists = true;
                        }
                    }
                }
                if (!relationAlreadyExists && area.SapPmFunctionalLocationId != null && area.SapPmFunctionalLocationId.Value > 0)
                {
                    SapPmLocation sapPmLocation = await _sapPmManager.GetSapPmFunctionalLocationAsync(companyId: companyId, functionalLocationId: area.SapPmFunctionalLocationId.Value);
                    if (!sapPmLocation.MarkedForDeletion)
                    {
                        var possibleId = await _sapPmManager.AddAreaSapPmLocationRelationAsync(companyId: companyId, areaId: areaId, locationId: area.SapPmFunctionalLocationId.Value, userId: userId);
                    }
                }
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// SetAreaActiveAsync; Set Area active/inactive based on AreaId.
        /// Following stored procedures will be used for database data retrieval: "set_area_active"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a Area to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetAreaActiveAsync(int companyId, int userId, int areaId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area.ToString(), areaId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", areaId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteNonQueryAsync("set_area_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (isActive == false)
            {
                await RemoveActionAssignedAreaAsync(companyId: companyId, userId: userId, areaId: areaId);
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area.ToString(), areaId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_area.ToString(), objectId: areaId, userId: userId, companyId: companyId, description: "Changed area active state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// Remove all actions from assigned area
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task<int> RemoveActionAssignedAreaAsync(int companyId, int userId, int areaId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action_assigned_areas.ToString(), Models.Enumerations.TableFields.area_id.ToString(), areaId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var count = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_assigned_area_from_actions", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action_assigned_areas.ToString(), Models.Enumerations.TableFields.area_id.ToString(), areaId);
            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action_assigned_areas.ToString(), objectId: areaId, userId: userId, companyId: companyId, description: "Changed area action relation collection.");


            return count;
        }
        #endregion


        #region - private methods -
        /// <summary>
        /// FilterAreas; Filter only items that are allowed.
        /// Note; this method will filter an existing collection after the collection is already retrieved from the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (dB: user_profiles.id)</param>
        /// <param name="filters">Filter object containing the filters. Only allowed only will be used. Other filters are already done on retrieval from db</param>
        /// <param name="nonFilteredCollection">Collection of Area items.</param>
        /// <returns>A filtered list of areas.</returns>
        private async Task<IList<Area>> FilterAreas(int companyId, AreaFilters filters, IList<Area> nonFilteredCollection, int? userId = null)
        {
            var filtered = nonFilteredCollection;
            if (filters.HasFilters())
            {
                if (filters.AllowedOnly.HasValue && filters.AllowedOnly.Value && userId.HasValue)
                {
                    filtered = await FilterAreasAllowedOnly(companyId: companyId, userId: userId.Value, areas: filtered);
                }
            }
            return filtered;
        }

        /// <summary>
        /// FilterAreas; Filter areas on allowed only areas. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (dB: user_profiles.id)</param>
        /// <param name="areas">Collection of Area items</param>
        /// <returns>Return a list of filtered items.</returns>
        private async Task<IList<Area>> FilterAreasAllowedOnly(int companyId, int userId, IList<Area> areas)
        {
            var allowedAreas = await _userAccessManager.GetAllowedAreaIdsWithUserAsync(companyId: companyId, userId: userId);

             areas = areas.Where(x => allowedAreas.Contains(x.Id)).ToList();

            return areas;
        }

        /// <summary>
        /// CreateTreeView; Create a tree view based on a list of areas.
        /// Note! The list must be sorted (SQL sorting) based on level, parent_id, tree_id, name.
        /// Note! The list must contain the root items. (the items that are on level on and do not have a ParentId)
        /// </summary>
        /// <param name="areas">List of area's containing the structure.</param>
        /// <returns>A list of area's based, containing sub-areas, that contain sub-areas etc. etc.</returns>
        private List<Area> CreateTreeView(List<Area> areas)
        {
            var output = new List<Area>();

            if(areas != null)
            {
                foreach (var item in areas)
                {
                    var currentArea = item;
                    if (item.ParentId.HasValue && item.ParentId > 0)
                    {
                        var foundItem = FindRecursivelyAreaInListByParentId(output, item.ParentId.Value);
                        if (foundItem != null)
                        {
                            foundItem.Children.Add(currentArea);
                        }
                        else
                        {
                            output.Add(currentArea);
                        }
                    }
                    else
                    {
                        output.Add(currentArea); //root item, does not have a parent.
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// FindRecursivelyAreaInListByParentId; Find a specific area based on a ParentId, if found the item will be returned, if not found then all child area items will be checked and so on.
        /// </summary>
        /// <param name="areas">Areas that possibly have the current parent area.</param>
        /// <param name="parentId">The id of the object that needs to be found.</param>
        /// <returns>Area that is found. If no Areas are found, return NULL is the default.</returns>
        private Area FindRecursivelyAreaInListByParentId(List<Area> areas, int parentId)
        {
            if (areas.Where(x => x.Id == parentId).Any())
            {
                return areas.Where(x => x.Id == parentId).FirstOrDefault();
            }
            else
            {
                foreach (var item in areas)
                {
                    if(item.Children != null)
                    {
                        var foundItem = FindRecursivelyAreaInListByParentId(item.Children, parentId);
                        if(foundItem != null)
                        {
                            return foundItem;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// AppendCompanyRoot; Append company root to list as root item.
        /// </summary>
        /// <param name="areas">List of areas where company needs to be added</param>
        /// <param name="companyId">CompanyId of the company that needs to be added.</param>
        /// <returns>Area list, with root item attached.</returns>
        private async Task<List<Area>> AppendCompanyRoot(List<Area> areas, int companyId)
        {
            var company = await GetCompanyAsync(companyId: companyId);

            if(company != null && company.Id > 0)
            {
                //add root area, default to 1, number is not used.
                var rootArea = new Area() { Id = 1, CompanyId = company.Id, Name = company.Name, Picture = company.Picture };
                foreach (var area in areas)
                {
                    if(area.Level == 0 && !area.ParentId.HasValue)
                    {
                        area.ParentId = 1; //set to root value;
                    }
                }
                areas.Insert(0, rootArea);
            }

            return areas;
        }

        /// <summary>
        /// GetCompanyAsync; Get company based on the CompanyId
        /// Following stored procedures will be used for database data retrieval: "get_company"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A Company, depending on include parameter this will also contains a Shift collection.</returns>
        public async Task<Company> GetCompanyAsync(int companyId)
        {
            var company = new Company();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", companyId));

                using (dr = await _manager.GetDataReader("get_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillCompanyFromReader(dr, company: company);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetCompanyAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

             return company;

        }

        /// <summary>
        /// GetAreaHasActiveRelations; Gets a partial area object with 'has' parameters for checking if area has a active relation yes/no
        /// Following stored procedures will be used for database data retrieval: "get_area_has_active_relations"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <returns>AreaActiveRelations including has parameters.</returns>
        public async Task<AreaActiveRelations> GetAreaHasActiveRelations(int companyId, int areaId)
        {
            var output = new AreaActiveRelations();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", areaId));

                using (dr = await _manager.GetDataReader("get_area_has_active_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillAreaActiveRelationsFromReader(dr, arearelations: output);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreaHasActiveRelations(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetAreaNumberActiveRelations; Gets a partial area object with 'nr' parameters for checking if area how many active relation the area has
        /// Following stored procedures will be used for database data retrieval: "get_area_nr_active_relations"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId"></param>
        /// <returns>AreaActiveRelations including number parameters.</returns>
        public async Task<AreaActiveRelations> GetAreaNumberActiveRelations(int companyId, int areaId)
        {
            var output = new AreaActiveRelations();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", areaId));

                using (dr = await _manager.GetDataReader("get_area_nr_active_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillAreaActiveRelationsFromReader(dr, arearelations: output);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreaNumberActiveRelations(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// CreateOrFillAreaFromReader; creates and fills a Area object from a DataReader.
        /// NOTE! intended for use with the area stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="area">Area object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled Area object.</returns>
        private Area CreateOrFillAreaFromReader(NpgsqlDataReader dr, Area area = null) {
            if (area == null) area = new Area();

            area.Id = Convert.ToInt32(dr["id"]);
            area.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["parent_id"] != DBNull.Value)
            {
                area.ParentId = Convert.ToInt32(dr["parent_id"]);
            }
            area.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                area.Description = dr["description"].ToString();
            }
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                area.Picture = dr["picture"].ToString();
            }
            area.Level = Convert.ToInt32(dr["level"]);
            area.FullDisplayName = dr["FullDisplayName"].ToString();
            if(dr.HasColumn ("fulldisplayids"))
            {
                area.FullDisplayIds = dr["fulldisplayids"].ToString();
            }

            return area;
        }

        private int CreateOrFillUserIdFromReader(NpgsqlDataReader dr, int userId = 0)
        {
            userId = Convert.ToInt32(dr["user_id"]);
            return userId;
        }

        private async Task<List<Area>> AppendFunctionalLocationsToAreas(List<Area> areas, int companyId)
        {
            //get sap info if feature on
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP") && areas != null && areas.Count > 0)
            {
                var functionalLocationRelations = await _sapPmManager.GetAreaFunctionalLocationRelationsAsync(companyId: companyId);
                if (functionalLocationRelations != null && functionalLocationRelations.Count > 0 && functionalLocationRelations.FirstOrDefault() != null)
                {
                    foreach (var area in areas)
                    {
                        var funcLocationRelation = functionalLocationRelations.Where(r => r.AreaId == area.Id).FirstOrDefault();
                        if (funcLocationRelation != null)
                        {
                            area.SapPmFunctionalLocationId = functionalLocationRelations.Where(r => r.AreaId == area.Id).FirstOrDefault().LocationId;
                        }
                    }
                }
            }

            List<int> locationIds = null;
            if (areas != null && areas.Count > 0)
            {
                locationIds = areas.Where(a => a.SapPmFunctionalLocationId != null).Select(a => a.SapPmFunctionalLocationId.Value).ToList();
            }
            if (locationIds != null && locationIds.Count > 0)
            {
                var functionalLocations = await _sapPmManager.GetFunctionalLocationsByLocationIdsAsync(companyId: companyId, locationIds);

                foreach (var area in areas)
                {
                    if (area.SapPmFunctionalLocationId != null)
                    {
                        area.SapPmLocation = functionalLocations.Where(f => f.Id == area.SapPmFunctionalLocationId).FirstOrDefault();
                    }
                }
            }

            return areas;
        }

        private async Task<Area> AppendFunctionalLocationToArea(Area area, int companyId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            //get sap info if feature on
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP") && area != null)
            {
                var functionalLocationRelations = await _sapPmManager.GetAreaFunctionalLocationRelationsAsync(companyId: companyId, areaId: area.Id, connectionKind: connectionKind);
                if (functionalLocationRelations != null && functionalLocationRelations.Count > 0 && functionalLocationRelations.FirstOrDefault() != null)
                {
                    area.SapPmFunctionalLocationId = functionalLocationRelations.FirstOrDefault().LocationId;
                }
                if (area.SapPmFunctionalLocationId.HasValue)
                {
                    area.SapPmLocation = await _sapPmManager.GetSapPmFunctionalLocationAsync(companyId: companyId, area.SapPmFunctionalLocationId.Value);
                }
            }

            return area;
        }

        /// <summary>
        /// CreateOrFillCompanyFromReader; creates and fills a Company object from a DataReader.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="company">Company object containing all data needed for updating the database. (DB: companies_company)</param>
        /// <returns>A filled Company object.</returns>
        private Company CreateOrFillCompanyFromReader(NpgsqlDataReader dr, Company company = null)
        {

            if (company == null) company = new Company();

            company.Id = Convert.ToInt32(dr["id"]);
            company.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                company.Picture = dr["picture"].ToString();
            }

            return company;
        }

        /// <summary>
        /// CreateOrFillAreaActiveRelationsFromReader; creates and fills a AreaActiveRelations object from a DataReader.
        /// NOTE! intended for use with the area stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="arearelations">AreaHasRelations object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled AreaActiveRelations object.</returns>
        private AreaActiveRelations CreateOrFillAreaActiveRelationsFromReader(NpgsqlDataReader dr, AreaActiveRelations arearelations = null)
        {
            if (arearelations == null) arearelations = new AreaActiveRelations();

            arearelations.Id = Convert.ToInt32(dr["id"]);
            arearelations.Name = dr["name"].ToString();

            if (dr.HasColumn("nr_active_tasktemplates") && dr["nr_active_tasktemplates"] != DBNull.Value)
            {
                arearelations.NrActiveTaskTemplates = Convert.ToInt32(dr["nr_active_tasktemplates"]);
            }

            if (dr.HasColumn("nr_active_actions") && dr["nr_active_actions"] != DBNull.Value)
            {
                arearelations.NrActiveActions = Convert.ToInt32(dr["nr_active_actions"]);
            }

            if (dr.HasColumn("nr_active_checklisttemplates") && dr["nr_active_checklisttemplates"] != DBNull.Value)
            {
                arearelations.NrActiveChecklistTemplates = Convert.ToInt32(dr["nr_active_checklisttemplates"]);
            }

            if (dr.HasColumn("nr_active_audittemplates") && dr["nr_active_audittemplates"] != DBNull.Value)
            {
                arearelations.NrActiveAuditTemplates = Convert.ToInt32(dr["nr_active_audittemplates"]);
            }

            if (dr.HasColumn("nr_active_shifts") && dr["nr_active_shifts"] != DBNull.Value)
            {
                arearelations.NrActiveShifts = Convert.ToInt32(dr["nr_active_shifts"]);
            }

            if (dr.HasColumn("nr_active_children") && dr["nr_active_children"] != DBNull.Value)
            {
                arearelations.NrActiveChildren = Convert.ToInt32(dr["nr_active_children"]);
            }

            if (dr.HasColumn("nr_active_workinstructions") && dr["nr_active_workinstructions"] != DBNull.Value)
            {
                arearelations.NrActivWorkinstructions = Convert.ToInt32(dr["nr_active_workinstructions"]);
            }

            if (dr.HasColumn("nr_active_assessmenttemplates") && dr["nr_active_assessmenttemplates"] != DBNull.Value)
            {
                arearelations.NrActiveAssessmentTemplates = Convert.ToInt32(dr["nr_active_assessmenttemplates"]);
            }

            if (dr.HasColumn("nr_active_matrices") && dr["nr_active_matrices"] != DBNull.Value)
            {
                arearelations.NrActiveMatrices = Convert.ToInt32(dr["nr_active_matrices"]);
            }

            if (dr.HasColumn("has_active_tasktemplates") && dr["has_active_tasktemplates"] != DBNull.Value)
            {
                arearelations.HasActiveTaskTemplates = Convert.ToBoolean(dr["has_active_tasktemplates"]);
            }

            if (dr.HasColumn("has_active_actions") && dr["has_active_actions"] != DBNull.Value)
            {
                arearelations.HasActiveActions = Convert.ToBoolean(dr["has_active_actions"]);
            }

            if (dr.HasColumn("has_active_checklisttemplates") && dr["has_active_checklisttemplates"] != DBNull.Value)
            {
                arearelations.HasActiveChecklistTemplates = Convert.ToBoolean(dr["has_active_checklisttemplates"]);
            }

            if (dr.HasColumn("has_active_audittemplates") && dr["has_active_audittemplates"] != DBNull.Value)
            {
                arearelations.HasActiveAuditTemplates = Convert.ToBoolean(dr["has_active_audittemplates"]);
            }

            if (dr.HasColumn("has_active_shifts") && dr["has_active_shifts"] != DBNull.Value)
            {
                arearelations.HasActiveShifts = Convert.ToBoolean(dr["has_active_shifts"]);
            }

            if (dr.HasColumn("has_active_children") && dr["has_active_children"] != DBNull.Value)
            {
                arearelations.HasActiveChildren = Convert.ToBoolean(dr["has_active_children"]);
            }

            if (dr.HasColumn("has_active_workinstruction_templates") && dr["has_active_workinstruction_templates"] != DBNull.Value)
            {
                arearelations.HasActiveWorkInstructionTemplates = Convert.ToBoolean(dr["has_active_workinstruction_templates"]);
            }

            if (dr.HasColumn("has_active_assessment_templates") && dr["has_active_assessment_templates"] != DBNull.Value)
            {
                arearelations.HasActiveAssessmentTemplates = Convert.ToBoolean(dr["has_active_assessment_templates"]);
            }

            if (dr.HasColumn("has_active_skills_matrices") && dr["has_active_skills_matrices"] != DBNull.Value)
            {
                arearelations.HasActiveSkillsMatrices = Convert.ToBoolean(dr["has_active_skills_matrices"]);
            }



            return arearelations;
        }

        /// <summary>
        /// GetNpgsqlParametersFromArea; Creates a list of NpgsqlParameter, and fills it based on the supplied Area object.
        /// NOTE! Intended for use with the area stored procedures within the database.
        /// NOTE! Area uses a specific object for system information, if not supplied it will default to certain settings.
        /// </summary>
        /// <param name="area">The supplied Area object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromArea(Area area, int companyId, int areaId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (areaId > 0) parameters.Add(new NpgsqlParameter("@_id", areaId));

            parameters.Add(new NpgsqlParameter("@_name", area.Name));
            parameters.Add(new NpgsqlParameter("@_description", area.Description));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_picture", area.Picture));
            if (area.ParentId.HasValue && area.ParentId.Value > 0)
            {
                parameters.Add(new NpgsqlParameter("@_parentid", Convert.ToInt32(area.ParentId)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_parentid", DBNull.Value));
            }
            if (area.SystemInformation != null && !string.IsNullOrEmpty(area.SystemInformation.SystemRole))
            {
                parameters.Add(new NpgsqlParameter("@_systemrole", area.SystemInformation.SystemRole));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_systemrole", DBNull.Value));
            }

            return parameters;
        }

        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion

    }
}
