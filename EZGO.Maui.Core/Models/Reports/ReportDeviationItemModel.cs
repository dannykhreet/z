using EZGO.Api.Models.Reports;
using EZGO.Maui.Core.Models.ModelInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportDeviationItemModel : ReportDeviationItem, IBase<BasicReportDeviationItemModel>
    {
        public new double Percentage { get; set; }
        public new int Id { get; set; }
        public new int CountNr { get; set; }
        public new int ParentTemplateId { get; set; }

        public double MaxPercentage { get; set; }
        public double CalculatedPercentage { get; set; }
        public int PercentageActionDone { get; set; }
        public string DisplayAmount { get; set; }

        public BasicReportDeviationItemModel ToBasic()
        {
            var result = new BasicReportDeviationItemModel
            {
                Percentage = this.Percentage,
                Id = this.Id,
                CountNr = this.CountNr,
                ParentTemplateId = this.ParentTemplateId,
                MaxPercentage = this.MaxPercentage,
                CalculatedPercentage = this.CalculatedPercentage,
                PercentageActionDone = this.PercentageActionDone,
                DisplayAmount=this.DisplayAmount,
                ActionCount = this.ActionCount,
                ActionDoneCount = this.ActionDoneCount,
                Name=this.Name
            };
            return result;
        }
    }
}
