using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EEZGO.Api.Utils.Data
{
    /// <summary>
    /// DataReaderExtensions; Extension for use with the data reader objects. (can be used in some cases with objects that make use of the IDataRecord interface).
    /// </summary>
    public static class DataReaderExtensions
    {
        /// <summary>
        /// HasColumn; For use with data readers for checking if certain columns exist. This can be used when multiple data sources or logic uses multiple stored procedures which have
        /// the same output structure but only differ for a few columns. With this extension you can check if the columns exists before processing is.
        /// </summary>
        /// <param name="dr">Data reader that is used for checking the column existence.</param>
        /// <param name="columnName">ColumnName that will be searched for.</param>
        /// <returns>true/false depending on outcome.</returns>
        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
