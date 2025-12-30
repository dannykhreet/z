using System;
using System.Xml.Linq;

namespace WebApp.Models.Pdf
{
    public class CompletedPdfHeaderModel
    {
        public string CompletedListName { get; set; }
        public string CompletedByUserName { get; set; }
        public DateTime CompletedAt { get; set; }
        public int AuditTotalScorePercentage { get; set; }

        public int CompletedAuditHeaderHeight
        {
            get { return 22 + (CompletedListName.Length / 80 * 5); }
        }

        public int CompletedChecklistHeaderHeight
        {
            get { return 22 + (CompletedListName.Length / 80 * 5); }
        }
    }
}
