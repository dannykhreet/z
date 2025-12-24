using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Data
{
    public interface IDatabaseLogWriter
    {
        Task<int> WriteToLog(string message, string type, string eventid, string eventname, string description, string source);

        Task<int> WriteToLog(string domain, string path, string query, string status, string header, string request, string response);

        Task<bool> GetRequestResponseLoggingEnabled(int userid);

        Task<int> GetLatestLogId();

    }
}
