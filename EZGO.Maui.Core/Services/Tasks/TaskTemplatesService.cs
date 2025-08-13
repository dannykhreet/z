using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using System.Web;

namespace EZGO.Maui.Core.Services.Tasks
{
    public class TaskTemplatesService : ITaskTemplatesSerivce
    {
        #region Services

        /// <summary>
        /// The default API client
        /// </summary>
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IInstructionsService _instructionsService;
        private readonly ICachingService _cache;

        #endregion

        #region Constructors

        public TaskTemplatesService(IApiRequestHandler apiRequestHandler, IInstructionsService instructionsService)
        {
            _apiRequestHandler = apiRequestHandler;
            _cache = DependencyService.Get<ICachingService>();
            _instructionsService = instructionsService;
        }

        #endregion

        #region Implementation

        public async Task<List<TaskTemplateModel>> GetAllTemplatesForAreaAsync(int areaId, bool refresh = false, bool isFromSyncService = false)
        {
            var result = await GetTaskTemplatesAsync(options =>
            {
                if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                    options
                       .FromArea(areaId)
                       .WithAreaFilterType(FilterAreaTypeEnum.RecursiveRootToLeaf)
                       .Includes(IncludesEnum.Steps, IncludesEnum.Recurrency, IncludesEnum.RecurrencyShifts, IncludesEnum.InstructionRelations, IncludesEnum.Tags)
                       .WithLimit(0);
                else
                    options
                        .FromArea(areaId)
                        .WithAreaFilterType(FilterAreaTypeEnum.RecursiveRootToLeaf)
                        .Includes(IncludesEnum.Steps, IncludesEnum.Recurrency, IncludesEnum.RecurrencyShifts, IncludesEnum.InstructionRelations)
                        .WithLimit(0);
            }, refresh: refresh, isFromSyncService: isFromSyncService);

            foreach (var task in result)
            {
                if (!string.IsNullOrWhiteSpace(task.DescriptionFile))
                {
                    task.Attachments ??= new List<Attachment>();
                    task.Attachments.Add(new Attachment
                    {
                        Uri = task.DescriptionFile,
                        AttachmentType = "Pdf"
                    });
                }
            }

            return result;
        }

        public async Task<List<TaskTemplateModel>> GetAllTemplatesForCurrentAreaAsync(bool refresh = false)
        {
            return await GetAllTemplatesForAreaAsync(Settings.WorkAreaId, refresh: refresh);
        }

        public async Task<bool> UpdateOrCreateTemplateAsync(TaskTemplateModel newModel)
        {
            if (newModel.Id == 0)
            {
                // Create new
                // TBD
                newModel.MachineStatus = "not_applicable";
                newModel.Type = "task";

                // TBD
                // model.Index = 
                newModel.StepsCount = newModel.Steps.Count;

                // Set defaults
                newModel.CompanyId = UserSettings.CompanyId;
                newModel.Recurrency.CompanyId = UserSettings.CompanyId;

                var response = await _apiRequestHandler.HandlePostRequest("tasktemplate/add", newModel);
                return response.IsSuccessStatusCode;
            }
            else
            {
                // Update
                newModel.StepsCount = newModel.Steps.Count();

                // For all the new steps set the template ID
                newModel.Steps.ForEach(x =>
                {
                    if (x.Id == 0)
                    {
                        x.TaskTemplateId = newModel.Id;
                    }
                });

                var oldModel = (await GetAllTemplatesForAreaAsync(Settings.WorkAreaId)).FirstOrDefault(x => x.Id == newModel.Id);
                if (oldModel != null && oldModel.Steps != null)
                {
                    // Determine the steps that are no longer in the new model
                    var deletedStepsIds = oldModel.Steps
                        .Select(x => x.Id)
                        .Except(newModel.Steps.Select(x => x.Id))
                        .ToList();

                    // And disable them
                    foreach (var stepsId in deletedStepsIds)
                        await SetTaskTemplateStepStatus(stepsId, false);
                }

                // If the recurrency is new (it shouldn't but just in case)
                if (newModel.Recurrency.Id == 0)
                    // Set the template ID
                    newModel.Recurrency.TemplateId = newModel.Id;

                var response = await _apiRequestHandler.HandlePostRequest($"tasktemplate/change/{newModel.Id}", newModel);

                if (response.IsSuccessStatusCode)
                {
                    // Figure out the request url... 
                    var url = new Uri(new Uri(Statics.ApiUrl), GetAllTemplatesUrl());
                    // Update cached template
                    await _cache.AlterCachedRequestListAsync(url.ToString(), (TaskTemplate currentTemplate) =>
                    {
                        currentTemplate.Name = newModel.Name;
                        currentTemplate.Description = newModel.Description;
                        currentTemplate.Picture = newModel.Picture;
                        currentTemplate.Video = newModel.Video;
                        currentTemplate.VideoThumbnail = newModel.VideoThumbnail;
                        currentTemplate.DescriptionFile = newModel.DescriptionFile;
                        currentTemplate.Steps = newModel.Steps.Cast<Step>().ToList();
                        currentTemplate.StepsCount = newModel.Steps?.Count();
                        currentTemplate.Recurrency = newModel.Recurrency;
                        currentTemplate.RecurrencyType = newModel.RecurrencyType;
                    }, x => x.Id == newModel.Id);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, Constants.TaskTemplatesChanged);
                        MessagingCenter.Send(this, Constants.TaskTemplatesChanged, newModel);
                    });
                }

