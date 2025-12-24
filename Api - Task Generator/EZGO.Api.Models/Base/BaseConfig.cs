using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Base
{
    /// <summary>
    /// BaseConfig; Base configuration for market items for use with external platforms.
    /// </summary>
    public class BaseConfig
    {
        /// <summary>
        /// IsEnabled; Check if config is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Version; Version of item
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// SystemKey; Technical key
        /// </summary>
        public string SystemKey { get; set; }

        /// <summary>
        /// ApiKey; API key used for config
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Uri; Uri of system. 
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// UserName; Username used for system
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password used for system.
        /// </summary>
        public string Password { get; set; }

    }
}
