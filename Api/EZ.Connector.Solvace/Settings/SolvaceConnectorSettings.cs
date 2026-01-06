using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Solvace.Settings
{
    public static class SolvaceConnectorSettings
    {
        /// <summary>
        /// ACTION_CONNECTOR_URL_CONFIG_KEY; Key for config part that contains the primary URL
        /// </summary>
        public const string ACTION_CONNECTOR_URL_CONFIG_KEY = "SolvaceConnectorConfig:UrlActionConnection";
        /// <summary>
        /// UID_CONFIG_KEY; Key for the user id or username used within the basic authentication
        /// </summary>
        public const string UID_CONFIG_KEY = "SolvaceConnectorConfig:UID";
        /// <summary>
        /// PWD CONFIG_KEY; key for password of user
        /// </summary>
        public const string PWD_CONFIG_KEY = "SolvaceConnectorConfig:PWD";
        /// <summary>
        /// ACTION_CONNECTION_COMPANIES_CONFIG_KEY; Key for value containing all companies that may use the connector.
        /// </summary>
        public const string ACTION_CONNECTION_COMPANIES_CONFIG_KEY = "SolvaceConnectorConfig:ActionConnectionActiveCompanies";


    }
}
