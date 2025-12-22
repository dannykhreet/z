using EZGO.Api.Models;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Setup;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Converters;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Company;
using WebApp.Models.Settings;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;
        private readonly IApiDatawarehouseConnector _datawarehouseConnector;

        public CompanyController(ILogger<CompanyController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService, IApiDatawarehouseConnector datawarehouseConnector) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
            _datawarehouseConnector = datawarehouseConnector;

        }


        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        public async Task<IActionResult> Index()
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }

                var output = new CompaniesViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.EnableCompanyManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableCompanyManagement");
                output.EnableHoldingManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableHoldingManagement");
                var companiesResult = await _connector.GetCall(@"/v1/companies?include=companysettings,holding");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                }

                var companiesFeaturesResult = await _connector.GetCall(@"/v1/companiesfeatures");
                if (companiesFeaturesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.CompaniesFeatures = companiesFeaturesResult.Message.ToObjectFromJson<List<CompanyFeaturesModel>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                var settingsResult = await _connector.GetCall(@"/v1/tools/resources/settings");
                if (settingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Settings = settingsResult.Message.ToObjectFromJson<List<SettingModel>>().ToList();
                }

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }

                output.ApplicationSettings = await GetApplicationSettings();
                output.CompanySettings = BuildGeneralSettingsListCompany(settingModels: output.Settings, companies: output.Companies);

                foreach (var item in output.Companies)
                {
                    item.HoldingCompanySecurityGUID = item.Settings.Where(x => x.ResourceId == 71).Any() ? item.Settings.Where(x => x.ResourceId == 71).FirstOrDefault().Value : string.Empty;
                }

                return View(output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/features/overview")]
        public async Task<IActionResult> FeatureOverview()
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }

                var output = new CompaniesViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.EnableCompanyManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableCompanyManagement");
                output.EnableHoldingManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableHoldingManagement");
                var companiesResult = await _connector.GetCall(@"/v1/companies?include=companysettings");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                }

                var companiesFeaturesResult = await _connector.GetCall(@"/v1/companiesfeatures");
                if (companiesFeaturesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.CompaniesFeatures = companiesFeaturesResult.Message.ToObjectFromJson<List<CompanyFeaturesModel>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                var settingsResult = await _connector.GetCall(@"/v1/tools/resources/settings");
                if (settingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Settings = settingsResult.Message.ToObjectFromJson<List<SettingModel>>().ToList();
                }

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }

                output.ApplicationSettings = await GetApplicationSettings();
                output.CompanySettings = BuildGeneralSettingsListCompany(settingModels: output.Settings, companies: output.Companies);
                return View("~/Views/Company/FeaturesOverview.cshtml", output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }


        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/createserviceuser")]
        public async Task<IActionResult> AddServiceAccount([FromBody] int companyid)
        {
            string generatedName = string.Empty;

            if (IsAdminCompany)
            {
                var result = await _connector.PostCall("/v1/tools/company/createserviceuser", companyid.ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    generatedName = result.Message.ToString();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, result.Message.ToJsonFromObject());
                }
            }

            if (!string.IsNullOrEmpty(generatedName))
            {
                return StatusCode((int)HttpStatusCode.OK, generatedName.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Error while posting service user.".ToJsonFromObject());
            }
        }


        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/reducetaskgenerationconfigs")]
        public async Task<IActionResult> ReduceTaskGenerationConfigs([FromBody] int companyid)
        {
            if (IsAdminCompany)
            {
                var result = await _connector.PostCall("/v1/tools/reducetaskgenerationconfigs", companyid.ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return StatusCode((int)HttpStatusCode.OK, result.Message.ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, result.Message.ToJsonFromObject());
                }
            }
            return StatusCode((int)HttpStatusCode.BadRequest, $"Error while reducing task generation configs for company {companyid}.".ToJsonFromObject());
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        public async Task<IActionResult> Details(int id)
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }
                var output = new CompanyViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.CompanySettings = new SetupCompanySettings() { CompanyId = id };
                if (id > 0)
                {
                    var companyResult = await _connector.GetCall(string.Concat("/v1/company/", id, "?include=users,holding,holdingunits,companysettings"));
                    if (companyResult.StatusCode == HttpStatusCode.OK)
                    {
                        var company = companyResult.Message.ToObjectFromJson<Company>();
                        output.Name = company.Name;
                        output.Description = company.Description;
                        output.Id = company.Id;
                        output.Picture = company.Picture;
                        output.ManagerId = company.ManagerId;

                        if (company.Holding != null)
                        {
                            output.HoldingId = company.Holding.Id;
                            output.HoldingCompanySecurityGUID = company.HoldingCompanySecurityGUID;
                        }
                        if (company.HoldingUnits != null && company.HoldingUnits.Any())
                        {
                            output.HoldingUnitIds = company.HoldingUnits.Select(x => x.Id).ToList();
                            output.HoldingUnits = company.HoldingUnits;
                        }

                        if (company.Users != null)
                        {
                            output.Users = company.Users;
                        }

                        foreach (var companySetting in company.Settings)
                        {
                            switch (companySetting.ResourceId)
                            {
                                case 1:
                                    output.CompanySettings.TimeZone = companySetting.Value;
                                    break;
                                case 123:
                                    output.CompanySettings.Coords = companySetting.Value;
                                    break;
                                case 124:
                                    output.CompanySettings.Country = companySetting.Value;
                                    break;
                                case 125:
                                    output.CompanySettings.MapsJson = companySetting.Value;
                                    break;
                                case 43:
                                    output.CompanySettings.Locale = companySetting.Value;
                                    break;
                                case 112:
                                    output.CompanySettings.SapPmCompanyId = companySetting.Value;
                                    break;
                                case 113:
                                    output.CompanySettings.SapPmNotificationOptions = companySetting.Value;
                                    break;
                                case 119:
                                    output.CompanySettings.SapPmAuthorizationUrl = companySetting.Value;
                                    break;
                                case 120:
                                    output.CompanySettings.SapPmFunctionalLocationUrl = companySetting.Value;
                                    break;
                                case 121:
                                    output.CompanySettings.SapPmNotificationUrl = companySetting.Value;
                                    break;
                                case 131:
                                    output.CompanySettings.IpRestrictionList = companySetting.Value;
                                    break;
                                case 127:
                                    output.CompanySettings.VirtualTeamLeadModules = companySetting.Value;
                                    break;
                                case 132:
                                    output.CompanySettings.TranslationModules = companySetting.Value;
                                    break;
                                case 129:
                                    output.CompanySettings.TranslationLanguages = companySetting.Value;
                                    break;
                                case 133:
                                    output.CompanySettings.SapPmTimezone = companySetting.Value;
                                    break;
                            }
                        }
                    }

                    var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                    if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                    }

                    var companiesFeaturesResult = await _connector.GetCall(@"/v1/companiesfeatures");
                    if (companiesFeaturesResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.CompaniesFeatures = companiesFeaturesResult.Message.ToObjectFromJson<List<CompanyFeaturesModel>>();
                    }

                    var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                    if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                    }

                    output.Countries = CountryList.GetCountries();

                    var featureSettingsResult = await _connector.GetCall(@"/v1/tools/resources/settings");
                    if (featureSettingsResult.StatusCode == HttpStatusCode.OK)
                    {
                        var featureSettings = featureSettingsResult.Message.ToObjectFromJson<List<SettingResource>>();
                        var relevantSettings = featureSettings.Where(s => (s.Value.Contains(id.ToString()) || s.Value == "ALL") && "16,17,18,27,84,93,130,126,128".Contains(s.Id.ToString())).OrderBy(s => s.Id).ToList();
                        foreach (var relevantSetting in relevantSettings)
                        {
                            if (relevantSetting.Id == 16)
                            {
                                output.CompanySettings.TierLevel = "essential";
                            }
                            else if (relevantSetting.Id == 17)
                            {
                                output.CompanySettings.TierLevel = "advanced";
                            }
                            else if (relevantSetting.Id == 18)
                            {
                                output.CompanySettings.TierLevel = "advanced";
                            }
                            else if (relevantSetting.Id == 27)
                            {
                                output.CompanySettings.EnableTaskGeneration = true;
                            }
                            else if (relevantSetting.Id == 84)
                            {
                                output.CompanySettings.EnableDataWarehouse = true;
                            }
                            else if (relevantSetting.Id == 93)
                            {
                                output.CompanySettings.EnableWorkInstructionChangesNotifications = true;
                            }
                            else if (relevantSetting.Id == 130)
                            {
                                output.CompanySettings.EnableIpRestrictions = true;
                            }
                            else if (relevantSetting.Id == 126)
                            {
                                output.CompanySettings.EnableVirtualTeamLead = true;
                            }
                            else if (relevantSetting.Id == 128)
                            {
                                output.CompanySettings.EnableTranslations = true;
                            }
                        }
                    }

                    //Possible DW data
                    if (output.Id > 0)
                    {
                        //Check if data is available to retrieve data from DW
                        if(!string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_USER")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_PWD")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_APPID")))
                        {
                            var dwCompanyUser = new EZ.Api.DataWarehouse.Models.User();
                            dwCompanyUser.CompanyId = output.Id;
                            dwCompanyUser.HoldingId = 0;

                            var usersCompanyDWResult = await _datawarehouseConnector.PostCall(@"/data/management/user", dwCompanyUser.ToJsonFromObject(), username: _configurationHelper.GetValueAsString("DW_USER"), password: _configurationHelper.GetValueAsString("DW_PWD"), appid: _configurationHelper.GetValueAsString("DW_APPID"));
                            if (usersCompanyDWResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(usersCompanyDWResult.Message))
                            {
                                output.DatawarehouseCompanyUser = usersCompanyDWResult.Message.ToObjectFromJson<EZ.Api.DataWarehouse.Models.User>();
                            }

                            output.EnableDatawarehouseUserMutation = _configurationHelper.GetValueAsBool("AppSettings:EnableDWUserCreationFromCMS");

                            if (output.HoldingId > 0)
                            {
                                var dwHoldingUser = new EZ.Api.DataWarehouse.Models.User();
                                dwHoldingUser.CompanyId = 0;
                                dwHoldingUser.HoldingId = output.HoldingId;

                                var usersHoldingDWResult = await _datawarehouseConnector.PostCall(@"/data/management/user", dwHoldingUser.ToJsonFromObject(), username: _configurationHelper.GetValueAsString("DW_USER"), password: _configurationHelper.GetValueAsString("DW_PWD"), appid: _configurationHelper.GetValueAsString("DW_APPID"));
                                if (usersHoldingDWResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(usersHoldingDWResult.Message))
                                {
                                    output.DatawarehouseHoldingUser = usersHoldingDWResult.Message.ToObjectFromJson<EZ.Api.DataWarehouse.Models.User>();
                                }
                            }

                        } else
                        {
                            output.EnableDatawarehouseUserMutation = false; //dispable mutation if no DW info available for retrieving data through dw api. 
                            Debug.WriteLine("No DW information available to retrieve data");
                        }
                        
                    }

                }

                output.ApplicationSettings = await GetApplicationSettings();
                return View(output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/create")]
        [Route("/company/new")]
        public async Task<IActionResult> New()
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }
                var output = new CompanyViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                output.Countries = CountryList.GetCountries();

                //var settingsResult = await _connector.GetCall(@"/v1/tools/resources/settings");
                //if (settingsResult.StatusCode == System.Net.HttpStatusCode.OK)
                //{
                //    output.Settings = settingsResult.Message.ToObjectFromJson<List<SettingModel>>().ToList();
                //}

                output.ApplicationSettings = await GetApplicationSettings();
                return View("~/Views/Company/New.cshtml", output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/create")]
        public async Task<IActionResult> Create([FromBody] SetupCompany company)
        {
            if (IsAdminCompany)
            {
                if (company != null)
                {
                    var result = await _connector.PostCall("/v1/company/setup", company.ToJsonFromObject());
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var companyResult = result.Message.ToObjectFromJson<SetupCompany>();
                        if (companyResult.CompanyId > 0)
                        {
                            return StatusCode((int)HttpStatusCode.OK, companyResult.ToJsonFromObject());
                        }
                        else
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, "Creation failed, something went wrong");
                        }
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, result.Message.ToJsonFromObject());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            };

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/save")]
        public async Task<IActionResult> Save([FromBody] CompanyWithSettings company)
        {
            if (IsAdminCompany)
            {
                if (company != null)
                {
                    var companySettings = company.CompanySettings;
                    company.CompanySettings = null;
                    var result = await _connector.PostCall(string.Concat("/v1/company/change/", company.Id), company.ToJsonFromObject());
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var companyResult = result.Message.ToObjectFromJson<bool>();
                        if (companyResult)
                        {
                            //update settings
                            var settingsRequest = await _connector.PostCall(string.Concat("/v1/company/setup/companysettings/", companySettings.CompanyId), companySettings.ToJsonFromObject());

                            var settingsResult = settingsRequest.Message.ToObjectFromJson<bool>();
                            if (settingsResult)
                            {
                                return StatusCode((int)HttpStatusCode.OK, companyResult.ToJsonFromObject());
                            }
                        }
                        else
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, "Change failed, something went wrong");
                        }
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, result.Message.ToJsonFromObject());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            };

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/validate")]
        public async Task<IActionResult> Validate([FromBody] SetupCompany company)
        {
            if (IsAdminCompany)
            {
                if (company != null)
                {
                    var result = await _connector.PostCall("/v1/company/setup/validate", company.ToJsonFromObject());
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var success = Convert.ToBoolean(result.Message);
                        if (success == true)
                        {
                            return StatusCode((int)HttpStatusCode.OK, success.ToJsonFromObject());
                        }
                        else
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, "Validation failed, something went wrong");
                        }
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, result.Message.ToJsonFromObject());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            };

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/remove/{companyid}/{datevalidator}/{deletevalidator}")]
        public async Task<IActionResult> RemoveCompany([FromBody] SetupCompany company, [FromRoute] int companyid, [FromRoute] string datevalidator, [FromRoute] string deletevalidator)
        {
            if (IsAdminCompany)
            {
                if (company != null && company.CompanyId.HasValue && company.CompanyId.Value > 1) //don't remove company 1
                {
                    if (datevalidator == DateTime.Now.ToString("ddMMyyyy") && deletevalidator == string.Concat("DELETE", company.CompanyId.Value))
                    {
                        var companyIdToBeRemoved = company.CompanyId.Value;
                        var result = await _connector.PostCall(string.Concat("/v1/company/remove/", companyIdToBeRemoved), company.ToJsonFromObject());
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            return StatusCode((int)HttpStatusCode.OK, result.Message.ToJsonFromObject());
                        }
                        else
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, result.Message.ToJsonFromObject());
                        }
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, "No correct date and confirmation message".ToJsonFromObject());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            };

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/details/upload")]
        [Route("/holding/details/upload")]
        public async Task<string> Upload(IFormCollection data)
        {
            if (IsAdminCompany)
            {
                foreach (IFormFile item in data.Files)
                {
                    //var fileContent = item;
                    if (item != null && item.Length > 0)
                    {
                        // get a stream
                        using (var ms = new MemoryStream())
                        {

                            item.CopyTo(ms);
                            var fileBytes = ms.ToArray();

                            using var form = new MultipartFormDataContent();
                            using var fileContent = new ByteArrayContent(fileBytes);
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            form.Add(fileContent, "file", Path.GetFileName(item.FileName));

                            int mediaType = 21;//company image

                            var endpoint = string.Format(Logic.Constants.Checklist.UploadPictureUrl, mediaType);

                            ApiResponse filepath = await _connector.PostCall(endpoint, form);
                            string output = filepath.Message;
                            if (data["filekind"] != "video")
                            {
                                output = filepath.Message.Replace("media/", "");
                            }
                            return output;

                        }

                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            return string.Empty;

        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        public async Task<IActionResult> SaveSetting([FromBody] SettingResourceItem setting)
        {
            if (IsAdminCompany)
            {
                if (setting != null && setting.CompanyId > 0 && setting.ResourceId > 0)
                {
                    ApiResponse result = null;
                    result = await _connector.PostCall(string.Format("/v1/company/{0}/settings/change/", setting.CompanyId), setting.ToJsonFromObject());

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


            //
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        #region - reporting / stats -
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/reports")]
        public async Task<IActionResult> Reports()
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }
                var output = new CompanyReportViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;

                output.ApplicationSettings = await GetApplicationSettings();
                return View("~/Views/Company/Report.cshtml", output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/reports/{year}/{month}")]
        public async Task<IActionResult> Report(int month, int year)
        {
            if (IsAdminCompany)
            {
                var startDate = new DateTime(year: year, month: month, day: 1);
                var endDate = new DateTime(year: (month == 12) ? year + 1 : year, month: (month == 12) ? 1 : month + 1, day: 1);
                ApiResponse result = null;
                result = await _connector.GetCall(string.Format("/v1/reporting/companystatistics?starttime={0}&endtime={1}", startDate.ToString("dd-MM-yyyy HH:mm:ss"), endDate.ToString("dd-MM-yyyy HH:mm:ss")));

                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                }
                else
                {
                    //other status returned, somethings wrong or can not continue due to business logic.
                    return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                }

            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }




        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/statistics")]
        [Route("/company/statistics/{companyid}")]
        [Route("/holding/statistics/{holdingid}")]
        public async Task<IActionResult> Statistics([FromRoute] int companyid, [FromRoute] int holdingid)
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }
                var output = new CompanyStatisticViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.CompanyId = companyid;
                output.HoldingId = holdingid;

                var companiesResult = await _connector.GetCall(@"/v1/companies");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                    output.Companies = output.Companies.Where(w => !w.Name.StartsWith("DELETED") && !w.Name.Contains("TO BE DELETED")).OrderBy(x => x.Name).ToList();
                }

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }

                output.ApplicationSettings = await GetApplicationSettings();
                return View("~/Views/Company/Statistics.cshtml", output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/statistics/{companyid}/retrieve")]
        [Route("/holding/statistics/{holdingid}/retrieve")]
        public async Task<IActionResult> StatisticsCompany(int companyid, int holdingid)
        {
            if (IsAdminCompany)
            {
                string statsUrl = string.Format("/v1/company/{0}/statistics", companyid);
                if (companyid == 0)
                {
                    statsUrl = "/v1/company/all/statistics";
                };
                if (holdingid != 0)
                {
                    statsUrl = string.Format("/v1/company/holding/{0}/statistics", holdingid);
                };

                ApiResponse result = null;
                result = await _connector.GetCall(statsUrl);

                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                }
                else
                {
                    //other status returned, somethings wrong or can not continue due to business logic.
                    return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                }

            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }


        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/loginoverview")]
        [Route("/company/loginoverview/{companyid}")]
        [Route("/holding/loginoverview/{holdingid}")]
        public async Task<IActionResult> CompanyLoginOverview([FromRoute] int companyid, [FromRoute] int holdingid)
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }
                var output = new CompanyLoginsViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.CompanyId = companyid;
                output.HoldingId = holdingid;

                var companiesResult = await _connector.GetCall(@"/v1/companies?include=companysettings");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                    output.Companies = output.Companies.Where(w => !w.Name.StartsWith("DELETED") && !w.Name.Contains("TO BE DELETED")).OrderBy(x => x.Name).ToList();
                }

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                var dataResult = await _connector.GetCall(string.Format("/v1/tools/raw/0/log_app?starttimestamp={0}&endtimestamp={1}", DateTime.Now.AddMinutes(-60).ToString("dd-MM-yyyy HH:mm"), DateTime.Now.ToString("dd-MM-yyyy HH:mm")));
                if (dataResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.Data = dataResult.Message.ToObjectFromJson<RawData>();
                }

                //get all company ids from data collection
                var loggedInCompanyIds = new List<int>();
                if (output.Data != null && output.Data.Data != null)
                {

                    foreach (var item in output.Data.Data)
                    {
                        if (item != null)
                        {
                            //get companyid
                            if (item.ToArray()[1] != null)
                            {
                                loggedInCompanyIds.Add(Convert.ToInt32(item.ToArray()[1]));
                            }
                        }
                    }
                }

                //get all countries
                var countryList = new List<string>();
                var tc = new TimezoneToCountryConverter();
                foreach (var companyId in loggedInCompanyIds)
                {
                    var foundCompany = output.Companies.Where(x => x.Id == companyId).FirstOrDefault();
                    if (foundCompany != null)
                    {
                        var foundTimezoneString = foundCompany.Settings?.Where(y => y.ResourceId == 1)?.FirstOrDefault()?.Value;

                        if (!string.IsNullOrEmpty(foundTimezoneString))
                        {
                            countryList.Add(tc.GetCountryIsoCode(foundTimezoneString));
                        }
                    }

                }

                //get js output for display
                var outputJsDataString = new StringBuilder();

                foreach (var countryCode in countryList.Distinct())
                {
                    outputJsDataString.AppendFormat("['{0}', {1}],", countryCode, countryList.Count(z => z == countryCode));
                }
                output.JsDataOutput = outputJsDataString.ToString();

                outputJsDataString.Clear();
                outputJsDataString = null;


                //foreach (var companyId in loggedInCompanyIds.Distinct()) {
                //    var tz = output.se
                //    outputJsDataString.AppendFormat("['{0}', {1}]", tc.GetCountryIsoCode(), ;
                //}


                //var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
                //var cu = new CultureInfo("nl-nl");
                //var ro = new RegionInfo(cu.LCID);

                //string defaultTimeZoneId = cu.Name;
                //TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(defaultTimeZoneId);


                output.ApplicationSettings = await GetApplicationSettings();
                return View("~/Views/Company/LoginOverview.cshtml", output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/loginoverview/retrieve")]
        [Route("/company/loginoverview/retrieve/{companyid}")]
        [Route("/holding/loginoverview/retrieve/{holdingid}")]
        public async Task<IActionResult> CompanyLoginOverviewRetrieveData([FromRoute] int companyid, [FromRoute] int holdingid)
        {
            if (IsAdminCompany)
            {

                var output = new CompanyLoginsViewModel();
                output.CompanyId = companyid;
                output.HoldingId = holdingid;

                var companiesResult = await _connector.GetCall(@"/v1/companies?include=companysettings");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                    output.Companies = output.Companies.Where(w => !w.Name.StartsWith("DELETED") && !w.Name.Contains("TO BE DELETED")).OrderBy(x => x.Name).ToList();
                }

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                var dataResult = await _connector.GetCall(string.Format("/v1/tools/raw/0/log_app?starttimestamp={0}&endtimestamp={1}", DateTime.Now.AddMinutes(-5).ToString("dd-MM-yyyy HH:mm"), DateTime.Now.ToString("dd-MM-yyyy HH:mm")));
                if (dataResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.Data = dataResult.Message.ToObjectFromJson<RawData>();
                }

                //get all company ids from data collection
                var loggedInCompanyIds = new List<int>();
                if (output.Data != null && output.Data.Data != null)
                {
                    foreach (var item in output.Data.Data)
                    {
                        if (item != null)
                        {
                            //get companyid
                            if (item.ToArray()[1] != null)
                            {
                                loggedInCompanyIds.Add(Convert.ToInt32(item.ToArray()[1]));
                            }
                        }
                    }
                }


                //get all countries
                var countryList = new List<string>();
                var tc = new TimezoneToCountryConverter();
                foreach (var companyId in loggedInCompanyIds)
                {
                    var foundCompany = output.Companies.Where(x => x.Id == companyId).FirstOrDefault();
                    if (foundCompany != null)
                    {
                        var foundTimezoneString = foundCompany.Settings?.Where(y => y.ResourceId == 1)?.FirstOrDefault()?.Value;

                        if (!string.IsNullOrEmpty(foundTimezoneString))
                        {
                            countryList.Add(tc.GetCountryIsoCode(foundTimezoneString));
                        }
                    }

                }

                //get js output for display
                var outputJsDataString = new StringBuilder();

                foreach (var countryCode in countryList.Distinct())
                {
                    outputJsDataString.AppendFormat("{0},{1}|", countryCode, countryList.Count(z => z == countryCode) * 10);
                }
                if (!string.IsNullOrEmpty(outputJsDataString.ToString()))
                {
                    output.JsDataOutput = string.Format("Country,Popularity|{0}", !string.IsNullOrEmpty(outputJsDataString.ToString()) ? outputJsDataString.ToString().Substring(0, outputJsDataString.ToString().Length - 1) : "");
                }
                else
                {
                    output.JsDataOutput = "Country,Popularity";
                }


                outputJsDataString.Clear();
                outputJsDataString = null;
                //return output for JS parsing, normal JSparse and double arrays won't parse properly through ajax call 
                return StatusCode((int)HttpStatusCode.OK, output.JsDataOutput);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        #endregion

        #region - holding -

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/holding/save")]
        public async Task<IActionResult> SaveHolding([FromBody] Holding holding)
        {
            if (IsAdminCompany)
            {
                ApiResponse result = null;
                result = await _connector.PostCall("/v1/company/holdings/save", holding.ToJsonFromObject());

                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    //TODO Add responses handling
                    return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                }
                else
                {
                    //other status returned, somethings wrong or can not continue due to business logic.
                    return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                }

            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/holding/generateguid")]
        public async Task<IActionResult> GenerateGuidHolding()
        {
            if (IsAdminCompany)
            {
                return StatusCode((int)HttpStatusCode.OK, Guid.NewGuid().ToString().ToJsonFromObject());
            };

            await Task.CompletedTask;

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/holding/unit/save")]
        public async Task<IActionResult> SaveHoldingUnit([FromBody] HoldingUnit holdingunit)
        {
            if (IsAdminCompany)
            {
                if (holdingunit.HoldingId > 0)
                {
                    ApiResponse result = null;
                    result = await _connector.PostCall($"/v1/company/holding/{holdingunit.HoldingId}/unit/save", holdingunit.ToJsonFromObject());

                    if (result != null && result.StatusCode == HttpStatusCode.OK)
                    {
                        //TODO Add responses handling
                        return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                    }
                    else
                    {
                        //other status returned, somethings wrong or can not continue due to business logic.
                        return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                    }
                }

            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/holding/overview")]
        public async Task<IActionResult> CompanyHoldingOverview()
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }

                var output = new CompaniesViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.EnableCompanyManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableCompanyManagement");
                output.EnableHoldingManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableHoldingManagement");
                var companiesResult = await _connector.GetCall(@"/v1/companies?include=companysettings,holding,holdingunits");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                }

                var companiesFeaturesResult = await _connector.GetCall(@"/v1/companiesfeatures");
                if (companiesFeaturesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.CompaniesFeatures = companiesFeaturesResult.Message.ToObjectFromJson<List<CompanyFeaturesModel>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                var settingsResult = await _connector.GetCall(@"/v1/tools/resources/settings");
                if (settingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Settings = settingsResult.Message.ToObjectFromJson<List<SettingModel>>().ToList();
                }

                var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings?include=holdingunits");
                if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                }



                output.ApplicationSettings = await GetApplicationSettings();
                output.CompanySettings = BuildGeneralSettingsListCompany(settingModels: output.Settings, companies: output.Companies);

                foreach (var item in output.Companies)
                {
                    item.HoldingCompanySecurityGUID = item.Settings.Where(x => x.ResourceId == 71).Any() ? item.Settings.Where(x => x.ResourceId == 71).FirstOrDefault().Value : string.Empty;
                }

                return View("~/Views/Company/HoldingOverview.cshtml", output);
            }
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/holding/create")]
        [Route("/company/holding/details/{id}")]
        public async Task<IActionResult> CompanyHoldingDetails([FromRoute]int id)
        {
            if (IsAdminCompany)
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }

                var output = new HoldingViewModel();
                output.IsAdminCompany = IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.COMPANIES;
                output.HoldingId = id;

                var companiesResult = await _connector.GetCall(@"/v1/companies?include=companysettings,holding,holdingunits");
                if (companiesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                }

                var databaseTimezonesResult = await _connector.GetCall(@"/v1/tools/database_supported_timezones");
                if (databaseTimezonesResult.StatusCode == HttpStatusCode.OK)
                {
                    output.Timezones = databaseTimezonesResult.Message.ToObjectFromJson<List<DatabaseTimezoneItem>>();
                }

                if(output.HoldingId > 0)
                {
                    var holdingResult = await _connector.GetCall($"/v1/company/holding/{output.HoldingId}?include=holdingsettings");
                    if (holdingResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Holding = holdingResult.Message.ToObjectFromJson<Holding>();
                    }

                    //Check if data is available to retrieve data from DW
                    if (!string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_USER")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_PWD")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_APPID")))
                    {
                        var dwHoldingUser = new EZ.Api.DataWarehouse.Models.User();
                        dwHoldingUser.CompanyId = 0;
                        dwHoldingUser.HoldingId = output.HoldingId;

                        var usersHoldingDWResult = await _datawarehouseConnector.PostCall(@"/data/management/user", dwHoldingUser.ToJsonFromObject(), username: _configurationHelper.GetValueAsString("DW_USER"), password: _configurationHelper.GetValueAsString("DW_PWD"), appid: _configurationHelper.GetValueAsString("DW_APPID"));
                        if (usersHoldingDWResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(usersHoldingDWResult.Message))
                        {
                            output.DatawarehouseHoldingUser = usersHoldingDWResult.Message.ToObjectFromJson<EZ.Api.DataWarehouse.Models.User>();
                        }

                        output.EnableDatawarehouseUserMutation = _configurationHelper.GetValueAsBool("AppSettings:EnableDWUserCreationFromCMS");
                    }
                    else
                    {
                        Debug.WriteLine("No DW information available to retrieve data");
                    }

                }

                output.ApplicationSettings = await GetApplicationSettings();

                return View("~/Views/Company/HoldingDetails.cshtml", output);
            }
            ;
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        #endregion

        [NonAction]
        private Dictionary<int, List<string>> BuildGeneralSettingsListCompany(List<SettingModel> settingModels, List<Company> companies)
        {
            var output = new Dictionary<int, List<string>>();
            var companyIds = companies.Select(x => x.Id).ToList();
            var parsableSettings = new[]{
                "FEATURE_ACTIONONTHESPOT",
                "FEATURE_ACTIONS",
                "FEATURE_AUDITS",
                "FEATURE_AUDITTEMPLATEPROPERTIES",
                "FEATURE_CHECKLISTS",
                "FEATURE_CHECKLISTTEMPLATEPROPERTIES",
                "FEATURE_COMMENT",
                "FEATURE_EXPORTS",
                "FEATURE_EXPORTS_ADVANCED",
                "FEATURE_EXPORTS_TASKPROPERTIES",
                "FEATURE_FACTORY_FEED",
                "FEATURE_MARKET",
                "FEATURE_ORDER_AUDITS",
                "FEATURE_ORDER_CHECKLISTS",
                "FEATURE_ORDER_TASKS",
                "FEATURE_REPORTING",
                "FEATURE_REQUIREPROOF",
                "FEATURE_RUNNING_ASSESSMENTS_CMS",
                "FEATURE_SKILLASSESSMENTS",
                "FEATURE_SKILLSMATRIX",
                "FEATURE_SKILLSMATRIX_MANDATORYSKILLS",
                "FEATURE_SKILLSMATRIX_OPERATIONALSKILLS",
                "FEATURE_SKILLSMATRIX_OPERATIONALBEHAVIOUR",
                "FEATURE_TAGS",
                "FEATURE_TASKS",
                "FEATURE_TASKTEMPLATEPROPERTIES",
                "FEATURE_TASKTEMPLATEPROPERTIES_AUDITS",
                "FEATURE_TASKTEMPLATEPROPERTIES_CHECKLISTS",
                "FEATURE_TIER_ADVANCED",
                "FEATURE_TIER_ESSENTIALS",
                "FEATURE_TIER_PREMIUM",
                "FEATURE_WORKINSTRUCTIONS",
                "MARKET_SAP",
                "MARKET_SOLVACE",
                "MARKET_ULTIMO",
                "TECH_FIREBASELOGGING_CMS",
                "TECH_SUPPORT_CHAT",
                "TECH_TASKGENERATION",
                "TECH_DATAWAREHOUSE",
                "FEATURE_WORKINSTRUCTION_ITEM_ATTACHMENT_PDF",
                "FEATURE_WORKINSTRUCTION_ITEM_ATTACHMENT_LINK",
                "FEATURE_TEMPLATE_SHARING",
                "FEATURE_AUDIT_TRAIL_DETAILS",
                "FEATURE_TRANSFERABLE_CHECKLISTS",
                "TECH_FLATTEN_DATA",
                "TECH_FLATTEN_DATA_FALLBACK",
                "TECH_FLATTEN_DATA_SEARCH",
                "FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS",
                "FEATURE_CHECKLIST_STAGES",
                "FEATURE_MODIFY_OWN_COMPANY_SETTINGS_WORKINSTRUCTION",
                "FEATURE_MODIFY_OWN_COMPANY_SETTINGS_ASSESSMENT",
                "FEATURE_MODIFY_OWN_COMPANY_SETTINGS_MATRIX",
                "FEATURE_MATRIX_CHANGED_SCORE_STANDARD",

            };
            foreach (var companyId in companyIds)
            {
                output.Add(companyId, new List<string>());
            }

            foreach (var settingModel in settingModels)
            {
                if (parsableSettings.Contains(settingModel.SettingsKey))
                {
                    if (settingModel.Value == "ALL")
                    {
                        foreach (var companyId in companyIds)
                        {
                            if (output.Keys.Contains(companyId))
                            {
                                var currentList = output[companyId];
                                if (currentList != null)
                                {
                                    currentList.Add(settingModel.SettingsKey);
                                    output[companyId] = currentList;
                                }
                            }

                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(settingModel.Value))
                        {
                            foreach (var id in settingModel.Value.Split(","))
                            {
                                int companyId = 0;
                                if (int.TryParse(id, out companyId))
                                {
                                    if (output.Keys.Contains(companyId))
                                    {
                                        var currentList = output[companyId];
                                        if (currentList != null)
                                        {
                                            currentList.Add(settingModel.SettingsKey);
                                            output[companyId] = currentList;
                                        }
                                    }


                                }
                            }
                        }
                    }
                }

            }

            return output;
        }
    }
}
