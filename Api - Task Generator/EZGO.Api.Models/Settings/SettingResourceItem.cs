using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    public class SettingResourceItem
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public string Value { get; set; }

        public int? ResourceId { get; set; }

        public int? CompanyId { get; set; }

        public int? HoldingId { get; set; }
    }
}
