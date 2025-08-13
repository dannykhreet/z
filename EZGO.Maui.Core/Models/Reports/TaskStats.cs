namespace EZGO.Maui.Core.Models.Reports
{
    public class TaskStats
    {
        public string Title { get; set; }
        public int Total { get; set; }
        public int DoneNr { get; set; }
        public int Ok { get; set; }
        public double PercentageOk { get; set; }
        public int NotOk { get; set; }
        public double PercentageNotOk { get; set; }
        public double EndNotOk { get; set; }
        public int Skipped { get; set; }
        public double PercentageSkipped { get; set; }
        public double EndSkipped { get; set; }
        public int Todo { get; set; }
        public double PercentageTodo { get; set; }

        public int Percentage { get; set; }
    }
}
