using EZGO.Api.Data.Enumerations;
using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Settings;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.WorkerService.Data.Helpers
{
    /// <summary>
    /// DatabaseAccessHelper; service class for getting DataCommands, Connections and Execute queries on a database (PostgreSQL).
    /// </summary>
    public class DatabaseAccessHelper : IDatabaseAccessHelper, IDisposable, IAsyncDisposable
    {
        private readonly IConnectionHelper _connectionHelper;
        private readonly ILogger _logger;
        private readonly IConfigurationHelper _configHelper;
        //private bool disposedValue;
        private const string ErrorType = "ERROR";
        private const string EventTypeExecuteNonQueryAsync = "950";
        private const string EventTypeExecuteScalarAsync = "951";
        private const string EventTypeGetDataReader = "952";
        private const string EventTypeGetDataTable = "953";

        public NpgsqlConnection Connection { get; set; }
        public NpgsqlConnection ReaderConnection { get; set; }
        public NpgsqlConnection WriterConnection { get; set; }

        /// <summary>
        /// TableNames, used for internal logging functionality (data auditing, certain tech messages). Do not use for anything else.
        /// NOTE! these will directly be converted to strings, therefor they will not be adhering to the naming convention.
        /// </summary>
        public enum TableNames
        {
            actions_action,
            actions_action_assigned_areas,
            actions_action_assigned_users,
            actions_actioncomment,
            actions_actioncommentviewed,
            audits_audit,
            audits_audit_task,
            audits_audittemplate,
            audits_audittemplate_task,
            checklists_checklist,
            checklists_checklist_tasks,
            checklists_checklisttemlate,
            checklists_checklisttemplate_tasks,
            comments,
            companies_area,
            companies_company,
            companies_shift,
            profiles_user,
            profiles_user_allowed_areas,
            profiles_user_areas,
            resource_languages,
            tasks_task,
            tasks_taskrecurrency,
            tasks_taskrecurrency_one_time_shifts,
            tasks_taskrecurrency_shifts,
            tasks_tasktemplate,
            tasks_tasktemplate_tags_links,
            tasks_tasktemplatestep,
            tasks_tasktemplatetag,
            uploads_requesteds3link
        }

        public DatabaseAccessHelper(IConnectionHelper connhelper, IConfigurationHelper configHelper, ILogger<DatabaseAccessHelper> logger)
        {
            _connectionHelper = connhelper;
            _logger = logger;
            _configHelper = configHelper;
        }

        /// <summary>
        /// GetConnection; Get a connection for use with a command. The connection is based on the ConnectionHelper connection string.
        /// </summary>
        /// <returns>A NpgsqlConnection object.</returns>
        public async Task<NpgsqlConnection> GetConnection()
        {
            NpgsqlConnection conn = new NpgsqlConnection(_connectionHelper.GetConnectionString());
            await Task.CompletedTask;
            return conn;
        }


        /// <summary>
        /// GetConnection; Get a connection for use with a command. The connection is based on the ConnectionHelper connection string for use with a writer
        /// </summary>
        /// <returns>A NpgsqlConnection object.</returns>
        public async Task<NpgsqlConnection> GetWriterConnection()
        {
            NpgsqlConnection conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringWriter());
            await Task.CompletedTask;
            return conn;
        }

        /// <summary>
        /// GetConnection; Get a connection for use with a command. The connection is based on the ConnectionHelper connection string for use with a reader
        /// </summary>
        /// <returns>A NpgsqlConnection object.</returns>
        public async Task<NpgsqlConnection> GetReaderConnection()
        {
            NpgsqlConnection conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader());
            await Task.CompletedTask;
            return conn;
        }

        /// <summary>
        /// GetCommand; Get a database command for executing queries and stored procedures.
        /// </summary>
        /// <param name="connection">Connection for use with the command.</param>
        /// <param name="commandText">CommandText, query or procedure.</param>
        /// <param name="commandType">CommandType (query or stored procedure).</param>
        /// <returns>A Database command for further use.</returns>
        public NpgsqlCommand GetCommand(NpgsqlConnection connection, string commandText, CommandType commandType, List<NpgsqlParameter> parameters = null)
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            string cmdText = commandType == CommandType.StoredProcedure
           ? DataConnectorHelper.WrapFunctionCommand(commandText, parameters)
           : commandText;

            var command = new NpgsqlCommand(cmdText, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 1200
            };

            // Add parameters here if they exist
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var param in parameters)
                {
                    // Handle binary data properly - ensure byte[] parameters are set correctly
                    if (param.Value is byte[] byteArray)
                    {
                        // For binary data, make sure it's properly handled
                        var newParam = new NpgsqlParameter(param.ParameterName, NpgsqlTypes.NpgsqlDbType.Bytea)
                        {
                            Value = byteArray
                        };
                        command.Parameters.Add(newParam);
                    }
                    else
                    {
                        command.Parameters.Add(param);
                    }
                }
            }
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            return command;
        }

        /// <summary>
        /// ExecuteNonQueryAsync; Execute a query with no specific output. (returns rows effected.)
        /// </summary>
        /// <param name="procedureNameOrQuery">Query or stored procedure name that is being used.</param>
        /// <param name="parameters">SQLParameters that are used. (can be empty)</param>
        /// <param name="commandType">Command type, query or stored procedure.</param>
        /// <returns>The number of rows effected or default integer value.</returns>
        public async Task<int> ExecuteNonQueryAsync(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, ConnectionKind connectionKind = ConnectionKind.Writer)
        {
            int returnValue = -1;

            //NpgsqlConnection conn = null;
            NpgsqlCommand cmd = null;

            try
            {
                if(Connection == null) Connection = await this.GetConnection();

                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                cmd = this.GetCommand(Connection, procedureNameOrQuery, commandType, parameters);

                returnValue = await cmd.ExecuteNonQueryAsync();

                if (cmd != null) await cmd.DisposeAsync();

            }
            catch (Exception ex)
            {
                if (cmd != null) await cmd.DisposeAsync();

                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

                await WriteToErrorLog(string.Format("Error occurred ExecuteNonQueryAsync() : {0}", procedureNameOrQuery),
                                                    ErrorType,
                                                    EventTypeExecuteNonQueryAsync,
                                                    "",
                                                    GenerateMessage(procedureNameOrQuery, ex, parameters),
                                                    string.Empty);

                _logger.LogError(exception: ex, message: "(DB) Error occurred ExecuteNonQueryAsync()");

                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// ExecuteScalarAsync; Execute query or SP and return the first result from the returning set of data.
        /// </summary>
        /// <param name="procedureNameOrQuery">Query or stored procedure name that is being used.</param>
        /// <param name="parameters">SQLParameters that are used. (can be empty).</param>
        /// <param name="commandType">Command type, query or stored procedure.</param>
        /// <returns>Object containing the scalar result from query.</returns>
        public async Task<object> ExecuteScalarAsync(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, ConnectionKind connectionKind = ConnectionKind.Writer)
        {
            object returnValue = null;

            //NpgsqlConnection conn = null;
            NpgsqlCommand cmd = null;

            try
            {
                if (Connection == null) Connection = await this.GetConnection();

                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                cmd = this.GetCommand(Connection, procedureNameOrQuery, commandType, parameters);

                returnValue = await cmd.ExecuteScalarAsync();

                if (cmd != null) await cmd.DisposeAsync();


            }
            catch (Exception ex)
            {
                if (cmd != null) await cmd.DisposeAsync();

                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

                await WriteToErrorLog(string.Format("Error occurred ExecuteScalarAsync() : {0}", procedureNameOrQuery),
                                    ErrorType,
                                    EventTypeExecuteScalarAsync,
                                    "",
                                    GenerateMessage(procedureNameOrQuery, ex, parameters),
                                    string.Empty);

                _logger.LogError(exception: ex, message: "(DB) Error occurred ExecuteScalarAsync()");

                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// GetDataReader; Execute query or SP and return a DataReader for use in method.
        /// </summary>
        /// <param name="procedureNameOrQuery">Query or stored procedure name that is being used.</param>
        /// <param name="parameters">SQLParameters that are used. (can be empty).</param>
        /// <param name="commandType">Command type, query or stored procedure.</param>
        /// <returns>Object containing a reader from query.</returns>
        public async Task<NpgsqlDataReader> GetDataReader(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;
            NpgsqlCommand cmd = null;

            try
            {
                if (Connection == null) Connection = await this.GetConnection();

                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                cmd = this.GetCommand(Connection, procedureNameOrQuery, commandType, parameters);

                dr = await cmd.ExecuteReaderAsync();

                if (cmd != null) await cmd.DisposeAsync();
                //await CleanUp(cmd);  -> changed to direct calls not through async method.
            }
            catch (Exception ex)
            {
                if (cmd != null) await cmd.DisposeAsync();

                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

                await WriteToErrorLog(string.Format("Error occurred GetDataReader() : {0}", procedureNameOrQuery),
                                                    ErrorType,
                                                    EventTypeGetDataReader,
                                                    "",
                                                    GenerateMessage(procedureNameOrQuery, ex, parameters),
                                                    string.Empty);

                _logger.LogError(exception: ex, message: "(DB) Error occurred GetDataReader()");

                throw;
            }

            return dr;
        }

        /// <summary>
        /// GetDataTable; Get a datatable based on a query or procedure.
        /// </summary>
        /// <param name="procedureNameOrQuery">The name of the procedure or query.</param>
        /// <param name="parameters">Parameters that need to be used.</param>
        /// <param name="commandType">CommandType (query or stored procedure)</param>
        /// <returns>A DataTable or empty datatable.</returns>
        public async Task<DataTable> GetDataTable(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, string dataTableName = "", ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;
            //NpgsqlConnection conn = await GetConnection();

            try
            {

                if (Connection == null) Connection = await GetConnection();

                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                using (dr = await GetDataReader(procedureNameOrQuery, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    DataTable dt = new DataTable(string.IsNullOrEmpty(dataTableName) ? "Data" : dataTableName); //TODO add default name to constants
                    dt.Load(dr);

                    await dr.CloseAsync(); dr = null;

                    return dt;

                }
            }
            catch (Exception ex)
            {
                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

                await WriteToErrorLog(string.Format("Error occurred GetDataTable() : {0}", procedureNameOrQuery),
                                    ErrorType,
                                    EventTypeGetDataTable,
                                    "",
                                    GenerateMessage(procedureNameOrQuery, ex, parameters),
                                    string.Empty);

                _logger.LogError(exception: ex, message: string.Concat("(DB) DatabaseAccessHelper.GetDataTable(): ", ex.Message));
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }

            }

            return new DataTable(); //always return a empty table.
        }

        /// <summary>
        /// GetBaseParameters; Get a base set of parameters containing at least the companyId.
        /// </summary>
        /// <param name="companyId">CompanyId based on companyId (companies_company.id)</param>
        /// <returns>A list of NpgsqlParameters</returns>
        public List<NpgsqlParameter> GetBaseParameters(int companyId)
        {
            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            return parameters;
        }

        /// <summary>
        /// GetDataRowAsJson; Get a data row based on a table; Can be used for auditing or direct output.
        /// </summary>
        /// <param name="tableName">TableName, constant parameter collection from this DLL can be used.</param>
        /// <param name="id">Id of the row that must be returned</param>
        /// <returns>A string containing raw database data for further implementation in logic.</returns>
        public async Task<string> GetDataRowAsJson(string tableName, int id)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_id", id));
                parameters.Add(new NpgsqlParameter("@_table_name", tableName));

                var possibleOutCome = await ExecuteScalarAsync("get_data_row_json", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return possibleOutCome.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DatabaseAccessHelper.GetDataRowAsJson(): ", ex.Message));
            }
            return string.Empty;

        }

        /// <summary>
        /// GetDataRowAsJson; Get a data row based on a table; Can be used for auditing or direct output.
        /// NOTE! if user profile data is retrieved an other SP will be used and password data will be removed from JSON.
        /// NOTE! only use for internal querying only. DO NOT USE IN COMBINATION WITH DYNAMIC INPUT/USER INPUT.
        /// NOTE! if more records are found a collection will be returned. 
        /// </summary>
        /// <param name="tableName">TableName, from where record needs to be retrieved, constant parameter collection from this DLL can be used. Do not manually input a table and watch out when retrieving sensitive data. Password with user profile will be replaced.</param>
        /// <param name="fieldName">FieldName, on which to query, constant parameter collection from this DLL can be used. Do not manually input a field and watch out when retrieving sensitive data. Password with user profile will be replaced.</param>
        /// <param name="id">Id of the row that must be returned</param>
        /// <returns>A string containing raw database data for further implementation in logic.</returns>
        public async Task<string> GetDataRowAsJson(string tableName, string fieldName, int id)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_id", id));

                string sp = "get_data_row_by_field_json";
                if (tableName.ToLower() == DatabaseAccessHelper.TableNames.profiles_user.ToString().ToLower())
                {
                    //user different row retrieval due to security policy. 
                    sp = "get_data_row_userprofile_json";
                }
                else
                {
                    sp = "get_data_row_by_field_json";
                    parameters.Add(new NpgsqlParameter("@_table_name", tableName));
                    parameters.Add(new NpgsqlParameter("@_table_field", fieldName));
                }

                var possibleOutCome = await ExecuteScalarAsync(procedureNameOrQuery: sp, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return possibleOutCome != null ? possibleOutCome.ToString() : "";
            }
            catch (Exception ex)
            {
                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()), "(DB)");

                _logger.LogError(exception: ex, message: string.Concat("(DB) DatabaseAccessHelper.GetDataRowAsJson(): ", ex.Message));
            }
            return string.Empty;

        }

        public async Task<string> GetDataRowAsJson(string tableName, string fieldName, int id, string fieldName2, int id2)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// GenerateMessage; Generate a message for writing to the database log.
        /// </summary>
        /// <param name="messageStart">Start of message, usually contains the stored procedure name and/or subject.</param>
        /// <param name="exception">Exception that occurs</param>
        /// <param name="parameters">Parameters that where used when the exception occurred.</param>
        /// <returns></returns>
        private string GenerateMessage(string messageStart, Exception exception, List<NpgsqlParameter> parameters)
        {
            try
            {
                string outputFormat = "[{0}] [{1}] [{2}]";
                var output = string.Format(outputFormat,
                                           messageStart,
                                           GenerateParameterMessage(parameters: parameters),
                                           GenerateExceptionMessage(exception: exception));

                return output;
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// GenerateParameterMessage; Get a string containing the parameter values and names
        /// </summary>
        /// <param name="parameters">Parameters that are used.</param>
        /// <returns></returns>
        private string GenerateParameterMessage(List<NpgsqlParameter> parameters)
        {
            string output = "";

            try
            {
                if (parameters != null)
                {
                    var sb = new StringBuilder();

                    foreach (NpgsqlParameter param in parameters)
                    {
                        sb.AppendFormat("|{0}:{1}|", param.ParameterName, param.Value.ToString());
                    }
                    output = sb.ToString();
                    sb.Clear();
                    sb = null;
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }

            return output;
        }

        /// <summary>
        /// GenerateExceptionMessage; Get exception information.
        /// </summary>
        /// <param name="exception">Exception that occurs.</param>
        /// <returns></returns>
        private string GenerateExceptionMessage(Exception exception)
        {
            string output = "";

            try
            {
                var sb = new StringBuilder();

                sb.AppendFormat("|{0}|", exception.Message);
                sb.AppendFormat("|{0}|", exception.Source);
                sb.AppendFormat("|{0}|", exception.StackTrace);

                output = sb.ToString();
                sb.Clear();
                sb = null;
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }

            return output;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="eventid"></param>
        /// <param name="eventname"></param>
        /// <param name="description"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<int> WriteToErrorLog(string message, string type, string eventid, string eventname, string description, string source)
        {
            if (_configHelper.GetValueAsBool(ApiSettings.ENABLE_DB_LOG_CONFIG_KEY))
            {
                if (string.IsNullOrEmpty(source)) source = _configHelper.GetValueAsString(ApiSettings.APPLICATION_NAME_CONFIG_KEY);

                NpgsqlCommand cmd = null;

                try
                {
                    if (Connection == null) Connection = await this.GetConnection();

                    if (Connection.State != ConnectionState.Open)
                        await Connection.OpenAsync();

                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventid));
                    if (string.IsNullOrEmpty(eventname))
                    {
                        parameters.Add(new NpgsqlParameter("@_eventname", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_eventname", eventname));
                    }
                    if (string.IsNullOrEmpty(source))
                    {
                        parameters.Add(new NpgsqlParameter("@_source", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_source", source));
                    }
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    cmd = this.GetCommand(Connection, "add_log", CommandType.StoredProcedure, parameters);

                    var returnValue = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    if (cmd != null) await cmd.DisposeAsync();

                    return returnValue;

#pragma warning disable CS0168 // Variable is declared but never used
                }
                catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    if (cmd != null) await cmd.DisposeAsync();
                    //db error log write does not function, ignore.
                }

            }
            return 0;
        }

        #region - disposable -
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {

            await DisposeAsyncCore();

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Debug.WriteLine("Dispose(bool disposing)");

            if (Connection != null) Connection.Close();
            if (ReaderConnection != null) ReaderConnection.Close();
            if (WriterConnection != null) WriterConnection.Close();

            if (disposing)
            {
                if (Connection != null) { Connection.Close(); Connection.Dispose(); }
                if (ReaderConnection != null) { ReaderConnection.Close(); ReaderConnection.Dispose(); }
                if (WriterConnection != null) { WriterConnection.Close(); WriterConnection.Dispose(); }
            }

            Connection = null;
            ReaderConnection = null;
            WriterConnection = null;

        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            Debug.WriteLine("DisposeAsyncCore");
            if (Connection != null) { await Connection.CloseAsync(); await Connection.DisposeAsync(); }
            Connection = null;

            if (ReaderConnection != null) { await ReaderConnection.CloseAsync(); await ReaderConnection.DisposeAsync(); }
            ReaderConnection = null;

            if (WriterConnection != null) { await WriterConnection.CloseAsync(); await WriterConnection.DisposeAsync(); }
            WriterConnection = null;
        }
        #endregion
    }
}
