using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;
using WebApp.Models.Skills;
using WebApp.Helpers;


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
            output.SkillMatrixLegend = await GetLegendOrDefault(output.SkillMatrixLegend);
            return PartialView("_skillsMatrix", output);
        }

        [HttpGet]
        public async Task<IActionResult> SkillMatrixLegend()
        {
            var legend = await GetLegendOrDefault(new List<SkillMatrixLegendItem>());
            return Ok(legend);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSkillMatrixLegend([FromBody] List<SkillMatrixLegendItem> legendItems)
        {
            var payload = legendItems ?? new List<SkillMatrixLegendItem>();
            var postResult = await _connector.PostCall("/v1/skillsmatrix/legend", payload.ToJsonFromObject(true));

            if (postResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(legendItems);
            }

            return StatusCode((int)postResult.StatusCode, postResult.Message);
        }

        private async Task<List<SkillMatrixLegendItem>> GetLegendOrDefault(List<SkillMatrixLegendItem> current)
        {
            var legendItems = current ?? new List<SkillMatrixLegendItem>();

            var legendResult = await _connector.GetCall("/v1/skillsmatrix/legend");
            if (legendResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    legendItems = legendResult.Message.ToObjectFromJson<List<SkillMatrixLegendItem>>();
                }
                catch
                {
                }
            }

            if (legendItems == null || !legendItems.Any())
            {
                legendItems = SkillMatrixLegendItem.CreateDefaultLegend();
            }

            return legendItems.OrderBy(x => x.DisplayOrder).ToList();
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
    }
}
