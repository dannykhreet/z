using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.WorkerService.Exporter.Utils
{
    public static class Converters
    {

        public static DateTime MilitaryToDateTime(int militaryTime)
        {
            int hours = militaryTime / 100;
            int minutes = militaryTime - hours * 100;
            DateTime dateTimeResult = DateTime.MinValue;

            dateTimeResult = dateTimeResult.AddHours(hours);
            dateTimeResult = dateTimeResult.AddMinutes(minutes);

            return dateTimeResult;
        }
    }
}
