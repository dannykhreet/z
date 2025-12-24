using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    /// <summary>
    /// StatisticGenericItem; General statistic items, used for collection that only contain an id and some kind of count number and a reference name.
    /// </summary>
    public class StatisticGenericItem
    {
        public decimal? AverageNr { get; set; }
        public int? Id { get; set; }
        public int? CountNr { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? Week { get; set; }
    }
}
