using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    public class DatabaseTimezoneItem
    {
        public string Name { get; set; }
        public string Abbrivation { get; set; }
        public string UtcOffset { get; set; }
        public bool IsDayLightSavingsTime { get; set; }
    }
}
