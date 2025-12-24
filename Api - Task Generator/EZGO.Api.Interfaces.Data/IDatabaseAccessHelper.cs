using EZGO.Api.Data.Enumerations;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Data
{
    /// <summary>
    /// IDatabaseAccessHelper, interface used for Database Access Helper.
    /// NOTE! this interface is not included in the normal interfaces library because of dependencies to the Npgsql packages.
    /// We will not be adding those to our models or interfaces libraries to prevent dependency issues in the future.
    /// </summary>
    public interface IDatabaseAccessHelper
    {
        NpgsqlConnection Connection { get; set; }
        NpgsqlConnection ReaderConnection { get; set; }
        NpgsqlConnection WriterConnection { get; set; }
        NpgsqlCommand GetCommand(NpgsqlConnection connection, string commandText, CommandType commandType, List<NpgsqlParameter> parameters = null);
        Task<int> ExecuteNonQueryAsync(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, ConnectionKind connectionKind = ConnectionKind.Writer);
        Task<NpgsqlConnection> GetConnection();
        Task<NpgsqlConnection> GetReaderConnection();
        Task<NpgsqlConnection> GetWriterConnection();
        Task<NpgsqlDataReader> GetDataReader(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<object> ExecuteScalarAsync(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, ConnectionKind connectionKind = ConnectionKind.Writer);
        Task<DataTable> GetDataTable(string procedureNameOrQuery, List<NpgsqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure, string dataTableName = "", ConnectionKind connectionKind = ConnectionKind.Reader);
        List<NpgsqlParameter> GetBaseParameters(int companyId);
        Task<string> GetDataRowAsJson(string tableName, int id);
        Task<string> GetDataRowAsJson(string tableName, string fieldName, int id);
        Task<string> GetDataRowAsJson(string tableName, string fieldName, int id, string fieldname2, int id2);
    }
}
