using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Users;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class UserStandingManager : BaseManager<UserStandingManager>, IUserStandingManager
    {
        #region - private(s) -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IAreaManager _areaManager;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor(s) -
        public UserStandingManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, IAreaManager areaManager, IDataAuditing dataAuditing, ILogger<UserStandingManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _areaManager = areaManager;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - publics User groups -
        public async Task<List<UserGroup>> GetUserGroupsAsync(int companyId)
        {
            //get user_groups 
            var output = new List<UserGroup>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_usergroups", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var userGroup = CreateOrFillUserGroupFromReader(dr);
                        output.Add(userGroup);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserGroupsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            //get profiles_user?

            return output;
        }

        public async Task<UserGroup> GetUserGroupAsync(int companyId, int userGroupId)
        {
            NpgsqlDataReader dr = null;
            var output = new UserGroup();

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", userGroupId));

                using (dr = await _manager.GetDataReader("get_usergroup", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillUserGroupFromReader(dr, output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserGroupAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<int> AddUserGroupAsync(int companyId, int userId, UserGroup userGroup)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_name", userGroup.Name));
            parameters.Add(new NpgsqlParameter("@_group_type", (int)userGroup.GroupType));

            if (!string.IsNullOrEmpty(userGroup.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", userGroup.Description));
            }

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_usergroup", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_groups.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.user_groups.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added user group.");
            }
            return possibleId;
        }

        public async Task<int> ChangeUserGroupAsync(int companyId, int userId, int userGroupId, UserGroup userGroup)
        {
            if (userGroup == null)
                return -1;
            else
                userGroup.Id = userGroupId;
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_groups.ToString(), userGroupId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", userGroupId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_name", userGroup.Name));
            parameters.Add(new NpgsqlParameter("@_group_type", (int)userGroup.GroupType));

            if (!string.IsNullOrEmpty(userGroup.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", userGroup.Description));
            }

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_usergroup", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_groups.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.user_groups.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Changed user group.");
            }
            return possibleId;
        }

        public async Task<bool> AddUserToUserGroup(int userProfileId, int userGroupId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_userid", userProfileId));
            parameters.Add(new NpgsqlParameter("@_user_group_id", userGroupId));

            var possibleId = await _manager.ExecuteScalarAsync("add_user_to_usergroup", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            if (possibleId == null)
            {
                return false;
            }
            else
            {
                return Convert.ToInt32(possibleId) > 0;
            }
        }

        public async Task<bool> RemoveUserFromUserGroup(int id, int userProfileId, int userGroupId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", id));
            parameters.Add(new NpgsqlParameter("@_userid", userProfileId));
            parameters.Add(new NpgsqlParameter("@_usergroupid", userGroupId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_user_from_usergroup", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return possibleId > 0;
        }

        public async Task<bool> SetUserGroupActiveAsync(int companyId, int userId, int userGroupId, bool isActive)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_groups.ToString(), userGroupId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", userGroupId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_usergroup_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_groups.ToString(), userGroupId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.user_groups.ToString(), objectId: userGroupId, userId: userId, companyId: companyId, description: "Changed user group active state.");
            }

            return (rowseffected > 0);
        }

        #endregion

        #region - publics User skills -
        public async Task<List<UserSkill>> GetUserSkills(int companyId)
        {
            //get user_skills
            var output = new List<UserSkill>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_userskills", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var userSkill = CreateOrFillUserSkillFromReader(dr);
                        output.Add(userSkill);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserSkillsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            //get profiles_user?

            return output;
        }

        public async Task<UserSkill> GetUserSkill(int companyId, int userSkillId)
        {
            //get user_skills
            var userSkill = new UserSkill();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", userSkillId));

                using (dr = await _manager.GetDataReader("get_userskill", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillUserSkillFromReader(dr, userSkill);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserSkillAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            //get profiles_user?

            return userSkill;
        }

        public async Task<int> AddUserSkill(int companyId, int userId, UserSkill userSkill)
        {
            //add user skill
            //mandatory: companyid, skill_type, assessment_template_id, is_active = true
            //optional name, description, goal, expiry_in_days, valid_from, valid_to, etc.
            var parameters = GetNpgsqlParametersFromUserSkill(companyId, userId, userSkill, isUpdate: false);
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_userskill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skills.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.user_skills.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added user skill.");
            }
            return possibleId;
        }

        public async Task<int> ChangeUserSkill(int companyId, int userId, int userSkillId, UserSkill userSkill)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skills.ToString(), userSkillId);

            var parameters = GetNpgsqlParametersFromUserSkill(companyId, userId, userSkill, isUpdate: true);
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_userskill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skills.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.user_skills.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Changed user skill.");
            }
            return possibleId;
        }

        public async Task<bool> SetUserSkillActiveAsync(int companyId, int userId, int userSkillId, bool isActive)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skills.ToString(), userSkillId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", userSkillId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_userskill_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skills.ToString(), userSkillId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.user_skills.ToString(), objectId: userSkillId, userId: userId, companyId: companyId, description: "Changed user skill active state.");
            }

            return (rowseffected > 0);
        }

        public async Task<bool> RemoveUserSkillRelationsAsync(int userSkillId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_userskillid", userSkillId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_matrix_user_skill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return (rowseffected > 0);
        }

        #endregion

        #region - publics User skill values -
        public async Task<List<UserSkillValue>> GetUserSkillValues(int companyId, int userId, int? limit, int? offset = 0)
        {
            //get user_skills
            var output = new List<UserSkillValue>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_limit", limit));
                parameters.Add(new NpgsqlParameter("@_offset", offset));

                using (dr = await _manager.GetDataReader("get_userskillvalues", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var userSkillValue = CreateOrFillUserSkillValueFromReader(dr);
                        output.Add(userSkillValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserSkillValues(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            //get profiles_user?

            return output;
        }

        public async Task<UserSkillValue> GetUserSkillValue(int companyId, int id)
        {
            var userSkillValue = new UserSkillValue();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", id)); 

                using (dr = await _manager.GetDataReader("get_userskillvalue_byid", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillUserSkillValueFromReader(dr, userSkillValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserSkillValue(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return userSkillValue;
        }

        public async Task<UserSkillValue> GetUserSkillValue(int companyId, int userSkillId, int userId)
        {
            var userSkillValue = new UserSkillValue();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter(@"_companyid", companyId));
                parameters.Add(new NpgsqlParameter(@"_user_id", userId));
                parameters.Add(new NpgsqlParameter(@"_user_skill_id", userSkillId));

                using (dr = await _manager.GetDataReader("get_userskillvalue_byuserskill", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillUserSkillValueFromReader(dr, userSkillValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserStandingManager.GetUserSkillValue(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return userSkillValue;
        }

        public async Task<int> AddUserSkillValue(int companyId, int userId, UserSkillValue userSkillValue)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter(@"_companyid", companyId));
            parameters.Add(new NpgsqlParameter(@"_user_skill_id", userSkillValue.UserSkillId));
            parameters.Add(new NpgsqlParameter(@"_user_id", userSkillValue.UserId));
            parameters.Add(new NpgsqlParameter(@"_score", userSkillValue.Score));
            parameters.Add(new NpgsqlParameter(@"_score_type", (object)0));
            parameters.Add(new NpgsqlParameter(@"_value_date", new DateTime(userSkillValue.ValueDate.Ticks)));
            parameters.Add(new NpgsqlParameter(@"_user_id_creating", userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_userskillvalue", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skill_uservalues.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.user_skill_uservalues.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added user skill value.");
            }
            return possibleId;
        }

        public async Task<int> ChangeUserSkillValueById(int companyId, int userId, UserSkillValue userSkillValue)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skill_uservalues.ToString(), userSkillValue.Id);
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter(@"_id", userSkillValue.Id));

            parameters.Add(new NpgsqlParameter(@"_companyid", companyId));
            parameters.Add(new NpgsqlParameter(@"_score", userSkillValue.Score));
            parameters.Add(new NpgsqlParameter(@"_value_date", new DateTime(userSkillValue.ValueDate.Ticks)));
            parameters.Add(new NpgsqlParameter(@"_user_skill_id", userSkillValue.UserSkillId));
            parameters.Add(new NpgsqlParameter(@"_user_id", userSkillValue.UserId));
            parameters.Add(new NpgsqlParameter(@"_user_id_modified", userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_userskillvalue_byid", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skill_uservalues.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.user_skill_uservalues.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Changed user skill value by id.");
            }
            return possibleId;
        }

        public async Task<int> ChangeUserSkillValueByUserSkill(int companyId, int userId, UserSkillValue userSkillValue)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter(@"_user_id", userSkillValue.UserId));
            parameters.Add(new NpgsqlParameter(@"_user_skill_id", userSkillValue.UserSkillId));

            parameters.Add(new NpgsqlParameter(@"_companyid", companyId));
            parameters.Add(new NpgsqlParameter(@"_score", userSkillValue.Score));
            parameters.Add(new NpgsqlParameter(@"_value_date", new DateTime(userSkillValue.ValueDate.Ticks)));
            parameters.Add(new NpgsqlParameter(@"_user_id_modified", userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_userskillvalue_byuserskill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.user_skill_uservalues.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: "unknown", mutated: mutated, Models.Enumerations.TableNames.user_skill_uservalues.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Changed user skill value by user skill.");
            }
            return possibleId;
        }

        #endregion

        #region - privates UserGroups -
        private UserGroup CreateOrFillUserGroupFromReader(NpgsqlDataReader dr, UserGroup userGroup = null)
        {
            if (userGroup == null) userGroup = new UserGroup();

            /*
            public int Id { get; set; } id
            public string Name { get; set; } name
            public string Description { get; set; } description
            public GroupTypeEnum GroupType { get; set; } group_type
            public List<int> UserIds { get; set; } user_group_users user_group_id user_id inner join on user_group_id
            public List<UserProfile> Users { get; set; } get from profiles_user ??? get user ids ask jordan
            */

            userGroup.Id = Convert.ToInt32(dr["id"]);
            userGroup.Name = Convert.ToString(dr["name"]);

            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(Convert.ToString(dr["description"])))
            {
                userGroup.Description = Convert.ToString(dr["description"]);
            }

            if (Enum.TryParse(dr["group_type"].ToString(), out GroupTypeEnum groupType) &&
                Enum.IsDefined(typeof(GroupTypeEnum), groupType))
            {
                userGroup.GroupType = groupType;
            }

            if (dr.HasColumn("in_use") && dr["in_use"] != DBNull.Value)
            {
                userGroup.InUseInMatrix = Convert.ToBoolean(dr["in_use"]);
            }

            return userGroup;
        }
        #endregion

        #region - privates UserSkills -
        private UserSkill CreateOrFillUserSkillFromReader(NpgsqlDataReader dr, UserSkill userSkill = null)
        {
            if (userSkill == null) { userSkill = new UserSkill(); }
            /*
            public int? SkillAssessmentId { get; set; } assessment_template_id 
            public string Name { get; set; } name 
            public string Description { get; set; } description 
            public int Goal { get; set; } goal
            //public int Result { get; set; } hoeft niet
            //public int GoalResultDifference { get; set; } hoeft niet
            public SkillTypeEnum SkillType { get; set; } skill_type
            public int? ExpiryInDays { get; set; } expiry_in_days
            public int? NotificationWindowInDays { get; set; } in test database notification_window_in_days
            public DateTime? ValidFrom { get; set; } valid_from
            public DateTime? ValidTo { get; set; } valid_to
            //public List<UserSkillValue> Values { get; set; } user_skill_uservalues table hoeft niet
            */
            if (userSkill == null) userSkill = new UserSkill();

            userSkill.Id = Convert.ToInt32(dr["id"]);

            if (dr["assessment_template_id"] != DBNull.Value)
            {
                userSkill.SkillAssessmentId = Convert.ToInt32(dr["assessment_template_id"]);
            }



            userSkill.Name = Convert.ToString(dr["name"]);

            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(Convert.ToString(dr["description"])))
            {
                userSkill.Description = Convert.ToString(dr["description"]);
            }

            if (dr["goal"] != DBNull.Value)
            {
                userSkill.Goal = Convert.ToInt32(dr["goal"]);
            }

            if (Enum.TryParse(dr["skill_type"].ToString(), out SkillTypeEnum skillType) &&
                Enum.IsDefined(typeof(SkillTypeEnum), skillType))
            {
                userSkill.SkillType = skillType;
            }

            if (dr.HasColumn("notification_window_in_days") && dr["notification_window_in_days"] != DBNull.Value)
            {
                userSkill.NotificationWindowInDays = Convert.ToInt32(dr["notification_window_in_days"]);
            }

            if (dr["expiry_in_days"] != DBNull.Value)
            {
                userSkill.ExpiryInDays = Convert.ToInt32(dr["expiry_in_days"]);
            }


            if (dr["valid_from"] != DBNull.Value)
            {
                userSkill.ValidFrom = Convert.ToDateTime(dr["valid_from"]);
            }
            if (dr["valid_to"] != DBNull.Value)
            {
                userSkill.ValidTo = Convert.ToDateTime(dr["valid_to"]);
            }

            if (dr["default_target"] != DBNull.Value)
            {
                userSkill.DefaultTarget = Convert.ToInt32(dr["default_target"]);
            }

            if (dr.HasColumn("in_use") && dr["in_use"] != DBNull.Value)
            {
                userSkill.InUseInMatrix = Convert.ToBoolean(dr["in_use"]);
            }

            return userSkill;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromUserSkill(int companyId, int userId, UserSkill userSkill, bool isUpdate = false)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (isUpdate)
                parameters.Add(new NpgsqlParameter("@_id", userSkill.Id));

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_skill_type", (int)userSkill.SkillType));
            if (userSkill.SkillAssessmentId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_assessment_template_id", userSkill.SkillAssessmentId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_assessment_template_id", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_userid", userId));

            parameters.Add(new NpgsqlParameter("@_name", userSkill.Name));

            if (userSkill.ValidFrom != null && userSkill.ValidTo != null)
            {
                parameters.Add(new NpgsqlParameter("@_valid_from", new DateTime(userSkill.ValidFrom.Value.Ticks)));
                parameters.Add(new NpgsqlParameter("@_valid_to", new DateTime(userSkill.ValidTo.Value.Ticks)));
            }
            else if (userSkill.ExpiryInDays != null && userSkill.ExpiryInDays != 0)
            {
                parameters.Add(new NpgsqlParameter("@_valid_from", DateTime.Now));
                parameters.Add(new NpgsqlParameter("@_valid_to", DateTime.Now.AddDays(userSkill.ExpiryInDays.Value)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_valid_from", DBNull.Value));
                parameters.Add(new NpgsqlParameter("@_valid_to", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(userSkill.Description))
                parameters.Add(new NpgsqlParameter("@_description", userSkill.Description));
            else
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));

            if (userSkill.Goal != 0)
                parameters.Add(new NpgsqlParameter("@_goal", userSkill.Goal));
            else
                parameters.Add(new NpgsqlParameter("@_goal", DBNull.Value));

            if (userSkill.DefaultTarget != 0)
                parameters.Add(new NpgsqlParameter("@_default_target", userSkill.DefaultTarget));
            else
                parameters.Add(new NpgsqlParameter("@_default_target", DBNull.Value));

            if (userSkill.ExpiryInDays != 0)
                parameters.Add(new NpgsqlParameter("@_expiry_in_days", userSkill.ExpiryInDays));
            else
                parameters.Add(new NpgsqlParameter("@_expiry_in_days", DBNull.Value));

            if (userSkill.NotificationWindowInDays != 0)
                parameters.Add(new NpgsqlParameter("@_notification_window_in_days", userSkill.NotificationWindowInDays));
            else
                parameters.Add(new NpgsqlParameter("@_notification_window_in_days", DBNull.Value));

            return parameters;
        }

        #endregion

        #region - privates UserSkillValues -
        private UserSkillValue CreateOrFillUserSkillValueFromReader(NpgsqlDataReader dr, UserSkillValue userSkillValue = null)
        {
            if (userSkillValue == null) { userSkillValue = new UserSkillValue(); }
            /*
            public int Id { get; set; } id
            public int UserSkillId { get; set; } user_skill_id
            public int UserId { get; set; } user_id
            public int Score { get; set; } score
            public bool IsDynamic { get; set; } //e.g. score from skill assessment, else forced input. //idk
            public List<string> Attachments { get; set; }//idk
            public DateTime ValueDate { get; set; } value_date
            public DateTime? CreatedAt { get; set; } created_at
            public DateTime? ModifiedAt { get; set; } modified_at
            */

            userSkillValue.Id = Convert.ToInt32(dr["id"]);
            userSkillValue.UserSkillId = Convert.ToInt32(dr["user_skill_id"]);
            userSkillValue.UserId = Convert.ToInt32(dr["user_id"]);
            userSkillValue.Score = Convert.ToDecimal(dr["score"]);
            userSkillValue.ValueDate = Convert.ToDateTime(dr["value_date"]);
            userSkillValue.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            userSkillValue.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            if (dr.HasColumn("value_expiration_date") && dr["value_expiration_date"] != DBNull.Value)
            {
                userSkillValue.ValueExpirationDate = Convert.ToDateTime(dr["value_expiration_date"]);
            }

            return userSkillValue;
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
