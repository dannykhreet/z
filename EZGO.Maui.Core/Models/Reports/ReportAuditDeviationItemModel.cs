using EZGO.Api.Models.Reports;
using EZGO.Maui.Core.Models.ModelInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class ReportAuditDeviationItemModel : ReportAuditDeviationItem, IBase<ReportDeviationItemModel>
    {
        public new int NumberOfQuestions { get; set; }
        public double QuestionWeight { get; set; }
        public new double DeviationScore { get; set; }
        public new double DeviationPercentage { get; set; }

        public double MaxDeviationScore { get; set; }
        public int PercentageActionDone { get; set; }

        public ReportDeviationItemModel ToBasic()
        {
            var result = new ReportDeviationItemModel
            {
                ActionCount = this.ActionCount,
                ActionDoneCount = this.ActionDoneCount,
                Id = this.TaskTemplateId,
                Name = this.TaskTemplateName,
                Percentage = this.DeviationPercentage
            };
            return result;
        }
    }
}
