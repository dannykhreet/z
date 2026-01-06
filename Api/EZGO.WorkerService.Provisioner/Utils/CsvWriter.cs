using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO add iso code compliant number
namespace EZGO.WorkerService.Provisioner.Utils
{
    /// <summary>
    /// CsvWriter; writer functionality for writing a DataSource to a stream containing a csv like format for use with exports.
    /// </summary>
    public sealed class CsvWriter
    {
        private const string DEFAULT_DELIMITER = ";";
        private const string DEFAULT_REPLACE_DELIMITER = ":";

        /// <summary>
        /// WriteFromDataTable; write a per record based CSV to a stream based on a DataTable.
        /// </summary>
        /// <param name="source">DataTable source, containing the information for creating the CSV stream.</param>
        /// <param name="stream">IO.Stream, will be the stream that the data is converted to.</param>
        /// <param name="includeHeaders">Boolean, default set to true for adding the headers from the DataTable as the first row in the CSV stream.</param>
        /// <returns>Task (void, not used only for async call)</returns>
        public static async Task WriteFromDataTable(DataTable source, Stream stream, bool includeHeaders = true, bool leaveStreamOpen = false)
        {

            await using (StreamWriter sw = new StreamWriter(stream, leaveOpen: leaveStreamOpen))
            {
                if (includeHeaders)
                {
                    try
                    {
                        await sw.WriteLineAsync(String.Join(DEFAULT_DELIMITER, CsvWriter.GetHeaderFromDataTable(source)));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                IEnumerable<String> items = null;

                foreach (DataRow row in source.Rows)
                {
                    //In case of invalid data in a specific row, that row is ignored and writing data will continue until finished.
                    try
                    {
                        items = row.ItemArray.Select(o => o is DateTime ? CsvWriter.ParseValueDateTime((DateTime)o) : CsvWriter.ParseValue(o?.ToString() ?? String.Empty));
                        await sw.WriteLineAsync(String.Join(DEFAULT_DELIMITER, items));

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }

                //cleanup
                items = null;
                await sw.DisposeAsync();
            }

        }

        /// <summary>
        /// WriteFromDataTableAsString; write a per record based CSV to a stream based on a DataTable and output it as a string
        /// </summary>
        /// <param name="source">DataTable source, containing the information for creating the CSV stream.</param>
        /// <param name="includeHeaders">Boolean, default set to true for adding the headers from the DataTable as the first row in the CSV stream.</param>
        /// <returns>String containing all data.</returns>
        public static async Task<string> WriteFromDataTableAsString(DataTable source, bool includeHeaders = true)
        {
            var sb = new StringBuilder();
            if (includeHeaders)
            {
                try
                {
                    sb.AppendLine(String.Join(DEFAULT_DELIMITER, CsvWriter.GetHeaderFromDataTable(source)));
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in source.Rows)
            {
                //In case of invalid data in a specific row, that row is ignored and writing data will continue until finished.
                try
                {
                    items = row.ItemArray.Select(o => o is DateTime ? CsvWriter.ParseValueDateTime((DateTime)o) : CsvWriter.ParseValue(o?.ToString() ?? String.Empty));
                    sb.AppendLine(String.Join(DEFAULT_DELIMITER, items));

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }
            //cleanup
            items = null;
            await Task.CompletedTask;

            return sb.ToString();

        }


        /// <summary>
        /// GetHeaderFromDataTable; Get the headers (based on DataTable.Columns) as a IEnumerable of string for use withing the WriteFromDataTable method.
        /// </summary>
        /// <param name="source">DataTable source, containing the information for the headers.</param>
        /// <returns>IEnumerable of string.</returns>
        private static IEnumerable<String> GetHeaderFromDataTable(DataTable source)
        {
            //Due to restrictions in the CSV format the value is parsed so the delimiter is not contained withing the value that is used withing a item in the header.
            IEnumerable<String> headerValues = source.Columns.OfType<DataColumn>().Select(column => ParseValue(column.ColumnName));
            return headerValues;
        }

        /// <summary>
        /// ParseValue; parses value and replaces delimiter with replacement
        /// </summary>
        /// <param name="value">String, value where possible delimited needs to be replaced.</param>
        /// <returns>String</returns>
        private static string ParseValue(string value)
        {
            return value.Replace(DEFAULT_DELIMITER, DEFAULT_REPLACE_DELIMITER).Replace("\r","").Replace("\n","");
        }

        /// <summary>
        /// ParseValue; Specifically for datetime fields. 
        /// </summary>
        /// <param name="value">Value containing a date.</param>
        /// <returns></returns>
        private static string ParseValueDateTime(DateTime? value)
        {
            if(value.HasValue)
            {
                return value.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return string.Empty;
        }

        /// <summary>
        /// ParseAndQuoteValue; parses value and adds quotes to the beginning and the end.
        /// </summary>
        /// <param name="value">String, value where possible delimited needs to be replaced.</param>
        /// <returns>String</returns>
        private static string ParseAndQuoteValue(string value)
        {
            return String.Concat("\"", value.Replace("\"", "\"\""), "\"");
        }
    }
}
