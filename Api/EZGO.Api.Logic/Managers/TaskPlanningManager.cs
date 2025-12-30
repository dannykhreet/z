
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.TaskGeneration;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class TaskPlanningManager : BaseManager<TaskPlanningManager>, ITaskPlanningManager
    {
        #region - privates -

        private readonly IDatabaseAccessHelper _manager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IConfigurationHelper _configurationHelper;

        #endregion

        #region - constructor(s) -
        public TaskPlanningManager(IDatabaseAccessHelper manager,  IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, ILogger<TaskPlanningManager> logger) : base(logger)
        {

            _manager = manager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;

        }
        #endregion

        #region - planning -
        public async Task<PlanningConfiguration> GetPlanningConfiguration(int companyId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new PlanningConfiguration();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));


                using (dr = await _manager.GetDataReader("get_companies_generation_config", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillPlanningFromReader(dr);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskPlanningManager.GetPlanningConfiguration(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;

        }

        public async Task<int> SavePlanningConfiguration(int companyId, int userId, PlanningConfiguration planning)
        {
            var retrieveOriginalObject = await GetPlanningConfiguration(companyId: companyId);
            var original = retrieveOriginalObject.Id.HasValue && retrieveOriginalObject.Id > 0 ? await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_generation_config.ToString(), retrieveOriginalObject.Id.Value) : "";

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_planningconfig", planning.ConfigurationItems != null ? planning.ConfigurationItems.ToJsonFromObject() : ""));
            parameters.Add(new NpgsqlParameter("@_generatorconfig", planning.ConfigurationItems != null ? planning.ToGenerationConfiguration().ConfigurationItems.ToJsonFromObject() : "")); //TODO add converter from planning to generator planning

            var rowseffected =  Convert.ToInt32(await _manager.ExecuteScalarAsync("save_companies_generation_config", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutatedObject = await GetPlanningConfiguration(companyId: companyId, connectionKind: ConnectionKind.Writer); //retrieve object
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_generation_config.ToString(), mutatedObject.Id.Value); //retrieve json
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_generation_config.ToString(), objectId: mutatedObject.Id.Value, userId: userId, companyId: companyId, description: "Created or changed planning tasks.");
            }

            return rowseffected;
        }


        private PlanningConfiguration CreateOrFillPlanningFromReader(NpgsqlDataReader dr, PlanningConfiguration planning = null)
        {
            if (planning == null) planning = new PlanningConfiguration();

            planning.Id = Convert.ToInt32(dr["id"]);
            planning.UserId = Convert.ToInt32(dr["user_id"]);
            if (dr["modified_at"] != DBNull.Value)
            {
                planning.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }
            if (dr["created_at"] != DBNull.Value)
            {
                planning.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if(dr["planning_config"] != DBNull.Value)
            {
                try
                {
                    if(!string.IsNullOrEmpty(dr["planning_config"].ToString()) && dr["planning_config"].ToString() != "{}")
                    {
                        planning.ConfigurationItems = dr["planning_config"].ToString().ToObjectFromJson<List<PlanningConfigurationItem>>();
                    } else
                    {
                        planning.ConfigurationItems = new List<PlanningConfigurationItem>();
                    }
                    
                } catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("TaskPlanningManager.CreateOrFillPlanningFromReader(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

                    //TODO add logging?
                }
            }
            return planning;
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
