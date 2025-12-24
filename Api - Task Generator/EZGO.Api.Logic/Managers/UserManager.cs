using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Data;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using EZGO.Api.Settings;
using EZGO.Api.Logic.Helpers;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Relations;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models.Users;
using EZGO.Api.Utils.Json;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using EZGO.Api.Utils.Security;
using EEZGO.Api.Utils.Data;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// UserManager; The UserManager contains all logic for retrieving and setting Users and UserProfiles.
    /// </summary>
    public class UserManager : BaseManager<UserManager>, IUserManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IGeneralManager _generalManager;
        private readonly IUserDataManager _userdatamanager;
        private readonly ICryptography _cryptography;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;

        #endregion

        #region - constructor(s) -
        public UserManager(IDatabaseAccessHelper manager, IGeneralManager generalManager, IConfigurationHelper configurationhelper, ILogger<UserManager> logger, IUserDataManager userDataManager, IDataAuditing dataAuditing, ICryptography cryptography) : base(logger)
        {
            _manager = manager;
            _generalManager = generalManager;
            _userdatamanager = userDataManager;
            _configurationHelper = configurationhelper;
            _cryptography = cryptography;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetUserProfilesAsync; Get all user profiles for a company.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">Filters that can be used to filter to collection.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. Used for including extra data. </param>
        /// <returns>List of UserProfile</returns>
        public async Task<List<UserProfile>> GetUserProfilesAsync(int companyId, UserFilters? filters = null, string include = null)
        {
            var output = new List<UserProfile>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_userprofiles", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var userprofile = CreateOrFillUserProfileFromReader(dr);

                        output.Add(userprofile);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetUserProfilesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                output = (await FilterUserProfiles(companyId: companyId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Areas.ToString().ToLower())) output =  await GetAllowedAreasWithProfiles(companyId: companyId, users: output);

            return output;
        }

        /// <summary>
        /// GetUserProfileAsync; Get specific user profile.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of userprofile.</param>
        /// <returns>UserProfile object.</returns>
        public async Task<UserProfile> GetUserProfileAsync(int companyId, int userId, string include = null)
        {
            var userprofile = new UserProfile();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", userId));

                using (dr = await _manager.GetDataReader("get_userprofile", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillUserProfileFromReader(dr, userprofile: userprofile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetUserProfileAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (userprofile.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Areas.ToString().ToLower())) userprofile.AllowedAreas = await GetAllowedAreasWithProfile(companyId: companyId, userId: userprofile.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Company.ToString().ToLower())) userprofile.Company = await GetCompanyWithProfile(companyId: companyId); //TODO add companyid to user profile.
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.DisplayAreas.ToString().ToLower())) userprofile.DisplayAreas = await GetDisplayAreasWithUser(userId: userprofile.Id, companyId: companyId); //TODO add companyid to user profile.
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Roles.ToString().ToLower())) userprofile = await GetRoles(userId: userprofile.Id, companyId: companyId, userProfile: userprofile); 

                return userprofile;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetUserProfileByTokenAsync; Gets the user profile token (DB:authtoken_token) for usage within the application or API.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userToken">User token (encrypted or unencrypted depending on tokenIsEncrypted)</param>
        /// <param name="tokenIsEncrypted">TokenIsEncrypted can be true/false if true, the token will be decrypted before adding to the SP of the database.</param>
        /// <param name="include">Include extra items. Currently Areas and Company data is supported.</param>
        /// <returns>A user profile based on data.</returns>
        public async Task<UserProfile> GetUserProfileByTokenAsync(int companyId, string userToken, bool tokenIsEncrypted = false, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var userprofile = new UserProfile();

            NpgsqlDataReader dr = null;

            //refactor
            var decryptedToken = userToken;
            if (tokenIsEncrypted)
            {
                if (!string.IsNullOrEmpty(userToken))
                {
                    try
                    {
                        //For use with build-in .net protection api.
                        //decryptedToken = _protector.Unprotect(userToken);
                        decryptedToken = _cryptography.Decrypt(userToken);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(message: "Error occurred on token un-protection. (UserManager.GetUserProfileByTokenAsync())", exception: ex);

                        if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                    }
                }
            }

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_token", decryptedToken));

                using (dr = await _manager.GetDataReader("get_userprofile_by_token", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        userprofile = CreateOrFillUserProfileFromReader(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetUserProfileByTokenAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (userprofile.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Areas.ToString().ToLower())) userprofile.AllowedAreas = await GetAllowedAreasWithProfile(companyId: companyId, userId: userprofile.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Company.ToString().ToLower())) userprofile.Company = await GetCompanyWithProfile(companyId: companyId);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Roles.ToString().ToLower())) userprofile = await GetRoles(userId: userprofile.Id, companyId: companyId, userProfile: userprofile);

                return userprofile;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get id, name of user and profile picture from database for all users of a company (including deleted users).
        /// If user is deleted, picture will not be retrieved.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>UserBasic object for user with given id</returns>
        public async Task<List<UserBasic>> GetUsersBasicAsync(int companyId)
        {
            List<UserBasic> usersBasic = new();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId)
            };

            await using (NpgsqlDataReader dr = await _manager.GetDataReader("get_userprofiles_basic", commandType: CommandType.StoredProcedure, parameters: parameters))
            {
                while (await dr.ReadAsync())
                {
                    usersBasic.Add(CreateOrFillUserBasicFromReader(dr));
                }
            }

            return usersBasic;
        }

        /// <summary>
        /// GetUsersIdsAsync; retrieves a list of user ids that are active with a company, used for validation purposes.
        /// This includes deleted users.
        /// </summary>
        /// <param name="companyId">CompanyId of the company where the users need to be retrieved.</param>
        /// <param name="userGuid">Current UserGuid of logged in user to be retrieved.</param>
        /// <param name="userGuid">UserGuid of possible user to be retrieved.</param>
        /// <param name="userSyncGuid">SyncGuid of possible user to be retrieved.</param>
        /// <returns>List of user ids.</returns>
        public async Task<List<int>> GetUsersIdsAsync(int companyId, string currentUserGuid = null, string userGuid = null, string userSyncGuid = null)
        {
            List<int> userIds = new();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId)
            };

            if(userGuid != null)
            {
                parameters.Add(new NpgsqlParameter("@_userguid", userGuid));
            };

            if(userSyncGuid != null)
            {
                parameters.Add(new NpgsqlParameter("@_usersyncguid", userSyncGuid));
            };

            if (userSyncGuid != null && userSyncGuid != null)
            {
                parameters.Add(new NpgsqlParameter("@_userguidvalidtimeframeinhours", 48));
            };

            await using (NpgsqlDataReader dr = await _manager.GetDataReader("get_userprofile_all_ids", commandType: CommandType.StoredProcedure, parameters: parameters))
            {
                while (await dr.ReadAsync())
                {
                    userIds.Add(Convert.ToInt32(dr["id"]));
                }
            }

            return userIds;
        }

        /// <summary>
        /// Get id, name of user and profile picture from database.
        /// If user is deleted, picture will not be retrieved.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="userId"></param>
        /// <returns>UserBasic object for user with given id</returns>
        public async Task<UserBasic> GetUserBasicAsync(int companyId, int userId)
        {
            UserBasic userbasic = new();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_id", userId)
            };

            await using (NpgsqlDataReader dr = await _manager.GetDataReader("get_userprofile_basic", commandType: CommandType.StoredProcedure, parameters: parameters))
            {
                while (await dr.ReadAsync())
                {
                    userbasic = CreateOrFillUserBasicFromReader(dr);
                }
            }

            return userbasic;
        }


        /// <summary>
        /// RetrieveSystemUserId; Retrieve system user for use while processing data
        /// </summary>
        /// <param name="companyId">CompanyId of user which needs to be retrieved (DB: company_companies.id)</param>
        /// <returns>int containing the ID</returns>
        public async Task<int> RetrieveSystemUserId(int companyId, int? holdingId = null)
        {
            int output = 0;
            if(holdingId == null)
            {
                holdingId = 0;
            }
            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _manager.GetDataReader("get_system_users_based_on_holding_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["company_id"] != DBNull.Value && dr["user_id"] != DBNull.Value)
                        {
                            output = Convert.ToInt32(dr["user_id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.RetrieveSystemUserId(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            return output;
        }

        #endregion

        #region - public add/change -

        /// <summary>
        /// AddUserProfileAsync; Add user profile to database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        public async Task<int> AddUserProfileAsync(int companyId, int userId, UserProfile userProfile)
        {
            int possibleId = 0; //await AddUserProfile(int companyId,  UserProfile userProfile)

            try
            {
                bool emailFound = false;
                bool usernameFound = false;
                bool upnFound = false;

                //check if email already exists.
                emailFound = await CheckEmail(userProfile.Email);

                //check if username already exists.
                usernameFound = await CheckUserName(userProfile.UserName);

                //check if UPN already exists.
                upnFound = await CheckUPN(userProfile.UPN);

                if (!emailFound && !usernameFound && !upnFound)
                {
                    if (!string.IsNullOrEmpty(userProfile.FirstName) && userProfile.FirstName.Length > 250)
                    {
                        userProfile.FirstName = userProfile.FirstName.Substring(0, 249);
                    }

                    if (!string.IsNullOrEmpty(userProfile.LastName) && userProfile.LastName.Length > 250)
                    {
                        userProfile.LastName = userProfile.LastName.Substring(0, 249);
                    }

                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                    parameters.Add(new NpgsqlParameter("@_firstname", userProfile.FirstName));
                    parameters.Add(new NpgsqlParameter("@_lastname", userProfile.LastName));

                    if (string.IsNullOrEmpty(userProfile.Picture))
                    {
                        parameters.Add(new NpgsqlParameter("@_picture", DBNull.Value));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_picture", userProfile.Picture));
                    }

                    parameters.Add(new NpgsqlParameter("@_email", userProfile.Email));
                    parameters.Add(new NpgsqlParameter("@_username", userProfile.UserName));
                    parameters.Add(new NpgsqlParameter("@_role", userProfile.Role));

                    if (string.IsNullOrEmpty(userProfile.UPN))
                    {
                        parameters.Add(new NpgsqlParameter("@_upn", DBNull.Value));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_upn", userProfile.UPN));
                    }
                    if (userProfile.IsTagManager.HasValue)
                        parameters.Add(new NpgsqlParameter("@_istagmanager", userProfile.IsTagManager.Value));

                    parameters.Add(new NpgsqlParameter("@_password", GenerateRandomPassword()));

                    possibleId = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "add_userprofile", parameters: parameters);

                    if (possibleId > 0 && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "MARKET_SAP"))
                    {
                        var sapParameters = new List<NpgsqlParameter>();

                        sapParameters.Add(new NpgsqlParameter("@_companyid", companyId));
                        sapParameters.Add(new NpgsqlParameter("@_id", possibleId));
                        sapParameters.Add(new NpgsqlParameter("@_username", userProfile.UserName));

                        if (!string.IsNullOrEmpty(userProfile.SapPmUsername) && userProfile.SapPmUsername.Length > 0 && userProfile.SapPmUsername.Length <= 12)
                        {
                            sapParameters.Add(new NpgsqlParameter("@_sappmusername", userProfile.SapPmUsername));
                        }
                        else if(!string.IsNullOrEmpty(userProfile.FirstName) || !string.IsNullOrEmpty(userProfile.LastName))
                        {
                            var firstNameShort = (userProfile.FirstName?.Length > 3) ? userProfile.FirstName.Substring(0, 3) : (userProfile.FirstName ?? "");
                            var lastNameShort = (userProfile.LastName?.Length > 9) ? userProfile.LastName.Substring(0, 9) : (userProfile.LastName ?? "");
                            var sapUsername = string.Concat(firstNameShort.ToUpperInvariant(), lastNameShort.ToUpperInvariant());
                            //filter out spaces
                            sapUsername = sapUsername.Replace(" ", "");
                            if (!string.IsNullOrEmpty(sapUsername) && sapUsername.Length <= 12)
                            {
                                sapParameters.Add(new NpgsqlParameter("@_sappmusername", sapUsername));
                            }
                            else
                            {
                                sapParameters.Add(new NpgsqlParameter("@_sappmusername", DBNull.Value));
                            }
                        }
                        else
                        {
                            sapParameters.Add(new NpgsqlParameter("@_sappmusername", DBNull.Value));
                        }

                        _ = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_userprofile_sap_information", parameters: sapParameters);

                    }

                    if (possibleId > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), possibleId);
                        await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.profiles_user.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added user profile.");

                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred AddUserProfileAsync()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }


            if (userProfile.AllowedAreas != null)
            {
                await AddOrChangeUserAreas(areas: userProfile.AllowedAreas, profileUserId: possibleId, userId: userId, companyId: companyId);
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeUserProfileAsync; Changes specific user profile. Used for updates from APP
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of userprofile.</param>
        /// <param name="userProfile">UserProfile containing all data for update.</param>
        /// <returns>bool true/false depending on outcome.</returns>
        public async Task<bool> ChangeUserProfileAsync(int companyId, int userId, UserProfile userProfile)
        {
            try
            {
                bool emailFound = false;
                int userProfileChanged = 0;

                //check if email already exists
                emailFound = await CheckEmail(userProfile.Email, userId: userId, companyId: companyId);

                userProfile = PrepareAndCleanUserProfile(userprofile: userProfile);

                if (!emailFound)
                {
                    var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfile.Id);

                    if(!string.IsNullOrEmpty(userProfile.FirstName) && userProfile.FirstName.Length > 250)
                    {
                        userProfile.FirstName = userProfile.FirstName.Substring(0, 249);
                    }

                    if (!string.IsNullOrEmpty(userProfile.LastName) && userProfile.LastName.Length > 250)
                    {
                        userProfile.LastName = userProfile.LastName.Substring(0, 249);
                    }

                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    parameters.Add(new NpgsqlParameter("@_id", userId));
                    parameters.Add(new NpgsqlParameter("@_firstname", userProfile.FirstName));
                    parameters.Add(new NpgsqlParameter("@_lastname", userProfile.LastName));
                    parameters.Add(new NpgsqlParameter("@_picture", string.IsNullOrEmpty(userProfile.Picture) ? string.Empty : userProfile.Picture));
                    parameters.Add(new NpgsqlParameter("@_email", userProfile.Email));

                    
                    userProfileChanged = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_userprofile", parameters: parameters);

                    if (userProfileChanged > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfile.Id);
                        await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user.ToString(), objectId: userProfile.Id, userId: userId, companyId: companyId, description: "Changed user profile.");

                    }

                    return userProfileChanged == 1;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred ChangeUserProfileAsync()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }
            // When username is changed check if it does not already exists
            // When email is changed check if it does not already exists
            // Update user profile
            return false;
        }


        /// <summary>
        /// ChangeUserProfileIncludingAreasAsync; Changes specific user profile.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of user executing change.</param>
        /// <param name="userProfileId">UserId of userprofile.</param>
        /// <param name="userProfile">UserProfile containing all data for update.</param>
        /// <returns>bool true/false depending on outcome.</returns>
        public async Task<bool> ChangeUserProfileIncludingAreasAsync(int companyId, int userId, int userProfileId, UserProfile userProfile)
        {
            var output = false;

            try
            {
                bool emailFound = false;
                bool usernameFound = false;
                bool upnFound = false;
                int userProfileChanged = 0;

                //check if email already exists
                emailFound = await CheckEmail(userProfile.Email, userId: userProfile.Id, companyId: companyId);

                //check if username already exists
                usernameFound = await CheckUserName(userProfile.UserName, userProfile.Id, companyId: companyId);

                //check if UPN already exists.
                upnFound = await CheckUPN(userProfile.UPN, userId: userProfile.Id, companyId: companyId);

                if (!emailFound && !usernameFound && !upnFound && userProfileId == userProfile.Id)
                {
                    var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfile.Id);

                    if (!string.IsNullOrEmpty(userProfile.FirstName) && userProfile.FirstName.Length > 250)
                    {
                        userProfile.FirstName = userProfile.FirstName.Substring(0, 249);
                    }

                    if (!string.IsNullOrEmpty(userProfile.LastName) && userProfile.LastName.Length > 250)
                    {
                        userProfile.LastName = userProfile.LastName.Substring(0, 249);
                    }

                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                    parameters.Add(new NpgsqlParameter("@_id", userProfile.Id));

                    parameters.Add(new NpgsqlParameter("@_firstname", userProfile.FirstName));
                    parameters.Add(new NpgsqlParameter("@_lastname", userProfile.LastName));

                    if (string.IsNullOrEmpty(userProfile.Picture))
                    {
                        parameters.Add(new NpgsqlParameter("@_picture", DBNull.Value));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_picture", userProfile.Picture));
                    }

                    parameters.Add(new NpgsqlParameter("@_email", userProfile.Email));
                    parameters.Add(new NpgsqlParameter("@_username", userProfile.UserName));
                    parameters.Add(new NpgsqlParameter("@_role", userProfile.Role));

                    if (string.IsNullOrEmpty(userProfile.UPN))
                    {
                        parameters.Add(new NpgsqlParameter("@_upn", DBNull.Value));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_upn", userProfile.UPN));
                    }

                    if (userProfile.IsTagManager.HasValue)
                        parameters.Add(new NpgsqlParameter("@_istagmanager", userProfile.IsTagManager.Value));

                    userProfileChanged = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "change_userprofile", parameters: parameters);

                    if (userProfile.Id > 0 && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "MARKET_SAP"))
                    {
                        var sapParameters = new List<NpgsqlParameter>();

                        sapParameters.Add(new NpgsqlParameter("@_companyid", companyId));
                        sapParameters.Add(new NpgsqlParameter("@_id", userProfile.Id));
                        sapParameters.Add(new NpgsqlParameter("@_username", userProfile.UserName));

                        if (!string.IsNullOrEmpty(userProfile.SapPmUsername) && userProfile.SapPmUsername.Length > 0 && userProfile.SapPmUsername.Length <= 12)
                        {
                            sapParameters.Add(new NpgsqlParameter("@_sappmusername", userProfile.SapPmUsername));
                        }
                        else if (!string.IsNullOrEmpty(userProfile.FirstName) || !string.IsNullOrEmpty(userProfile.LastName))
                        {
                            var firstNameShort = (userProfile.FirstName?.Length > 3) ? userProfile.FirstName.Substring(0, 3) : (userProfile.FirstName ?? "");
                            var lastNameShort = (userProfile.LastName?.Length > 9) ? userProfile.LastName.Substring(0, 9) : (userProfile.LastName ?? "");
                            var sapUsername = string.Concat(firstNameShort.ToUpperInvariant(), lastNameShort.ToUpperInvariant());
                            //filter out spaces
                            sapUsername = sapUsername.Replace(" ", "");
                            if (!string.IsNullOrEmpty(sapUsername) && sapUsername.Length <= 12)
                            {
                                sapParameters.Add(new NpgsqlParameter("@_sappmusername", sapUsername));
                            }
                            else
                            {
                                sapParameters.Add(new NpgsqlParameter("@_sappmusername", DBNull.Value));
                            }
                        }
                        else
                        {
                            sapParameters.Add(new NpgsqlParameter("@_sappmusername", DBNull.Value));
                        }

                        _ = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_userprofile_sap_information", parameters: sapParameters);

                    }

                    if (userProfileChanged > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfile.Id);
                        await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user.ToString(), objectId: userProfile.Id, userId: userId, companyId: companyId, description: "Changed user profile.");

                    }

                    output = (userProfileChanged == 1);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred ChangeUserProfileAsync()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }

            if (userProfile.AllowedAreas != null)
            {
                await AddOrChangeUserAreas(areas: userProfile.AllowedAreas, profileUserId: userProfileId, userId: userId, companyId: companyId);
            }

            return output;
        }

        /// <summary>
        /// ChangeUserPasswordAsync; Change user password for an specific password. Normally this would be called from a CMS.
        /// There or no specific extra checks within this methods except for isnull and confirmation check. If used all checks (on valid password etc) must be done in the 'calling' logic.
        /// </summary>
        /// <param name="companyId">Current companyId, is used for updating the password.</param>
        /// <param name="userId">UserId of user executing the change. </param>
        /// <param name="userProfileId">UserId of user that is being updated. </param>
        /// <param name="userPassword">User password (new one)</param>
        /// <param name="userPasswordConfirmation">User password configuration (new one)</param>
        /// <returns>true/false depending on outcome</returns>
        public async Task<bool> ChangeUserPasswordAsync(int companyId, int userId, int userProfileId, string userPassword, string userPasswordConfirmation)
        {

            //TODO refactor
            int result = 0;
            // Init authenticator
            Authenticator authenticator = new Authenticator();

            //check the new passwords
            if (!string.IsNullOrEmpty(userPassword) && userPassword == userPasswordConfirmation)
            {

                var newSalt = authenticator.GenerateNewSalt();
                // Hash incoming password with current salt
                var newEncryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, authenticator.GetBytePassword(userPassword), authenticator.GetBytePassword(newSalt), Authenticator.iterations);
                //// Reconstruct full password
                var newGeneratedPassword = authenticator.GeneratePasswordForStorage(newSalt, authenticator.GetBase64PasswordHash(newEncryptedPasswordByte));

                try
                {
                    var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfileId);

                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    parameters.Add(new NpgsqlParameter("@_id", userProfileId));
                    parameters.Add(new NpgsqlParameter("@_password", newGeneratedPassword));

                    result = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_userprofile_password", parameters: parameters);

                    if (result > 0)
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfileId);
                        await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user.ToString(), objectId: userProfileId, userId: userId, companyId: companyId, description: "Changed user profile password.");

                    }

                    return result == 1;
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred in UserManager.ChangeUserPasswordAsync()");

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
                finally
                {

                }

                return true;

            }
            return false;

        }

        /// <summary>
        /// ChangeUserPasswordAsync; Change user password for an specific password. Normally this would be called from a CMS.
        /// There is a check based on the current user password. There are no specific checks for password validation except for confirmation and is null check.
        /// If used all checks (on valid password etc) must be done in the 'calling' logic.
        /// </summary>
        /// <param name="companyId">Current companyId, is used for updating the password.</param>
        /// <param name="userId">UserId of user that executing the cahnge. </param>
        /// <param name="userProfileId">UserId of user that is being updated. </param>
        /// <param name="currentPassword">CurrentPassword of user.</param>
        /// <param name="userPassword">User password (new one)</param>
        /// <param name="userPasswordConfirmation">User password configuration (new one)</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ChangeUserPasswordAsync(int companyId, int userId, int userProfileId, string currentUserPassword, string userPassword, string userPasswordConfirmation)
        {
            //TODO refactor
            int result = 0;
            // Init authenticator
            Authenticator authenticator = new Authenticator();

            //check the new passwords
            if (!string.IsNullOrEmpty(userPassword) && userPassword == userPasswordConfirmation)
            {

                // Get password from database
                var databasePassword = await _userdatamanager.GetUserPasswordByUserId(userProfileId);
                // Get salt password
                var currentSalt = authenticator.GetSaltFromPassword(hashedpassword: databasePassword);
                // Hash incoming password with current salt
                var encryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, authenticator.GetBytePassword(currentUserPassword), authenticator.GetBytePassword(currentSalt), Authenticator.iterations);
                // Reconstruct full password
                var generatedPassword = authenticator.GeneratePasswordForStorage(currentSalt, authenticator.GetBase64PasswordHash(encryptedPasswordByte));

                //check if database password is the same with the given password
                if (generatedPassword == databasePassword)
                {
                    //REPLACE BELOW WITH GenerateEncryptedPassword

                    var newSalt = authenticator.GenerateNewSalt();
                    // Hash incoming password with current salt
                    var newEncryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, authenticator.GetBytePassword(userPassword), authenticator.GetBytePassword(newSalt), Authenticator.iterations);
                    //// Reconstruct full password
                    var newGeneratedPassword = authenticator.GeneratePasswordForStorage(newSalt, authenticator.GetBase64PasswordHash(newEncryptedPasswordByte));

                    try
                    {
                        var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfileId);

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                        parameters.Add(new NpgsqlParameter("@_id", userProfileId));
                        parameters.Add(new NpgsqlParameter("@_password", newGeneratedPassword));

                        result = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_userprofile_password", parameters: parameters);

                        if (result > 0)
                        {
                            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user.ToString(), userProfileId);
                            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user.ToString(), objectId: userProfileId, userId: userId, companyId: companyId, description: "Changed user profile password.");

                        }

                        return result == 1;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred in UserManager.ChangeUserPasswordAsync()");

                        if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                    }
                    finally
                    {
                    }
                    await Task.CompletedTask;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// AddOrChangeExtendedUserProfileDetails; Add or changes extended user details.
        /// </summary>
        /// <param name="companyId">Company id of the user that is modifying the data</param>
        /// <param name="userId">User id of the user that is modifying the data</param>
        /// <param name="userProfileId">The user id of the specific user where the data is being modified,</param>
        /// <param name="details">Details.</param>
        /// <returns></returns>
        public async Task<bool> AddOrChangeExtendedUserProfileDetails(int companyId, int userId, int userProfileId, UserExtendedDetails details)
        {
            try
            {
                string original = string.Empty;
                var currentProfile = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: userProfileId);
                if (currentProfile != null)
                {
                    original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentProfile.Id);
                }

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", details.UserId));
                parameters.Add(new NpgsqlParameter("@_lastmodifiedbyid", userId));
                parameters.Add(new NpgsqlParameter("@_employeeid", details.EmployeeId == null ? DBNull.Value : details.EmployeeId));
                parameters.Add(new NpgsqlParameter("@_employeefunction", details.EmployeeFunction == null ? DBNull.Value : details.EmployeeFunction));
                parameters.Add(new NpgsqlParameter("@_bio", details.Bio == null ? DBNull.Value : details.Bio));
                parameters.Add(new NpgsqlParameter("@_description", details.Description == null ? DBNull.Value : details.Description));

                //save_user_extended_details(_companyid integer, _userid integer, _lastmodifiedbyid int, _employeeid varchar, _employeefunction varchar)
                var result = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "save_user_extended_details", parameters: parameters);

                //result updated rows
                if (result > 0)
                {
                    //set audit trail based on just inserted data.
                    if (currentProfile == null)
                    {
                        //retrieve current profile
                        currentProfile = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: details.UserId);
                        if (currentProfile.Id > 0)
                        {
                            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentProfile.Id);
                            await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), objectId: currentProfile.Id, userId: userId, companyId: companyId, description: "Added extended user profile details.");
                        }
                    }
                    else
                    {
                        currentProfile = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: details.UserId);
                        if (currentProfile.Id > 0)
                        {
                            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentProfile.Id);
                            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), objectId: currentProfile.Id, userId: userId, companyId: companyId, description: "Changed extended user profile details.");
                        }

                    }

                    return true;

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred AddOrChangeExtendedUserProfileDetails()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }

            return false;
        }

        /// <summary>
        /// AddOrChangeUserSettings; Save the user settings (as json) within the Extended User Details. 
        /// NOTE! this method will only save the 'user_settings' column within the profiles_user_extended_details table. 
        /// When a records does not exists for this user, it will be created. 
        /// </summary>
        /// <param name="companyId">Company id of the user that is modifying the data</param>
        /// <param name="userId">User id of the user that is modifying the data</param>
        /// <param name="userProfileId">The user id of the specific user where the data is being modified,</param>
        /// <param name="userSettings">Settings containing settings items (roles etc.)</param>
        /// <returns>true/false</returns>
        public async Task<bool> AddOrChangeUserSettings(int companyId, int userId, int userProfileId, UserSettings userSettings)
        {
            try
            {
                string original = string.Empty;
                var currentExtendedUserProfileDetails = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: userProfileId);
                if (currentExtendedUserProfileDetails != null)
                {
                    original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentExtendedUserProfileDetails.Id);
                }

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userSettings.UserId));
                parameters.Add(new NpgsqlParameter("@_lastmodifiedbyid", userId));
                parameters.Add(new NpgsqlParameter("@_settings", userSettings == null ? DBNull.Value : userSettings.ToJsonFromObject()));

                //save_user_settings(_companyid integer, _userid integer, _lastmodifiedbyid int, _settings varchar)
                var result = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "save_user_settings", parameters: parameters);

                //result updated rows
                if (result > 0)
                {
                    var newlyCreatedExtendedProfileDetails = currentExtendedUserProfileDetails == null;
                    //reload user details for auditing
                    currentExtendedUserProfileDetails = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: userSettings.UserId);
                    if (currentExtendedUserProfileDetails.Id > 0) //id empty? should not happen, but just to make sure. 
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentExtendedUserProfileDetails.Id);
                        await _dataAuditing.WriteDataAudit(original: newlyCreatedExtendedProfileDetails ? string.Empty : original, mutated: mutated, Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), objectId: currentExtendedUserProfileDetails.Id, userId: userId, companyId: companyId, description: "Added/Updated extended user profile details settings.");
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred AddOrChangeUserSettings()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {

            }

            return false;
        }

        /// <summary>
        /// AddOrChangeUserAppPreferences; Save the user app preferences (as json) within the Extended User Details. 
        /// NOTE! this method will only save the 'user_app_preferences' column within the 'profiles_user_extended_details' table. 
        /// When a records does not exist for this user, it will be created. 
        /// </summary>
        /// <param name="companyId">Company id of the user that is modifying the data</param>
        /// <param name="userId">User id of the user that is modifying the data</param>
        /// <param name="userProfileId">The user id of the specific user where the data is being modified,</param>
        /// <param name="userAppPreferences">Object containing app preferences (like BackgroundImage.)</param>
        /// <returns>true/false</returns>
        public async Task<bool> AddOrChangeUserAppPreferences(int companyId, int userId, int userProfileId, UserAppPreferences userAppPreferences)
        {
            try
            {
                string original = string.Empty;
                var currentExtendedUserProfileDetails = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: userProfileId);
                if (currentExtendedUserProfileDetails != null)
                {
                    original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentExtendedUserProfileDetails.Id);
                }

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userProfileId));
                parameters.Add(new NpgsqlParameter("@_lastmodifiedbyid", userId));
                parameters.Add(new NpgsqlParameter("@_preferences", userAppPreferences == null ? DBNull.Value : userAppPreferences.ToJsonFromObject()));

                var result = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "save_user_preferences", parameters: parameters);

                //result updated rows
                if (result > 0)
                {
                    var newlyCreatedExtendedProfileDetails = currentExtendedUserProfileDetails == null;
                    //reload user details for auditing
                    currentExtendedUserProfileDetails = await GetExtendedUserProfileDetails(companyId: companyId, userId: userId, userProfileId: userProfileId);
                    if (currentExtendedUserProfileDetails.Id > 0) //id empty? should not happen, but just to make sure. 
                    {
                        var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), currentExtendedUserProfileDetails.Id);
                        await _dataAuditing.WriteDataAudit(original: newlyCreatedExtendedProfileDetails ? string.Empty : original, mutated: mutated, Models.Enumerations.TableNames.profiles_user_extended_details.ToString(), objectId: currentExtendedUserProfileDetails.Id, userId: userId, companyId: companyId, description: "Added/Updated user profile app preferences.");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred AddOrChangeUserAppPreferences()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {

            }

            return false;
        }

        /// <summary>
        /// GetExtendedUserProfileDetails; Get user extended profile
        /// </summary>
        /// <param name="companyId">Company id of the user that is modifying the data</param>
        /// <param name="userId">User id of the user that is modifying the data</param>
        /// <param name="userProfileId">The user id of the specific user where the data is being modified,</param>
        /// <returns>return UserExtendedDetails containing possible information</returns>
        public async Task<UserExtendedDetails> GetExtendedUserProfileDetails(int companyId, int userId, int userProfileId)
        {

            var output = new UserExtendedDetails();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userProfileId));

                using (dr = await _manager.GetDataReader("get_user_extended_details", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var extendedDetails = new UserExtendedDetails();
                        extendedDetails.Id = Convert.ToInt32(dr["id"]);
                        extendedDetails.UserId = Convert.ToInt32(dr["user_id"]);
                        if(dr["employee_id"] != DBNull.Value)
                        {
                            extendedDetails.EmployeeId = dr["employee_id"].ToString();
                        };
                        if (dr["employee_function"] != DBNull.Value)
                        {
                            extendedDetails.EmployeeFunction = dr["employee_function"].ToString();
                        };
                        if (dr["description"] != DBNull.Value)
                        {
                            extendedDetails.Description = dr["description"].ToString();
                        };
                        if (dr["bio"] != DBNull.Value)
                        {
                            extendedDetails.Bio = dr["bio"].ToString();
                        };
                        output = extendedDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetExtendedUserProfileDetails(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetUserProfileAppPreferences; Get user profile app preferences
        /// </summary>
        /// <param name="companyId">Company id of the user that is retrieving the data</param>
        /// <param name="userId">User id of the user that is retrieving the data (ignored for now)</param>
        /// <param name="userProfileId">The user id of the specific user that the data has to be retrieved for</param>
        /// <returns>return UserAppPreferencesWithMetadata containing possible information</returns>
        public async Task<UserAppPreferencesWithMetadata> GetUserProfileAppPreferences(int companyId, int userId, int userProfileId)
        {
            var output = new UserAppPreferencesWithMetadata();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userProfileId));

                using (dr = await _manager.GetDataReader("get_user_extended_details", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var extendedDetails = new UserAppPreferencesWithMetadata();
                        extendedDetails.Id = Convert.ToInt32(dr["id"]);
                        extendedDetails.UserId = Convert.ToInt32(dr["user_id"]);

                        if (dr.HasColumn("user_app_preferences") && dr["user_app_preferences"] != DBNull.Value)
                        {
                            extendedDetails.UserAppPreferences = dr["user_app_preferences"].ToString().ToObjectFromJson<UserAppPreferences>();
                        }
                        output = extendedDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetUserProfileAppPreferences(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output != null && output.UserAppPreferences != null ? output : null;
        }

        /// <summary>
        /// GetExtendedUserDetailsSettings; Get settings from extended user table.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="userProfileId"></param>
        /// <returns></returns>
        private async Task<UserSettings> GetExtendedUserDetailsSettings(int companyId, int userId)
        {
            var output = new UserSettings();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("get_user_extended_details", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["user_settings"] != DBNull.Value && dr["user_settings"].ToString() != string.Empty)
                        {
                            output = dr["user_settings"].ToString().ToObjectFromJson<UserSettings>();
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetExtendedUserDetailsSettings(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        private async Task<UserProfile> GetRoles(int companyId, int userId, UserProfile userProfile)
        {
            var settings = await GetExtendedUserDetailsSettings(companyId: companyId, userId: userId);
            if(settings != null && settings.Roles != null && settings.Roles.Count > 0)
            {
                if (userProfile.Roles == null) userProfile.Roles = new List<RoleTypeEnum>();
                userProfile.Roles.AddRange(settings.Roles);
            }
            return userProfile;
        }

        /// <summary>
        /// SetUserProfileActiveAsync; Set a user profile active/inactive
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userProfileId">UserProfileId (DB: profile_user.id)</param>
        /// <param name="isActive">Is active, true or false.</param>
        /// <returns>true/false not based on outcome.</returns>
        public async Task<bool> SetUserProfileActiveAsync(int companyId, int userProfileId, bool isActive = true)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", userProfileId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_profile_user_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// GenerateUserPassword; Method for simple password generation. Used for Setup purposes. Will not be used for API output.
        /// NOTE! GenerateUserPassword is NOT to be used for output generation ONLY for setup purposes.
        /// </summary>
        /// <param name="password">Incoming unencrypted password.</param>
        /// <returns>A encrypted password.</returns>
        public async Task<string> GenerateUserPassword(string password)
        {
            Authenticator authenticator = new Authenticator();
            await Task.CompletedTask;
            return authenticator.GenerateEncryptedPassword(unencryptedPassword: password);
        }

        /// <summary>
        /// ResetOrCreateAuthenticationDbToken; Reset the database (django) authentication token for user when logging in.
        /// </summary>
        /// <param name="encryptedPassword">Encrypted password to be used for user validation</param>
        /// <param name="userName">Username where the token must be reset. This will be used as extra validation.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ResetOrCreateAuthenticationDbToken(string userName, string encryptedPassword)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_username", userName));
            parameters.Add(new NpgsqlParameter("@_password", encryptedPassword));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_random_authtoken", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// ResetOrCreateAuthenticationDbTokenIfExpired; Reset the database (django) authentication token for user when logging in .
        /// </summary>
        /// <param name="encryptedPassword">Encrypted password to be used for user validation</param>
        /// <param name="userName">Username where the token must be reset. This will be used as extra validation.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ResetOrCreateAuthenticationDbTokenIfExpired(string userName, string encryptedPassword)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_username", userName));
            parameters.Add(new NpgsqlParameter("@_password", encryptedPassword));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_random_authtoken_if_expired", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// ResetOrCreateAuthenticationDbToken; Reset the database (django) authentication token for user when logging in.
        /// </summary>
        /// <param name="encryptedUserPassword">Encrypted password to be used for user validation</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ResetOrCreateAuthenticationDbTokenByUserName(string userName)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_username", userName));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_random_authtoken_by_username", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// ResetOrCreateAuthenticationDbTokenIfExpired; Reset the database (django) authentication token for user when logging in .
        /// </summary>
        /// <param name="encryptedUserPassword">Encrypted password to be used for user validation</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ResetOrCreateAuthenticationDbTokenIfExpiredByUserName(string userName)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_username", userName));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_random_authtoken_if_expired_by_username", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// GetUserNameByUPN; Get username based on the UPN
        /// </summary>
        /// <param name="upn">UPN to check.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>string containing the username.</returns>
        public async Task<string> GetUserNameByUPN(string upn, int companyId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_upn", upn));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var output = Convert.ToString(await _manager.ExecuteScalarAsync("get_username_by_upn", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure, connectionKind: connectionKind));
            return output;
        }

        /// <summary>
        /// GetUserIdByUPN; Get User Id based on upn.
        /// </summary>
        /// <param name="upn">Upn to check.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>UserId.</returns>
        public async Task<int> GetUserIdByUPN(string upn, int companyId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_upn", upn));
            parameters.Add(new NpgsqlParameter("@_companyId", companyId));

            var output = Convert.ToInt32(await _manager.ExecuteScalarAsync("get_userid_by_upn", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure, connectionKind: connectionKind));
            return output;
        }

        /// <summary>
        /// GetCompanyIdByUPN; Get companyId based on UPN.
        /// </summary>
        /// <param name="upn">Upn to check.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>CompanyId</returns>
        public async Task<int> GetCompanyIdByUPN(string upn, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_upn", upn));

            var output = Convert.ToInt32(await _manager.ExecuteScalarAsync("get_companyid_by_upn", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure, connectionKind: connectionKind));
            return output;
        }

        /// <summary>
        /// GetTenantByCompanyId; Get tenentid (MSAL) for validation.
        /// </summary>
        /// <param name="upn">Upn to check.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Tenent Key</returns>
        public async Task<string> GetTenantByCompanyId(int companyId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var output = Convert.ToString(await _manager.ExecuteScalarAsync("get_msal_tenant_by_companyid", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure, connectionKind: connectionKind));
            return output;
        }
        #endregion

        #region - private methods Filters -
        /// <summary>
        /// FilterUserProfiles; FilterUserProfiles is the primary filter method for filtering UserProfiles. Within this method the specific filters are determined based on the supplied UserFilters object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">UserFilters, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered UserProfiles objects.</param>
        /// <returns>A filtered list of UserProfiles objects.</returns>
        private async Task<IList<UserProfile>> FilterUserProfiles(int companyId, UserFilters filters, IList<UserProfile> nonFilteredCollection)
        {
            var filtered = nonFilteredCollection;
            if (filters.IsStaff.HasValue)
            {
                filtered = await FilterUserProfilesOnIsStaff(isStaff: filters.IsStaff.Value, userProfiles: filtered);
            }
            if (filters.IsSuperUser.HasValue)
            {
                filtered = await FilterUserProfilesOnIsSuperUser(isSuperUser: filters.IsSuperUser.Value, userProfiles: filtered);
            }
            if (filters.RoleType.HasValue)
            {
                filtered = await FilterUserProfilesOnRole(role: filters.RoleType.Value, userProfiles: filtered);
            }
            return filtered;
        }

        /// <summary>
        /// FilterUserProfilesOnIsStaff; Filter a UserPRofiles collection on IsStaff.
        /// </summary>
        /// <param name="isStaff">;</param>
        /// <param name="userProfiles">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<UserProfile>> FilterUserProfilesOnIsStaff(bool isStaff, IList<UserProfile> userProfiles)
        {
            userProfiles = userProfiles.Where(x => x.IsStaff == isStaff).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return userProfiles;
        }

        /// <summary>
        /// FilterActionsOnIsSuperUser; Filter a UserPRofiles collection on IsSuperUser.
        /// </summary>
        /// <param name="isSuperUser">;</param>
        /// <param name="userProfiles">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<UserProfile>> FilterUserProfilesOnIsSuperUser(bool isSuperUser, IList<UserProfile> userProfiles)
        {
            userProfiles = userProfiles.Where(x => x.IsSuperUser == isSuperUser).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return userProfiles;
        }

        /// <summary>
        /// FilterAuditTemplatesOnRole; Filter a UserProfile collection on role.
        /// </summary>
        /// <param name="role">RoleTypeEnum, roles are stored as a string in the database. Internally we use a enumerator to represent those stings.</param>
        /// <param name="userProfiles">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<UserProfile>> FilterUserProfilesOnRole(RoleTypeEnum role, IList<UserProfile> userProfiles)
        {
            userProfiles = userProfiles.Where(x => x.Role == role.ToString().ToLower()).ToList();
            await Task.CompletedTask;
            return userProfiles;
        }
        #endregion

        #region - private methods -
        /// <summary>
        /// GetAreasWithProfile; Gets a list of AreaBasic objects for use with profile
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>A list of AreaBasic objects.</returns>
        public async Task<List<AreaBasic>> GetAllowedAreasWithProfile(int companyId, int userId)
        {
            var areasWithUser = new List<AreaBasic>();
            var areasWithCompany = await GetAreasWithCompanyAsync(companyId: companyId, maxLevel: 100);
            List<int> areaIdsWithProfile = await GetAllowedAreaIdsWithUserAsync(companyId: companyId, userId: userId);
            if(areasWithCompany != null && areasWithCompany.Any() && areaIdsWithProfile != null && areaIdsWithProfile.Any())
            {
                foreach (var item in areasWithCompany.Where(x => areaIdsWithProfile.Contains(x.Id)))
                {
                    //TODO add checks
                    //TODO move to extension
                    //TODO add namepath
                    areasWithUser.Add(new AreaBasic() { Id = item.Id, Name = item.Name, ParentId = item.ParentId });
                }
            }

            return areasWithUser;
        }

        /// <summary>
        /// GetAreasWithCompanyAsyncl Get areas with companies (for use only in this manager, can not use area manager due to reference loop.).
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="maxLevel">Max level deep</param>
        /// <returns>List of areas</returns>
        private async Task<List<Area>> GetAreasWithCompanyAsync(int companyId, int maxLevel)
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
                        var area = new Area();
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

                        output.Add(area);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetAreasWithCompanyAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetAllowedAreasWithProfiles; Get the allowed areas with Profiles. This call is meant for Management purposes, for use with include parameter with users overview.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="users">List of users</param>
        /// <returns>Return list of users with appended information.</returns>
        private async Task<List<UserProfile>> GetAllowedAreasWithProfiles(int companyId, List<UserProfile> users)
        {
            if(users != null && users.Count > 0)
            {
                var userRelationAreas = await GetUserAreaRelationsWithUsersAsync(companyId: companyId);
                if (userRelationAreas!=null && userRelationAreas.Count > 0)
                {
                    foreach(var user in users)
                    {
                        user.AllowedAreas = userRelationAreas.Where(x => x.UserId == user.Id).Select(y => y.ToBasicArea()).ToList();
                    }
                }
            }

            return users;
        }

        /// <summary>
        /// GetAllowedAreaIdsWithUser; Get a list of areas ids that are allowed for a certain user. Will be used for further processing.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId, (DB: profiles_user.id)</param>
        /// <returns>A list of ids.</returns>
        private async Task<List<int>> GetAllowedAreaIdsWithUserAsync(int companyId, int userId)
        {
            //TODO replace with IUserAccessManager
            var ids = new List<int>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("get_allowedareaids_by_user", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetAllowedAreaIdsWithUser(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ids;
        }

        /// <summary>
        /// GetDisplayAreaIdsWithUserAsync; Get a list of areas ids that are displayed in the CMS for a certain user. Will be used for further processing.
        /// NOTE! this methods is used because of a legacy construction in the database that uses 2 tables, one for display (profiles_user_areas) and one for the security (profiles_user_allowedareas);
        /// This needs to be merge somehow based on children/parent structure of areas; Current structures needs a lot of logic and overhead to work properly and the profiles_user_areas is a mess and does
        /// not have consistant data (parents are added, but sometimes one or more of the children are also located in the database, but in the allowed areas the parent and all children are used, which would be logical and
        /// in the current structure only the parent should be in the display areas part of the logic)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId, (DB: profiles_user.id)</param>
        /// <returns>A list of ids.</returns>
        private async Task<List<int>> GetAllowedDisplayAreaIdsWithUserAsync(int companyId, int userId)
        {
            //TODO refactor and merge with other ids methods for user;
            var ids = new List<int>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("get_alloweddisplayareaids_by_user", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetDisplayAreaIdsWithUserAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ids;
        }

        /// <summary>
        /// GetUserAreaRelationsWithUsersAsync; Get UserAreaRelation with all users from a company.
        /// </summary>
        /// <param name="companyId">CompanyId</param>
        /// <returns>A list of UserRelationsAreas</returns>
        private async Task<List<UserRelationArea>> GetUserAreaRelationsWithUsersAsync(int companyId)
        {
            var output = new List<UserRelationArea>();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                string sp = "get_user_areas";

                using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var area = CreateOrFillUserAreaRelationFromReader(dr);
                        output.Add(area);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreasAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// GetCompanyWithProfile; Get basic company information based on the companyId;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A single CompanyBasic item.</returns>
        public async Task<CompanyBasic> GetCompanyWithProfile(int companyId)
        {
            CompanyBasic company = new CompanyBasic();
            var foundCompany = await GetCompanyAsync(companyId: companyId, getCompanyId: companyId);
            if (foundCompany != null && foundCompany.Id > 0)
            {
                //TODO make extension
                company.Id = foundCompany.Id;
                company.Name = foundCompany.Name;
                company.Picture = foundCompany.Picture;

                //get holding id
                int possibleHoldingId = await GetCompanyHoldingIdAsync(company.Id);
                company.HoldingId = possibleHoldingId > 0 ? possibleHoldingId : null;
            }

            return (company.Id > 0 ? company : null);
        }

        /// <summary>
        /// GetCompanyAsync; Get company based on the CompanyId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="getCompanyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A Company, depending on include parameter this will also contains a Shift collection.</returns>
        private async Task<Company> GetCompanyAsync(int companyId, int getCompanyId)
        {
            var company = new Company();
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", getCompanyId));

                using (dr = await _manager.GetDataReader("get_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        company.Id = Convert.ToInt32(dr["id"]);
                        if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
                        {
                            company.Description = dr["description"].ToString();
                        }
                        if (dr["manager_id"] != DBNull.Value)
                        {
                            company.ManagerId = Convert.ToInt32(dr["manager_id"]);
                        }
                        company.Name = dr["name"].ToString();
                        if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
                        {
                            company.Picture = dr["picture"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyUserManager.GetCompanyAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return company;
        }

        /// <summary>
        /// GetCompanyHoldingIdAsync; Get holding id with company. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <returns>holdingid</returns>
        private async Task<int> GetCompanyHoldingIdAsync(int companyId)
        {
            if (companyId > 0)
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                var possibleHoldingId = await _manager.ExecuteScalarAsync("get_company_holding_id", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return (possibleHoldingId != null ? Convert.ToInt32(possibleHoldingId) : 0);
            }
            return 0;
        }

        /// <summary>
        /// CreateOrFillUserProfileFromReader; creates and fills a UserProfile object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="userprofile">User profile, if not supplied will be created.</param>
        /// <returns>Filled in user profile.</returns>
        private UserProfile CreateOrFillUserProfileFromReader(NpgsqlDataReader dr, UserProfile userprofile = null)
        {
            if (userprofile == null) userprofile = new UserProfile();

            userprofile.Id = Convert.ToInt32(dr["id"]);
            if (dr["email"] != DBNull.Value && !string.IsNullOrEmpty(dr["email"].ToString()))
            {
                userprofile.Email = dr["email"].ToString();
            }
            userprofile.FirstName = dr["first_name"].ToString();
            userprofile.LastName = dr["last_name"].ToString();
            userprofile.IsStaff = Convert.ToBoolean(dr["is_staff"]);
            userprofile.IsSuperUser = Convert.ToBoolean(dr["is_superuser"]);
            userprofile.Role = dr["role"].ToString();

            if (dr.HasColumn("is_service_account") && dr["is_service_account"] != DBNull.Value)
            {
                userprofile.IsServiceAccount = Convert.ToBoolean(dr["is_service_account"]);
            }
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                userprofile.Picture = dr["picture"].ToString();
            }
            if (dr.HasColumn("upn") && dr["upn"] != DBNull.Value && !string.IsNullOrEmpty(dr["upn"].ToString()))
            {
                userprofile.UPN = dr["upn"].ToString();
            }
            if (dr.HasColumn("username") && dr["username"] != DBNull.Value && !string.IsNullOrEmpty(dr["username"].ToString()))
            {
                userprofile.UserName = dr["username"].ToString();
            }
            if(dr.HasColumn("successor_id") && dr["successor_id"] != DBNull.Value)
            {
                userprofile.SuccessorId = Convert.ToInt32(dr["successor_id"]);
            }

            if (dr.HasColumn("modified_at") && dr["modified_at"] != DBNull.Value)
            {
                userprofile.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            if (dr.HasColumn("is_tag_manager") && dr["is_tag_manager"] != DBNull.Value)
            {
                userprofile.IsTagManager = Convert.ToBoolean(dr["is_tag_manager"]);
            }

            if (dr.HasColumn("guid") && dr["guid"] != DBNull.Value)
            {
                userprofile.UserGUID = dr["guid"].ToString();
            }

            if (dr.HasColumn("company_language_culture") && dr["company_language_culture"] != DBNull.Value)
            {
                userprofile.CompanyLanguageCulture = dr["company_language_culture"].ToString();
            }

            if (dr.HasColumn("company_timezone") && dr["company_timezone"] != DBNull.Value)
            {
                userprofile.CompanyTimezone = dr["company_timezone"].ToString();
            }

            if (dr.HasColumn("sap_pm_username") && dr["sap_pm_username"] != DBNull.Value)
            {
                userprofile.SapPmUsername = dr["sap_pm_username"].ToString();
            }

            if (userprofile.Roles == null) userprofile.Roles = new List<RoleTypeEnum>();

            if (userprofile.Role == "basic")
            {
                userprofile.Roles.Add(RoleTypeEnum.Basic);
            }
            else if (userprofile.Role == "manager")
            {
                userprofile.Roles.Add(RoleTypeEnum.Manager);
            }
            else if (userprofile.Role == "shift_leader")
            {
                userprofile.Roles.Add(RoleTypeEnum.ShiftLeader);
            }

            if (userprofile.IsTagManager.HasValue && userprofile.IsTagManager.Value)
            {
                userprofile.Roles.Add(RoleTypeEnum.TagManager);
            }

            if (userprofile.IsServiceAccount)
            {
                userprofile.Roles.Add(RoleTypeEnum.ServiceAccount);
            }

            //Disabled, change if needed.
            //if (string.IsNullOrEmpty(userprofile.Picture))
            //{
            //    userprofile.Picture = ImageHelper.GetImage((int)DefaultImageTypeEnum.General);
            //}

            return userprofile;
        }

        private UserBasic CreateOrFillUserBasicFromReader(NpgsqlDataReader dr, UserBasic userBasic = null)
        {
            userBasic ??= new UserBasic();

            userBasic.Id = Convert.ToInt32(dr["id"]);
            userBasic.Name = dr["name"].ToString();

            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                userBasic.Picture = dr["picture"].ToString();
            }

            return userBasic;
        }

            /// <summary>
            /// CheckUserName; Check if username exists, if so return true else false.
            /// </summary>
            /// <param name="userName">Username to be checked.</param>
            /// <param name="userId">UserId (profiles_user.id)</param>
            /// <param name="companyId">CompanyId (companies_company.id)</param>
            /// <returns>true/false depending on outcome, if something went wrong true is returned to make sure logic will continue and we will not insert double user names. </returns>
            public async Task<bool> CheckUserName(string userName, int? userId = null, int? companyId = null)
        {
            try
            {

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                if (companyId.HasValue) parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (userId.HasValue)  parameters.Add(new NpgsqlParameter("@_id", userId));
                parameters.Add(new NpgsqlParameter("@_username", userName));

                return (int)(await _manager.ExecuteScalarAsync("check_username", parameters)) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred CheckUserName()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }
            return true; //something went wrong, return true.
        }

        /// <summary>
        /// CheckEmail; Check if email address exists
        /// </summary>
        /// <param name="email">Email address to be checked</param>
        /// <param name="userId">UserId (profiles_user.id)</param>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <returns>true/false depending on outcome, if something went wrong true is returned to make sure logic will continue and we will not insert double user names. </returns>
        public async Task<bool> CheckEmail(string email, int? userId = null, int? companyId = null)
        {
            if (string.IsNullOrEmpty(email)) return false; //no email supplied, empty email will not be checked.

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                if (companyId.HasValue) parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (userId.HasValue) parameters.Add(new NpgsqlParameter("@_id", userId));
                parameters.Add(new NpgsqlParameter("@_email", email));

                return (int)(await _manager.ExecuteScalarAsync("check_email", parameters)) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred CheckEmail()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
            }
            return true; //something went wrong, return true.
        }


        /// <summary>
        /// CheckUPN; UPN must be checked for uniqueness.
        /// </summary>
        /// <param name="upn">UPN as supplied by management tooling</param>
        /// <param name="userId">UserId (if supplied) of user where UPN can be ignored (user is probably beeing updated).</param>
        /// <param name="companyId">CompanyId (DB: companyID)</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> CheckUPN(string upn, int? userId = null, int? companyId = null)
        {
            if (string.IsNullOrEmpty(upn)) return false; //no upn supplied, empty email will not be checked.

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                if (companyId.HasValue) parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (userId.HasValue) parameters.Add(new NpgsqlParameter("@_id", userId));
                parameters.Add(new NpgsqlParameter("@_upn", upn));

                return (int)(await _manager.ExecuteScalarAsync("check_upn", parameters)) > 0;

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred CheckUPN()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {

            }
            return true; //something went wrong, return true.
        }

        /// <summary>
        /// SetLastLoggedInDate; Sets the last logged-in date for user.
        /// NOTE! method should only be used after logged-in user. Not for general UPDATE usage.
        /// </summary>
        /// <param name="userId">Current User Id</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SetLastLoggedInDate(int userId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", userId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_user_lastloggedindate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return (rowseffected > 0);
        }

        /// <summary>
        /// CreateOrFillUserAreaRelationFromReader; create of fill user area relation from dr.
        /// </summary>
        /// <param name="dr">DataRecords containing data.</param>
        /// <param name="area">UserRelationArea item that needs to be filled, if not supplied it will be created.</param>
        /// <returns>UserRelationArea object</returns>
        private UserRelationArea CreateOrFillUserAreaRelationFromReader(NpgsqlDataReader dr, UserRelationArea area = null)
        {
            if (area == null) area = new UserRelationArea();

            area.AreaId = Convert.ToInt32(dr["id"]);
            if (dr.HasColumn("area_name_full") && dr["area_name_full"] != DBNull.Value)
            {
                area.AreaNamePath = dr["area_name_full"].ToString();
            }
            area.AreaName = dr["name"].ToString();
            area.UserId =  Convert.ToInt32(dr["user_id"]);

            return area;
        }

        /// <summary>
        /// SetSuccessor; Set succor number id.
        /// </summary>
        /// <param name="userId">UserId of user</param>
        /// <param name="companyId">Company of user.</param>
        /// <param name="successorid">Successor (user id)</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SetSuccessor(int companyid, int userid, int successorid)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_userid", userid));
            parameters.Add(new NpgsqlParameter("@_successorid", successorid));
            parameters.Add(new NpgsqlParameter("@_companyid", companyid));
            // parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_user_successor", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }
        #endregion

        #region - add/change areas with users -
        /// <summary>
        /// AddOrChangeUserAreas; Add or changes the areas that have a relation with the user;
        /// Two area collections will possibly be changed. The profile areas and the profile allowed areas.
        /// </summary>
        /// <param name="areas">Areas that were selected</param>
        /// <param name="profileUserId">UserId of user of the changed user profile</param>
        /// <param name="userId">UserId of user executing the change</param>
        /// <param name="companyId">Company of user.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddOrChangeUserAreas(List<AreaBasic> areas, int profileUserId, int userId, int companyId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_areas.ToString(), Models.Enumerations.TableFields.user_id.ToString(), profileUserId);

            var currentAreas = await GetAllowedDisplayAreaIdsWithUserAsync(companyId: companyId, userId: profileUserId);
            var removedAreas = currentAreas.Where(x => !areas.Select(y => y.Id).ToList().Contains(x));

            //remove all areas that are not in the supplied collection anymore.
            foreach(var removedId in removedAreas)
            {
                await RemoveAreaFromUser(areaId: removedId, companyId: companyId, profileUserId: profileUserId);
            }

            //add all areas to the profile areas that are not already added;
            foreach(var area in areas.Where(x => !currentAreas.Contains(x.Id)))
            {
                await AddAreaToFromUser(areaId: area.Id, companyId: companyId, profileUserId: profileUserId);
            }

            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_areas.ToString(), Models.Enumerations.TableFields.user_id.ToString(), profileUserId);
            await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user_areas.ToString(), objectId: profileUserId, userId: userId, companyId: companyId, description: "Changed user area relation collection.");


            await GenerateAllAllowedAreasAndAddToUser(companyId: companyId, userId: userId, profileUserId: profileUserId);

            return true;
        }

        /// <summary>
        /// RemoveAreaFromUser; Removes an area from a users. (based on the profile_user_areas)
        /// </summary>
        /// <param name="areaId">AreaId to be removed;</param>
        /// <param name="companyId">CompanyId of the user</param>
        /// <param name="profileUserId">UserId where the area needs to be removed.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> RemoveAreaFromUser(int areaId, int companyId, int profileUserId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_userid", profileUserId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));
           // parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_profile_user_areas", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// RemoveAreaFromUser; Adds an area from a users. (based on the profile_user_areas)
        /// </summary>
        /// <param name="areaId">AreaId to be added;</param>
        /// <param name="companyId">CompanyId of the user</param>
        /// <param name="profileUserId">UserId where the area needs to be added.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddAreaToFromUser(int areaId, int companyId, int profileUserId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_userid", profileUserId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_profile_user_areas", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowseffected > 0;
        }

        /// <summary>
        /// GenerateAllAllowedAreasAndAddToUser; Generated all allowed areas based on the profile user areas. This will automatically remove and add areas if needed.
        /// </summary>
        /// <param name="companyId">CompanyId of the user</param>
        /// <param name="userId">UserId where the areas are being added.</param>
        /// <returns>true/false depending on outcome. Note! could return 0 updates if everything stays the same.</returns>
        private async Task<bool> GenerateAllAllowedAreasAndAddToUser(int companyId, int userId, int profileUserId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_allowed_areas.ToString(), Models.Enumerations.TableFields.user_id.ToString(), profileUserId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_userid", profileUserId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_allowed_areas", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.profiles_user_allowed_areas.ToString(), Models.Enumerations.TableFields.user_id.ToString(), profileUserId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.profiles_user_allowed_areas.ToString(), objectId: profileUserId, userId: userId, companyId: companyId, description: "Changed user allowed area relation collection.");

            }

            return rowseffected > 0;
        }

        /// <summary>
        ///GetDisplayAreasWithUser; Get display areas.
        /// NOTE! this construction needs to change uses a lot of code and overhead to create and table should be fased out and only the allowed area structure should be used.
        /// </summary>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns></returns>
        private async Task<List<AreaBasic>> GetDisplayAreasWithUser(int userId, int companyId)
        {
            var output = new List<AreaBasic>();

            var areasWithCompany = await GetAreasWithCompanyAsync(companyId: companyId, maxLevel: 100);
            List<int> displayAreaIdsWithProfile = await GetAllowedDisplayAreaIdsWithUserAsync(companyId: companyId, userId: userId);

            foreach (var item in areasWithCompany.Where(x => displayAreaIdsWithProfile.Contains(x.Id)))
            {
                output.Add(new AreaBasic() { Id = item.Id, Name = item.Name, ParentId = item.ParentId });
            }

            return output;
        }

        /// <summary>
        /// GenerateRandomPasswordl generate a new random password.
        /// </summary>
        /// <returns></returns>
        private string GenerateRandomPassword()
        {
            Authenticator authenticator = new Authenticator();
            var userPassword = DateTime.Now.ToString();
            var newSalt = authenticator.GenerateNewSalt();
            // Hash incoming password with current salt
            var newEncryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, authenticator.GetBytePassword(userPassword), authenticator.GetBytePassword(newSalt), Authenticator.iterations);
            //// Reconstruct full password
            var newGeneratedPassword = authenticator.GeneratePasswordForStorage(newSalt, authenticator.GetBase64PasswordHash(newEncryptedPasswordByte));

            return newGeneratedPassword;
        }

        #endregion

        #region - logging / specific security settings -

        /// <summary>
        /// AddGenerationLogEvent; Login security logger. Logs a record to the logging_security tables.
        /// </summary>
        /// <param name="eventId">CErtain event number.</param>
        /// <param name="message">Message to be posted</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> AddLoginSecurityLogEvent(string message, string description, int eventId = 0, string type = "INFORMATION", string source = null)
        {

            if (_configurationHelper.GetValueAsBool(ApiSettings.ENABLE_DB_LOG_CONFIG_KEY))
            {
                try
                {
                    if (string.IsNullOrEmpty(source)) source = _configurationHelper.GetValueAsString(ApiSettings.APPLICATION_NAME_CONFIG_KEY);

                    var parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventId.ToString()));
                    parameters.Add(new NpgsqlParameter("@_eventname", string.Empty));

                    if (string.IsNullOrEmpty(source))
                    {
                        parameters.Add(new NpgsqlParameter("@_source", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_source", source));
                    }
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    var output = await _manager.ExecuteScalarAsync("add_log_security", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    // Error occurred within the logging. Log it to normal logger.
                    _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.AddSecurityLogEvent(): ", ex.Message));
                }
                finally
                {

                }
            }
            return true;
        }
        #endregion

        #region - security authentication settings -


        #endregion

        #region - cleaners -
        /// <summary>
        /// PrepareAndCleanUserProfile; Checks a user profile, and prepares it for saving.
        /// Will remove incorrect data.
        /// </summary>
        /// <param name="userprofile">UserProfile ot check</param>
        /// <returns>Cleaned object.</returns>
        private UserProfile PrepareAndCleanUserProfile(UserProfile userprofile)
        {
            if (!string.IsNullOrEmpty(userprofile.Picture))
            {
                if(userprofile.Picture.Length > 100)
                {
                    userprofile.Picture = "";
                }
            }
            return userprofile;
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
