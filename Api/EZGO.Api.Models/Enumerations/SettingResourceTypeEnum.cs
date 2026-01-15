using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// SettingResourceTypeEnum; References keys in settings table.
    /// </summary>
    public enum SettingResourceTypeEnum
    {
        /// <summary>
        /// Setting for company timezone. Will be a textual reference to the timezones that can be used in the queries.
        /// </summary>
        COMPANY_TIMEZONE = 1,

        FEATURE_TIER_ESSENTIALS = 16,

        FEATURE_TIER_ADVANCED = 17,

        FEATURE_TIER_PREMIUM = 18,

        COMPANY_TAG_LIMIT = 74,

        GENERAL_TAG_LIMIT = 75,

        COMPANY_TAGGROUP_LIMIT = 76,

        GENERAL_TAGGROUP_LIMIT = 77,

        COMPANY_PROPERTY_LIMIT = 78,

        GENERAL_PROPERTY_LIMIT = 79,

        COMPANY_OPEN_FIELDS_LIMIT = 80,

        GENERAL_OPEN_FIELDS_LIMIT = 81,
    }
}
