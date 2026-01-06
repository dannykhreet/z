using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.SAP.Settings
{
    public static class SAPConnectorSettings
    {
        /// <summary>
        /// ACTION_CONNECTOR_URL_CONFIG_KEY; Key for config part that contains the primary URL
        /// </summary>
        public const string ACTION_CONNECTOR_URL_CONFIG_KEY = "SAPConnectorConfig:UrlActionConnection";
        /// <summary>
        /// UID_CONFIG_KEY; Key for the user id or username used within the basic authentication
        /// </summary>
        public const string UID_CONFIG_KEY = "SAPConnectorConfig:UID";
        /// <summary>
        /// PWD CONFIG_KEY; key for password of user
        /// </summary>
        public const string PWD_CONFIG_KEY = "SAPConnectorConfig:PWD";
        /// <summary>
        /// ACTION_CONNECTION_COMPANIES_CONFIG_KEY; Key for value containing all companies that may use the connector.
        /// </summary>
        public const string ACTION_CONNECTION_COMPANIES_CONFIG_KEY = "SAPConnectorConfig:ActionConnectionActiveCompanies";


    }
}
