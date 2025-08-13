using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Models.Reports;

namespace EZGO.Maui.Core.Interfaces.Reports
{
    public interface ITaskPercentageCounter
    {
        public static int CalculatePercentage(List<ReportsAverage> list, string text)
        {
            return (int)Math.Round(list.FirstOrDefault(x => x.Name.Contains(text.ToLower(), StringComparison.OrdinalIgnoreCase))?.AverageNr ?? 0);
        }
    }
}
