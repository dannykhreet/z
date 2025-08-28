using EZGO.Api.Models.Basic;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Data
{
    /// <summary>
    /// Service to synchronise local data with the API.
    /// </summary>
    public interface ISyncService : IDisposable
    {
        /// <summary>
        /// Uploads the local data asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        Task UploadLocalDataAsync();

        Task UpdateLocalDataAsync();

        Task UploadUpdateLocalActionDataAsync(int actionId);

        Task GetLocalDataAsync();

        Task LoadActionsAsync();

        Task LoadAuditTemplatesAsync();

        Task LoadCompletedChecklistsAsync();

        Task LoadCompletedAuditsAsync();

        Task LoadTasksAsync();

        Task<List<BasicTaskStatusModel>> ReloadTasksStatussesAsync();

        Task<List<TaskExtendedDataBasic>> GetPropertyValuesUpdatesAsync();

        void StartMediaDownload();

        void StopMediaDownload();
        Task UploadUnpostedData();
        Task LoadAssessmentTemplatesAsync();
        Task LoadAssessmentAsync(int id);
        Task LoadAssessmentsAsync();
        Task LoadTaskCommentsAsync();
        Task LoadEzFeedAsync();
        Task LoadChecklistTemplatesAsync();
    }
}
