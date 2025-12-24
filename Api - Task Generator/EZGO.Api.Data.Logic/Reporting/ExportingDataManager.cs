using EZGO.Api.Data.Base;
using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Models.Enumerations;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Data.Reporting
{
    /// <summary>
    /// DataManager; the reporting DataManager is used for retrieving data sets from the database using stored procedures.
    /// These dataset can be used for reporting and exports. These usually are not used in the application but in management and export/import systems.
    /// NOTE! this class and structures will need to be moved to the normal manager functionality.
    /// </summary>
    public class ExportingDataManager : BaseManager<ExportingDataManager>, IExportingDataManager
    {
        #region - privates -
        private readonly IConnectionHelper _connectionHelper;
        #endregion

        #region - constructor(s) -
        public ExportingDataManager(IConnectionHelper connhelper, ILogger<ExportingDataManager> logger) : base(logger)
        {
            _connectionHelper = connhelper;
        }
        #endregion

        #region - public export methods -

        /// <summary>
        /// GetTasksDataTableByCompanyAndDate; get a DataTable for further processing.
        ///
        /// Source of the data is the stored procedure get_task_data_bi.
        ///
        /// Note there is no other validation on input except DataTypes.
        /// When supplied with no valid data or in case of an error a null will be returned.
        /// </summary>
        /// <param name="companyid">Int CompanyID (see in database table and column: Companies_Company.id) </param>
        /// <param name="from">DateTime used for querying the stored procedure.</param>
        /// <param name="to">DateTime used for querying the stored procedure.</param>
        /// <returns>Task - DataTable containing all relevant data based on the supplied input parameters.</returns>
        public async Task<DataTable> GetTasksDataTableByCompanyAndDateAsync(int companyid, DateTime from, DateTime to)
        {
            NpgsqlConnection conn = null;
            try
            {
                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {

                    try
                    {
                        await conn.OpenAsync();

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                        parameters.Add(new NpgsqlParameter("@_current_company_id", companyid));
                        parameters.Add(new NpgsqlParameter("@_start_date_utc", from));
                        parameters.Add(new NpgsqlParameter("@_end_date_utc", to));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_task_data_bi_by_company_by_startenddate", parameters), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddRange(parameters.ToArray());

                            NpgsqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                            DataTable dt = new DataTable();
                            dt.Load(dr);

                            await dr.CloseAsync(); dr = null;

                            return dt;

                        }

                    } catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred GetTasksDataTableByCompanyAndDateAsync()");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred GetTasksDataTableByCompanyAndDateAsync()");
            }
            finally
            {
                if (conn != null) await conn.CloseAsync();
            }

            return null;
        }

        /// <summary>
        /// GetTasksDetailsDataTableByCompanyAndDateAsync; Gets a specific DataTable containing template and task details data.
        /// </summary>
        /// <param name="companyid">Int CompanyID (see in database table and column: Companies_Company.id)</param>
        /// <param name="from">DateTime used for querying the stored procedure.</param>
        /// <param name="to">DateTime used for querying the stored procedure.</param>
        /// <returns>Task - DataTable containing all relevant data based on the supplied input parameters.</returns>
        public async Task<DataTable> GetTasksDetailsDataTableByCompanyAndDateAsync(int companyid, DateTime from, DateTime to)
        {
            NpgsqlConnection conn = null;
            try
            {

                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {


                    try
                    {
                        await conn.OpenAsync();

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_current_company_id", companyid));
                        parameters.Add(new NpgsqlParameter("@_start_date_utc", from));
                        parameters.Add(new NpgsqlParameter("@_end_date_utc", to));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_task_details_data_bi_by_company_by_startenddate", parameters), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddRange(parameters.ToArray());

                            NpgsqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                            DataTable dt = new DataTable();
                            dt.Load(dr);

                            await dr.CloseAsync(); dr = null;

                            return dt;

                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred GetTasksDetailsDataTableByCompanyAndDateAsync()");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred GetTasksDetailsDataTableByCompanyAndDateAsync()");
            }
            finally
            {
                if (conn != null) await conn.CloseAsync();
            }

            return null;
        }


        /// <summary>
        /// GetTaskTemplateDetailsDataTableByCompanyAsync; get a DataTable for further processing.
        ///
        /// Source of the data is the stored procedure get_tasktemplate_data_bi.
        ///
        /// Note there is no other validation on input except DataTypes.
        /// When supplied with no valid data or in case of an error a null will be returned.
        /// </summary>
        /// <param name="companyid">Int CompanyID (see in database table and column: Companies_Company.id).</param>
        /// <returns>Task - DataTable containing all relevant data based on the supplied input parameters.</returns>
        public async Task<DataTable> GetTaskTemplateOverviewDataTableByCompanyAsync(int companyId, ExportTaskTemplateOverviewTypeEnum overviewType, DateTime? from = null, DateTime? to = null)
        {
            NpgsqlConnection conn = null;
            try
            {

                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {

                    try
                    {
                        string sp = "export_data_tasktemplate_nonshifts_overview";
                        if (overviewType == ExportTaskTemplateOverviewTypeEnum.NonShifts )
                        {
                            sp = "export_data_tasktemplate_nonshifts_overview";
                        } else
                        {
                            sp = "export_data_tasktemplate_shiftsonly_overview";
                        }

                        await conn.OpenAsync();

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand(sp, parameters), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddRange(parameters.ToArray());

                            NpgsqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                            DataTable dt = new DataTable();
                            dt.Load(dr);

                            await dr.CloseAsync(); dr = null;

                            return dt;

                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred GetTaskTemplateOverviewDataTableByCompanyAsync()");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred GetTaskTemplateOverviewDataTableByCompanyAsync()");
            }
            finally
            {
                if (conn != null) await conn.CloseAsync();
            }

            return null;
        }

        /// <summary>
        /// GetTaskTemplateDetailsDataTableByCompanyAsync; get a DataTable for further processing.
        ///
        /// Source of the data is the stored procedure get_tasktemplate_data_bi.
        ///
        /// Note there is no other validation on input except DataTypes.
        /// When supplied with no valid data or in case of an error a null will be returned.
        /// </summary>
        /// <param name="companyid">Int CompanyID (see in database table and column: Companies_Company.id).</param>
        /// <returns>Task - DataTable containing all relevant data based on the supplied input parameters.</returns>
        public async Task<DataTable> GetTaskTemplateDetailsDataTableByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null)
        {
            NpgsqlConnection conn = null;
            try
            {

                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {


                    try
                    {
                        await conn.OpenAsync();

                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_current_company_id", companyid));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_tasktemplate_data_bi", parameters), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddRange(parameters.ToArray());

                            NpgsqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                            DataTable dt = new DataTable();
                            dt.Load(dr);

                            await dr.CloseAsync(); dr = null;

                            return dt;

                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred GetTaskTemplateDetailsDataTableByCompanyAsync()");
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred GetTaskTemplateDetailsDataTableByCompanyAsync()");
            }
            finally
            {
                if (conn != null) await conn.CloseAsync();
            }

            return null;
        }
        #endregion
    }
}

