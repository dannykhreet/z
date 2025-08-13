using EZGO.Api.Models.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportTaskDeviationsModel : ReportTaskDeviations
    {
        public new List<ReportDeviationItemModel> DeviationsSkipped { get; set; }
        public new List<ReportDeviationItemModel> DeviationsNotOk { get; set; }
        public new List<ReportDeviationItemModel> DeviationsTodo { get; set; }

        public ReportTaskDeviationsModel()
        {
            DeviationsSkipped = new List<ReportDeviationItemModel>();
            DeviationsNotOk = new List<ReportDeviationItemModel>();
            DeviationsTodo = new List<ReportDeviationItemModel>();
        }
    }
}