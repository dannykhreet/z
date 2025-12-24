using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EZGO.Api.Interfaces.Settings;
using Microsoft.Extensions.Configuration;

namespace EZGO.Api.Settings.Helpers
{
    /// <summary>
    /// ConfigurationHelper; ConfigurationHelper gets settings from the configuration locations of the project.
    /// </summary>
    public class ConfigurationHelper : IConfigurationHelper
    {
        private readonly IConfiguration _configuration;
        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GetValueAsString; Get configuration setting as normal string.
        /// Setting will be retrieved from the environment settings, if not available the configuration json will be checked.
        /// If the environment and config json settings are not available empty string will be returned.
        /// </summary>
        /// <param name="keyname">Key of setting</param>
        /// <returns>string containing setting value.</returns>
        public string GetValueAsString(string keyname) {
            if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(keyname)))
            {
                return Environment.GetEnvironmentVariable(keyname);
            } else if (_configuration.GetSection(keyname) !=null && !string.IsNullOrEmpty(_configuration.GetSection(keyname).Value)) {
                return _configuration.GetSection(keyname).Value;
            }
            return "";
        }

        /// <summary>
        /// GetValueAsBool; Get configuration setting as normal boolean.
        /// Setting will be retrieved from the environment settings, if not available the configuration json will be checked.
        /// If the environment and config json settings are not available false will be returned.
        /// </summary>
        /// <param name="keyname">Key of setting</param>
        /// <returns>bool setting value.</returns>
        public bool GetValueAsBool(string keyname)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(keyname)))
            {
                return Convert.ToBoolean(Environment.GetEnvironmentVariable(keyname));
            }
            else if (_configuration.GetSection(keyname) != null && !string.IsNullOrEmpty(_configuration.GetSection(keyname).Value))
            {
                return Convert.ToBoolean(_configuration.GetSection(keyname).Value);
            }
            return false;
        }


        /// <summary>
        /// GetValueAsBool; Get configuration setting as normal boolean.
        /// Based on the CompanyId data will be checked if it contains the companyid.
        /// </summary>
        /// <param name="keyname">Key of setting</param>
        /// <returns>bool setting value.</returns>
        public bool GetValueAsBoolBasedOnCompanyId(string keyname, int companyid)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(keyname)))
            {
                var val = Environment.GetEnvironmentVariable(keyname);
                return (val.Split(",").Contains(companyid.ToString()));

            }
            else if (_configuration.GetSection(keyname) != null && !string.IsNullOrEmpty(_configuration.GetSection(keyname).Value))
            {
                var val = _configuration.GetSection(keyname).Value;
                return (val.Split(",").Contains(companyid.ToString()));
            }
            return false;
        }

        /// <summary>
        /// GetValueAsBool; Get configuration setting as normal int.
        /// Setting will be retrieved from the environment settings, if not available the configuration json will be checked.
        /// If the environment and config json settings are not available 0 (zero) will be returned as value.
        /// </summary>
        /// <param name="keyname">Key of setting</param>
        /// <returns>int setting value.</returns>
        public int GetValueAsInteger(string keyname)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(keyname)))
            {
                return Convert.ToInt32(Environment.GetEnvironmentVariable(keyname));
            }
            else if (_configuration.GetSection(keyname) != null && !string.IsNullOrEmpty(_configuration.GetSection(keyname).Value))
            {
                return Convert.ToInt32(_configuration.GetSection(keyname).Value);
            }
            return 0;
        }
    }
}
