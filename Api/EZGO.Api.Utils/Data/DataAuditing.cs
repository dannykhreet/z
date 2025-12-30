using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Data;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Data
{
    public class DataAuditing : IDataAuditing
    {
        private readonly ILogger _logger;
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;

        #region - constructor(s) -
        public DataAuditing(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ILogger<DataAuditing> logger)
        {
            _logger = logger;
            _manager = manager;
            _configurationHelper = configurationHelper;
        }
        #endregion


        /// <summary>
        /// WriteDataAuditLog; Write a data audit record to the database. This will contain the original and the mutated item.
        /// </summary>
        /// <param name="original">Original data set as JSON</param>
        /// <param name="mutated">Mutated data set as JSON</param>
        /// <param name="objecttype">Kind of object being updated (action, task etc)</param>
        /// <param name="objectid">ObjectId of item.</param>
        /// <param name="userid">UserId of user that is doing the mutating.</param>
        /// <param name="companyid">CompanyId of user.</param>
        /// <param name="description">Description of the event</param>
        /// <returns>number of rows effected.</returns>
        public async Task<bool> WriteDataAudit(string original, string mutated, string objecttype, int objectId, int userId, int companyId, string description)
        {
            if(original != mutated)
            {
                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_original_object", original));
                    parameters.Add(new NpgsqlParameter("@_mutated_object", mutated));
                    parameters.Add(new NpgsqlParameter("@_object_type", objecttype));
                    parameters.Add(new NpgsqlParameter("@_object_id", objectId));
                    parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                    parameters.Add(new NpgsqlParameter("@_user_id", userId));
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_log_data_auditing", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                    return (rowseffected > 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("DataAuditing.WriteDataAudit(): ", ex.Message));
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Write data auditing log record to the database.
        /// Only used for accepting or declining a shared template.
        /// Record will be added for the company that shared the template.
        /// 
        /// </summary>
        /// <param name="sharedTemplateId">Id of the shared template record [Table: shared_template]</param>
        /// <param name="statusDescription">Should contain information about the status: accepted or declined.</param>
        /// <returns>Id of the created log record</returns>
        public async Task<bool> WriteDataAuditForSharedTemplate(int sharedTemplateId, string statusDescription)
        {
            try
            {
                List<NpgsqlParameter> parameters = new() { 
                    new NpgsqlParameter("_description", statusDescription),
                    new NpgsqlParameter("_sharedtemplateid", sharedTemplateId)
                };

                int id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_log_data_auditing_shared_template", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                return id > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.WriteDataAuditForSharedTemplate(): ", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// GetObjectDataLatestMutation; retrieve latest mutation records of certain object.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="objectId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<AuditingObjectData> GetObjectDataLatestMutation(int companyId, int objectId, string type)
        {
            var output = new AuditingObjectData();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_objectid", objectId));
                parameters.Add(new NpgsqlParameter("@_objecttype", type));

                using (dr = await _manager.GetDataReader("get_log_auditing_latest_by_object", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillAuditingObjectDataFromReader(dr: dr, auditingObjectData: output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetObjectDataLatestMutation(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Id > 0) return output;
            return null;

        }


        public async Task<AuditingObjectData> GetObjectDataMutation(int companyId, int id)
        {
            var output = new AuditingObjectData();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", id));

                using (dr = await _manager.GetDataReader("get_log_auditing_by_id", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillAuditingObjectDataFromReader(dr: dr, auditingObjectData: output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetObjectDataMutation(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Id > 0) return output;
            return null;

        }

        /// <summary>
        /// GetObjectDataMutations;
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="objectId"></param>
        /// <param name="type"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<List<AuditingObjectData>> GetObjectDataMutations(int companyId, int objectId, string type, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_objectid", objectId));
                parameters.Add(new NpgsqlParameter("@_objecttype", type));
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_by_object", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetObjectDataMutations(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetObjectDataByParent(int companyId, int parentObjectId, string type, int? limit = null, int? offset = null)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task<List<AuditingObjectData>> GetObjectDataUserHistory(int companyId, int userId, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                if(limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if(offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_by_user", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetObjectDataUserHistory(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetObjectData(int companyId, string type, string description, int? objectId = null, int? userId = null, DateTime? createdOnStart = null, DateTime? createdOnEnd = null, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (!string.IsNullOrEmpty(type)) { parameters.Add(new NpgsqlParameter("@_objecttype", type)); };
                if (!string.IsNullOrEmpty(description)) { parameters.Add(new NpgsqlParameter("@_description", description)); };
                if (objectId.HasValue) { parameters.Add(new NpgsqlParameter("@_objectid", objectId.Value)); }
                if (userId.HasValue) { parameters.Add(new NpgsqlParameter("@_userid", userId.Value)); }
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); } else { parameters.Add(new NpgsqlParameter("@_limit", 50)); } //set default, for technical load purposes.
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }
                if (createdOnStart.HasValue) { parameters.Add(new NpgsqlParameter("@_createdonstart", createdOnStart.Value)); }
                if (createdOnEnd.HasValue) { parameters.Add(new NpgsqlParameter("@_createdonend", createdOnEnd.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetObjectData(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetObjectDataMutationsOverview(int companyId, string[] types, string description, int? objectId = null, int? userId = null, DateTime? createdOnStart = null, DateTime? createdOnEnd = null, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if ((types != null) && (types.Length > 0)) { parameters.Add(new NpgsqlParameter("@_objecttypes", types)); };
                if (!string.IsNullOrEmpty(description)) { parameters.Add(new NpgsqlParameter("@_description", description)); };
                if (objectId.HasValue) { parameters.Add(new NpgsqlParameter("@_objectid", objectId.Value)); }
                if (userId.HasValue) { parameters.Add(new NpgsqlParameter("@_userid", userId.Value)); }
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); } else { parameters.Add(new NpgsqlParameter("@_limit", 50)); } //set default, for technical load purposes.
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }
                if (createdOnStart.HasValue) { parameters.Add(new NpgsqlParameter("@_createdonstart", createdOnStart.Value)); }
                if (createdOnEnd.HasValue) { parameters.Add(new NpgsqlParameter("@_createdonend", createdOnEnd.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_overview", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetObjectData(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        private AuditingObjectData CreateOrFillAuditingObjectDataFromReader(NpgsqlDataReader dr, AuditingObjectData auditingObjectData = null)
        {
            if (auditingObjectData == null) auditingObjectData = new AuditingObjectData();
            if (dr.HasColumn("id") && dr["id"] != DBNull.Value)
            {
                auditingObjectData.Id = Convert.ToInt32(dr["id"]);
            }
            auditingObjectData.CreatedOn = Convert.ToDateTime(dr["created_on"]);
            if (dr.HasColumn("description") && dr["description"] != DBNull.Value)
            {
                auditingObjectData.Description = dr["description"].ToString();
            }
            if (dr.HasColumn("mutated_object") && dr["mutated_object"] != DBNull.Value)
            {
                auditingObjectData.MutatedObjectDataJson = dr["mutated_object"].ToString();
            }
            auditingObjectData.ObjectId = Convert.ToInt32(dr["object_id"]);
            if (dr.HasColumn("name") && dr["name"] != DBNull.Value)
            {
                auditingObjectData.ObjectName = dr["name"].ToString();
            }
            if (dr.HasColumn("object_type") && dr["object_type"] != DBNull.Value)
            {
                auditingObjectData.ObjectType = dr["object_type"].ToString();
            }
            if (dr.HasColumn("original_object") && dr["original_object"] != DBNull.Value)
            {
                auditingObjectData.OriginalObjectDataJson = dr["original_object"].ToString();
            }
            if (dr.HasColumn("user_full_name") && dr["user_full_name"] != DBNull.Value)
            {
                auditingObjectData.UserFullName = dr["user_full_name"].ToString();
            }
            auditingObjectData.UserId = Convert.ToInt32(dr["user_id"]);

            //"id" int8, "object_type" varchar, "object_id" int4, "company_id" int4, "description" text, "created_on" timestamptz, "user_id" int4, "user_full_name" varchar

            return auditingObjectData;
        }

        
        public async Task<List<AuditingObjectData>> GetChecklistTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_logauditingid", logAuditingId)
                };
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }
                
                using (dr = await _manager.GetDataReader("get_log_auditing_details_checklisttemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetChecklistTemplateDataMutationsDetails(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetAuditTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_logauditingid", logAuditingId)
                };
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_details_audittemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetAuditTemplateDataMutationsDetails(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetAssessmentTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_logauditingid", logAuditingId)
                };
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_details_assessmenttemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetAssessmentTemplateDataMutationsDetails(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetWorkInstructionTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_logauditingid", logAuditingId)
                };
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_details_workinstructiontemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetWorkInstructionTemplateDataMutationsDetails(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        public async Task<List<AuditingObjectData>> GetTaskTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null)
        {
            var output = new List<AuditingObjectData>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_logauditingid", logAuditingId)
                };
                if (limit.HasValue) { parameters.Add(new NpgsqlParameter("@_limit", limit.Value)); }
                if (offset.HasValue) { parameters.Add(new NpgsqlParameter("@_offset", offset.Value)); }

                using (dr = await _manager.GetDataReader("get_log_auditing_details_tasktemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.Add(CreateOrFillAuditingObjectDataFromReader(dr: dr));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataAuditing.GetTaskTemplateDataMutationsDetails(): ", ex.Message));

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }
    }
}
