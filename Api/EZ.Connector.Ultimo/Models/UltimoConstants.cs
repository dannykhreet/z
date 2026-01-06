using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Ultimo.Models
{
    public static class UltimoConstants
    {
        public static string EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT { get; set; } = "READY_TO_BE_SENT";
        public static string EXTERNAL_RELATION_STATUS_SENT { get; set; } = "SENT";
        public static string EXTERNAL_RELATION_STATUS_ERROR { get; set; } = "ERROR";

        public static string EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT_DESCRIPTION { get; set; } = "The action is ready to be sent to Ultimo.";
        public static string EXTERNAL_RELATION_STATUS_SENT_DESCRIPTION { get; set; } = "The action has been successfully sent to Ultimo.";
        public static string EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION { get; set; } = "EZ-GO Action not sent to Ultimo! Status Code: {0}, Content: {1}";

        public static string LOGGING_STATUS_SENT_DESCRIPTION { get; set; } = "EZ-GO Action successfully added to Ultimo!";
        public static string LOGGING_STATUS_ERROR_DESCRIPTION { get; set; } = "EZ-GO Action not sent to Ultimo! (Statuscode {0})";
    }
}
