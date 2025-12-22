using EZGO.Api.Models;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Settings;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class SettingsController : BaseController
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public SettingsController(ILogger<SettingsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            // DI
            _logger = logger;
            _connector = connector;
            _languageService = language;

        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        public async Task<IActionResult> Index()
        {
            if (this.IsAdminCompany)
            {
                var output = new SettingsViewModel();
                output.IsAdminCompany = this.IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.SETTINGS;
                var companiesResult = await _connector.GetCall(@"/v1/companies");
                if (companiesResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                }
                var settingsResult = await _connector.GetCall(@"/v1/tools/resources/settings");
                if (settingsResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.Settings = ForceSpecificSort(settingsResult.Message.ToObjectFromJson<List<SettingModel>>().ToList());
                }
                output.ConnectionIP = _httpContextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"].ToString();

                output.ApplicationSettings = await this.GetApplicationSettings();
                return View(output);
            };

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SettingModel setting)
        {
            if (this.IsAdminCompany)
            {
                if (setting != null)
                {
                    ApiResponse result = null;

                    if (setting.Id > 0)
                    {
                        var settingsValueToSave = setting.Value;
                        if(!string.IsNullOrEmpty(setting.Value) && setting.Value != "ALL")
                        {
                            var settingValues = setting.Value.Split(",");
                            var companyIds = new List<int>();

                            bool nonNumericSettingFound = false;

                            foreach (var settingValue in settingValues)
                            {
                                if(!nonNumericSettingFound && int.TryParse(settingValue, out var companyId))
                                {
                                    companyIds.Add(companyId);
                                }
                                else
                                {
                                    nonNumericSettingFound = true;
                                    break;
                                }
                            }
                            if (!nonNumericSettingFound)
                            {
                                companyIds = companyIds.Distinct().OrderBy(c => c).ToList();
                                settingsValueToSave = string.Join(",", companyIds);
                            }
                        }
                        result = await _connector.PostCall(string.Concat("/v1/tools/resources/settings/change/", setting.Id, "?fulloutput=true"), settingsValueToSave.ToJsonFromObject());
                    }

                    if (result != null && result.StatusCode == HttpStatusCode.OK)
                    {
                        return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                    }
                    else
                    {
                        //other status returned, somethings wrong or can not continue due to business logic.
                        return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                    }
                }

                return StatusCode((int)HttpStatusCode.NoContent);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Route("/switchlanguage/{local}")]
        public async Task<IActionResult> SwitchLanguage(string local)
        {
            if (!string.IsNullOrEmpty(local))
            {
                SetCookie(Constants.General.LANGUAGE_COOKIE_STORAGE_KEY, local);
            }

            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, "");
        }

        private List<SettingModel> ForceSpecificSort(List<SettingModel> settingModels)
        {
            var sortOrder = new List<SettingModel>();

            settingModels = settingModels.OrderBy(x => x.SettingResourceType).ThenBy(x => x.Name).ToList();

            sortOrder.Add(settingModels.Where(x => x.SettingsKey == "FEATURE_TIER_ESSENTIALS").FirstOrDefault());
            sortOrder.Add(settingModels.Where(x => x.SettingsKey == "FEATURE_TIER_ADVANCED").FirstOrDefault());
            sortOrder.Add(settingModels.Where(x => x.SettingsKey == "FEATURE_TIER_PREMIUM").FirstOrDefault());

            sortOrder.AddRange(settingModels.Where(x => x.SettingsKey != "FEATURE_TIER_ESSENTIALS" && x.SettingsKey != "FEATURE_TIER_ADVANCED" && x.SettingsKey != "FEATURE_TIER_PREMIUM"));

            return sortOrder;
        }


    }
}
