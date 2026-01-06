using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.CLI.MigrationService.Data
{
    /// <summary>
    /// DatabaseAccessHelper; Database access helper; contains functionalities to create a database connection and execute queries. Based on the standard API connector, stripped unused functionalities.
    /// </summary>
    public class DatabaseAccessHelper: IDisposable, IAsyncDisposable
    {
        private readonly string _connectionString;

        public NpgsqlConnection Connection { get; set; }

        public DatabaseAccessHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// GetConnection; Get a connection for use with a command. The connection is based on the ConnectionHelper connection string.
        /// </summary>
        /// <returns>A NpgsqlConnection object.</returns>
        public async Task<NpgsqlConnection> GetConnection()
        {
            NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
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
            NpgsqlCommand command = new NpgsqlCommand(commandText, connection)
            {
                CommandType = commandType,
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
        public async Task<int> ExecuteNonQueryAsync(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            int returnValue = -1;

            NpgsqlCommand cmd = null;

            try
            {
                if (Connection == null) Connection = await this.GetConnection();

                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                cmd = this.GetCommand(Connection, procedureNameOrQuery, commandType);

                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                returnValue = await cmd.ExecuteNonQueryAsync();

                if (cmd != null) await cmd.DisposeAsync();

            }
            catch (Exception ex)
            {
                if (cmd != null) await cmd.DisposeAsync();

                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

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
        public async Task<object> ExecuteScalarAsync(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            object returnValue = null;

            NpgsqlCommand cmd = null;

            try
            {
                if (Connection == null) Connection = await this.GetConnection();

                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                cmd = this.GetCommand(Connection, procedureNameOrQuery, commandType);

                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                returnValue = await cmd.ExecuteScalarAsync();

                if (cmd != null) await cmd.DisposeAsync();


            }
            catch (Exception ex)
            {
                if (cmd != null) await cmd.DisposeAsync();

                ex.Data.Add(string.Concat("Execution Error ", Guid.NewGuid().ToString()) , "(DB)");

                throw;
            }

            return returnValue;
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
            if (Connection != null) Connection.Close();

            if (disposing)
            {
                if (Connection != null) { Connection.Close(); Connection.Dispose(); }

            }

            Connection = null;
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (Connection != null) { await Connection.CloseAsync(); await Connection.DisposeAsync(); }
            Connection = null;
        }
        #endregion
    }
}
