using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Ultimo.Models
{
    public class UltimoActionResponse
    {
        public string Id { get; set; }
        public string StatusActiveDate { get; set; }
        public int Context { get; set; }
        public string DataProvider { get; set; }
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public string FeedbackText { get; set; }
        public string StatusFinishedDate { get; set; }
        public int Status { get; set; }
        public string StatusCreatedReportDate { get; set; }
        public string ReportText { get; set; }
        public string ScheduledStartDate { get; set; }
        public string ServiceContractTargetFinishedDate { get; set; }
        public string ServiceContractTargetResponseDate { get; set; }
        public string TargetDate { get; set; }
        public string Text { get; set; }
        public string CostCenter { get; set; }
        public string Department { get; set; }
        public string Employee { get; set; }
        public string Equipment { get; set; }
        public string EquipmentType { get; set; }
        public string FailType { get; set; }
        public string ProcessFunction { get; set; }
        public string Priority { get; set; }
        public string ProgressStatus { get; set; }
        public string ReportForeignKeyEmployee { get; set; }
        public string Site { get; set; }
        public string SkillCategory { get; set; }
        public string Space { get; set; }
        public string ServiceContract { get; set; }
        public string Vendor { get; set; }
        public string WorkOrderType { get; set; }

    }
}
