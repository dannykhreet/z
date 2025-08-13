using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Models.Reports;

namespace EZGO.Maui.Core.Interfaces.Reports
{
    public interface IResultReportCounter
    {
        public static int CountMyResult(List<ReportsCount> list, string text)
        {
            return list.FirstOrDefault(x => x.Name.Equals(text, StringComparison.OrdinalIgnoreCase))?.CountNr ?? 0;
        }

        public static int CountMyResultTextEquals(List<ReportsCount> list, string text)
        {
            return list.FirstOrDefault(x => x.Name.ToLower() == text.ToLower())?.CountNr ?? 0;
        }
    }
}
