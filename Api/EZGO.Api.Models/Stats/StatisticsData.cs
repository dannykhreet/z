using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class StatisticsData
    {
        public List<string> Columns { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Data { get; set; }
    }
}
