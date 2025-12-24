using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Settings;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Data.Helpers
{
    /// <summary>
    /// DatabaseLogWriter; Database log writer; Writes data to the logging_log table. Depending on implementation this will only execute when this is enabled for this user/company based on settings.
    /// </summary>
    public class DatabaseLogWriter : IDatabaseLogWriter
    {
        #region - privates -
        private IConnectionHelper _connectionHelper;
        private IConfigurationHelper _configHelper;
        #endregion

        #region - constructor -
        public DatabaseLogWriter(IConnectionHelper connectionHelper, IConfigurationHelper configurationHelper)
        {
            _connectionHelper = connectionHelper;
            _configHelper = configurationHelper;
        }
        #endregion

        #region - public methods - 
        /// <summary>
        /// GetRequestResponseLoggingEnabled; Check if request logging is enabled (usually only on test and or acceptance)
        /// NOTE! will use it's own database connection and not will use the data access helper.
        /// </summary>
        /// <param name="userid">User id to check</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> GetRequestResponseLoggingEnabled(int userid)
        {
            if (_configHelper.GetValueAsBool(ApiSettings.ENABLE_DB_LOG_REQUESTRESPONSE_CONFIG_KEY)) return true;
            if (userid <= 0) return false;

            var success = false;
            NpgsqlConnection conn = null;

            try
            {
                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringWriter()))
                {

                    try
                    {
                        await conn.OpenAsync();

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_userid", userid));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_resourcesetting_dblogging_enabled_by_user", parameters), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddRange(parameters.ToArray());

                            bool.TryParse((await cmd.ExecuteScalarAsync()).ToString(), out success);

                        }

                    }
                    catch (Exception ex)
                    {
                        var e = ex;
                        //Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    }

                    if (conn != null)
                    {
                        await conn.CloseAsync();
                        await conn.DisposeAsync();
                    }

                }
            }
            catch (Exception)
            {
                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
            }
            finally
            {
                if (conn != null)
                {
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                }
            }

            return success;
        }

        /// <summary>
        /// WriteToLog; WriteToLog, writes a data record to the database logging table. 
        /// </summary>
        /// <param name="message">Message to be stored</param>
        /// <param name="type">String type</param>
        /// <param name="eventid">EventId (mostly a int value, but stored as strings)</param>
        /// <param name="eventname">EventName for reference purposes</param>
        /// <param name="description">Description to be posted</param>
        /// <param name="source">Source, if not filled confirmation source will be posted.</param>
        /// <returns>number of posted records (if successful, will be 1)</returns>
        public async Task<int> WriteToLog(string message, string type, string eventid, string eventname, string description, string source)
        {
            if(_configHelper.GetValueAsBool(ApiSettings.ENABLE_DB_LOG_CONFIG_KEY))
            {
                if (string.IsNullOrEmpty(source)) source = _configHelper.GetValueAsString(ApiSettings.APPLICATION_NAME_CONFIG_KEY);

                NpgsqlConnection conn = null;
                try
                {
                    using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringWriter()))
                    {

                        try
                        {
                            await conn.OpenAsync();

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

                            using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("add_log", parameters), conn))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddRange(parameters.ToArray());

                                int.TryParse((await cmd.ExecuteScalarAsync()).ToString(), out int logId);

                                return logId;

                            }

                        }
                        catch (Exception ex)
                        {
                            var e = ex;
                           //Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                        }

                        if (conn != null)
                        {
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                        }

                    }
                }
                catch (Exception)
                {
                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                }
                finally
                {
                    if (conn != null) {
                        await conn.CloseAsync();
                        await conn.DisposeAsync();
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// WriteToLog; Writes a record to the requestresponse logging table. This method will be used in a middleware connection; 
        /// </summary>
        /// <param name="domain">Domain of the request being done. (domain this api also runs on)</param>
        /// <param name="path">Path of the uri</param>
        /// <param name="query">Query string parameters of the uri</param>
        /// <param name="status">Http status to be used</param>
        /// <param name="header">Possible header parameters as string</param>
        /// <param name="request">Request body</param>
        /// <param name="response">Response body</param>
        /// <returns>number of posted records (if successful, will be 1)</returns>
        public async Task<int> WriteToLog(string domain, string path, string query, string status, string header, string request, string response)
        {

            NpgsqlConnection conn = null;
            try
            {
                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringWriter()))
                {

                    try
                    {
                        await conn.OpenAsync();

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_domain", domain));
                        parameters.Add(new NpgsqlParameter("@_path", path));
                        parameters.Add(new NpgsqlParameter("@_query", query));
                        parameters.Add(new NpgsqlParameter("@_status", status));
                        parameters.Add(new NpgsqlParameter("@_header", header));
                        parameters.Add(new NpgsqlParameter("@_request", request.Replace("\x00", "")));
                        parameters.Add(new NpgsqlParameter("@_response", response.Replace("\x00", "")));


                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("add_log_requestresponse", parameters), conn))
                        {
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.AddRange(parameters.ToArray());

                            int.TryParse((await cmd.ExecuteScalarAsync()).ToString(), out int logId);

                            return logId;

                        };
#pragma warning disable CS0162 // Unreachable code detected
                        if (conn != null)
#pragma warning restore CS0162 // Unreachable code detected
                        {
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                        }

                    }
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
                        //Error occurred within the logging. Ignore it or else the helper can end up in a painful loop of death.
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                    finally
                    {
                        if (conn != null)
                        {
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                        }
                    }

                }
            }
            catch (Exception)
            {
                // Error occurred within the logging. Ignore it or else the helper can end up in a painful loop of death.
            }
            finally
            {
                if (conn != null)
                {
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                }
            }

            return 0;
        }

        /// <summary>
        /// GetLatestLogId; Get latest log id, used for API health check reasons.
        /// </summary>
        /// <returns>Id of latest record</returns>
        public async Task<int> GetLatestLogId()
        {
            NpgsqlConnection conn = null;
            try
            {
                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {

                    try
                    {
                        await conn.OpenAsync();

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_tools_latest_log_id"), conn))
                        {
                            cmd.CommandType = CommandType.Text;


                            int.TryParse((await cmd.ExecuteScalarAsync()).ToString(), out int logId);

                            return logId;

                        };
#pragma warning disable CS0162 // Unreachable code detected
                        if (conn != null)
#pragma warning restore CS0162 // Unreachable code detected
                        {
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                        }

                    }
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
                        //Error occurred within the logging. Ignore it or else the helper can end up in a painful loop of death.
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                    finally
                    {
                        if (conn != null)
                        {
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                        }
                    }

                }
            }
            catch (Exception)
            {
                // Error occurred within the logging. Ignore it or else the helper can end up in a painful loop of death.
            }
            finally
            {
                if (conn != null)
                {
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                }
            }

            return 0;
        }
        #endregion

    }


}
