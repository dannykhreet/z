using EZGO.Api.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.WorkerService.Processor.Data
{
    /// <summary>
    /// ConnectionHelper; helper object class, for getting and or building connection strings to databases and or external parties.
    /// The connection helper needs to be implemented as a service (startup.cs) see <see cref="StartupHelperExtension">StartupHelperExtension</see>.
    /// </summary>
    public class ConnectionHelper : IConnectionHelper
    {
        public const string DEFAULT_CONNECTIONSTRING_NAME = "DefaultConnection"; //ConnectionString located in the ApplicationConfig JSON (depending on project is differently named)
        public const string DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT = "API_DEFAULTCONNECTION";
        private static string _connectionstring = string.Empty;

        public ConnectionHelper(string connectionString)
        {
            _connectionstring = connectionString;
        }

        /// <summary>
        /// GetConnectionString; Gets the connection string from a config (can be local config or server, where server is leading).
        /// </summary>
        /// <returns>String containing a connection string for PostgreSQL in this case.</returns>
        public string GetConnectionString()
        {
            return _connectionstring;
        }

        public string GetConnectionStringReader()
        {
            return _connectionstring;
        }

        /// <summary>
        /// GetConnectionStringWriter; Gets the connection string from a config (can be local config or server, where server is leading).
        /// </summary>
        /// <returns>String containing a connection string for PostgreSQL in this case.</returns>
        public string GetConnectionStringWriter()
        {
            return _connectionstring;
        }

        /// <summary>
        /// GetActiveDatabaseEnvironment; Get's and checks based on the database connection which environment is currently running.
        /// </summary>
        /// <returns>Predefined string, use within applications for extra checks or visual queues for user. </returns>
        public string GetActiveDatabaseEnvironment()
        {
            if (_connectionstring.Contains("Server=192.168.150.90;Port=54321;") && _connectionstring.Contains("Database=ezgo_dev"))
            {
                return "DEVELOPMENT";
            }
            else if (_connectionstring.Contains("Server=192.168.150.90;Port=54321;") && _connectionstring.Contains("Database=ezgo_production_testing"))
            {
                return "TESTING PRODUCTION";
            }
            else if (_connectionstring.Contains("Server=192.168.150.90;Port=54321;") && _connectionstring.Contains("Database=ezgo_test"))
            {
                return "TESTING";
            }
            else if (_connectionstring.Contains("acc-aurora-postgresql"))
            {
                return "ACCEPTANCE";
            }
            else if (_connectionstring.Contains("prod-aurora-postgresql"))
            {
                return "PRODUCTION";
            }
            else if (_connectionstring.Contains("192.168.242.") && _connectionstring.Contains("developer"))
            {
                return "LOCAL";
            }
            return "UNKNOWN";
        }

    }
}
