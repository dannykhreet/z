using EZGO.Api.Models;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Tasks
{
    /// <summary>
    /// Manages operations on <see cref="TaskTemplate"/> such as getting and editing
    /// </summary>
    public interface ITaskTemplatesSerivce : IDisposable
    {
        /// <summary>
        /// Gets all task templates for the current area
        /// </summary>
        /// <returns>All task templates available for current area and it's subareas</returns>
        Task<List<TaskTemplateModel>> GetAllTemplatesForCurrentAreaAsync(bool refresh = false);

        /// <summary>
        /// Gets all task templates for a specific area
        /// </summary>
        /// <param name="areaId">Id of the area to filter the results with</param>
        /// <returns>All task templates available for the area and it's subareas</returns>
        Task<List<TaskTemplateModel>> GetAllTemplatesForAreaAsync(int areaId, bool refresh = false, bool isFromSyncService = false);

        /// <summary>
        /// Updates a task template if exists or creates a new one if it doesn't.
        /// </summary>
        /// <param name="newModel">New model of the task template.</param>
        /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/>.</returns>
        Task<bool> UpdateOrCreateTemplateAsync(TaskTemplateModel newModel);
    }
}
