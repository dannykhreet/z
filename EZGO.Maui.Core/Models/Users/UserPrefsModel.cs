using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Users
{
    public class UserPrefsModel
    {
        public int UserId { get; set; }
        public TimespanTypeEnum ReportPeriod { get; set; }
    }
}
