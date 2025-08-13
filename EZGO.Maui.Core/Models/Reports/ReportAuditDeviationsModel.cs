using EZGO.Api.Models.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportAuditDeviationsModel : ReportAuditDeviations
    {

        public new List<ReportAuditDeviationItemModel> Deviations { get; set; }
        public new List<ReportDeviationItemModel> DeviationsSkipped { get; set; }

        public ReportAuditDeviationsModel()
        {
            Deviations = new List<ReportAuditDeviationItemModel>();
            DeviationsSkipped = new List<ReportDeviationItemModel>();
        }
    }
}