using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class ReviewData
    {
        public int templateid { get; set; }
        public string name { get; set; }
        public int areaid { get; set; }
        public int realstatus { get; set; }
        public int signedbyid { get; set; }
        public int week { get; set; }
        public int day { get; set; }
        public int shiftperiod { get; set; }
        public int predictedstatus { get; set; }
    }
}
