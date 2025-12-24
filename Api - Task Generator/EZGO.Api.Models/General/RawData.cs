using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.General
{
    /// <summary>
    /// RawData; RawData collection. Containing a list of columns and a list of items containing a list of data (datarow)
    /// Can be used for displaying a set of raw data
    /// </summary>
    public class RawData
    {
        public List<string> Columns { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Data { get; set; }
    }
}
