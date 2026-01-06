using EZGO.Api.Models.Export;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.WorkerService.Provisioner.Objects
{
    public class ProvisionerScheduleSettings
    {
        /* Holding, Company information */
        public int CompanyId { get; set; }
        public int HoldingId { get; set; }
        /* Scheduling */
        public int RunTime { get; set; }
        public string TimeZone { get; set; }
        public DateTime LastRunDateCurrentSession { get; set; }
        public DateTime StartRunDateCurrentSession { get; set; }
        public ExportSchedule ExportSchedule { get; set; }
        /* Location where data needs to be delivered. */
        public bool RetrievalSFTP { get; set; }
        /* Retrieval of data to be delivered */
        public bool RetrievalUserDataAtoss { get; set; }
        public bool RetrievalScoreDataAtoss { get; set; }
        public bool RetrievalMasterDataAtoss { get; set; }
        public bool IgnoreFirstRowForProcessing { get; set; } //-> due to customer adds header (non complient) ignore first row
        /* SFTPSettings */
        public string SettingSFTPLocation { get; set; }
        public string SettingSFTPUserName { get; set; }
        public string SettingSFTPPassword { get; set; }
        /* Output names */
        public string InputName { get; set; }
        public string InputNameUserDataAtoss { get; set; }
        public string InputNameScoreDataAtoss { get; set; }
        public string InputNameMasterDataAtoss { get; set; }
        /* Schedule type */
        public string ScheduleType { get; set; }
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
