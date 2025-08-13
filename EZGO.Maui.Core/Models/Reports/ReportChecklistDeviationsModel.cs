using EZGO.Api.Models.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportChecklistDeviationsModel : ReportChecklistDeviations
    {
        public new List<ReportDeviationItemModel> DeviationsSkipped { get; set; }
        public new List<ReportDeviationItemModel> DeviationsNotOk { get; set; }

        public ReportChecklistDeviationsModel()
        {
            DeviationsSkipped = new List<ReportDeviationItemModel>();
            DeviationsNotOk = new List<ReportDeviationItemModel>();
        }
    }
}