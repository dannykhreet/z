using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Logs;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tools;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Preprocessors;
using EZGO.Api.Utils.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
 
namespace EZGO.Api.Controllers.General
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ToolsController : BaseController<ToolsController>
    {
        private readonly IDataCheckManager _dataCheckManager;
        private readonly IToolsManager _toolsManager;
        private readonly IGeneralManager _generalManager;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly IDatabaseMigationHelper _migrationManager;

        #region - constructor(s) -
        public ToolsController(ILogger<ToolsController> logger, IApplicationUser applicationUser, IDatabaseMigationHelper migrationManager, IGeneralManager generalManager, IConfigurationHelper configurationHelper, IDataCheckManager dataCheckManager, IToolsManager toolsManager, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider) : base(logger, applicationUser, configurationHelper)
        {
            _dataCheckManager = dataCheckManager;
            _toolsManager = toolsManager;
            _generalManager = generalManager;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _migrationManager = migrationManager;
        }
        #endregion

        #region - logging -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/log")]
        public async Task<IActionResult> GetLogs()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            var dbEnvironment = _dataCheckManager.GetEnvironment();
            string output = "";
            //check if environments can be used for getting log information.
            //Only when the app is in development or test and the database connections are on development or test logging may be retrieved.
            if ((new[] { "development", "localdevelopment", "test" }).Contains(appEnvironment.ToLower())
             && (new[] { "DEVELOPMENT", "TESTING PRODUCTION", "TESTING", "LOCAL", "UNKNOWN" }).Contains(dbEnvironment.ToUpper()))
            {
                var possibleUserForAccess = _configurationHelper.GetValueAsString(ApiSettings.ENABLE_LOG_READ_FOR_USER_CONFIG_KEY);
                if (!string.IsNullOrEmpty(possibleUserForAccess))
                {
                    if (await this.CurrentApplicationUser.GetAndSetUserIdAsync() == Convert.ToInt32(possibleUserForAccess))
                    {
                        output = await _toolsManager.GetLatestLogsAsJsonAsync();
                    }

                }
            };

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/logrequestresponse")]
        public async Task<IActionResult> GetLogsRequestResponse()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            var dbEnvironment = _dataCheckManager.GetEnvironment();
            string output = "";
            //check if environments can be used for getting log information.
            //Only when the app is in development or test and the database connections are on development or test logging may be retrieved.
            if ((new[] { "development", "localdevelopment", "test" }).Contains(appEnvironment.ToLower())
             && (new[] { "DEVELOPMENT", "TESTING PRODUCTION", "TESTING", "LOCAL", "UNKNOWN" }).Contains(dbEnvironment.ToUpper()))
            {
                var possibleUserForAccess = _configurationHelper.GetValueAsString(ApiSettings.ENABLE_LOG_READ_FOR_USER_CONFIG_KEY);
                if (!string.IsNullOrEmpty(possibleUserForAccess)) {
                    if (await this.CurrentApplicationUser.GetAndSetUserIdAsync() == Convert.ToInt32(possibleUserForAccess))
                    {
                        output = await _toolsManager.GetLatestLogsRequestResponseAsJsonAsync();
                    }
                }
            };

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/loges")]
        public async Task<IActionResult> GetLogsES()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            var dbEnvironment = _dataCheckManager.GetEnvironment();
            List<LogShortOutput> output = new List<LogShortOutput>();
            //check if environments can be used for getting log information.
            //Only when the app is in development or test and the database connections are on development or test logging may be retrieved.
            if ((new[] { "development", "localdevelopment", "test" }).Contains(appEnvironment.ToLower())
             && (new[] { "DEVELOPMENT", "TESTING PRODUCTION", "TESTING", "LOCAL", "UNKNOWN" }).Contains(dbEnvironment.ToUpper()))
            {
                
               output = await _toolsManager.GetLatestESLogs();

            };

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/logauditing/all")]
        public async Task<IActionResult> GetLogAuditing(bool includedata = false)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            var dbEnvironment = _dataCheckManager.GetEnvironment();
            //check if environments can be used for getting log information.
            //Only when the app is in development or test and the database connections are on development or test logging may be retrieved.
            if ((new[] { "development", "localdevelopment", "test" }).Contains(appEnvironment.ToLower())
             && (new[] { "DEVELOPMENT", "TESTING PRODUCTION", "TESTING", "LOCAL", "UNKNOWN" }).Contains(dbEnvironment.ToUpper()))
            {
                var possibleUserForAccess = _configurationHelper.GetValueAsString(ApiSettings.ENABLE_LOG_READ_FOR_USER_CONFIG_KEY);
                if (!string.IsNullOrEmpty(possibleUserForAccess))
                {
                    if (await this.CurrentApplicationUser.GetAndSetUserIdAsync() == Convert.ToInt32(possibleUserForAccess))
                    {
                        var output = await _toolsManager.GetLatestAuditingLogs();

                        AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

                        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
                    }
                }
            };

            return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/logauditing/user")]
        public async Task<IActionResult> GetLogAuditingForUser(bool includedata = false)
        {
            var output = await _toolsManager.GetLatestAuditingLogsForUser(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), includeData: includedata);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/logauditing/company")]
        public async Task<IActionResult> GetLogAuditingForCompany(bool includedata = false)
        {

            var output = await _toolsManager.GetLatestAuditingLogsForCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), includeData: includedata);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }
        #endregion

        #region - setup -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/setup/users/{companyid}")]
        public async Task<IActionResult> SetupUsers(int companyid)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            //Call logic for creating setup or test users, only on dev, test and staging.
            var output = "";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/setup/company")]
        public async Task<IActionResult> SetupCompany()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            //Call logic for creating setup or test test company, only on dev, test and staging.
            var output = "";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/setup/users")]
        public async Task<IActionResult> SetupUsers()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            var dbEnvironment = _dataCheckManager.GetEnvironment();
            //check if environments can be used for getting log information.
            //Only when the app is in development or test and the database connections are on development or test logging may be retrieved.
            if ((new[] { "development", "localdevelopment", "test", "acceptance" }).Contains(appEnvironment.ToLower())
             && (new[] { "DEVELOPMENT", "TESTING PRODUCTION", "TESTING", "LOCAL", "UNKNOWN" }).Contains(dbEnvironment.ToUpper()))
            {
                //ADD setup user logic
            };


            //Call logic for creating setup or test test company, only on dev, test and staging.
            var output = "";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }
        #endregion

        #region - supported -
        [HttpGet]
        [Route("tools/database_supported_timezones")]
        public async Task<IActionResult> GetDbTimezones()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _toolsManager.GetDatabaseSupportedTimezones();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("tools/core_supported_timezones")]
        public async Task<IActionResult> GetCoreTimezones()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution.getcoretimezones", ApiConstants.ActionExec);

            var output = await _toolsManager.GetCoreSupportedTimezones();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }
        #endregion

        #region - technical tooling -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/resetpool")]
        public async Task<IActionResult> ResetConnectionPooling([FromQuery] bool all = true)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var output = await _toolsManager.ResetConnectionPool(all);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/reducetaskgenerationconfigs")]
        public async Task<IActionResult> ReduceTaskGenerationConfigsForCompany([FromBody] int companyId)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            if (companyId == 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            var output = await _toolsManager.ReduceTaskGenerationConfigsForCompany(companyId);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }
        #endregion

        #region - settings -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/resources/settings")]
        public async Task<IActionResult> GetResourceSettings(string include = null)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var output = await _generalManager.GetSettingResources(companyid: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), include: include);

            AppendCapturedExceptionToApm(_generalManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/resources/settings/change/{id}")]
        public async Task<IActionResult> ResourceSettingsChangeSetting([FromRoute] int id, [FromBody] string value)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var settingValue = value;
            try
            {
                settingValue = SettingValuePreprocessor.PreprocessSettingValue(value);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"Invalid setting value supplied, value: {value}");
            }

            var result = await _generalManager.ChangeSettingResource(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), id: id, value: settingValue);

            AppendCapturedExceptionToApm(_generalManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/resources/settings/add")]
        public async Task<IActionResult> ResourceSettingsChangeSetting([FromBody] string value)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var output = "DISABLED";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.NotFound, output.ToJsonFromObject());
        }

        #endregion

        #region - language resources -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/resources/language/{key}/change/{culture}")]
        public async Task<IActionResult> ResourceLanguageUpdateKey([FromRoute] string key, [FromRoute] string culture, [FromBody] string value)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var output = await _generalManager.UpdateLanguageKeyAsync(key: key, culture: culture, value: value, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_generalManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/resources/language/{key}/create")]
        public async Task<IActionResult> ResourceLanguageCreateKey([FromRoute] string key, [FromBody] string description)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var output = await _generalManager.AddLanguageKeyAsync(key: key, description: description);

            AppendCapturedExceptionToApm(_generalManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/resources/language/{key}/set/description")]
        public async Task<IActionResult> ResourceLanguageUpdateDescription([FromRoute] string key, [FromBody] string description)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            var output = await _generalManager.UpdateDescriptionKeyAsync(key: key, description: description);

            AppendCapturedExceptionToApm(_generalManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("tools/resources/language/cultures")]
        public async Task<IActionResult> ResourceLanguageCultures()
        {
            /*
              Currently supported and translated languages (note this changes):
              "en_us", "en_gb", "nl_nl", "de_de", "fr_fr", "es_es", "pt_pt"


              Available technical languages
              "it_it", "el_gr", "nb_no", "fi_fi", "da_dk", "sv_se", "is_is", "pl_pl", "lt_lt", "lv_lv", "et_ee", "ro_ro", "bg_bg", "cs_cz", "hr_hr", "hu_hu", "hy_am",
              "ka_ge", "mk_mk", "sk_sk", "sl_si", "sq_al", "uk_ua", "af_za", "ar_sa", "he_il", "id_id", "ja_jp", "ko_kr", "ru_ru", "tr_tr", "zh_cn", "hi_in", "gd_gb",
              "ga_ie", "fy_nl", "my_mm", "vi_vn", "th_th", "lo_la", "ms_my", "ms_sg", "fil_ph", "bn_bd", "si_lk", "km_kh"
            */

            string[] supportedLanguagesShorts = new[] { "en_us", "en_gb", "nl_nl", "de_de", "fr_fr", "es_es", "pt_pt", "th_th" };
            string[] defaultSupportedLanguagesShorts = new[] { "en_us", "en_gb", "nl_nl", "de_de", "fr_fr", "es_es", "pt_pt", "th_th" };
            //string[] possibleTechnicalLanguagesShorts = new[] {"it_it", "el_gr", "nb_no", "fi_fi", "da_dk", "sv_se", "is_is", "pl_pl", "lt_lt", "lv_lv", "et_ee", "ro_ro", "bg_bg", "cs_cz", "hr_hr", "hu_hu", "hy_am",
            //  "ka_ge", "mk_mk", "sk_sk", "sl_si", "sq_al", "uk_ua", "af_za", "ar_sa", "he_il", "id_id", "ja_jp", "ko_kr", "ru_ru", "tr_tr", "zh_cn", "hi_in", "gd_gb",
            //  "ga_ie", "fy_nl", "my_mm", "vi_vn", "th_th", "lo_la", "ms_my", "ms_sg", "fil_ph", "bn_bd", "si_lk", "km_kh"};
            SortedList<string, string> supportedLanguages = new SortedList<string, string>();
            SortedList<string, string> supportedTechnicalLanguages = new SortedList<string, string>();


            string supportLanguagesFromDb = await _toolsManager.GetSupportedLanguages();
            if(!string.IsNullOrEmpty(supportLanguagesFromDb))
            {
                supportedLanguagesShorts = supportLanguagesFromDb.Split(",");
            }

            //CultureInfo.GetCultureInfo(culture)
            foreach (var code in supportedLanguagesShorts)
            {
                var culture = CultureInfo.GetCultureInfo(code.Replace("_", "-"));
                if (culture != null)
                {
                    supportedLanguages.Add(code, string.Format("{0} ({1})", culture.NativeName != null ? culture.NativeName : culture.EnglishName, culture.Name));
                }

            }

            if(supportedLanguages.Count == 0) {
                foreach (var code in defaultSupportedLanguagesShorts)
                {
                    var culture = CultureInfo.GetCultureInfo(code.Replace("_", "-"));
                    if (culture != null)
                    {
                        supportedLanguages.Add(code, string.Format("{0} ({1})", culture.NativeName != null ? culture.NativeName : culture.EnglishName, culture.Name));
                    }

                }
            }

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                //Console.WriteLine("{0} {1}", ci.Name, string.Format("{0} | {1} ({2})", ci.EnglishName, ci.NativeName, ci.Name));
                supportedTechnicalLanguages.Add(ci.Name.ToLower().Replace("-","_"), string.Format("{0} ({1})", ci.NativeName != null ? ci.NativeName : ci.EnglishName, ci.Name));
            }

            var languages = new Languages();
            languages.SupportedLanguages = supportedLanguages;
            languages.TechnicalPossibleSupportedLanguages = supportedTechnicalLanguages;

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, languages.ToJsonFromObject());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("tools/resources/language/technical_cultures")]
        public async Task<IActionResult> ResourceLanguageTechnicalCultures()
        {
            SortedList<string, string> languages = new SortedList<string, string>();
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                //Console.WriteLine("{0} {1}", ci.Name, string.Format("{0} | {1} ({2})", ci.EnglishName, ci.NativeName, ci.Name));
                languages.Add(ci.Name, string.Format("{0} | {1} ({2})", ci.EnglishName, ci.NativeName, ci.Name));
            }

            await Task.CompletedTask;

            return StatusCode((int)HttpStatusCode.OK, languages.ToJsonFromObject());
        }
        #endregion

        #region - raw views -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/raw/{companyid}/{rawreference}")]
        public async Task<IActionResult> RawTasks([FromRoute] string rawreference, [FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            if (Settings.RawSettings.RawReferences.Contains(rawreference))
            {
   
                if (rawreference == "log_logging" || rawreference == "log_auditing" || rawreference == "log_generation" || rawreference == "log_security" || rawreference == "log_export" || rawreference == "log_provisioner")
                {
                    var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
                    var validUsersForAccess = _configurationHelper.GetValueAsString(ApiSettings.ENABLE_RESTRICTED_RAWVIEWER_CONFIG_KEY);
                    //only for main admin, support and certain environments, make dynamic when roles are added.
                    if (!validUsersForAccess.Split(",").ToList().Contains((await this.CurrentApplicationUser.GetAndSetUserIdAsync()).ToString()) && await this.CurrentApplicationUser.GetAndSetUserIdAsync() != 20 && await this.CurrentApplicationUser.GetAndSetUserIdAsync() != 16 && !(new[] { "development", "localdevelopment", "test", "acceptance" }).Contains(appEnvironment.ToLower())) 
                    {
                        return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
                    }
                }

                DateTime parsedStartTimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { };

                DateTime parsedEndTimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { };

                var output = new RawData();

                if (rawreference == "log_data_warehouse" || rawreference == ("log_data_warehouse_security") || rawreference == ("synchronisation_data_warehouse"))
                {
                    output = await _toolsManager.GetRawDataFromDataWarehouse(companyId: companyid, rawReference: rawreference, startDateTime: parsedStartTimestamp, endDateTime: parsedEndTimestamp);
                }
                else
                {
                    output = await _toolsManager.GetRawData(companyId: companyid, rawReference: rawreference, startDateTime: parsedStartTimestamp, endDateTime: parsedEndTimestamp);
                }

                AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

            }

            return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/raw/{companyid}/schedule")]
        [Route("tools/raw/schedule")]
        public async Task<IActionResult> RawSchedule([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            DateTime parsedStartTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { };

            DateTime parsedEndTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { };

            var output = await _toolsManager.GetRawScheduleData(companyId: companyid, startDateTime: parsedStartTimestamp, endDateTime: parsedEndTimestamp);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }
        #endregion

        #region - application information -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/routes")]
        public async Task<RootResultModel> GetRoutes()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Where(
                ad => ad.AttributeRouteInfo != null).Select(ad => new RouteModel
                {
                    Name = ad.AttributeRouteInfo.Template,
                    Method = ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.First(),
                }).ToList();

            var res = new RootResultModel
            {
                Routes = routes.OrderBy(x => x.Name).ToList()
            };

            await Task.CompletedTask;

            return res;
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("tools/headers")]
        public async Task<IActionResult> GetHeaders()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var headers = Request.Headers;
            StringBuilder sb = new StringBuilder();
            foreach (var item in headers)
            {
                sb.Append(string.Concat(item.Key, ":", item.Value.ToString(), " | "));
            }

            return StatusCode((int)HttpStatusCode.OK, sb.ToString());
        }
        #endregion

        #region - bulk updates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/base/users/generateguid")]
        public async Task<IActionResult> GetAndUpdateBaseModified()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.GenerateUserProfileGuids();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/base/{holdingid}/{companyid}")]
        public async Task<IActionResult> GetAndUpdateBaseModified(int companyid, int holdingid)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.UpdateModifiedBaseStructures(companyId: companyid, holdingId: holdingid);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/cleanuplogging")]
        public async Task<IActionResult> GetAndCleanupLogging()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CleanupLoggingTable();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/cleanuplogginggeneration")]
        public async Task<IActionResult> GetAndCleanupLoggingGeneration()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CleanupLoggingGenerationTable();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/cleanuploggingrequestresponse")]
        public async Task<IActionResult> GetAndCleanupLoggingRequestResponse()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CleanupLoggingRequestResponseTable();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/cleanuploggingsecurity")]
        public async Task<IActionResult> GetAndCleanupLoggingSecurity()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CleanupLoggingSecurityTable();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/cleanuploggingexport")]
        public async Task<IActionResult> GetAndCleanupLoggingExport()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CleanupLoggingExportTable();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/cleanuploggingmigration")]
        public async Task<IActionResult> GetAndCleanupLoggingMigation()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CleanupLoggingMigrationTable();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/generatesystemusers")]
        public async Task<IActionResult> GenerateSystemUsers()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CreateSystemUsers();

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/migrations/run")]
        public async Task<IActionResult> RunMigrations([FromBody]string migrationkey)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            if(string.IsNullOrEmpty(migrationkey))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Bad migration key.");
            }

            if(!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            var output = await _migrationManager.RunMigrations(migrationKey: migrationkey);

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/migration/run")]
        public async Task<IActionResult> RunMigration([FromBody] string migrationkey, [FromQuery]string migrationname)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            if (string.IsNullOrEmpty(migrationkey) || string.IsNullOrEmpty(migrationname))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Bad migration key.");
            }

            if (!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            var output = await _migrationManager.RunMigration(migrationKey: migrationkey, specificMigration: migrationname);
            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/migration/retrieve")]
        public async Task<IActionResult> RetrieveMigration([FromBody] string migrationkey, [FromQuery] string migrationname)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            if (string.IsNullOrEmpty(migrationkey) || string.IsNullOrEmpty(migrationname))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Bad migration key.");
            }

            if (!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            var output = await _migrationManager.RetrieveMigration(migrationKey: migrationkey, specificMigration: migrationname);
            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/migrations/retrieve")]
        public async Task<IActionResult> RetrieveMigrations([FromBody] string migrationkey)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            if (string.IsNullOrEmpty(migrationkey))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Bad migration key.");
            }

            if (!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            var output = await _migrationManager.RetrieveMigrations(migrationKey: migrationkey);
            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("tools/guid")]
        public async Task<IActionResult> GenerateGuid()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, Guid.NewGuid().ToString());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/bulk/actions")]
        public async Task<IActionResult> UpdateActions(ToolFilter filter)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            if (!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }

            var output = await _toolsManager.UpdateModifiedActions(toolFilter:filter);

            AppendCapturedExceptionToApm(_toolsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/validationtest")]
        public async Task<IActionResult> TestActions(ToolFilter filter)
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, string.Concat(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation"), "|", RetrieveIp(), "|",CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation"))));
        }
        #endregion

        #region - company creation tooling addons -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("tools/company/createserviceuser")]
        public async Task<IActionResult> CreateServiceUserAndReturnName([FromBody]int companyid)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return null;
            }

            var output = await _toolsManager.CreateServiceUserForCompany(companyId: companyid);
            return StatusCode((int)HttpStatusCode.OK, output);
        }

        #endregion

        #region - test -
        [AllowAnonymous]
        [HttpGet]
        [Route("tools/test")]
        public async Task<IActionResult> ToolsTest()
        {
            List<IndexItem> test = new List<IndexItem>();
            test.Add(new IndexItem() { Id = 1, Index = 1});
            test.Add(new IndexItem() { Id = 2, Index = 2 });

            var item = test.ToJsonFromObject();

            await Task.CompletedTask;

            return StatusCode((int)HttpStatusCode.OK, test);
        }
        #endregion

        #region - two factor -
        //TODO : to be moved to own controller when other security meassures are added.
        [HttpGet]
        [Route("tools/security/twofactor")]
        public async Task<IActionResult> ToolsSecurityTwoFactor()
        {
            //TODO make dynamic, for testing purposes now hardcoded, this will be twofactorsecretcode based on user data. 
            var Tfa = new TwoFactorAuthenticator();
            var SetupTfa = Tfa.GenerateSetupCode("EZF", "EZ Factory", Encoding.ASCII.GetBytes("SOME_SECRET_KEY"));
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, SetupTfa.ToJsonFromObject());
        }

        //TODO : to be moved to own controller when other security meassures are added.
        [HttpPost]
        [Route("tools/security/twofactor/verify")]
        public async Task<IActionResult> ToolsSecurityTwoFactorVerify([FromBody] string keycode)
        {
            var Tfa = new TwoFactorAuthenticator();
            var result = Tfa.ValidateTwoFactorPIN(Encoding.ASCII.GetBytes("SOME_SECRET_KEY"), keycode);
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        }


        #endregion
        /// <summary>
        /// private support class for displaying language arrays of items that can be used.
        /// </summary>
        private class Languages
        {
            public SortedList<string, string> SupportedLanguages { get; set; }
            public SortedList<string, string> TechnicalPossibleSupportedLanguages { get; set; }

        }
    }
}
