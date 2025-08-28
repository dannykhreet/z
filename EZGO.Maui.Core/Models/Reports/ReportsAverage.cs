using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportsAverage
    {
        public string Name { get; set; }
        public float? AverageNr { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public int Year { get; set; }
    }
}