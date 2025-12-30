using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class IntegerConverter
    {
        /// <summary>
        /// Convert a object to integer, it must contain a ValueKind containing a numeric value
        /// </summary>
        /// <param name="value">Value containing possible value.</param>
        /// <returns>return true / false; Will default to false.</returns>
        public static int ConvertObjectToInteger(object value)
        {
            int output = 0;
            if (value != null)
            {
                int.TryParse(value.ToString(), out output);
            }
            return output;
        }
    }
}
