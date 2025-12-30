namespace EZGO.Api.Models.Reports
{
    public class ActionCountsStatistic
    {
        public int TotalActions { get; set; }
        public int ResolvedActions { get; set; }
        public int OpenActions { get; set; }
        public int OverdueActions { get; set; }
    }
}