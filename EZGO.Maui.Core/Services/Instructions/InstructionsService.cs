using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Services.Instructions
{
    public class InstructionsService : IInstructionsService
    {
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IRoleFunctionsWrapper _roleFunctionsWrapper;
        private readonly ICachingService _cachingService;

        public InstructionsService(IApiRequestHandler apiRequestHandler, IRoleFunctionsWrapper roleFunctionsWrapper, ICachingService cachingService)
        {
            _apiRequestHandler = apiRequestHandler;
            _roleFunctionsWrapper = roleFunctionsWrapper;
            _cachingService = cachingService;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }

        public async Task<InstructionsModel> GetInstruction(int id, bool refresh = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
                return new InstructionsModel();

            var instructions = await GetInstructions(refresh: refresh);
            var instruction = instructions.FirstOrDefault(i => i.Id == id);
            return instruction ?? new InstructionsModel();
        }

        public async Task<InstructionsModel> GetInstructionFromApi(int id, bool refresh = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
                return new InstructionsModel();

            var url = $"workinstructiontemplate/{id}?include=items";
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                url = $"workinstructiontemplate/{id}?include=items,tags";
            var instruction = await _apiRequestHandler.HandleRequest<InstructionsModel>(url, refresh);
            return instruction;
        }


        public async Task<List<InstructionsModel>> GetInstructions(bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
                return new List<InstructionsModel>();

            int instructionType = (int)InstructionTypeEnum.BasicInstruction;

            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            var url = $"workinstructiontemplates/availableforarea/{Settings.AreaSettings.WorkAreaId}?include=items&allowedonly={allowed}&instructiontype={instructionType}&limit=0";
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                url = $"workinstructiontemplates/availableforarea/{Settings.AreaSettings.WorkAreaId}?include=items,tags&allowedonly={allowed}&instructiontype={instructionType}&limit=0";
            var instructions = await _apiRequestHandler.HandleListRequest<InstructionsModel>(url, refresh, isFromSyncService).ConfigureAwait(false);
            return instructions;
        }

        public async Task<InstructionsModel> GetInstructionForCurrentArea(int id)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
                return new InstructionsModel();

            var instructions = await GetInstructionsForCurrentArea();
            var instruction = instructions.FirstOrDefault(i => i.Id == id);
            return instruction ?? new InstructionsModel();
        }

        public async Task<List<InstructionsModel>> GetInstructionsForCurrentArea(bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
                return new List<InstructionsModel>();

            int instructionType = (int)InstructionTypeEnum.BasicInstruction;

            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();
            var url = $"workinstructiontemplates?include=items&areaid={Settings.AreaSettings.WorkAreaId}&allowedonly={allowed}&instructiontype={instructionType}&limit=0";
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                url = $"workinstructiontemplates?include=items,tags&areaid={Settings.AreaSettings.WorkAreaId}&allowedonly={allowed}&instructiontype={instructionType}&limit=0";
            var instructions = await _apiRequestHandler.HandleListRequest<InstructionsModel>(url, refresh, isFromSyncService);
            return instructions;
        }

        public async Task SetWorkInstructionRelations(List<BasicTaskModel> tasks, List<InstructionsModel> allInstructions = null)
        {
            if (tasks.IsNullOrEmpty())
                return;

            var tasksWithWorkRelations = tasks.Where(r => r?.WorkInstructionRelations != null && r.WorkInstructionRelations.Any()).ToList();
            if (tasksWithWorkRelations.Any())
            {
                var availableWorkInstructionTemplates = allInstructions ?? await GetInstructions();
                var availableWorkInstructionTemplateIds = availableWorkInstructionTemplates.Select(i => i.Id).ToList();

                foreach (var task in tasksWithWorkRelations)
                {
                    task.WorkInstructionRelations = task.WorkInstructionRelations.Where(i => availableWorkInstructionTemplateIds.Contains(i.WorkInstructionTemplateId)).ToList();
                    for (int i = 0; i < task.WorkInstructionRelations.Count; i++)
                    {
                        var relation = task.WorkInstructionRelations[i];
                        var instructionTemplate = availableWorkInstructionTemplates.FirstOrDefault(x => x.Id == relation.WorkInstructionTemplateId);
                        task.WorkInstructionRelations[i] = instructionTemplate;
                        task.WorkInstructionRelations[i].WorkInstructionTemplateId = instructionTemplate.Id;
                    }
                }
            }
        }

        public async Task SetWorkInstructionRelations(List<TaskTemplateModel> taskTemplates, List<InstructionsModel> allInstructions = null)
        {
            if (taskTemplates.IsNullOrEmpty())
                return;

            var templatesWithWorkRelations = taskTemplates.Where(r => r?.WorkInstructionRelations != null && r.WorkInstructionRelations.Any()).ToList();
            if (templatesWithWorkRelations.Any())
            {
                var availableWorkInstructionTemplates = allInstructions ?? await GetInstructions().ConfigureAwait(false);
                var availableWorkInstructionTemplateIds = availableWorkInstructionTemplates.Select(i => i.Id).ToList();

                foreach (var task in templatesWithWorkRelations)
                {
                    task.WorkInstructionRelations = task.WorkInstructionRelations.Where(i => availableWorkInstructionTemplateIds.Contains(i.WorkInstructionTemplateId)).ToList();
                    for (int i = 0; i < task.WorkInstructionRelations.Count; i++)
                    {
                        var relation = task.WorkInstructionRelations[i];
                        var instructionTemplate = availableWorkInstructionTemplates.FirstOrDefault(x => x.Id == relation.WorkInstructionTemplateId);
                        task.WorkInstructionRelations[i] = instructionTemplate;
                        task.WorkInstructionRelations[i].WorkInstructionTemplateId = instructionTemplate.Id;
                    }
                }
            }
        }

        public async Task<List<WorkInstructionTemplateChangeNotification>> GetWorkInstructionChanges(int id)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled || id == 0)
                return new List<WorkInstructionTemplateChangeNotification>();

            var url = $"workinstructiontemplatechangenotifications?workInstructionTemplateId={id}";
            var changes = await _apiRequestHandler.HandleListRequest<WorkInstructionTemplateChangeNotification>(url, refresh: true);
            return changes.OrderByDescending(x => x.ModifiedAt).ToList();
        }

        public async Task<bool> ConfirmWorkInstructionChanges(int id)
        {
            if (!CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled || id == 0)
                return false;

            if (!await InternetHelper.HasInternetAndApiConnectionAsync())
                return false;

            var url = $"workinstructiontemplatechangenotifications/{id}/confirm";
            var response = await _apiRequestHandler.HandlePostRequest(url, "");
            if (response.IsSuccessStatusCode)
            {
                await AlterWorkInstructionTemplateCache(id);
                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.WorkInstructionsTemplateNotificationConfirmed); });
                return true;
            }
            return false;
        }

        private async Task AlterWorkInstructionTemplateCache(int id)
        {
            static void alteringFunction(InstructionsModel instruction)
            {
                instruction.UnreadChangesNotificationsCount = 0;
            }

            int instructionType = (int)InstructionTypeEnum.BasicInstruction;

            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();
            var uri = $"workinstructiontemplates?include=items&areaid={Settings.AreaSettings.WorkAreaId}&allowedonly={allowed}&instructiontype={instructionType}&limit=0";
            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri = $"workinstructiontemplates?include=items,tags&areaid={Settings.AreaSettings.WorkAreaId}&allowedonly={allowed}&instructiontype={instructionType}&limit=0";

            // Alter assessments cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: uri).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<InstructionsModel>(uri, alteringFunction, (instruction) => instruction.Id == id);
        }
    }
}
