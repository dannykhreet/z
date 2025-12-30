using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Export
{
    public class ExportData
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? Date { get; set; }
        /// <summary>
        /// ExportType: XLSX; CSV depending on what kind of export types the API supports, these differ per export.
        /// </summary>
        public string ExportType { get; set; }

        /// <summary>
        /// CompanyId; currently only for internal use, do not expose through front-end code;
        /// </summary>
        public int? CompanyId { get; set; }
    }
}
