using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;


namespace WebApp.Controllers
{
    public class CompanySettingsController : BaseController
    {
        private readonly ILogger<CompanySettingsController> _logger;
        private readonly ILanguageService _languageService;
        private readonly IApiConnector _connector;

        public CompanySettingsController(ILogger<CompanySettingsController> logger, IApiConnector connector, ILanguageService language, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        public async Task<IActionResult> Index()
        {
            var output = new CompanySettingsViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Setting";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CompanySetting;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            return PartialView(output);
        }

        public async Task<IActionResult> WorkInstructions()
        {
            var output = new CompanySettingsViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CompanySetting;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            return PartialView("_workInstructions", output);
        }

        public IActionResult Assessment()
        {
            return PartialView("_assessment");
        }

        public async Task<IActionResult> SkillsMatrix()
        {
            var output = new CompanySettingsViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CompanySetting;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            return PartialView("_skillsMatrix", output);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSkillMatrixChangeScoreStyle([FromBody] bool settingValue)
        {
            var companyId = this.User.GetProfile().Company.Id;
            var SettingValue = new List<SettingResourceItemTrueFalse>()
            {
                new SettingResourceItemTrueFalse()
                {
                    ResourceKey = ChangeableCompanySettings.FEATURE_MATRIX_CHANGED_SCORE_STANDARD.ToString(),
                    ResourceValue = settingValue
                }
            };

            var result = await _connector.PostCall(string.Format(Logic.Constants.Company.SetCompanySetting, companyId), SettingValue.ToJsonFromObject());
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWorkInstrucationChangesStatus([FromBody] bool settingValue)
        {
            var companyId = this.User.GetProfile().Company.Id;
            var SettingValue = new List<SettingResourceItemTrueFalse>()
            {
                new SettingResourceItemTrueFalse()
                {
                    ResourceKey = ChangeableCompanySettings.FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS.ToString(),
                    ResourceValue = settingValue
                }
            };

            var result = await _connector.PostCall(string.Format(Logic.Constants.Company.SetCompanySetting, companyId), SettingValue.ToJsonFromObject());
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        #region Legend Configuration
        [HttpGet]
        [Route("companysettings/legend")]
        public async Task<IActionResult> GetLegendConfiguration()
        {
            var companyId = this.User.GetProfile().Company.Id;
            var result = await _connector.GetCall(string.Format(Logic.Constants.Skills.SkillMatrixLegendUrl, companyId));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Content(result.Message, "application/json");
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpPost]
        [Route("companysettings/legend")]
        public async Task<IActionResult> SaveLegendConfiguration([FromBody] JsonElement configuration)
        {
            var companyId = this.User.GetProfile().Company.Id;
            var result = await _connector.PostCall(string.Format(Logic.Constants.Skills.SkillMatrixLegendUrl, companyId), configuration.GetRawText());
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Content(result.Message, "application/json");
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpPost]
        [Route("companysettings/legend/item")]
        public async Task<IActionResult> UpdateLegendItem([FromBody] JsonElement item)
        {
            var companyId = this.User.GetProfile().Company.Id;
            var result = await _connector.PostCall(string.Format(Logic.Constants.Skills.SkillMatrixLegendItemUrl, companyId), item.GetRawText());
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Content(result.Message, "application/json");
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpPost]
        [Route("companysettings/legend/reset")]
        public async Task<IActionResult> ResetLegendToDefault()
        {
            var companyId = this.User.GetProfile().Company.Id;
            var result = await _connector.PostCall(string.Format(Logic.Constants.Skills.SkillMatrixLegendResetUrl, companyId), "{}");
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Content(result.Message, "application/json");
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }
        #endregion
    }
}