using EZGO.Api.Models.Export;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.WorkerService.Exporter.Objects
{
    public class ExportScheduleSettings
    {
        /* Holding, Company information */
        public int CompanyId { get; set; }
        public int HoldingId { get; set; }
        /* Scheduling */
        public int RunTime { get; set; }
        public string TimeZone { get; set; }
        public int DataRangeStartTime { get; set; }
        public int DataRangeEndTime { get; set; }
        public int DataRangeDirection { get; set; }
        public List<int> DayOfWeek { get; set; }
        public int TimeFrameInMinutes { get; set; }
        public DateTime LastRunDateCurrentSession { get; set; }
        public DateTime StartRunDateCurrentSession { get; set; }
        public ExportSchedule ExportSchedule { get; set; }
        /* Location where data needs to be delivered. */
        public bool DeliveryPost { get; set; }
        public bool DeliverySFTP { get; set; }
        public bool DeliveryEmail { get; set; } //not used
        /* Kind of output that needs to be delivered */
        public bool OutputCSV { get; set; }
        public bool OutputXLS { get; set; }
        public bool OutputSQL { get; set; }
        public bool OutputJSON { get; set; }
        /* Retrieval of data to be delivered */
        public bool RetrievalUserDataAtoss { get; set; }
        public bool RetrievalScoreDataAtoss { get; set; }
        public bool RetrievalMasterDataAtoss { get; set; }
        /* SFTPSettings */
        public string SettingSFTPLocation { get; set; }
        public string SettingSFTPUserName { get; set; }
        public string SettingSFTPPassword { get; set; }
        /* Output names */
        public string OutputName { get; set; }
        public string OutputNameUserDataAtoss { get; set; }
        public string OutputNameScoreDataAtoss { get; set; }
        public string OutputNameMasterDataAtoss { get; set; }
        /* Settings */
        public bool IsActive { get; set; }
        /* Logic */
        public bool Running { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var prop in this.GetType().GetProperties())
            {
                sb.Append(string.Format("[{0}={1}]", prop.Name, prop.GetValue(this, null)));
            }

            return sb.ToString() ;
        }
    }
}
