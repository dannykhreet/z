using System.Collections.Generic;
using System.Linq;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Models.Reports
{
    public class MyEzFeedStats : NotifyPropertyChanged
    {
        public int MyCommentsTotal { get; set; }
        public int MyLikesTotal { get; set; }
        public int MyPostsTotal { get; set; }

        public MyEzFeedStats(IEnumerable<ReportsCount> reportsCount)
        {
            MyCommentsTotal = reportsCount?.FirstOrDefault(r => r.Name == ReportsConstants.MyCommentsTotal)?.CountNr ?? 0;
            MyLikesTotal = reportsCount?.FirstOrDefault(r => r.Name == ReportsConstants.MyLikesTotal)?.CountNr ?? 0;
            MyPostsTotal = reportsCount?.FirstOrDefault(r => r.Name == ReportsConstants.MyPostsTotal)?.CountNr ?? 0;
        }
    }
}
