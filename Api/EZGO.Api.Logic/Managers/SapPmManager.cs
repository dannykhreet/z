using CsvHelper;
using CsvHelper.Configuration;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class SapPmManager : BaseManager<SapPmManager>, ISapPmManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ICompanyManager _companyManager;
        private readonly IGeneralManager _generalManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IUserManager _userManager;

        #endregion

        #region - constructors -
        public SapPmManager(ICompanyManager companyManager, IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, IGeneralManager generalManager, IDataAuditing dataAuditing, IUserManager userManager, ILogger<SapPmManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _companyManager = companyManager;
            _generalManager = generalManager;
            _dataAuditing = dataAuditing;
            _userManager = userManager;
        }
        #endregion

        #region - public methods locations-
        public async Task<List<SapPmLocation>> SearchLocationsAsync(int companyId, string searchText, int? functionalLocationId)
        {
            var result = new List<SapPmLocation>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = _manager.GetBaseParameters(companyId);
                parameters.Add(new NpgsqlParameter("@_searchtext", searchText ?? (object)DBNull.Value));
                parameters.Add(new NpgsqlParameter("@_functionallocationid", functionalLocationId ?? (object)DBNull.Value));

                using (dr = await _manager.GetDataReader("get_functional_locations_by_text_search", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                    {
                        while (await dr.ReadAsync())
                        {
                            result.Add(CreateOrFillSapPmFunctionalLocationFromReader(dr));
                        }
                    }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.SearchLocationsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return result;
        }
        public async Task<List<SapPmLocation>> GetLocationChildren(int companyId, int? functionalLocationId)
        {
            var result = new List<SapPmLocation>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = _manager.GetBaseParameters(companyId);
                parameters.Add(new NpgsqlParameter("@_parentlocationid", functionalLocationId ?? (object)DBNull.Value));

                using (dr = await _manager.GetDataReader("get_functional_locations_by_parent_location_id", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        result.Add(CreateOrFillSapPmFunctionalLocationFromReader(dr));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.getLocationChildren(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return result;
        }

        public async Task<List<SapPmLocationImportData>> GetLocationImportDataForCsv(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = ";"
            };

            try
            {
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<SapPmLocationImportData>().ToList();
                    foreach (var record in records)
                    {
                        if (record.FunctionalLocation == null || record.FunctionalLocationName == null)
                        {
                            _logger.LogWarning("Skipping functional location with empty functional location or functional location name." + record);
                        }
                    }
                    records = records.Where(r => r.FunctionalLocation != null && r.FunctionalLocationName != null).ToList();

                    return records;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetLocationImportDataForCsv(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return null;
        }

        public async Task<DateTime> GetLastChangeDateForFunctionalLocationsByCompanyId(int companyId)
        {
            DateTime result = new DateTime(2015, 1, 1); // Default date if company doesn't exists in DB yet
            
            try
            {
                List<NpgsqlParameter> parameters = _manager.GetBaseParameters(companyId);

                var output = await _manager.ExecuteScalarAsync("get_functional_locations_last_change_date_by_company_id", parameters, System.Data.CommandType.StoredProcedure);

                if(output != DBNull.Value)
                {
                    result = (DateTime)output;
                }
                else
                {
                    _logger.LogWarning($"No functional locations found for {companyId}. Supplied {result} as a default cut off date.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetLastChangeDateForFunctionalLocationsByCompanyId(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return result;
        }

        public async Task<int> RegenerateFunctionalLocationsTreeStructure(int companyId)
        {
            int result = 0;
            try
            {
                List<NpgsqlParameter> parameters = _manager.GetBaseParameters(companyId);
                result = (int)await _manager.ExecuteScalarAsync("recalculate_sap_pm_functional_location_tree_structure", parameters, System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.RegenerateFunctionalLocationsTreeStructure(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            return result;
        }

        public async Task<int> SendFunctionalLocationsToDatabase(int companyId, string sapFunctionalLocations)
        {
            int result = 0;

            try
            {
                List<NpgsqlParameter> parameters = _manager.GetBaseParameters(companyId);
                parameters.Add(new NpgsqlParameter("@_jsoninput", sapFunctionalLocations));
                result = (int)await _manager.ExecuteScalarAsync("merge_functional_locations", parameters, System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.SendFunctionalLocationsToDatabase(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return result;
        }

        public async Task<int> ImportFunctionalLocationsInDatabase(string sapFunctionalLocations, string companyIds, bool recalculateTreeStructure = true)
        {
            int result = 0;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_jsoninput", sapFunctionalLocations));
                parameters.Add(new NpgsqlParameter("@_companyids", companyIds.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToArray()));
                parameters.Add(new NpgsqlParameter("@_recalc_tree_structure", recalculateTreeStructure));
                result = (int)await _manager.ExecuteScalarAsync("sap_pm_import_functional_locations", parameters, System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"JsonInput: {sapFunctionalLocations}");
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.ImportFunctionalLocationsInDatabase(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return result;
        }

        public async Task<int> ClearFunctionalLocationsInDatabase(string companyIds)
        {
            int result = 0;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyids", companyIds.Split(',').ToList().Select(x => Convert.ToInt32(x)).ToArray()));
                result = (int)await _manager.ExecuteScalarAsync("sap_pm_clear_data_before_fresh_import", parameters, System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.ClearFunctionalLocationsInDatabase(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return result;
        }


        /// <summary>
        /// GetSapPmFunctionalLocationAsync; Retrieve a specific SAP PM Functional Location specified by <paramref name="functionalLocationId"/>.
        /// </summary>
        /// <param name="companyId">The company id of the functional location</param>
        /// <param name="functionalLocationId">The id of the functional location</param>
        /// <returns>A functional location with a specific <paramref name="functionalLocationId"/>.</returns>
        public async Task<SapPmLocation> GetSapPmFunctionalLocationAsync(int companyId, int functionalLocationId)
        {
            var location = new SapPmLocation();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_id", functionalLocationId));
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_sap_pm_functional_location", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillSapPmFunctionalLocationFromReader(dr: dr, location: location);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetSapPmFunctionalLocationAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return location;
        }

        /// <summary>
        /// Retrieves a SAP PM functional location associated with the specified area ID.
        /// </summary>
        /// <remarks>This method executes a stored procedure to fetch the functional location data from
        /// the database. If an exception occurs during execution, it is logged and optionally added to the exception
        /// trace if the relevant configuration setting is enabled.</remarks>
        /// <param name="companyId">The ID of the company to which the functional location belongs.</param>
        /// <param name="functionalLocationId">The ID of the functional location to retrieve.</param>
        /// <param name="areaId">The ID of the area associated with the functional location.</param>
        /// <returns>A <see cref="SapPmLocation"/> object representing the functional location, or <see langword="null"/> if no
        /// matching location is found.</returns>
        public async Task<SapPmLocation> GetSapPmFunctionalLocationByAreaIdAsync(int companyId, int areaId)
        {
            SapPmLocation location = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new ("@_companyid", companyId),
                    new ("@_areaid", areaId)
                };
                using NpgsqlDataReader dr = await _manager.GetDataReader("get_functional_location_by_area_id", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);

                while (await dr.ReadAsync())
                {
                    location = CreateOrFillSapPmFunctionalLocationFromReader(dr: dr, location: location);
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"{nameof(SapPmManager)}.{nameof(GetSapPmFunctionalLocationByAreaIdAsync)}(): {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return location;
        }

        #endregion

        #region - public methods notifications -
        /// <summary>
        /// GetSapPmNotificationOptionsAsync; Retrieve Sap Pm Notification options related to a specific <paramref name="companyId"/>.
        /// </summary>
        /// <param name="companyId">The company id, for which to retrieve the notification options.</param>
        /// <returns>A SapPmNotificationOptions object with available notification options.</returns>
        public async Task<SapPmNotificationOptions> GetSapPmNotificationOptionsAsync(int companyId, int userId, int? areaId = null)
        {
            SapPmNotificationOptions result = null;

            Company company = await _companyManager.GetCompanyAsync(companyId, companyId, "companysettings");
            

            try
            {
                UserProfile user = await _userManager.GetUserProfileAsync(companyId, userId);

                if(string.IsNullOrEmpty(user.SapPmUsername))
                {
                    _logger.LogWarning($"User {userId} does not have SAP PM credentials set.");
                }

                if (!company.Settings.Exists(x => x.ResourceId == 112 & !string.IsNullOrEmpty(x.Value)))
                {
                    _logger.LogWarning($"Company {companyId} does not have SAP PM ID set (ResourceId 112).");
                }
                var locations = await GetLocationChildren(companyId, null);
                if (locations.Count == 0)
                {
                    _logger.LogWarning($"Company {companyId} does not have any SAP functional locations associated.");
                }

                var optionJson = await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyId, 113); //113 = Notification options
                if (!string.IsNullOrEmpty(optionJson))
                {
                    result = optionJson.ToObjectFromJson<SapPmNotificationOptions>();
                    if (areaId > 0)
                    {
                        result.FunctionalLocation = await GetSapPmFunctionalLocationByAreaIdAsync(companyId, areaId.Value);
                    }
                }
                else 
                {
                    _logger.LogWarning($"Company {companyId} does not have SAP PM notification options set (ResourceId 113).");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetSapPmNotificationOptionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return result;
        }

        public async Task<List<SapPmNotificationMessage>> GetSapPmNotificationMessagesAsync(int? companyId)
        {
            var result = new List<SapPmNotificationMessage>();
            NpgsqlDataReader dr = null;
            try
            {
                List<NpgsqlParameter> parameters;
                if (companyId != null)
                {
                    parameters = _manager.GetBaseParameters(companyId.Value);
                } 
                else
                {
                    parameters = new List<NpgsqlParameter>();
                }

                using (dr = await _manager.GetDataReader("get_unsent_sap_pm_notifications", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        result.Add(CreateOrFillSapPmNotificationMessageFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetSapPmNotificationMessagesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }
            return result;
        }

        public async Task<List<SapPmNotificationFailure>> GetSapPmNotificationMessageFailures()
        {
            List<SapPmNotificationFailure> result = new List<SapPmNotificationFailure>();
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_failuremessage", _configurationHelper.GetValueAsString("AppSettings:NotificationFailureMessage")));

                using (dr = await _manager.GetDataReader("get_sap_pm_notification_failures", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        result.Add(CreateOrFillSapPmNotificationFailureFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetSapPmNotificationMessagesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }
            return result;
        }
        #endregion

        #region - public methods settings -
        public async Task<bool> SetSapPmCredentialsAsync(int userId, int companyId, int? holdingId, Login sapPmCredentials)
        {
            if(holdingId == null)
            {
                return await _generalManager.ChangeSettingResourceCompany(companyid: companyId, setting: new SettingResourceItem
                {
                    CompanyId = companyId,
                    ResourceId = 122, //122 = SAP_PM_AUTHORISATION_CREDS
                    Value = sapPmCredentials.ToJsonFromObject(),
                }, encryptValue: true);
            }

            else
            {
                int dbHoldingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
                if(dbHoldingId != holdingId.Value)
                {
                    _logger.LogError($"Holding ID mismatch: Expected {holdingId.Value}, but got {dbHoldingId}.");
                    return false;
                }
                return await _generalManager.ChangeSettingResourceHolding(holdingId: holdingId.Value, encryptValue: true, setting: new SettingResourceItem
                {
                    HoldingId = holdingId.Value,
                    ResourceId = 122, //122 = SAP_PM_AUTHORISATION_CREDS
                    Value = sapPmCredentials.ToJsonFromObject(),
                });
            }
        }


        #endregion

        #region - public methods area location relations - 
        public async Task<List<AreaFunctionalLocationRelation>> GetAreaFunctionalLocationRelationsAsync(int companyId)
        {
            //get user_skills
            var output = new List<AreaFunctionalLocationRelation>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_area_sap_pm_location_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var userSkill = CreateOrFillAreaFunctionalLocationRelationFromReader(dr);
                        output.Add(userSkill);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetAreaFunctionalLocationRelationsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }
        //TODO ADD WRITER
        public async Task<List<AreaFunctionalLocationRelation>> GetAreaFunctionalLocationRelationsAsync(int companyId, int areaId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            //get user_skills
            var output = new List<AreaFunctionalLocationRelation>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_areaid", areaId));

                using (dr = await _manager.GetDataReader("get_area_sap_pm_location_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var userSkill = CreateOrFillAreaFunctionalLocationRelationFromReader(dr);
                        output.Add(userSkill);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetAreaFunctionalLocationRelationsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<int> AddAreaSapPmLocationRelationAsync(int companyId, int areaId, int locationId, int userId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));
            parameters.Add(new NpgsqlParameter("@_locationid", locationId));
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_area_sap_pm_location_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area_sap_pm_location_relations.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.companies_area_sap_pm_location_relations.ToString(), objectId: areaId, userId: userId, companyId: companyId, description: "Added area functional location relation.");
            }

            return possibleId;
        }

        public async Task<int> RemoveAreaSapPmLocationRelationAsync(int id, int areaId, int locationId, int companyId, int userId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_area_sap_pm_location_relations.ToString(), id);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", id));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));
            parameters.Add(new NpgsqlParameter("@_locationid", locationId));

            var count = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_area_sap_pm_location_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            await _dataAuditing.WriteDataAudit(original: original, mutated: string.Empty, Models.Enumerations.TableNames.companies_area_sap_pm_location_relations.ToString(), objectId: areaId, userId: userId, companyId: companyId, description: "Removed area functional location relation.");

            return count;
        }

        #endregion 

        #region - private methods -

        private SapPmLocation CreateOrFillSapPmFunctionalLocationFromReader(NpgsqlDataReader dr, SapPmLocation location = null)
        {
            location ??= new SapPmLocation();

            location.Id = Convert.ToInt32(dr["functional_location_id"]);
            location.FunctionalLocation = Convert.ToString(dr["functional_location"]);
            location.FunctionalLocationName = Convert.ToString(dr["functional_location_name"]);
            location.MarkedForDeletion = Convert.ToBoolean(dr["marked_for_deletion"]);
            location.HasChildren = Convert.ToBoolean(dr["has_children"]);

            return location;
        }

        private SapPmNotificationMessage CreateOrFillSapPmNotificationMessageFromReader(NpgsqlDataReader dr, SapPmNotificationMessage message = null)
        {
            message ??= new SapPmNotificationMessage();

            message.Id = Convert.ToInt32(dr["id"]);
            message.CompanyId = Convert.ToInt32(dr["company_id"]);
            message.ActionId = Convert.ToInt32(dr["action_id"]);

            DateTime creationDate = DateTime.SpecifyKind(Convert.ToDateTime(dr["created_at"]), DateTimeKind.Utc);
            string dateFormat = "dd.MM.yyyy";
            string timeFormat = "HH:mm:ss";
            string timezone = "UTC";

            if (dr["sap_timezone"] != DBNull.Value)
            {
                timezone = Convert.ToString(dr["sap_timezone"]);
            }

            TimeZoneInfo CompanySAPTimezone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            creationDate = TimeZoneInfo.ConvertTimeFromUtc(creationDate, CompanySAPTimezone);

            message.Payload = new SapPmNotificationPayload
            {

                NotificationText = Convert.ToString(dr["notification_text"]),
                MaintPriority = Convert.ToString(dr["maint_priority"]),
                NotificationType = Convert.ToString(dr["notification_type"]),
                ReportedByUser = Convert.ToString(dr["reported_by_user"]),
                NotificationCreationDate = creationDate.ToString(dateFormat),
                NotificationCreationTime = creationDate.ToString(timeFormat),
                MaintNotifLongTextForEdit = Convert.ToString(dr["maint_notif_long_text_for_edit"]),
                MaintenancePlannerGroup = Convert.ToString(dr["maintenance_planner_group"]),
                MaintenancePlanningPlant = Convert.ToString(dr["maintenance_planning_plant"]),
                MainWorkCenter = Convert.ToString(dr["main_work_center"]),
                MainWorkCenterPlant = Convert.ToString(dr["main_work_center_plant"]),
                FunctionalLocation = Convert.ToString(dr["functional_location"]),
            };

            return message;
        }

        private SapPmNotificationFailure CreateOrFillSapPmNotificationFailureFromReader(NpgsqlDataReader dr, SapPmNotificationFailure failure = null)
        {
            failure ??= new SapPmNotificationFailure();

            failure.ActionId = Convert.ToInt32(dr["action_id"]);
            failure.FailureCount = Convert.ToInt32(dr["last_trailing_fail_count"]);
            failure.MinutesSinceLastFailure = Convert.ToInt32(dr["minutes_since_last_failure"]);

            return failure;
        }

        public async Task<List<SapPmLocation>> GetFunctionalLocationsByLocationIdsAsync(int companyId, List<int> locationIds)
        {
            var result = new List<SapPmLocation>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (locationIds != null && locationIds.Count > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_locationids", locationIds.ToArray()));
                }

                using (dr = await _manager.GetDataReader("get_functional_locations_by_location_ids", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        result.Add(CreateOrFillSapPmFunctionalLocationFromReader(dr));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetFunctionalLocationsByLocationIdsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return result;
        }


        private AreaFunctionalLocationRelation CreateOrFillAreaFunctionalLocationRelationFromReader(NpgsqlDataReader dr, AreaFunctionalLocationRelation relation = null)
        {
            relation ??= new AreaFunctionalLocationRelation();

            relation.Id = Convert.ToInt32(dr["id"]);
            relation.AreaId = Convert.ToInt32(dr["area_id"]);
            relation.LocationId = Convert.ToInt32(dr["location_id"]);

            return relation;
        }


        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
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
