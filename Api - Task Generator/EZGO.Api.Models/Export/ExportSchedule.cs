using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Export
{
    public class ExportSchedule
    {
        public int CompanyId { get; set; }
        public int HoldingId { get; set; }
        public string EmailTo { get; set; }
        public string TimeZone { get; set; }
        public string OutputName { get; set; }
        public bool IsActive { get; set; }
        /* data retrieval */
        public bool RetrievalUserDataAtoss { get; set; }
        public bool RetrievalScoreDataAtoss { get; set; }
        public bool RetrievalMasterDataAtoss { get; set; }
        /* deliveries */
        public bool DeliverySFTP { get; set; }
        public bool DeliveryEmail { get; set; }
        public bool DeliveryPost { get; set; }
        /* outputs */
        public bool OutputCSV { get; set; }
        public bool OutputXLSX { get; set; }
        public bool OutputSQL { get; set; }
        public bool OutputJSON { get; set; }
        /* settings for specifics */
        public string SettingSFTPLocation { get; set; }
        public string SettingSFTPUserName { get; set; }
        public string SettingSFTPPassword { get; set; }
        /* output names */
        public string OutputNameUserDataAtoss { get; set; }
        public string OutputNameScoreDataAtoss { get; set; }
        public string OutputNameMasterDataAtoss { get; set; }
        /* schedules */
        public List<ExportScheduleItem> ScheduleItems { get; set; }
    }
}
