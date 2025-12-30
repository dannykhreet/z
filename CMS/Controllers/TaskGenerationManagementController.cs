using EZGO.Api.Models;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.Models.Company;
using WebApp.Models.Task;
using WebApp.Models.TaskManagement;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.TaskGenerationManager)]
    public class TaskGenerationManagementController : BaseController
    {

        private readonly ILogger<TaskGenerationManagementController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        TaskGenerationManagerViewModel model;

        public TaskGenerationManagementController(ILogger<TaskGenerationManagementController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("/taskgenerationmanagement")]
        public async Task<IActionResult> TaskManagement()
        {
            var resultArea = await _connector.GetCall(Logic.Constants.General.AreaList);
            var resultAreaFlattend = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            var resultShift = await _connector.GetCall(Logic.Constants.Shift.GetShifts);
            var resultTasks = await _connector.GetCall(Logic.Constants.Task.GetTaskTemplatesUrl);

            var resultPlanning = await _connector.GetCall("/v1/generation/planning");

            var areas = JsonConvert.DeserializeObject<List<Area>>(resultArea.Message);
            var areasFlattend = JsonConvert.DeserializeObject<List<Area>>(resultAreaFlattend.Message);
            var planning = JsonConvert.DeserializeObject<DowntimePlanningConfiguration>(resultPlanning.Message);
            var shifts = JsonConvert.DeserializeObject<List<TaskGenerationShiftViewModel>>(resultShift.Message);
            var taskTemplates = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(resultTasks.Message);

            model = new TaskGenerationManagerViewModel
            {
                MoreGenerationOptionsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskGenerationManagementMoreOptions"),
                DowntimePlannings = planning.ConfigurationItems,
                Areas = areas,
                AreasFlattend = areasFlattend,
                Shifts = shifts,
                Tasks = taskTemplates,
                CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result,
                PageTitle = "Task generation management",
                ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri"),
                Locale = _locale,
                ApplicationSettings = await this.GetApplicationSettings(),
                NewInboxItemsCount = GetInboxCount().Result
            };
            model.Filter.CmsLanguage = model.CmsLanguage;

            //set selected areas, shifts tasks (with reason) accordingly in model
            return PartialView("~/Views/TaskGenerationManagement/Index.cshtml", model);
        }

        [Route("/taskgenerationmanagement/{areaId}/tasks")]
        public async Task<IActionResult> LoadTasks(int areaId)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Task.GetTaskTemplatesByAreaId, areaId));
            var taskTemplates = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(result.Message);
            return PartialView("~/Views/TaskGenerationManagement/_tasks.cshtml", taskTemplates);
        }

        [HttpPost]
        [Route("taskgenerationmanagement/save")]
        public async Task<IActionResult> PostPlanning([FromBody] List<DowntimePlanning> planning)
        {
            var config = new DowntimePlanningConfiguration { ConfigurationItems = planning };
            var result = await _connector.PostCall("/v1/generation/set_planning", JsonConvert.SerializeObject(config));
            return StatusCode((int)result.StatusCode);
        }
    }
}
