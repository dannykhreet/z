using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    /// <summary>
    /// MachineStatusTypeEnumExtension; 
    /// </summary>
    public static class MachineStatusTypeEnumExtension
    {
        public static string ToDatabaseString(this MachineStatusTypeEnum status)
        {
            string output = "";
            switch (status)
            {
                case MachineStatusTypeEnum.Running: case MachineStatusTypeEnum.Stopped: output = status.ToString().ToLower(); break;
                case MachineStatusTypeEnum.NotApplicable: output = "not_applicable"; break;
            }
            return output;
        }
    }
}
