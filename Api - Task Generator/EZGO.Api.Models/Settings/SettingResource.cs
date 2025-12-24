using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    /// <summary>
    /// SettingResource; Settings resource; Contains a certain settings. Depending on type certain Resource items will be added.
    /// E.g. when the SettingsResource is meant per company, a list of company items it added.
    /// </summary>
    public class SettingResource
    {
        /// <summary>
        /// Id; internal id of record in db
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of setting
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description, gives a description of the settings, used for management purposes.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Key of the setting, used for referencing within code (see ResourceSettingTypeEnum)
        /// </summary>
        public string SettingsKey { get; set; }
        /// <summary>
        /// Value of the setting, in case of a general setting the data is also stored with the same record as settingsresource.
        /// Other types are stored in the specific values.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// SettingResourceType; type of setting; based on SettingResourceSettingsTypeEnum enum.
        /// </summary>
        public SettingResourceSettingsTypeEnum SettingResourceType { get; set; }
        /// <summary>
        /// ResourceItems; items that are available for this specific setting. When a list of setting resources is loaded all the data that is found for this specific resource is added.
        /// If a list of company specific resources are retrieved only the specific company settings are used.
        /// </summary>
        public List<SettingResourceItem> ResourceItems { get; set; }

    }
}
