using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class StatisticMonthYearItem
    {
        public int CountNr { get; set; }
        public DateTime? Date { get { return this.Month > 0 && this.Year > 0 ? new DateTime(day: 1, month: this.Month, year: this.Year) : new Nullable<DateTime>(); } }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
