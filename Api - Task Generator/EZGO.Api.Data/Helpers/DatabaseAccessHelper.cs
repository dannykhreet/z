using Elastic.Apm;
using Elastic.Apm.Api;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Data.Helpers
{
    //TODO refactor to ScopedConnectionHelper and add a non-scoped connector
    //TODO refactor to LoggedByAPM or related and add non APM logged connector

    /// <summary>
    /// DatabaseAccessHelper; service class for getting DataCommands, Connections and Execute queries on a database (PostgreSQL).
    /// This will be the primary connector for database usage. 
    /// Be aware of the following things when making changes or enhancements to this helper:
    /// - Async database connections if started within a other classes will not be correctly closed if passes by ref or by val, there for keep opening and closing within this object.
    /// - Depending on settings either 1 or 2 connections are used (writer reader connection or a septate writer and a separate reader connection)
    /// - Do not open database connections if not needed. Always use the same connection if possible
    /// - When used on parallel tasks/ execution of logic make sure that you logic handles everything correctly. PostgreSql queries can NOT be run parallel on the same connection due to limitations in NPGSQL and the PostgreSQL data handlers on the server side.
    /// </summary>
    public class DatabaseAccessHelper : IDatabaseAccessHelper, IDisposable, IAsyncDisposable
    {
        #region - privates -
        private readonly IConnectionHelper _connectionHelper;
        private readonly ILogger _logger;
        private readonly IConfigurationHelper _configHelper;
        private const string ErrorType = "ERROR";
        private const string EventTypeExecuteNonQueryAsync = "950";
        private const string EventTypeExecuteScalarAsync = "951";
        private const string EventTypeGetDataReader = "952";
        private const string EventTypeGetDataTable = "953";
        private readonly string[] possiblePII = {"@_comment", "@_description", "@_signedby1", "@_signedby2", "@_signedby", "@_username", "@_firstname", "@_lastname", "@_email", "@_upn", "@_mutated_object", "@_original_object" };
        #endregion

        #region - public properties -
        public NpgsqlConnection Connection { get; set; }
        public NpgsqlConnection ReaderConnection { get; set; }
        public NpgsqlConnection WriterConnection { get; set; }
        #endregion

        #region - constructor(s) -
        /// <summary>
        /// DatabaseAccessHelper
        /// </summary>
        /// <param name="connhelper"></param>
        /// <param name="configHelper"></param>
        /// <param name="logger"></param>
        public DatabaseAccessHelper(IConnectionHelper connhelper, IConfigurationHelper configHelper, ILogger<DatabaseAccessHelper> logger)
        {
            _connectionHelper = connhelper;
            _logger = logger;
            _configHelper = configHelper;
        }
        #endregion

        #region - public methods -
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
        /// 
     public NpgsqlCommand GetCommand(NpgsqlConnection connection, string commandText, CommandType commandType, List<NpgsqlParameter> parameters = null)
     {
    #pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        string cmdText = commandType == CommandType.StoredProcedure
            ? DataConnectorHelper.WrapFunctionCommand(commandText, parameters)
            : commandText;

        var command = new NpgsqlCommand(cmdText, connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 120
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
     #pragma warning restore CA2100

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
            NpgsqlCommand cmd = null;

            try
            {
                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentTransaction?.StartSpan("DBConnector.ExecuteNonQueryAsync", ApiConstants.ActionQuery);

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) SetApmLabels(procedureNameOrQuery, parameters);

                if(connectionKind == ConnectionKind.Writer)
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                } else if (connectionKind == ConnectionKind.Reader)
                {
                    await InitReaderConnection();
                    cmd = this.GetCommand(ReaderConnection, procedureNameOrQuery, commandType, parameters);
                } else
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                }

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

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.CaptureException(ex, labels: GetApmLabels(procedureNameOrQuery, parameters));

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.End();

                throw;
            }

            if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.End();

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
            NpgsqlCommand cmd = null;

            try
            {
                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentTransaction?.StartSpan("DBConnector.ExecuteScalarAsync", ApiConstants.ActionQuery);

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) SetApmLabels(procedureNameOrQuery, parameters);

                if (connectionKind == ConnectionKind.Writer)
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                }
                else if (connectionKind == ConnectionKind.Reader)
                {
                    await InitReaderConnection();
                    cmd = this.GetCommand(ReaderConnection, procedureNameOrQuery, commandType, parameters);
                }
                else
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                }

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

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.CaptureException(ex, labels: GetApmLabels(procedureNameOrQuery, parameters));

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.End();

                throw;
            }

            if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.End();

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
                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentTransaction?.StartSpan("DBConnector.GetDataReader", ApiConstants.ActionQuery);

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) SetApmLabels(procedureNameOrQuery, parameters);

                if (connectionKind == ConnectionKind.Writer)
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType,parameters);
                }
                else if (connectionKind == ConnectionKind.Reader)
                {
                    await InitReaderConnection();
                    cmd = this.GetCommand(ReaderConnection, procedureNameOrQuery, commandType , parameters);
                }
                else
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                }

                dr = await cmd.ExecuteReaderAsync();

               // if (cmd != null) await cmd.DisposeAsync();
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

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.CaptureException(ex, labels: GetApmLabels(procedureNameOrQuery, parameters));

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.End();

                throw;
            }

            if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) Agent.Tracer?.CurrentSpan?.End();

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
            NpgsqlCommand cmd = null;

            try
            {
                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) 
                    Agent.Tracer?.CurrentTransaction?.StartSpan("DBConnector.GetDataTable", ApiConstants.ActionQuery);

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) 
                    SetApmLabels(procedureNameOrQuery, parameters);

                // Get the reader with proper command handling
                if (connectionKind == ConnectionKind.Writer)
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                }
                else if (connectionKind == ConnectionKind.Reader)
                {
                    await InitReaderConnection();
                    cmd = this.GetCommand(ReaderConnection, procedureNameOrQuery, commandType, parameters);
                }
                else
                {
                    await InitWriterConnection();
                    cmd = this.GetCommand(WriterConnection, procedureNameOrQuery, commandType, parameters);
                }

                dr = await cmd.ExecuteReaderAsync();

                DataTable dt = new DataTable(string.IsNullOrEmpty(dataTableName) ? "Data" : dataTableName);
                dt.Load(dr);

                await dr.CloseAsync();
                await dr.DisposeAsync();
                await cmd.DisposeAsync();

                return dt;
            }
            catch (Exception ex)
            {
                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()), "(DB)");

                await WriteToErrorLog(string.Format("Error occurred GetDataTable() : {0}", procedureNameOrQuery),
                                    ErrorType,
                                    EventTypeGetDataTable,
                                    "",
                                    GenerateMessage(procedureNameOrQuery, ex, parameters),
                                    string.Empty);

                _logger.LogError(exception: ex, message: string.Concat("(DB) DatabaseAccessHelper.GetDataTable(): ", ex.Message));

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) 
                    Agent.Tracer?.CurrentSpan?.CaptureException(ex, labels: GetApmLabels(procedureNameOrQuery, parameters));
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }
        
                if (cmd != null)
                {
                    await cmd.DisposeAsync();
                }

                if (_configHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) 
                    Agent.Tracer?.CurrentSpan?.End();
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
        /// NOTE! if user profile data is retrieved an other SP will be used and password data will be removed from JSON.
        /// NOTE! only use for internal querying only. DO NOT USE IN COMBINATION WITH DYNAMIC INPUT/USER INPUT.
        /// NOTE! this will only retrieve a single row.
        /// </summary>
        /// <param name="tableName">TableName, from where record needs to be retrieved, constant parameter collection from this DLL can be used. Do not manually input a table and watch out when retrieving sensitive data. Password with user profile will be replaced.</param>
        /// <param name="id">Id of the row that must be returned</param>
        /// <returns>A string containing raw database data for further implementation in logic.</returns>
        public async Task<string> GetDataRowAsJson(string tableName, int id)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_id", id));

                string sp = "get_data_row_json";
                if (tableName.ToLower() == Models.Enumerations.TableNames.profiles_user.ToString().ToLower())
                {
                    //user different row retrieval due to security policy. 
                    sp = "get_data_row_userprofile_json";
                } else
                {
                    sp = "get_data_row_json";
                    parameters.Add(new NpgsqlParameter("@_table_name", tableName));
                }

                var possibleOutCome = await ExecuteScalarAsync(procedureNameOrQuery: sp, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return possibleOutCome != null ? possibleOutCome.ToString() : "";
            }
            catch (Exception ex)
            {
                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

                _logger.LogError(exception: ex, message: string.Concat("(DB) DatabaseAccessHelper.GetDataRowAsJson(): ", ex.Message));
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
                if (tableName.ToLower() == Models.Enumerations.TableNames.profiles_user.ToString().ToLower())
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

        /// <summary>
        /// GetDataRowAsJson; Get a data row based on a table; Can be used for auditing or direct output.
        /// NOTE! if user profile data is retrieved an other SP will be used and password data will be removed from JSON.
        /// NOTE! only use for internal querying only. DO NOT USE IN COMBINATION WITH DYNAMIC INPUT/USER INPUT.
        /// NOTE! if more records are found a collection will be returned. 
        /// </summary>
        /// <param name="tableName">TableName, from where record needs to be retrieved, constant parameter collection from this DLL can be used. Do not manually input a table and watch out when retrieving sensitive data. Password with user profile will be replaced.</param>
        /// <param name="fieldName">FIrst fieldName, on which to query, constant parameter collection from this DLL can be used. Do not manually input a field and watch out when retrieving sensitive data. Password with user profile will be replaced.</param>
        /// <param name="id">First id of the row that must be returned</param>
        /// <param name="fieldName2">Second fieldName, on which to query, constant parameter collection from this DLL can be used. Do not manually input a field and watch out when retrieving sensitive data. Password with user profile will be replaced.</param>
        /// <param name="id2">Second id of the row that must be returned</param>
        /// <returns>A string containing raw database data for further implementation in logic.</returns>
        public async Task<string> GetDataRowAsJson(string tableName, string fieldName, int id, string fieldName2, int id2)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_id", id));

                string sp = "get_data_row_by_two_fields_json";
                if (tableName.ToLower() == Models.Enumerations.TableNames.profiles_user.ToString().ToLower())
                {
                    //user different row retrieval due to security policy. 
                    sp = "get_data_row_userprofile_json";
                }
                else
                {
                    sp = "get_data_row_by_two_fields_json";
                    parameters.Add(new NpgsqlParameter("@_table_name", tableName));
                    parameters.Add(new NpgsqlParameter("@_table_field", fieldName));
                    parameters.Add(new NpgsqlParameter("@_table_field_two", fieldName2));
                    parameters.Add(new NpgsqlParameter("@_id_two", id2));
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

        #endregion

        #region - private methods -
        /// <summary>
        /// GenerateMessage; Generate a message for writing to the database log.
        /// </summary>
        /// <param name="messageStart">Start of message, usually contains the stored procedure name and/or subject.</param>
        /// <param name="exception">Exception that occurs</param>
        /// <param name="parameters">Parameters that where used when the exception occurred.</param>
        /// <returns>string message based on a message start, exception and possible parameters</returns>
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
        /// <returns>string containing parameter names and values.</returns>
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
                        if (param.ParameterName.Contains("password") || param.ParameterName.Contains("pwd")) //filter-out password messages. 
                        {
                            sb.AppendFormat("|{0}:{1}|", param.ParameterName, "************");
                        } else if (possiblePII.Contains(param.ParameterName)) 
                        {
                            sb.AppendFormat("|{0}:{1}|", param.ParameterName, param.Value != null ? string.Concat("Value length: ",param.Value.ToString().Length.ToString()) : string.Empty);
                        }
                        else
                        {
                            sb.AppendFormat("|{0}:{1}|", param.ParameterName, param.Value != null ? param.Value.ToString() : string.Empty);
                        }
                      
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
        /// <returns>string containing a extraction of a exception.</returns>
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
        /// SetApmLabels; Set APM logging labels if needed.
        /// </summary>
        /// <param name="procedureNameOrQuery">stored procedure that is being executed</param>
        /// <param name="parameters">list of parameters that are used with the stored procedure</param>
        private void SetApmLabels(string procedureNameOrQuery, List<NpgsqlParameter> parameters)
        {
            if (!string.IsNullOrEmpty(procedureNameOrQuery)) Agent.Tracer?.CurrentSpan?.SetLabel("stored_procedure_or_name", procedureNameOrQuery);

            try
            {
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (NpgsqlParameter param in parameters)
                    {
                        if (param.ParameterName.Contains("password") || param.ParameterName.Contains("pwd")) //filter out password parameters
                        {
                            Agent.Tracer?.CurrentSpan?.SetLabel(param.ParameterName.Replace("@", ""), "************");
                        } else if (possiblePII.Contains(param.ParameterName))
                        {
                            Agent.Tracer?.CurrentSpan?.SetLabel(param.ParameterName.Replace("@", ""), "************");
                        }
                        else
                        {
                            Agent.Tracer?.CurrentSpan?.SetLabel(param.ParameterName.Replace("@", ""), param.Value != null ? param.Value.ToString() : string.Empty);
                        }
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }
        }

        /// <summary>
        /// GetApmLabels; Get a list of APM labels based on stored procedure name or query and sql parameters.
        /// </summary>
        /// <param name="procedureNameOrQuery">stored procedure that is being executed</param>
        /// <param name="parameters">list of parameters that are used with the stored procedure</param>
        /// <returns>Dictionary containing a key and a APM label.</returns>
        private Dictionary<string, Label> GetApmLabels(string procedureNameOrQuery, List<NpgsqlParameter> parameters)
        {
            Dictionary<string, Label> output = new Dictionary<string, Label>();

            try
            {
                if (!string.IsNullOrEmpty(procedureNameOrQuery) || (parameters != null && parameters.Count > 0))
                {
                    if (!string.IsNullOrEmpty(procedureNameOrQuery)) output.Add("stored_procedure_or_name", new Label(procedureNameOrQuery));

                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (NpgsqlParameter param in parameters)
                        {
                            if (param.ParameterName.Contains("password") || param.ParameterName.Contains("pwd"))
                            {
                                output.Add(param.ParameterName.Replace("@", ""), new Label("************"));
                            }
                            else if (possiblePII.Contains(param.ParameterName))
                            {
                                output.Add(param.ParameterName.Replace("@", ""), new Label(param.Value != null ? string.Concat("Length | ", param.Value.ToString()) : string.Empty));
                            }
                            else
                            {
                                output.Add(param.ParameterName.Replace("@", ""), new Label(param.Value != null ? param.Value.ToString() : string.Empty));
                            }
                        }
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }
            return output;
        }

        /// <summary>
        /// WriteToErrorLog; Writes a message to the database based on the current connection.
        /// Method can be used to circumvent the normal logging structure (which should contain a database logger). 
        /// This will be more efficient seeing the normal logger opens it own connections; This will use the existing one and there for not overload anything. 
        /// </summary>
        /// <param name="message">Message to be stored</param>
        /// <param name="type">String type</param>
        /// <param name="eventid">EventId (mostly a int value, but stored as strings)</param>
        /// <param name="eventname">EventName for reference purposes</param>
        /// <param name="description">Description to be posted</param>
        /// <param name="source">Source, if not filled empty string will be posted.</param>
        /// <returns>number of posted records (if successful, will be 1)</returns>
        private async Task<int> WriteToErrorLog(string message, string type, string eventid, string eventname, string description, string source)
        {
            if (_configHelper.GetValueAsBool(ApiSettings.ENABLE_DB_LOG_CONFIG_KEY))
            {
                if (string.IsNullOrEmpty(source)) source = _configHelper.GetValueAsString(ApiSettings.APPLICATION_NAME_CONFIG_KEY);

                NpgsqlCommand cmd = null;

                try
                {

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


                    if (WriterConnection == null) WriterConnection = await this.GetWriterConnection();

                    if (WriterConnection.State != ConnectionState.Open)
                        await WriterConnection.OpenAsync();

                    cmd = this.GetCommand(WriterConnection, "add_log", CommandType.StoredProcedure, parameters);

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
        #endregion

        #region - connection structure -
        /// <summary>
        /// InitWriterConnection(); Created a WriterConnection object based on GetWriterConnection and opens the connection;
        /// </summary>
        public async Task InitWriterConnection()
        {
            if (WriterConnection == null) WriterConnection = await this.GetWriterConnection();

            if (WriterConnection.State != ConnectionState.Open)
                await WriterConnection.OpenAsync();

        }

        /// <summary>
        /// InitReaderConnection(); Created a ReaderConnection object based on GetReaderConnection and opens the connection;
        /// </summary>
        public async Task InitReaderConnection()
        {
            if (ReaderConnection == null) ReaderConnection = await this.GetReaderConnection();

            if (ReaderConnection.State != ConnectionState.Open)
                await ReaderConnection.OpenAsync();
        }
        #endregion



        #region - disposable implementation (normal and a-sync) -
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
            //Cleanup all connections
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
            //Cleanup all connections
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
