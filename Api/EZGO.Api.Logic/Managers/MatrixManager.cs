using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Data;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class MatrixManager : BaseManager<MatrixManager>, IMatrixManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IUserStandingManager _userStandingManager;
        private readonly IAreaManager _areaManager;
        private readonly IGeneralManager _generalManager;

        // Resource ID for Skill Matrix Legend Options setting
        private const int SKILL_MATRIX_LEGEND_OPTIONS_RESOURCE_ID = 134;
        #endregion

        #region - constructor(s) -
        public MatrixManager(IDatabaseAccessHelper manager, ITaskManager taskManager, IAreaManager areaManager, IConfigurationHelper configurationHelper, IDataAuditing dataAuditing, IUserStandingManager userStandingManager, IGeneralManager generalManager, ILogger<MatrixManager> logger) : base(logger)
        {
            _manager = manager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;
            _userStandingManager = userStandingManager;
            _areaManager = areaManager;
            _generalManager = generalManager;
        }
        #endregion

        #region - matrices basic -
        public async Task<List<SkillsMatrix>> GetMatricesAsync(int companyId, int? userId = null, MatrixFilters? filters = null, string include = null)
        {
            var output = new List<SkillsMatrix>();

            NpgsqlDataReader dr = null;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (filters.HasValue)
                {
                    if (filters.Value.ModifiedById != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_modified_by_id", filters.Value.ModifiedById));
                    }
                    if (filters.Value.CreatedById != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_created_by_id", filters.Value.CreatedById));
                    }

                    if (filters.Value.Limit != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit));
                    }
                    if (filters.Value.Offset != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset));
                    }
                    if (filters.Value.AreaId != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId));
                    }
                }
                using (dr = await _manager.GetDataReader("get_matrices", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var skillsMatrix = CreateOrFillSkillsMatrixSummaryFromReader(dr); //to be implemented

                        output.Add(skillsMatrix);
                    }
                }

                output = await AppendAreaPathsToMatricesAsync(companyId: companyId, matrices: output, addAreaPath: true, addAreaPathIds: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatricesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<SkillsMatrix> GetMatrixAsync(int companyId, int userId, int matrixId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            //TODO replace with SP not using collection (performance) and or refactor code and make more efficient. 
            var matrix = (await GetMatricesAsync(companyId: companyId, userId: userId)).Where(x => x.Id == matrixId).FirstOrDefault();

            if (matrix != null && matrix.Id > 0)
            {

                matrix.UserGroups = await GetMatrixUserGroupsAsync(companyId: companyId, userId: userId, matrixId: matrixId);
                var skills = await GetMatrixUserSkillsAsync(companyId: companyId, userId: userId, matrixId: matrixId); //retrieve all skills with matrix
                var users = await GetMatrixUsersAsync(companyId: companyId, userId: userId, matrixId: matrixId); //retrieve all users with matrix
                var userValues = await GetMatrixUserSkillValuesAsync(companyId: companyId, userId: userId, matrixId: matrixId); //retrieve all user values (based on user value table) with matrix

                //set all skill values (user values) for each skill
                foreach (var skill in skills)
                {
                    var possibleValues = userValues.Where(x => x.UserSkillId == skill.UserSkillId).ToList();
                    skill.Values = possibleValues;
                }

                matrix.MandatorySkills = skills.Where(x => x.SkillType == SkillTypeEnum.Mandatory)?.OrderBy(x => x.Name).ToList(); //create a list of only mandatory skills
                matrix.OperationalSkills = skills.Where(x => x.SkillType == SkillTypeEnum.Operational)?.OrderBy(x => x.Name).ToList(); //create a list of only operational skills

                //set all skills for a specific user (note! all skill values are included even possible older ones)
                foreach (var user in users)
                {
                    user.SkillValues = userValues.Where(x => x.UserId == user.UserProfileId).ToList();
                }

                //set all users for a specific group. 
                foreach (var group in matrix.UserGroups)
                {
                    var foundUsers = users.Where(x => x.GroupId == group.UserGroupId).ToList();
                    group.Users = foundUsers;
                }

                //get all operational behaviours
                //matrix.OperationalBehaviours = await GetMatrixOperationalBehaviour(companyId: companyId, matrixId: matrixId);

                //get company totals
                //matrix.MatrixTotals = await GetMatrixTotals(companyId: companyId, matrixId: matrixId);

                //check certain collections, if null add as empty for further processing. 
                if (matrix.MandatorySkills == null) { matrix.MandatorySkills = new List<SkillsMatrixItem>(); }
                if (matrix.OperationalSkills == null) { matrix.OperationalSkills = new List<SkillsMatrixItem>(); }
                if (matrix.UserGroups == null) { matrix.UserGroups = new List<SkillsMatrixUserGroup>(); }

            }

            return matrix;
        }

        public async Task<List<SkillsMatrixBehaviourItem>> GetMatrixOperationalBehaviour(int companyId, int matrixId)
        {
            var behaviour = new List<SkillsMatrixBehaviourItem>();
            var behaviourValues = new List<SkillsMatrixBehaviourItemValue>();
            var currentItemUid = "";
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));


                using (dr = await _manager.GetDataReader("get_matrix_statistics", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {


                        //D.user_id, D.nr::int4,  D.minval::int4, D.maxval::int4, D.technical_guid::varchar, D.name::varchar, D.description::varchar 

                        var currentValue = new SkillsMatrixBehaviourItemValue();
                        currentValue.ScoreOrNumber = Convert.ToInt32(dr["nr"]);
                        currentValue.TechnicalUid = dr["technical_guid"].ToString();
                        currentValue.UserId = Convert.ToInt32(dr["user_id"]);

                        behaviourValues.Add(currentValue);

                        if (currentItemUid != dr["name"].ToString())
                        {
                            var currentBehaviour = new SkillsMatrixBehaviourItem();
                            currentBehaviour.Name = dr["name"].ToString();
                            currentBehaviour.Description = dr["name"].ToString();
                            currentBehaviour.TechnicalUid = dr["technical_guid"].ToString();
                            currentBehaviour.MaxValue = Convert.ToInt32(dr["maxval"]);
                            currentBehaviour.MinValue = Convert.ToInt32(dr["minval"]);

                            behaviour.Add(currentBehaviour);
                        }

                        currentItemUid = dr["name"].ToString();

                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixUserGroupsAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (behaviour.Any() && behaviourValues.Any())
            {
                foreach (var beh in behaviour)
                {
                    beh.Values = behaviourValues.Where(x => x.TechnicalUid == beh.TechnicalUid).ToList();
                }
            }

            return behaviour;


        }

        public async Task<List<SkillsMatrixBehaviourItem>> GetMatrixTotals(int companyId, int matrixId)
        {
            var behaviour = new List<SkillsMatrixBehaviourItem>();
            var behaviourValues = new List<SkillsMatrixBehaviourItemValue>();
            var currentItemUid = "";
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));


                using (dr = await _manager.GetDataReader("get_matrix_totals", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {


                        //D.user_id, D.nr::int4,  D.minval::int4, D.maxval::int4, D.technical_guid::varchar, D.name::varchar, D.description::varchar 

                        var currentValue = new SkillsMatrixBehaviourItemValue();
                        currentValue.ScoreOrNumber = Convert.ToInt32(dr["nr"]);
                        currentValue.TechnicalUid = dr["technical_guid"].ToString();

                        behaviourValues.Add(currentValue);

                        if (currentItemUid != dr["name"].ToString())
                        {
                            var currentBehaviour = new SkillsMatrixBehaviourItem();
                            currentBehaviour.Name = dr["name"].ToString();
                            currentBehaviour.Description = dr["name"].ToString();
                            currentBehaviour.TechnicalUid = dr["technical_guid"].ToString();

                            behaviour.Add(currentBehaviour);
                        }

                        currentItemUid = dr["name"].ToString();

                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixTotalsAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (behaviour.Any() && behaviourValues.Any())
            {
                foreach (var beh in behaviour)
                {
                    beh.Values = behaviourValues.Where(x => x.TechnicalUid == beh.TechnicalUid).ToList();
                }
            }

            return behaviour;


        }

        public async Task<int> AddMatrixAsync(int companyId, int userId, SkillsMatrix matrix)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_name", matrix.Name));

            if (matrix.Description == null)
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_description", matrix.Description));
            }

            if (matrix.AreaId != null)
            {
                parameters.Add(new NpgsqlParameter("@_areaid", matrix.AreaId));
            }
            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_matrix", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrices.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.matrices.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Added matrix.");
            }
            return id;
        }

        //Always needs to be called with when adding/changing anything on a matrix. 
        public async Task<bool> ChangeMatrixAsync(int companyId, int userId, int matrixId, SkillsMatrix matrix)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrices.ToString(), matrixId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", matrix.Id));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_name", matrix.Name));
            parameters.Add(new NpgsqlParameter("@_description", matrix.Description));
            if (matrix.AreaId != null)
            {
                parameters.Add(new NpgsqlParameter("@_areaid", matrix.AreaId));
            }

            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_matrix", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (id > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrices.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.matrices.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Changed matrix.");
            }

            return id > 0;
        }

        public async Task<bool> SetMatrixActiveAsync(int companyId, int userId, int matrixId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrices.ToString(), matrixId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", matrixId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_matrix_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrices.ToString(), matrixId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.matrices.ToString(), objectId: matrixId, userId: userId, companyId: companyId, description: "Changed matrix active state.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - groups -
        public async Task<int> AddMatrixUserGroupRelationAsync(int companyId, int userId, int matrixId, MatrixRelationUserGroup matrixRelationUserGroup)
        {
            //get_matrix_usergroups and if count is > 0 return -1
            var matrixUserGroups = await GetMatrixUserGroupsAsync(companyId, userId, matrixId);
            if (matrixUserGroups != null && matrixUserGroups.Count > 0 && matrixRelationUserGroup != null && matrixUserGroups.Where(m => m.UserGroupId == matrixRelationUserGroup.UserGroupId).Count() > 0)
            {
                return -1;
            }

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));
            parameters.Add(new NpgsqlParameter("@_usergroupid", matrixRelationUserGroup.UserGroupId));
            parameters.Add(new NpgsqlParameter("@_index", matrixRelationUserGroup.Index));
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_matrix_user_group", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrix_user_groups.ToString(), matrixId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.matrix_user_groups.ToString(), objectId: matrixId, userId: userId, companyId: companyId, description: "Added matrix user group.");
            }
            return possibleId;
        }

        //TODO Add data auditing? How? Unknown id
        public async Task<bool> ChangeMatrixUserGroupRelationAsync(int companyId, int userId, int matrixId, int matrixRelationUserGroupId, MatrixRelationUserGroup matrixRelationUserGroup)
        {

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", matrixRelationUserGroup.Id));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));
            parameters.Add(new NpgsqlParameter("@_usergroupid", matrixRelationUserGroup.UserGroupId));
            parameters.Add(new NpgsqlParameter("@_index", matrixRelationUserGroup.Index));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_matrix_user_group", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return (rowseffected > 0);
        }

        public async Task<List<SkillsMatrixUserGroup>> GetMatrixUserGroupsAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var userGroups = new List<SkillsMatrixUserGroup>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));


                using (dr = await _manager.GetDataReader("get_matrix_usergroups", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var userGroup = CreateOrFillMatrixUserGroupFromReader(dr);
                        userGroups.Add(userGroup);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixUserGroupsAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return userGroups;
        }


        public async Task<SkillsMatrixUserGroup> GetMatrixUserGroupAsync(int companyId, int userId, int matrixId, int id, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var userGroup = new SkillsMatrixUserGroup();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixusergroupid", id));


                using (dr = await _manager.GetDataReader("get_matrix_usergroup", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        userGroup = CreateOrFillMatrixUserGroupFromReader(dr, userGroup: userGroup);

                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixUserGroupAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return userGroup;
        }

        public async Task<int> AddMatrixUserGroupAsync(int companyId, int userId, int matrixId, SkillsMatrixUserGroup matrixUserGroup)
        {
            var possibleGroupId = await _userStandingManager.AddUserGroupAsync(companyId: companyId,
                                                          userId: userId,
                                                          userGroup: matrixUserGroup.ToUserGroup());

            if (possibleGroupId > 0 && matrixId > 0)
            {
                var relation = new MatrixRelationUserGroup() { Index = 0, MatrixId = matrixId, UserGroupId = possibleGroupId };

                var possibleMatrixGroupId = await AddMatrixUserGroupRelationAsync(companyId: companyId, userId: userId, matrixId: matrixId, matrixRelationUserGroup: relation);

                return possibleMatrixGroupId;
            }

            return -1;
        }

        public async Task<int> ChangeMatrixUserGroupAsync(int companyId, int userId, int matrixId, SkillsMatrixUserGroup matrixUserGroup)
        {
            var matrixGroups = await GetMatrixUserGroupsAsync(companyId: companyId, userId: userId, matrixId: matrixId);
            var possibleGroup = matrixGroups.Where(x => x.UserGroupId == matrixUserGroup.UserGroupId).FirstOrDefault();

            var result = await _userStandingManager.ChangeUserGroupAsync(companyId: companyId,
                                                         userId: userId,
                                                         userGroupId: matrixUserGroup.UserGroupId,
                                                         userGroup: matrixUserGroup.ToUserGroup());

            if (matrixUserGroup.Id > 0 || possibleGroup.Id > 0) //check if relation already exists or is supplied, if so update; else insert a new relation.
            {
                var relation = new MatrixRelationUserGroup() { Index = 0, MatrixId = matrixId, UserGroupId = matrixUserGroup.Id > 0 ? matrixUserGroup.Id : possibleGroup.Id };
                await ChangeMatrixUserGroupRelationAsync(companyId: companyId, userId: userId, matrixId: matrixId, matrixRelationUserGroupId: relation.Id, matrixRelationUserGroup: relation);
                return relation.Id;
            }
            else
            {
                var newRelation = new MatrixRelationUserGroup() { Index = 0, MatrixId = matrixId, UserGroupId = matrixUserGroup.UserGroupId };
                var possibleMatrixGroupId = await AddMatrixUserGroupRelationAsync(companyId: companyId, userId: userId, matrixId: matrixId, matrixRelationUserGroup: newRelation);
                return possibleMatrixGroupId;
            }
        }

        //TODO Add data auditing? How? Unknown id
        public async Task<bool> RemoveMatrixUserGroupAsync(int companyId, int userId, int matrixId, MatrixRelationUserGroup matrixRelationUserGroup)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", matrixRelationUserGroup.Id));
            parameters.Add(new NpgsqlParameter("@_usergroupid", matrixRelationUserGroup.UserGroupId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_matrix_user_group", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return (rowseffected > 0);
        }


        #endregion

        #region - matrix users -

        public async Task<List<SkillsMatrixUser>> GetMatrixUsersAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var users = new List<SkillsMatrixUser>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));


                using (dr = await _manager.GetDataReader("get_matrix_users", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var user = CreateOrFillMatrixUserFromReader(dr);
                        users.Add(user);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixUsers(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return users;
        }


        public async Task<List<SkillsMatrixItemValue>> GetMatrixUserSkillValuesAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var userValues = new List<SkillsMatrixItemValue>();
            var skills = await GetMatrixUserSkillsAsync(companyId: companyId, userId: userId, matrixId: matrixId); //retrieve all skills with matrix
            var operationalSkills = skills.Where(x => x.SkillType == SkillTypeEnum.Operational)?.OrderBy(x => x.Name).ToList(); //create a list of only operational skills

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));

                using (dr = await _manager.GetDataReader("get_matrix_skill_uservalues", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var userValue = CreateOrFillMatrixUserValueFromReader(dr);
                        userValues.Add(userValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixUserSkillValues(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            foreach (var userValue in userValues)
            {
                var userSkill = skills.Where(skill => skill.SkillType == SkillTypeEnum.Mandatory && skill.UserSkillId == userValue.UserSkillId).FirstOrDefault();
                if (userSkill != null)
                {
                    //2 when now is smaller than expiry date and now is smaller than expiry date - notification window in days (ok)
                    //1 when now is bigger than expiry date (not ok)
                    //5 when now is smaller than expiry date (warning)
                    //0 when now is smaller than certification date

                    if (userValue.ValueDate > DateTime.UtcNow)
                    {
                        userValue.Score = 0;
                        continue;
                    }
                    else if(userSkill.ExpiryInDays == null)
                    {
                        userValue.Score = 2;
                        continue;
                    }
                    else if (userSkill.ExpiryInDays != null && userValue.ValueDate.Year != 1)
                    {
                        var expiryDate = userValue.ValueDate.AddDays(userSkill.ExpiryInDays.Value);
                        if (DateTime.UtcNow >= expiryDate)
                        {
                            userValue.Score = 1;
                            continue;
                        }
                        else if (userSkill.NotificationWindowInDays != null)
                        {
                            var notificationDate = expiryDate.AddDays(-userSkill.NotificationWindowInDays.Value);
                            if (DateTime.UtcNow >= notificationDate)
                            {
                                userValue.Score = 5;
                                continue;
                            }
                        }
                        userValue.Score = 2;
                    }
                }
            }
            return userValues;
        }
        #endregion

        #region - skills -
        public async Task<int> AddMatrixUserSkillRelationAsync(int companyId, int userId, int matrixId, MatrixRelationUserSkill matrixRelationUserSkill)
        {
            //get_matrix_userskills and if count is > 0 where id is  return -1
            var matrixUserSkills = await GetMatrixUserSkillsAsync(companyId, userId, matrixId);

            if (matrixUserSkills != null && matrixUserSkills.Count > 0 && matrixRelationUserSkill != null && matrixUserSkills.Where(m => m.UserSkillId == matrixRelationUserSkill.UserSkillId).Count() > 0)
            {
                return -1;
            }

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));
            parameters.Add(new NpgsqlParameter("@_userskillid", matrixRelationUserSkill.UserSkillId));
            parameters.Add(new NpgsqlParameter("@_index", matrixRelationUserSkill.Index));
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_matrix_user_skill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.matrix_user_skills.ToString(), matrixId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.matrix_user_skills.ToString(), objectId: matrixId, userId: userId, companyId: companyId, description: "Added matrix user skill. (objectId of matrix)");
            }
            return possibleId;
        }


        public async Task<bool> ChangeMatrixUserSkillRelationAsync(int companyId, int userId, int matrixId, int matrixRelationUserSkillId, MatrixRelationUserSkill matrixRelationUserSkill)
        {
            var original = await _manager.GetDataRowAsJson(TableNames.matrix_user_skills.ToString(), fieldName: TableFields.user_skill_id.ToString(), id: matrixRelationUserSkill.UserSkillId, fieldname2: TableFields.matrix_id.ToString(), id2: matrixId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", matrixRelationUserSkill.Id));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));
            parameters.Add(new NpgsqlParameter("@_userskillid", matrixRelationUserSkill.UserSkillId));
            parameters.Add(new NpgsqlParameter("@_index", matrixRelationUserSkill.Index));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_matrix_user_skill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(TableNames.matrix_user_skills.ToString(), fieldName: TableFields.user_skill_id.ToString(), id: matrixRelationUserSkill.UserSkillId, fieldname2: TableFields.matrix_id.ToString(), id2: matrixId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, TableNames.matrix_user_skills.ToString(), objectId: matrixId, userId: userId, companyId: companyId, description: "Changed matrix user skill relation. (objectId of matrix)");
            }

            return (rowseffected > 0);
        }


        public async Task<bool> RemoveMatrixUserSkillRelationAsync(int companyId, int userId, int matrixId, int matrixRelationUserSkillId, MatrixRelationUserSkill matrixRelationUserSkill)
        {
            var original = await _manager.GetDataRowAsJson(TableNames.matrix_user_skills.ToString(), fieldName: TableFields.user_skill_id.ToString(), id: matrixRelationUserSkill.UserSkillId, fieldname2: TableFields.matrix_id.ToString(), id2: matrixId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_id", matrixRelationUserSkill.Id));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));
            parameters.Add(new NpgsqlParameter("@_userskillid", matrixRelationUserSkill.UserSkillId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_matrix_user_skill", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(TableNames.matrix_user_skills.ToString(), fieldName: TableFields.user_skill_id.ToString(), id: matrixRelationUserSkill.UserSkillId, fieldname2: TableFields.matrix_id.ToString(), id2: matrixId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, TableNames.matrix_user_skills.ToString(), objectId: matrixId, userId: userId, companyId: companyId, description: "Removed user skill from matrix. (objectId of matrix)");
            }

            return (rowseffected > 0);
        }

        public async Task<List<SkillsMatrixItem>> GetMatrixUserSkillsAsync(int companyId, int userId, int matrixId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var items = new List<SkillsMatrixItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_matrixid", matrixId));

                using (dr = await _manager.GetDataReader("get_matrix_skillitems", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = CreateOrFillMatrixSkillItemFromReader(dr);
                        items.Add(item);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetMatrixUserSkills(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return items;
        }

        public async Task<int> AddMatrixUserSkillAsync(int companyId, int userId, int matrixId, SkillsMatrixItem matrixUserSkill)
        {

            var possibleSkillId = await _userStandingManager.AddUserSkill(companyId: companyId,
                                                          userId: userId,
                                                          userSkill: matrixUserSkill.ToUserSkill());

            if (possibleSkillId > 0 && matrixId > 0)
            {
                var relation = new MatrixRelationUserSkill() { Index = 0, MatrixId = matrixId, UserSkillId = possibleSkillId };

                var possibleMatrixUserSkillId = await AddMatrixUserSkillRelationAsync(companyId: companyId, userId: userId, matrixId: matrixId, matrixRelationUserSkill: relation);
                return possibleMatrixUserSkillId;
            }

            return -1;
        }
        #endregion

        #region - values -
        //TODO Add data auditing? How? Unknown id
        public async Task<bool> SaveMatrixUserSkillValue(int companyId, int userId, int matrixId, SkillsMatrixItemValue matrixItemValue)
        {
            var original = await _manager.GetDataRowAsJson(TableNames.user_skill_uservalues.ToString(), fieldName: TableFields.user_skill_id.ToString(), id: matrixItemValue.UserSkillId, fieldname2: TableFields.user_id.ToString(), id2: matrixItemValue.UserId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_userskillid", matrixItemValue.UserSkillId),
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_userprofileid", matrixItemValue.UserId),
                new NpgsqlParameter("@_score", matrixItemValue.Score),
                new NpgsqlParameter("@_valuedate", new DateTime(matrixItemValue.ValueDate.Ticks)),
                new NpgsqlParameter("@_userid", userId),
                new NpgsqlParameter("@_scoringmethod", matrixItemValue.IsDynamic ? ScoringMethodEnum.Assessment.ToString().ToLower() : ScoringMethodEnum.Manual.ToString().ToLower())
            };
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("save_userskillvalue", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(TableNames.user_skill_uservalues.ToString(), fieldName: TableFields.user_skill_id.ToString(), id: matrixItemValue.UserSkillId, fieldname2: TableFields.user_id.ToString(), id2: matrixItemValue.UserId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, TableNames.user_skill_uservalues.ToString(), objectId: matrixItemValue.UserSkillId, userId: userId, companyId: companyId, description: "Saved matrix user skill value.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - private methods SkillsMatrix - 
        private SkillsMatrix CreateOrFillSkillsMatrixSummaryFromReader(NpgsqlDataReader dr)
        {
            var skillsMatrix = new SkillsMatrix();
            skillsMatrix.Id = Convert.ToInt32(dr["id"]);
            skillsMatrix.CompanyId = Convert.ToInt32(dr["company_id"]);

            if (dr["name"] != DBNull.Value && !string.IsNullOrEmpty(dr["name"].ToString()))
            {
                skillsMatrix.Name = dr["name"].ToString();
            }

            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                skillsMatrix.Description = dr["description"].ToString();
            }

            if (dr["created_at"] != DBNull.Value)
            {
                skillsMatrix.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }

            if (dr["modified_at"] != DBNull.Value)
            {
                skillsMatrix.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            if (dr["area_id"] != DBNull.Value)
            {
                skillsMatrix.AreaId = Convert.ToInt32(dr["area_id"]);
            }

            skillsMatrix.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            skillsMatrix.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);

            skillsMatrix.CreatedBy = Convert.ToString(dr["created_by"]);
            skillsMatrix.ModifiedBy = Convert.ToString(dr["modified_by"]);

            skillsMatrix.NumberOfUserGroups = Convert.ToInt32(dr["nr_of_usergroups"]);
            skillsMatrix.NumberOfMandatorySkills = Convert.ToInt32(dr["nr_of_mandatory_skills"]);
            skillsMatrix.NumberOfOperationalSkills = Convert.ToInt32(dr["nr_of_operational_skills"]);



            return skillsMatrix;
        }
        private async Task<SkillsMatrix> AppendAreaPathsToMatrixAsync(int companyId, SkillsMatrix matrix, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {

                var area = areas?.Where(x => x.Id == matrix.AreaId)?.FirstOrDefault();
                if (area != null)
                {
                    if (addAreaPath) matrix.AreaPath = area.FullDisplayName;
                    if (addAreaPathIds) matrix.AreaPathIds = area.FullDisplayIds;
                }

            }
            return matrix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="matrices"></param>
        /// <param name="addAreaPath"></param>
        /// <param name="addAreaPathIds"></param>
        /// <returns></returns>
        private async Task<List<SkillsMatrix>> AppendAreaPathsToMatricesAsync(int companyId, List<SkillsMatrix> matrices, bool addAreaPath = true, bool addAreaPathIds = false)
        {
            if (matrices != null && matrices.Any())
            {
                var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 9999, useTreeview: false);
                if (areas != null && areas.Count > 0)
                {

                    foreach (var matrix in matrices)
                    {
                        var area = areas?.Where(x => x.Id == matrix.AreaId)?.FirstOrDefault();
                        if (area != null)
                        {
                            if (addAreaPath) matrix.AreaPath = area.FullDisplayName;
                            if (addAreaPathIds) matrix.AreaPathIds = area.FullDisplayIds;
                        }
                    }
                }
            }

            return matrices;
        }
        #endregion

        #region - private matrix user groups & users -
        private SkillsMatrixUser CreateOrFillMatrixUserFromReader(NpgsqlDataReader dr, SkillsMatrixUser user = null, string include = null)
        {
            if (user == null) user = new SkillsMatrixUser();

            //"id" int4, "company_id" int4, "first_name" varchar, "last_name" varchar, "picture" varchar, "role" varchar, "group_id" int4
            user.Id = Convert.ToInt32(dr["id"]);

            if (dr["first_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["first_name"].ToString())) { user.FirstName = dr["first_name"].ToString(); }
            if (dr["last_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["last_name"].ToString())) { user.LastName = dr["last_name"].ToString(); }
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString())) { user.Picture = dr["picture"].ToString(); }

            user.UserProfileId = Convert.ToInt32(dr["user_profile_id"]);
            user.GroupId = Convert.ToInt32(dr["group_id"]);

            if (dr["role"] != DBNull.Value)
            {
                if (Enum.TryParse(dr["role"].ToString(), out RoleTypeEnum roleType) &&
                    Enum.IsDefined(typeof(RoleTypeEnum), roleType))
                {
                    user.Role = roleType;
                }
            }

            return user;

        }

        private SkillsMatrixUserGroup CreateOrFillMatrixUserGroupFromReader(NpgsqlDataReader dr, SkillsMatrixUserGroup userGroup = null, string include = null)
        {
            if (userGroup == null) userGroup = new SkillsMatrixUserGroup();

            //"id" int4, "company_id" int4, "name" varchar, "description" text
            userGroup.Id = Convert.ToInt32(dr["id"]);
            userGroup.UserGroupId = Convert.ToInt32(dr["user_group_id"]);
            userGroup.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && string.IsNullOrEmpty(dr["description"].ToString()))
            {
                userGroup.Description = dr["description"].ToString();
            }
            return userGroup;

        }

        private SkillsMatrixItem CreateOrFillMatrixSkillItemFromReader(NpgsqlDataReader dr, SkillsMatrixItem item = null, string include = null)
        {
            if (item == null) item = new SkillsMatrixItem();

            //"id" int4, "company_id" int4, "name" varchar, "description" text, "skill_type" int4, "goal" int4, "expiry_in_days" int4, "valid_from" timestamptz, "valid_to" timestamptz, "notification_window_in_days" int4, "assessment_template_id" int4, "assessment_template_name" varchar
            if (dr["description"] != DBNull.Value && string.IsNullOrEmpty(dr["description"].ToString()))
            {
                item.Description = dr["description"].ToString();
            }
            if (dr["expiry_in_days"] != DBNull.Value)
            {
                item.ExpiryInDays = Convert.ToInt32(dr["expiry_in_days"]);
            }

            if (dr["goal"] != DBNull.Value)
            {
                item.Goal = Convert.ToInt32(dr["goal"]); //probably wrong place, move to matrix table not user skill table
            }

            item.Id = Convert.ToInt32(dr["id"]);
            item.Name = dr["name"].ToString();
            item.UserSkillId = Convert.ToInt32(dr["user_skill_id"]);
            if (dr["valid_from"] != DBNull.Value)
            {
                item.ValidFrom = Convert.ToDateTime(dr["valid_from"]);
            }
            if (dr["valid_to"] != DBNull.Value)
            {
                item.ValidTo = Convert.ToDateTime(dr["valid_to"]);
            }

            if (dr["skill_type"] != DBNull.Value)
            {
                if (Enum.TryParse(dr["skill_type"].ToString(), out SkillTypeEnum skillType) &&
                Enum.IsDefined(typeof(SkillTypeEnum), skillType))
                {
                    item.SkillType = skillType;
                }
            }

            if (item.SkillType == SkillTypeEnum.Operational)
            {
                if (dr["assessment_template_id"] != DBNull.Value)
                {
                    item.SkillAssessmentId = Convert.ToInt32(dr["assessment_template_id"]);
                    item.SkillAssessmentName = dr["assessment_template_name"].ToString();
                }

                if (dr.HasColumn("default_target") && dr["default_target"] != DBNull.Value)
                {
                    item.DefaultTarget = Convert.ToInt32(dr["default_target"]);
                }
            }

            if (dr["notification_window_in_days"] != DBNull.Value)
            {
                item.NotificationWindowInDays = Convert.ToInt32(dr["notification_window_in_days"]);
            }

            if (dr.HasColumn("index") && dr["index"] != DBNull.Value)
            {
                item.Index = Convert.ToInt32(dr["index"]);
            }

            return item;
        }

        private SkillsMatrixItemValue CreateOrFillMatrixUserValueFromReader(NpgsqlDataReader dr, SkillsMatrixItemValue userValue = null, string include = null)
        {
            userValue ??= new SkillsMatrixItemValue();

            if (dr["id"] != DBNull.Value)
            {
                userValue.Id = Convert.ToInt32(dr["id"]);
            }
            userValue.IsDynamic = userValue.Id > 0;
            if (dr["score"] != DBNull.Value)
            {
                userValue.Score = Convert.ToDecimal(dr["score"]);
            }
            userValue.UserId = Convert.ToInt32(dr["user_id"]);
            userValue.UserSkillId = Convert.ToInt32(dr["user_skill_id"]);
            userValue.ValueDate = Convert.ToDateTime(dr["value_date"]);
            if (dr.HasColumn("value_expiration_date") && dr["value_expiration_date"] != DBNull.Value)
            {
                userValue.ValueExpirationDate = Convert.ToDateTime(dr["value_expiration_date"]);
            }
            if (dr.HasColumn("scoring_method") && dr["scoring_method"] != DBNull.Value && Enum.TryParse(dr["scoring_method"].ToString(), out ScoringMethodEnum scoringMethod))
            {
                userValue.ScoringMethod = scoringMethod;
            }

            return userValue;

        }
        #endregion

        #region - legend configuration -
        /// <summary>
        /// Gets the legend configuration for a company. Returns null if none exists (frontend handles defaults).
        /// </summary>
        public async Task<SkillMatrixLegendConfiguration?> GetLegendConfigurationAsync(int companyId)
        {
            try
            {
                // Get company-specific legend configuration using IGeneralManager
                var jsonValue = await _generalManager.GetSettingValueForCompanyByResourceId(companyId, SKILL_MATRIX_LEGEND_OPTIONS_RESOURCE_ID);

                if (!string.IsNullOrEmpty(jsonValue))
                {
                    // Use case-insensitive deserialization to handle both old camelCase and new PascalCase data
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var parsed = JsonSerializer.Deserialize<SkillMatrixLegendConfiguration>(jsonValue, options);
                    if (parsed != null)
                    {
                        parsed.CompanyId = companyId;
                        return parsed;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.GetLegendConfigurationAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            // Return null if no configuration exists - frontend handles default values
            return null;
        }

        /// <summary>
        /// Saves the complete legend configuration for a company.
        /// </summary>
        public async Task<bool> SaveLegendConfigurationAsync(int companyId, int userId, SkillMatrixLegendConfiguration configuration)
        {
            try
            {
                configuration.CompanyId = companyId;

                // Update timestamps for all items
                var now = DateTime.UtcNow;
                if (configuration.MandatorySkills != null)
                {
                    foreach (var item in configuration.MandatorySkills)
                    {
                        item.CompanyId = companyId;
                        item.UpdatedAt = now;
                        item.UpdatedBy = userId;
                    }
                }
                if (configuration.OperationalSkills != null)
                {
                    foreach (var item in configuration.OperationalSkills)
                    {
                        item.CompanyId = companyId;
                        item.UpdatedAt = now;
                        item.UpdatedBy = userId;
                    }
                }

                var jsonValue = JsonSerializer.Serialize(configuration);

                // Use IGeneralManager to save the setting with ResourceId 134
                var settingItem = new Models.Settings.SettingResourceItem
                {
                    CompanyId = companyId,
                    ResourceId = SKILL_MATRIX_LEGEND_OPTIONS_RESOURCE_ID,
                    Value = jsonValue
                };

                return await _generalManager.ChangeSettingResourceCompany(companyid: companyId, setting: settingItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.SaveLegendConfigurationAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                return false;
            }
        }

        /// <summary>
        /// Updates a single legend item. Requires existing configuration - use SaveLegendConfigurationAsync for initial setup.
        /// </summary>
        public async Task<bool> UpdateLegendItemAsync(int companyId, int userId, SkillMatrixLegendItem item)
        {
            try
            {
                var config = await GetLegendConfigurationAsync(companyId);

                // Return false if no configuration exists - frontend should use SaveLegendConfigurationAsync first
                if (config == null)
                {
                    _logger.LogWarning("MatrixManager.UpdateLegendItemAsync(): No legend configuration exists for company {CompanyId}. Use SaveLegendConfigurationAsync first.", companyId);
                    return false;
                }

                // Find and update the item
                var targetList = item.SkillType == "mandatory" ? config.MandatorySkills : config.OperationalSkills;
                var existingItem = targetList?.FirstOrDefault(i => i.SkillLevelId == item.SkillLevelId);

                if (existingItem != null)
                {
                    existingItem.Label = item.Label;
                    existingItem.Description = item.Description;
                    existingItem.IconColor = item.IconColor;
                    existingItem.BackgroundColor = item.BackgroundColor;
                    existingItem.IconClass = item.IconClass;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.UpdatedBy = userId;
                }
                else
                {
                    item.CompanyId = companyId;
                    item.CreatedAt = DateTime.UtcNow;
                    item.CreatedBy = userId;
                    targetList?.Add(item);
                }

                return await SaveLegendConfigurationAsync(companyId, userId, config);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("MatrixManager.UpdateLegendItemAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                return false;
            }
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_userStandingManager.GetPossibleExceptions());
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
