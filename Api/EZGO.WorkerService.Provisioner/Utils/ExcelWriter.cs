using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.WorkerService.Provisioner.Utils
{
    /// <summary>
    /// ExcelWriter; Contains functionality to create a Excel file from a DataTable or DataSet and append that file to a incoming stream (for use with an API or MVC controller or other similar functionality).
    /// When using a DataTable, the excel file will contain one sheet, where the name of the sheet will be the table name. Depending on options also the headers can be rendered based on the column names within the DataTable.
    /// When using a DataSet, the excel file can contain multiple sheets, one sheet for each DataTable within the DataSet. The name of each sheet will be the DataTable name. Depending on options also the headers can be rendered based on the column names within the DataTable.
    ///
    /// NOTE! ExcelWriter depends on ClosedXml nuget package. This is a wrapper around the openxml structure.
    /// Will try to replace ClosedXml with more native structures (OpenXml)
    /// </summary>
    public class ExcelWriter
    {
        /// <summary>
        /// WriteFromDataTableAsync; NOT YET IMPLEMENTED.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stream"></param>
        /// <param name="includeHeaders"></param>
        /// <returns></returns>
        public static async Task WriteFromDataTableAsync(DataTable source, Stream stream, bool includeHeaders = true, int numberOfHeaderRows = 0)
        {

            if (source != null && stream != null)
            {
                var workbook = new XLWorkbook();
                DataTable dt = source;
                //TODO REFACTOR

                System.Diagnostics.Debug.WriteLine(string.Concat("Export - Create Table - ", dt.TableName, " - ", DateTime.Now.ToString())); ;

                var worksheet = workbook.Worksheets.Add((dt.TableName.Length > 31) ? dt.TableName.Substring(0,30) : dt.TableName);
                var currentRowNumber = 1;
                var numberOfColumns = dt.Columns.Count;
                if (includeHeaders)
                {
                    for (var i = 0; i < numberOfColumns; i++)
                    {
                        worksheet.Cell(currentRowNumber, i + 1).Value = dt.Columns[i].ColumnName;
                        worksheet.Cell(currentRowNumber, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRowNumber, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                    currentRowNumber++;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    for (var i = 0; i < numberOfColumns; i++)
                    {
                        var excelCellValue = XLCellValue.FromObject(dr[i]);

                        worksheet.Cell(currentRowNumber, i + 1).SetValue(excelCellValue);

                        if (currentRowNumber <= numberOfHeaderRows)
                        {
                            worksheet.Cell(currentRowNumber, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRowNumber, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }
                    }
                    currentRowNumber++;
                }

                System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Create Stream - ", DateTime.Now.ToString()));

                byte[] excelData;
                using (var temporaryStream = new MemoryStream())
                {
                    System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Save workbook - ", DateTime.Now.ToString()));
                    workbook.SaveAs(temporaryStream);
                    System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Create ExcelData from stream - ", DateTime.Now.ToString()));
                    excelData = temporaryStream.ToArray();

                }

                System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Write stream - ", DateTime.Now.ToString()));
                await stream.WriteAsync(excelData, 0, excelData.Length);
            }

        }

        /// <summary>
        /// WriteFromDataSetAsync; Creates a Excel sheet with ClosedXml based on a DataSet and write it to a incoming stream.
        /// </summary>
        /// <param name="source">DataSet, containing one or more DataTables with data to be rendered.</param>
        /// <param name="stream">Incoming stream where the document must be written to.</param>
        /// <param name="includeHeaders">True/False for rendering headers within sheet based on the column names.</param>
        /// <param name="numberOfHeaderRows">Number of rows where the header styles need to be applied.</param>
        /// <returns>Task void.</returns>
        public static async Task WriteFromDataSetAsync(DataSet source, Stream stream, bool includeHeaders = true, int numberOfHeaderRows = 0)
        {

            if (source != null && source.Tables != null && source.Tables.Count > 0)
            {
                var workbook = new XLWorkbook();
                //TODO added extra debug information, will be removed on later stage in development, needed for performance slowness of ClosedXml excel generation.
                foreach (DataTable dt in source.Tables)
                {
                    System.Diagnostics.Debug.WriteLine(string.Concat("Export - Create Table - ", dt.TableName, " - ", DateTime.Now.ToString()));;

                    var worksheet = workbook.Worksheets.Add((dt.TableName.Length > 31) ? dt.TableName.Substring(0, 30) : dt.TableName);
                    var currentRowNumber = 1;
                    var numberOfColumns = dt.Columns.Count;
                    if(includeHeaders) {
                        for(var i = 0; i < numberOfColumns; i++)
                        {
                            worksheet.Cell(currentRowNumber, i + 1).Value = dt.Columns[i].ColumnName;
                            worksheet.Cell(currentRowNumber, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRowNumber, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }
                        currentRowNumber++;
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        for (var i = 0; i < numberOfColumns; i++)
                        {
                            var excelCellValue = XLCellValue.FromObject(dr[i]);

                            worksheet.Cell(currentRowNumber, i + 1).SetValue(excelCellValue);
                            
                            if(currentRowNumber <= numberOfHeaderRows)
                            {
                                worksheet.Cell(currentRowNumber, i + 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRowNumber, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                            }
                        }
                        currentRowNumber++;
                    }
                }

                System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Create Stream - ", DateTime.Now.ToString()));

                byte[] excelData;
                using (var temporaryStream = new MemoryStream())
                {
                    System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Save workbook - ",DateTime.Now.ToString()));
                    workbook.SaveAs(temporaryStream);
                    System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Create ExcelData from stream - ", DateTime.Now.ToString()));
                    excelData = temporaryStream.ToArray();

                }

                System.Diagnostics.Debug.WriteLine(string.Concat("!! Export - Write stream - ", DateTime.Now.ToString()));
                await stream.WriteAsync(excelData, 0, excelData.Length);

            }

        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="datatable"></param>
        /// <param name="includeHeaders"></param>
        private static void CreateWorksheet(IXLWorkbook workbook, DataTable datatable, bool includeHeaders = true)
        {

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="datatable"></param>
        private static void Fillworksheet(IXLWorksheet worksheet, DataTable datatable)
        {

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="datatable"></param>
        private static void FillHeaders(IXLWorksheet worksheet, DataTable datatable)
        {

        }



    }
}
