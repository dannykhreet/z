using EZGO.Api.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Data.Helpers
{
    
    /// <summary>
    /// ConnectionHelper; helper object class, for getting and or building connection strings to databases and or external parties.
    /// The connection helper needs to be implemented as a service (startup.cs) see <see cref="StartupHelperExtension">StartupHelperExtension</see>.
    /// </summary>
    public class ConnectionHelper : IConnectionHelper
    {
        #region - privates / properties - 
        public const string DEFAULT_CONNECTIONSTRING_NAME = "DefaultConnection"; //ConnectionString located in the ApplicationConfig JSON (depending on project is differently named)
        public const string DEFAULT_CONNECTIONSTRING_NAME_READER = "DefaultConnection"; //ConnectionString located in the ApplicationConfig JSON (depending on project is differently named)
        public const string DEFAULT_CONNECTIONSTRING_NAME_WRITER = "DefaultConnectionWriter"; //ConnectionString located in the ApplicationConfig JSON (depending on project is differently named)
        public const string DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT = "API_DEFAULTCONNECTION"; //Default connection environment name for environmental variables.
        public const string DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT_READER = "API_DEFAULTCONNECTION_READER"; //Reader connection string name for environmental variables.
        public const string DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT_WRITER = "API_DEFAULTCONNECTION"; //Writer connection string name for environmental variables. (uses same connection as default)
        private static string _connectionstringReader = string.Empty;
        private static string _connectionstringWriter = string.Empty;
        private static string _applicationName = string.Empty;
        #endregion

        #region - constructor(s) -
        /// <summary>
        /// ConnectionHelper; Used when implementing a single data connection for both reader and writer connectivity.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public ConnectionHelper(string connectionString)
        {
            _connectionstringReader = connectionString;
            _connectionstringWriter = connectionString;
        }

        /// <summary>
        /// ConnectionHelper; Used when implementing a data connection with both reader and writer connectivity.
        /// </summary>
        /// <param name="connectionStringReader">ConnectionString for reader</param>
        /// <param name="connectionStringWriter">ConnectionString for writer</param>
        public ConnectionHelper(string connectionStringReader, string connectionStringWriter)
        {
            _connectionstringReader = connectionStringReader;
            _connectionstringWriter = connectionStringWriter;
        }
        #endregion

        #region - get connection strings and environments -
        /// <summary>
        /// GetConnectionString; Gets the connection string from a config (can be local config or server, where server is leading).
        /// </summary>
        /// <returns>String containing a connection string for PostgreSQL in this case.</returns>
        public string GetConnectionString()
        {
            return _connectionstringWriter;
        }

        /// <summary>
        /// GetConnectionStringReader; Gets the connection string from a config (can be local config or server, where server is leading).
        /// </summary>
        /// <returns>String containing a connection string for PostgreSQL in this case.</returns>
        public string GetConnectionStringReader()
        {
            return _connectionstringReader;
        }

        /// <summary>
        /// GetConnectionStringWriter; Gets the connection string from a config (can be local config or server, where server is leading).
        /// </summary>
        /// <returns>String containing a connection string for PostgreSQL in this case.</returns>
        public string GetConnectionStringWriter()
        {
            return _connectionstringWriter;
        }

        /// <summary>
        /// GetActiveDatabaseEnvironment; Get's and checks based on the database connection which environment is currently running.
        /// </summary>
        /// <returns>Predefined string, use within applications for extra checks or visual queues for user. </returns>
        public string GetActiveDatabaseEnvironment()
        {
            if(_connectionstringWriter.Contains("Server=192.168.150.90;Port=54321;") && _connectionstringWriter.Contains("Database=ezgo_dev"))
            {
                return "DEVELOPMENT";
            } else if (_connectionstringWriter.Contains("Server=192.168.150.90;Port=54321;") && _connectionstringWriter.Contains("Database=ezgo_production_testing"))
            {
                return "TESTING PRODUCTION";
            } else if(_connectionstringWriter.Contains("test-postgresql"))
            {
                return "TESTING";
            } else if (_connectionstringWriter.Contains("acc-aurora-postgresql"))
            {
                return "ACCEPTANCE";
            } else if (_connectionstringWriter.Contains("prod-aurora-postgresql"))
            {
                return "PRODUCTION";
            } else if (_connectionstringWriter.Contains("192.168.242.") && _connectionstringWriter.Contains("developer"))
            {
                return "LOCAL";
            }
            return "UNKNOWN";
        }
        #endregion

        #region - connectionstring enhancements to be implemented -
        /// <summary>
        /// ConnectionStringEnhancer; Enhance connection string with parameters;
        /// NOTE! not yet implemented.
        /// </summary>
        /// <param name="connectionString">incoming connection string</param>
        /// <returns>enhanced connection string</returns>
        private string ConnectionStringEnhancer(string connectionString)
        {
            if (connectionString != null)
            {
                var connectionStringParts = connectionString.Split(";");
                if (connectionStringParts.Length > 0)
                {
                    var connectionStringBuilder = new StringBuilder();
                    List<string> keys = new List<string>();
                    foreach (var keyVal in connectionStringParts)
                    {

                        if (keyVal.Contains("="))
                        {
                            var col = keyVal.Split("=");
                            if (col.Length > 1)
                            {
                                var key = col[0];
                                var value = col[1];
                                keys.Add(key);
                                connectionStringBuilder.AppendFormat("{0}={1};", key, ConnectionStringValueOutput(key: key, value: value));
                            }
                            else
                            {
                                connectionStringBuilder.AppendFormat("{0};",keyVal);
                            }

                        }
                    }

                    if(!keys.Contains("Connection Idle Lifetime"))
                    {

                    }

                    connectionString = connectionStringBuilder.ToString();

                    connectionStringBuilder.Clear();
                    connectionStringBuilder = null;
                }

            }
            return connectionString;
        }

        /// <summary>
        /// ConnectionStringValueOutput; Get output based on key; Depending on key setting is configurable. 
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <param name="value">Value for key</param>
        /// <returns>string containing the value</returns>
        private string ConnectionStringValueOutput(string key, string value)
        {
            var output = value;
            switch (key)
            {
                case "Connection Idle Lifetime": output = "10"; break;
                case "Connection Pruning Interval": output = "5"; break;
                case "Minimum Pool Size": output = "1"; break;
                case "Application Name": output = string.IsNullOrEmpty(_applicationName) ? "EZGO_DEFAULT" : _applicationName; break;
                case "Keepalive": output = "10"; break;
                case "Command Timeout": output = "120"; break;
                case "Maximum Pool Size": output = "100"; break;

            }
            //Examples for values
            //Connection Idle Lifetime=10;Connection Pruning Interval=5;Minimum Pool Size=1;Application Name=EZGO_API;
            //Keepalive=10;Connection Idle Lifetime=2400;Command Timeout=1200;Internal Command Timeout=-1;No Reset On Close=True;
            //Maximum Pool Size=100
            return output;
        }
        #endregion

    }
}
