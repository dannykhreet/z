using System;
using EZGO.Maui.Core.Enumerations;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportsCount
    {
        public int Id { get; set; }
        public int CountNr { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public int DayOfYear { get; set; }
        public DateTime ReportDate { get; set; }
        public string Subscript { get; set; }

        public decimal AverageNr { get; set; }

        public int MaxCountNr { get; set; }

        public int NrDone { get; set; }
        public int CountNrOverdue { get; set; }
        public int CountNrResolved { get; set; }
        public int CountNrUnresolved { get; set; }
        public int CountNrUnresolvedNotOverdue { get; set; }
        public int CountNrNotResolved => CountNr + CountNrOverdue + CountNrUnresolved + CountNrUnresolvedNotOverdue;

        public int NrOk { get; set; }
        public int NrNotOk { get; set; }
        public int NrTodo { get; set; }
        public int NrSkipped { get; set; }

        public double PercentageRelative { get; set; }
        public double PercentageOk { get; set; }
        public double PercentageNotOk { get; set; }
        public double PercentageSkipped { get; set; }
        public double PercentageStarted { get; set; }
        public double PercentageResolved { get; set; }
        public double PercentageOverdue { get; set; }

        public AggregationTimeInterval? TimespanType { get; set; }
    }
}