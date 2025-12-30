using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class BooleanConverter
    {
        /// <summary>
        /// Convert a object to boolean, it must contain a ValueKind containing True, true. TRUE, 1, 0, False, false, FALSE
        /// </summary>
        /// <param name="value">Value containing possible value.</param>
        /// <returns>return true / false; Will default to false.</returns>
        public static bool ConvertObjectToBoolean(object value)
        {
            if (value != null)
            {
                if (value.ToString().ToLower() == "true" || value.ToString().ToLower() == "1") return true;
                if (value.ToString().ToLower() == "false" || value.ToString().ToLower() == "0") return false;
                return false;
            }
            return false;
        }
    }
}
