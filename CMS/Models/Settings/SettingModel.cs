using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Settings
{
    public class SettingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SettingsKey { get; set; }
        public string Value { get; set; }
        public SettingResourceSettingsTypeEnum SettingResourceType {get; set; }

    }
}
