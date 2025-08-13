using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using NodaTime;

namespace EZGO.Maui.Core.Services.Tasks
{
    public class TaskReportService : ITaskReportService
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public TaskReportService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }


        public async Task<TaskOverviewReportModel> GetTaskOverviewReportOnlyAsync(LocalDateTime? timeStamp = null, bool refresh = false, bool isFromSyncService = false)
        {
            TaskOverviewReportModel taskOverviewReport = new TaskOverviewReportModel();

            if (timeStamp == null) timeStamp = DateTimeHelper.Now;

            string action = GetActionParameters(timeStamp);
            taskOverviewReport = await _apiRequestHandler.HandleRequest<TaskOverviewReportModel>(action, refresh, isFromSyncService).ConfigureAwait(false);

            var companyId = UserSettings.CompanyId;
            if (timeStamp.HasValue) Settings.TasksOverviewTimestamp = timeStamp.Value;

            return taskOverviewReport;
        }

        private string GetActionParameters(LocalDateTime? timeStamp)
        {
            timeStamp ??= Settings.TasksOverviewTimestamp;

            List<string> parameters = new List<string>();

            parameters.Add("timestamp=" + timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null));

            if (Settings.WorkAreaId != 0)
                parameters.Add("areaid=" + Settings.WorkAreaId);

            return $"reporting/taskspastoverview?{parameters.Aggregate((a, b) => a + "&" + b)}";
        }

        public async Task<TaskOverviewReportModel> GetTaskOverviewReportAsync(LocalDateTime? timeStamp = null, bool refresh = false, bool isFromSyncService = false)
        {
            timeStamp ??= Settings.TasksOverviewTimestamp;

            List<string> parameters = new List<string>();

            parameters.Add("timestamp=" + timeStamp.Value.ToString(Constants.ApiDateTimeFormat, null));

            if (Settings.WorkAreaId != 0)
                parameters.Add("areaid=" + Settings.WorkAreaId);

            string action = $"reporting/taskspastoverview?{parameters.Aggregate((a, b) => a + "&" + b)}";

            TaskOverviewReportModel taskOverviewReport = await _apiRequestHandler.HandleRequest<TaskOverviewReportModel>(action, refresh, isFromSyncService).ConfigureAwait(false);

            return taskOverviewReport;
        }

        private static List<TaskOverviewReportItemModel> ConvertTasksToReportItem(IEnumerable<BasicTaskModel> tasks)
        {
            List<TaskOverviewReportItemModel> reportItems = new List<TaskOverviewReportItemModel>();

            reportItems.Add(new TaskOverviewReportItemModel
            {
                TaskStatus = TaskStatusEnum.Todo,
                NrOfItems = tasks.Count(item => item.FilterStatus == TaskStatusEnum.Todo)
            });

            reportItems.Add(new TaskOverviewReportItemModel
            {
                TaskStatus = TaskStatusEnum.Ok,
                NrOfItems = tasks.Count(item => item.FilterStatus == TaskStatusEnum.Ok)
            });

            reportItems.Add(new TaskOverviewReportItemModel
            {
                TaskStatus = TaskStatusEnum.NotOk,
                NrOfItems = tasks.Count(item => item.FilterStatus == TaskStatusEnum.NotOk)
            });

            reportItems.Add(new TaskOverviewReportItemModel
            {
                TaskStatus = TaskStatusEnum.Skipped,
                NrOfItems = tasks.Count(item => item.FilterStatus == TaskStatusEnum.Skipped)
            });

            return reportItems;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }
    }
}
