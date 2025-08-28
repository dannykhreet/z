using EZGO.Api.Models.General;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Data
{
    /// <summary>
    /// Update service.
    /// </summary>
    public interface IUpdateService : IDisposable
    {
        /// <summary>
        /// Checks for updated items asynchronous.
        /// </summary>
        /// <returns>Collection of update check items.</returns>
        Task<IEnumerable<UpdateCheckItem>> CheckForUpdatedItemsAsync();
        Task<bool> CheckForUpdatedChatAsync(int actionId);
        Task<bool> CheckForUpdatedTasksAsync();
        Task<bool> CheckForUpdatedAssessmentsAsync();
        Task<bool> CheckForUpdatedTaskCommentsAsync();
        Task<bool> CheckForUpdatedEzFeedAsync();
        Task<bool> CheckForUpdatedEzFeedCommentsAsync();
        Task<bool> CheckForUpdatedPropertyValues();
        Task<List<int>> CheckForUpdatedChecklistsAsync();
    }
}
