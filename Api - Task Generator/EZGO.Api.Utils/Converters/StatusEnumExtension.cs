using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class StatusEnumExtension
    {
        public static string ToDatabaseString(this TaskStatusEnum status)
        {
            string output = "";
            switch(status)
            {
                case TaskStatusEnum.Ok: case TaskStatusEnum.Skipped: case TaskStatusEnum.Todo: output = status.ToString().ToLower();  break;
                case TaskStatusEnum.NotOk: output = "not ok"; break;
            }
            return output;
        }
    }
}