                return response.IsSuccessStatusCode;
            }
        }

        public string GetAllTemplatesUrl()
        {
            var options = new GetTaskTemplatesOptions();
            var areaId = Settings.WorkAreaId;
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                options
                   .FromArea(areaId)
                   .WithAreaFilterType(FilterAreaTypeEnum.RecursiveRootToLeaf)
                   .Includes(IncludesEnum.Steps, IncludesEnum.Recurrency, IncludesEnum.RecurrencyShifts, IncludesEnum.InstructionRelations, IncludesEnum.Tags)
                   .WithLimit(0);
            else
                options
                    .FromArea(areaId)
                    .WithAreaFilterType(FilterAreaTypeEnum.RecursiveRootToLeaf)
                    .Includes(IncludesEnum.Steps, IncludesEnum.Recurrency, IncludesEnum.RecurrencyShifts, IncludesEnum.InstructionRelations)
                    .WithLimit(0);

            return GetTaskTemplatesCreateUrl(options);
        }

        #endregion

        #region Private Helpers

        private async Task<bool> SetTaskTemplateStepStatus(int id, bool isActive)
        {
            var url = $"tasktemplatestep/setactive/{id}";
            using var result = await _apiRequestHandler.HandlePostRequest(url, isActive);
            return result.IsSuccessStatusCode;
        }

        /// <summary>
        /// API endpoint to get task templates
        /// </summary>
        private readonly string TaskTemplatesEndpoint = "tasktemplates";

        /// <summary>
        /// Gets task templates from using the HTTP client
        /// </summary>
        /// <param name="configure">Action that should configure the options for the query</param>
        /// <returns>Task templates received from the HTTP client</returns>
        private async Task<List<TaskTemplateModel>> GetTaskTemplatesAsync(Action<GetTaskTemplatesOptions> configure, bool refresh = false, bool isFromSyncService = false)
        {
            var options = new GetTaskTemplatesOptions();
            configure.Invoke(options);

            var url = GetTaskTemplatesCreateUrl(options);

            List<TaskTemplateModel> result = await _apiRequestHandler.HandleListRequest<TaskTemplateModel>(url, refresh: refresh, isFromSyncService: isFromSyncService);

            //workaround because work relations from api can come with wrong permissions
            await _instructionsService.SetWorkInstructionRelations(result);

            return result;
        }

        private string GetTaskTemplatesCreateUrl(GetTaskTemplatesOptions options)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (options.AreaId != null)
                query["areaid"] = HttpUtility.HtmlEncode((int)options.AreaId);

            if (options.AreaFilterType != null)
                query["filterareatype"] = HttpUtility.HtmlEncode((int)options.AreaFilterType);

            if (options.RoleType != null)
                query["role"] = HttpUtility.HtmlEncode((int)options.RoleType);

            if (options.TaskType != null)
                query["tasktype"] = HttpUtility.HtmlEncode((int)options.TaskType);

            if (options.Limit != null)
                query["limit"] = HttpUtility.HtmlEncode(options.Limit);

            if (options.Include != null)
                query["include"] = HttpUtility.HtmlEncode(string.Join(",", options.Include.Select(x => x.ToString().ToLower())));

            var queryString = query.ToString();
            var url = $"{TaskTemplatesEndpoint}?{queryString}";
            return url;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }

        #endregion
    }
}
