using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Utils;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Data
{
    /// <summary>
    /// Update service.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.Interfaces.Data.IUpdateService" />
    public class UpdateService : IUpdateService
    {
        private readonly IApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateService"/> class.
        /// </summary>
        public UpdateService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Checks for updated items asynchronous.
        /// </summary>
        /// <returns>
        /// Collection of update check items.
        /// </returns>
        public async Task<IEnumerable<UpdateCheckItem>> CheckForUpdatedItemsAsync()
        {
            string lastUpdateCheckString = Settings.LastUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}");

            // Only update the last update check date when there are updated items. 
            if (updatedItems != null)
            {
                if (updatedItems.Any())
                    Settings.LastUpdateCheck = DateTimeHelper.UtcNow;
            }
            return updatedItems ?? new List<UpdateCheckItem>();
        }

        public async Task<bool> CheckForUpdatedChatAsync(int actionId)
        {
            string lastUpdateCheckString = Settings.LastChatUpdateCheck.ToString(Constants.ApiDateTimeFormat, null);

            var updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"actioncomments/updatecheck?actionid={actionId}&timestamp={lastUpdateCheckString}");

            // Only update the last update check date when there are updated items. 
            if (!updatedItems.IsNullOrEmpty())
            {
                var updatedItem = updatedItems.First();

                if (updatedItem.NumberOfItems > 0)
                    Settings.LastChatUpdateCheck = DateTimeHelper.Now;

                return updatedItem.NumberOfItems > 0;
            }
            else
                return false;
        }

        public async Task<bool> CheckForUpdatedTasksAsync()
        {
            string lastUpdateCheckString = Settings.LastTasksUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}");

            // Only update the last update check date when there are updated items. 
            if (updatedItems != null)
            {
                if (updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.Tasks))
                    Settings.LastTasksUpdateCheck = DateTimeHelper.UtcNow;
            }
            updatedItems ??= new List<UpdateCheckItem>();

            return updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.Tasks);
        }


        public async Task<bool> CheckForUpdatedPropertyValues()
        {
            string lastUpdateCheckString = Settings.LastPropertyValuesUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}");

            // Only update the last update check date when there are updated items. 
            if (updatedItems != null)
            {
                if (updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.PropertyValues))
                    Settings.LastPropertyValuesUpdateCheck = DateTimeHelper.UtcNow;
            }
            updatedItems ??= new List<UpdateCheckItem>();

            return updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.PropertyValues);
        }

        public async Task<bool> CheckForUpdatedAssessmentsAsync()
        {
            string lastUpdateCheckString = Settings.LastAssessmentsUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.AssessmentsWorkAreaId}");

            // Only update the last update check date when there are updated items.             
            if (updatedItems?.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.Assessments) ?? false)
                Settings.LastAssessmentsUpdateCheck = DateTimeHelper.UtcNow;

            updatedItems ??= new List<UpdateCheckItem>();

            return updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.Assessments);
        }

        public async Task<bool> CheckForUpdatedEzFeedAsync()
        {
            string lastUpdateCheckString = Settings.LastEzFeedUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}");

            // Only update the last update check date when there are updated items.             
            if (updatedItems?.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.EzFeed) ?? false)
                Settings.LastEzFeedUpdateCheck = DateTimeHelper.UtcNow;

            updatedItems ??= new List<UpdateCheckItem>();

            return updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.EzFeed);
        }

        public async Task<bool> CheckForUpdatedEzFeedCommentsAsync()
        {
            string lastUpdateCheckString = Settings.LastEzFeedCommentsUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}");

            // Only update the last update check date when there are updated items.             
            if (updatedItems?.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.EZFeedMessages) ?? false)
                Settings.LastEzFeedCommentsUpdateCheck = DateTimeHelper.UtcNow;

            updatedItems ??= new List<UpdateCheckItem>();

            return updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.EZFeedMessages);
        }

        public async Task<bool> CheckForUpdatedTaskCommentsAsync()
        {
            string lastUpdateCheckString = Settings.LastTaskCommentUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}");

            // Only update the last update check date when there are updated items.             
            if (updatedItems?.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.Comments) ?? false)
                Settings.LastTaskCommentUpdateCheck = DateTimeHelper.UtcNow;

            updatedItems ??= new List<UpdateCheckItem>();

            return updatedItems.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.Comments);
        }

        public async Task<List<int>> CheckForUpdatedChecklistsAsync()
        {
            Debug.WriteLine($"CheckForUpdatedChecklistsAsync");

            string lastUpdateCheckString = Settings.LastTaskCommentUpdateCheck.ToString(Constants.UpdateCheckDateTimeFormat, null);

            List<UpdateCheckItem> updatedItems = await _apiClient.GetAllAsync<UpdateCheckItem>($"updatecheck?fromdateutc={lastUpdateCheckString}&areaid={Settings.WorkAreaId}").ConfigureAwait(false);

            // Only update the last update check date when there are updated items.             
            if (updatedItems?.Any(x => x.UpdateCheckType == UpdateCheckTypeEnum.OpenChecklists) ?? false)
                Settings.LastTaskCommentUpdateCheck = DateTimeHelper.UtcNow;

            updatedItems ??= new List<UpdateCheckItem>();

            var result = new List<int>();
            result = updatedItems.Where(x => x.UpdateCheckType == UpdateCheckTypeEnum.OpenChecklists).SelectMany(x => x.Ids).ToList();

            return result;

        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }
}
