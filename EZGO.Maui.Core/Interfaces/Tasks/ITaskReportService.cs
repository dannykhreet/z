using EZGO.Maui.Core.Models.Tasks;
using NodaTime;
using System;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Tasks
{
    public interface ITaskReportService : IDisposable
    {
        Task<TaskOverviewReportModel> GetTaskOverviewReportAsync(LocalDateTime? timeStamp = null, bool refresh = false, bool isFromSyncService = false);
        Task<TaskOverviewReportModel> GetTaskOverviewReportOnlyAsync(LocalDateTime? timeStamp = null, bool refresh = false, bool isFromSyncService = false);
    }
}
