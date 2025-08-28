using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Models.Reports;

namespace EZGO.Maui.Core.Interfaces.Reports
{
    public interface IReportNameRetriver
    {
        public static string GetNameByConstant(List<ReportsAverage> list, string text)
        {
            return list.FirstOrDefault(x => x.Name.Contains(text.ToLower(), StringComparison.OrdinalIgnoreCase))?.Name ?? text;
        }
    }
}
