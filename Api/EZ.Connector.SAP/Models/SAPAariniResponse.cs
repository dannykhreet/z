using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.SAP.Models
{
    public class SAPAariniResponse
    {
        public string NotificationNumber { get; set; }
        public List<string> Messages { get; set; }
        public string Message { get; set; }
    }
}
