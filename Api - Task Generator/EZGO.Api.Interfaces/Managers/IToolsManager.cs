using EZGO.Api.Models.General;
using EZGO.Api.Models.Logs;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IToolsManager
    {
        Task<string> GetLatestLogsAsJsonAsync();
        Task<string> GetLatestLogsRequestResponseAsJsonAsync();
        Task<List<LogAuditingItem>> GetLatestAuditingLogs(bool includeData = false);
        Task<List<LogAuditingItem>> GetLatestAuditingLogsForUser(int userId, int companyId, bool includeData = false);
        Task<List<LogAuditingItem>> GetLatestAuditingLogsForCompany(int companyId, bool includeData = false);
        Task<List<LogShortOutput>> GetLatestESLogs();
        Task<List<DatabaseTimezoneItem>> GetDatabaseSupportedTimezones();
        Task<List<DatabaseTimezoneItem>> GetCoreSupportedTimezones();
        Task<string> GetSupportedLanguages();
        Task<bool> ReduceTaskGenerationConfigsForCompany(int companyId);
        Task<bool> ResetConnectionPool(bool resetAll);
        Task<RawData> GetRawData(int companyId, string rawReference, DateTime startDateTime, DateTime endDateTime);
        Task<RawData> GetRawDataFromDataWarehouse(int companyId, string rawReference, DateTime startDateTime, DateTime endDateTime);
        Task<CalendarSchedule> GetRawScheduleData(int companyId, DateTime startDateTime, DateTime endDateTime);
        //Task<bool> AddClientDeviceLog(IHeaderDictionary headerDictionary, int companyId, int userId);
        Task<bool> FixAudtingData();
        Task<bool> GenerateUserProfileGuids();
        Task<int> UpdateModifiedBaseStructures(int companyId, int holdingId);
        Task<int> UpdateModifiedActions(ToolFilter toolFilter);
        Task<int> CleanupLoggingTable();
        Task<int> CleanupLoggingGenerationTable();
        Task<int> CleanupLoggingRequestResponseTable();
        Task<int> CleanupLoggingMigrationTable();
        Task<int> CleanupLoggingSecurityTable();
        Task<int> CleanupLoggingExportTable();
        Task<string> CreateServiceUserForCompany(int companyId);
        Task<bool> CreateSystemUsers();
        Task WriteToLog(string domain, string path, string query, string status, string header, string request, string response);

        List<Exception> GetPossibleExceptions();

    }
}
