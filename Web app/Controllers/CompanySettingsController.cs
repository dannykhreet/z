using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models.Skills;
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

        #region Skills Matrix Legend Configuration

        /// <summary>
        /// Gets the skills matrix legend configuration for the current company
        /// </summary>
        [HttpGet]
        [Route("/companysettings/legend")]
        public async Task<IActionResult> GetLegendConfiguration()
        {
            // Check if user has permission to view legend (all users can view)
            var companyId = this.User.GetProfile()?.Company?.Id;
            if (!companyId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            var result = await _connector.GetCall(string.Format(SkillMatrixLegendConstants.GetLegendConfiguration, companyId));
            if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                var legendConfig = JsonConvert.DeserializeObject<SkillMatrixLegendConfiguration>(result.Message);
                return Ok(legendConfig);
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                // Return default configuration if none exists
                var defaultConfig = SkillMatrixLegendConfiguration.CreateDefault();
                defaultConfig.CompanyId = companyId.Value;
                return Ok(defaultConfig);
            }

            return StatusCode((int)result.StatusCode);
        }

        /// <summary>
        /// Saves the skills matrix legend configuration for the current company
        /// Only team leaders and managers can edit
        /// </summary>
        [HttpPost]
        [Route("/companysettings/legend")]
        public async Task<IActionResult> SaveLegendConfiguration([FromBody] SkillMatrixLegendConfiguration legendConfig)
        {
            // Check if user has permission to edit legend (team leaders and managers only)
            var userProfile = this.User.GetProfile();
            var companyId = userProfile?.Company?.Id;

            if (!companyId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // Check role - only managers and team leaders can edit
            var userRole = userProfile?.Role?.ToLower();
            if (userRole != "manager" && userRole != "teamleader")
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "Only team leaders and managers can edit the legend configuration" });
            }

            // Validate the legend configuration
            var validationResult = ValidateLegendConfiguration(legendConfig);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors });
            }

            legendConfig.CompanyId = companyId.Value;
            legendConfig.Version = legendConfig.Version + 1;

            var result = await _connector.PostCall(string.Format(SkillMatrixLegendConstants.GetLegendConfiguration, companyId), legendConfig.ToJsonFromObject());
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Ok(new { success = true, version = legendConfig.Version });
            }

            return StatusCode((int)result.StatusCode, result.Message);
        }

        /// <summary>
        /// Updates a single legend item
        /// Only team leaders and managers can edit
        /// </summary>
        [HttpPost]
        [Route("/companysettings/legend/item")]
        public async Task<IActionResult> UpdateLegendItem([FromBody] SkillMatrixLegendItem legendItem)
        {
            // Check if user has permission to edit legend (team leaders and managers only)
            var userProfile = this.User.GetProfile();
            var companyId = userProfile?.Company?.Id;

            if (!companyId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // Check role - only managers and team leaders can edit
            var userRole = userProfile?.Role?.ToLower();
            if (userRole != "manager" && userRole != "teamleader")
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "Only team leaders and managers can edit the legend configuration" });
            }

            // Validate the legend item
            var validationErrors = ValidateLegendItem(legendItem);
            if (validationErrors.Count > 0)
            {
                return BadRequest(new { errors = validationErrors });
            }

            var result = await _connector.PostCall(string.Format(SkillMatrixLegendConstants.UpdateLegendItem, companyId), legendItem.ToJsonFromObject());
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Ok(new { success = true });
            }

            return StatusCode((int)result.StatusCode, result.Message);
        }

        /// <summary>
        /// Resets the legend configuration to default values
        /// Only team leaders and managers can reset
        /// </summary>
        [HttpPost]
        [Route("/companysettings/legend/reset")]
        public async Task<IActionResult> ResetLegendConfiguration()
        {
            // Check if user has permission to edit legend (team leaders and managers only)
            var userProfile = this.User.GetProfile();
            var companyId = userProfile?.Company?.Id;

            if (!companyId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // Check role - only managers and team leaders can edit
            var userRole = userProfile?.Role?.ToLower();
            if (userRole != "manager" && userRole != "teamleader")
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "Only team leaders and managers can reset the legend configuration" });
            }

            var result = await _connector.PostCall(string.Format(SkillMatrixLegendConstants.ResetLegendConfiguration, companyId), string.Empty);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                // API returns the reset configuration
                var resetConfig = JsonConvert.DeserializeObject<SkillMatrixLegendConfiguration>(result.Message);
                return Ok(resetConfig);
            }

            return StatusCode((int)result.StatusCode, result.Message);
        }

        /// <summary>
        /// Validates a complete legend configuration
        /// </summary>
        private (bool IsValid, List<string> Errors) ValidateLegendConfiguration(SkillMatrixLegendConfiguration config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("Legend configuration is required");
                return (false, errors);
            }

            // Validate mandatory skills
            if (config.MandatorySkills != null)
            {
                var mandatoryOrders = new HashSet<int>();
                var mandatoryIds = new HashSet<int>();

                foreach (var item in config.MandatorySkills)
                {
                    var itemErrors = ValidateLegendItem(item);
                    errors.AddRange(itemErrors);

                    // Check for duplicate order values
                    if (!mandatoryOrders.Add(item.Order))
                    {
                        errors.Add($"Duplicate order value {item.Order} in mandatory skills");
                    }

                    // Check for duplicate skill level IDs
                    if (!mandatoryIds.Add(item.SkillLevelId))
                    {
                        errors.Add($"Duplicate skill level ID {item.SkillLevelId} in mandatory skills");
                    }
                }
            }

            // Validate operational skills
            if (config.OperationalSkills != null)
            {
                var operationalOrders = new HashSet<int>();
                var operationalIds = new HashSet<int>();

                foreach (var item in config.OperationalSkills)
                {
                    var itemErrors = ValidateLegendItem(item);
                    errors.AddRange(itemErrors);

                    // Check for duplicate order values
                    if (!operationalOrders.Add(item.Order))
                    {
                        errors.Add($"Duplicate order value {item.Order} in operational skills");
                    }

                    // Check for duplicate skill level IDs
                    if (!operationalIds.Add(item.SkillLevelId))
                    {
                        errors.Add($"Duplicate skill level ID {item.SkillLevelId} in operational skills");
                    }
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validates a single legend item
        /// </summary>
        private List<string> ValidateLegendItem(SkillMatrixLegendItem item)
        {
            var errors = new List<string>();

            if (item == null)
            {
                errors.Add("Legend item is required");
                return errors;
            }

            // Label validation - must be non-empty
            if (string.IsNullOrWhiteSpace(item.Label))
            {
                errors.Add($"Label is required for skill level {item.SkillLevelId}");
            }

            // Icon color validation - must be valid 6-digit HEX
            if (!IsValidHexColor(item.IconColor))
            {
                errors.Add($"Invalid icon color format for skill level {item.SkillLevelId}. Must be a valid 6-digit HEX code (e.g., #FF8800)");
            }

            // Background color validation - must be valid 6-digit HEX
            if (!IsValidHexColor(item.BackgroundColor))
            {
                errors.Add($"Invalid background color format for skill level {item.SkillLevelId}. Must be a valid 6-digit HEX code (e.g., #FFFFFF)");
            }

            // Order validation - must be positive
            if (item.Order <= 0)
            {
                errors.Add($"Order must be a positive integer for skill level {item.SkillLevelId}");
            }

            // Skill type validation
            if (string.IsNullOrWhiteSpace(item.SkillType) ||
                (item.SkillType.ToLower() != "mandatory" && item.SkillType.ToLower() != "operational"))
            {
                errors.Add($"Skill type must be 'mandatory' or 'operational' for skill level {item.SkillLevelId}");
            }

            // Score value validation for operational skills
            if (item.SkillType?.ToLower() == "operational" && (!item.ScoreValue.HasValue || item.ScoreValue < 1 || item.ScoreValue > 5))
            {
                errors.Add($"Operational skill level {item.SkillLevelId} must have a score value between 1 and 5");
            }

            return errors;
        }

        /// <summary>
        /// Validates that a string is a valid 6-digit HEX color code
        /// </summary>
        private bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return false;
            }

            // Match #RRGGBB format
            var hexPattern = @"^#[0-9A-Fa-f]{6}$";
            return Regex.IsMatch(color, hexPattern);
        }

        #endregion
    }
}