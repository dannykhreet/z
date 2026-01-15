using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.UI;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Mappers;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Company = EZGO.Api.Models.Company;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// GeneralManager; The GeneralManager contains all logic for retrieving and setting specific items that are not part of the basic object of the EZGO platform.
    /// This will include certain menu's, settings and resources that are needed for the display within the application or for specific feature settings.
    /// NOTE! this object needs to be refactored and split up. 
    /// </summary>
    public class GeneralManager : BaseManager<GeneralManager>, IGeneralManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IToolsManager _toolsManager;
        private readonly IFeedManager _feedManager;
        private readonly ICryptography _cryptography;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor(s) -
        public GeneralManager(IConfigurationHelper configurationHelper, IDataAuditing dataAuditing, IDatabaseAccessHelper manager, IToolsManager toolsManager, IFeedManager feedManager, ICryptography cryptography, ILogger<GeneralManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _toolsManager = toolsManager;
            _feedManager = feedManager;
            _cryptography = cryptography;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetMainMenuAsync; Get main menu based on the company id. Currently the main menu is more or less static (with the exception for the statistic part).
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A Main menu, consisting of a MainMenu object with MenuItems. </returns>
        public async Task<MainMenu> GetMainMenuAsync(int companyId, DateTime timestamp, int? areaId = null, int? userId = null)
        {
            var mainmenu = new MainMenu();

            //add user roles.
            mainmenu.MenuItems.Add(await this.GetMenuItemAsync(companyId: companyId, menuType: MenuTypeEnum.Checklists));
            mainmenu.MenuItems.Add(await this.GetMenuItemAsync(companyId: companyId, menuType: MenuTypeEnum.Tasks, useStatistics: true, timestamp: timestamp, areaId: areaId, userId: userId));
            mainmenu.MenuItems.Add(await this.GetMenuItemAsync(companyId: companyId, menuType: MenuTypeEnum.Audits));
            mainmenu.MenuItems.Add(await this.GetMenuItemAsync(companyId: companyId, menuType: MenuTypeEnum.Reports));
            mainmenu.MenuItems.Add(await this.GetMenuItemAsync(companyId: companyId, menuType: MenuTypeEnum.Actions));

            return mainmenu;
        }

        /// <summary>
        /// GetMenuItemAsync; Get a menu item based on the MenuTypeEnum, if useStatistics is supplied with a true value statistics will be added.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="menuType">MenuTypeEnum of the menu item.</param>
        /// <param name="useStatistics">Boolean, if included, statistics are loaded.</param>
        /// <returns>MainMenuItem containing a title and possible statistics.</returns>
        public async Task<MainMenuItem> GetMenuItemAsync(int companyId, MenuTypeEnum menuType, DateTime? timestamp = null, bool useStatistics = false, int? areaId = null, int? userId = null)
        {
            var mainmenuitem = new MainMenuItem() { MenuType = menuType };

            if (useStatistics)
            {

                mainmenuitem.Statistics.AddRange((await GetStatisticsAsync(companyId: companyId, timestamp: timestamp.Value, areaId: areaId, userId: userId)).ToArray());
            }

            return mainmenuitem;
        }

        /// <summary>
        /// GetStatisticsAsync; Get a list of statistic items, containing a title and a number of occurrences.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of statistic items</returns>
        public async Task<List<StatsItem>> GetStatisticsAsync(int companyId, DateTime timestamp, int? areaId = null, int? userId = null)
        {
            var output = new List<StatsItem>();

            NpgsqlDataReader dr = null;

            if (timestamp == DateTime.MinValue) { timestamp = DateTime.Now; }

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (areaId.HasValue) parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));
                if (userId.HasValue) parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                parameters.Add(new NpgsqlParameter("@_timestamp", timestamp));

                using (dr = await _manager.GetDataReader("report_get_areas", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var stat = new StatsItem();
                        stat.Statistic = Convert.ToInt32(dr["Nr"]);
                        stat.Title = dr["Status"].ToString();

                        output.Add(stat);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("StatisticsManager.GetStatisticsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// GetApplicationSettings; Get the application settings. Based on customer or user id some settings can be specific to the user.
        /// </summary>
        /// <param name="customer">customer (companies_company.id)</param>
        /// <param name="userid">userid (profiles_user.id)</param>
        /// <returns>An application Setting</returns>
        public async Task<ApplicationSettings> GetApplicationSettings(int companyid, int? userid = null)
        {
            //NOTE! this will be added to a database or local structure.
            var result = new Models.Settings.ApplicationSettings();
            result.ImageStorageBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ImageStorageBaseUrl"); //e.g. "https://acceptance.ezfactory.nl/";
            result.VideoStorageBaseUrl = _configurationHelper.GetValueAsString("AppSettings:VideoStorageBaseUrl"); //e.g. "https://acceptance.ezfactory.nl/";
            result.ActiveApiVersion = _configurationHelper.GetValueAsString("AppSettings:ActiveApiVersion");  //e.g. "v1";
            result.AvailableLanguages = await GetAvailableLanguagesForSettings(); //basic list, real language list can be retrieved with different calls.
            result.RunningEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            result.PasswordValidationRegEx = _configurationHelper.GetValueAsString("AppSettings:PasswordPolicyVersion").Equals("V2") ? Settings.AuthenticationSettings.PASSWORD_VALIDATION_REGEX_V2 : Settings.AuthenticationSettings.PASSWORD_VALIDATION_REGEX;

            if (result.RunningEnvironment.ToLower() == Settings.ApiSettings.EnvironmentNameDev
                || result.RunningEnvironment.ToLower() == Settings.ApiSettings.EnvironmentNameLocalDev
                || result.RunningEnvironment.ToLower() == Settings.ApiSettings.EnvironmentNameStaging
                || result.RunningEnvironment.ToLower() == Settings.ApiSettings.EnvironmentNameTest)
            {
                result.ApiAcceptanceUri = _configurationHelper.GetValueAsString("AppSettings:ApiAcceptanceUri");
                result.ApiDevelopmentUri = _configurationHelper.GetValueAsString("AppSettings:ApiDevelopmentUri");
                result.ApiTestUri = _configurationHelper.GetValueAsString("AppSettings:ApiTestUri");
            }

            result.ApiProductionUri = _configurationHelper.GetValueAsString("AppSettings:ApiProductionUri");

            result.Features = await GetFeatures(companyId: companyid, userId: userid);

            //Get all settings from settings resources with company information.
            var settings = await GetSettingResourcesForSettings(companyid: companyid);
            try
            {
                if (settings != null)
                {
                    result.CompanyTimezone = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.COMPANY_TIMEZONE.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    //tag limit
                    var CompanyTagLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.COMPANY_TAG_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (string.IsNullOrEmpty(CompanyTagLimit)) CompanyTagLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.GENERAL_TAG_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (!int.TryParse(CompanyTagLimit, out int tagLimit))
                    {
                        tagLimit = 25; //default to 25 if nothing is set
                    }
                    result.TagLimit = tagLimit;

                    //tag group limit
                    var CompanyTagGroupLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.COMPANY_TAGGROUP_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (string.IsNullOrEmpty(CompanyTagGroupLimit)) CompanyTagGroupLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.GENERAL_TAGGROUP_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (!int.TryParse(CompanyTagGroupLimit, out int tagGroupLimit))
                    {
                        tagGroupLimit = 8; //default to 8 if nothing is set
                    }
                    result.TagGroupLimit = tagGroupLimit;

                    //property limit
                    var CompanyPropertyLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.COMPANY_PROPERTY_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (string.IsNullOrEmpty(CompanyPropertyLimit)) CompanyPropertyLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.GENERAL_PROPERTY_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (!int.TryParse(CompanyPropertyLimit, out int propertyLimit))
                    {
                        propertyLimit = 5; //default to 5 if nothing is set
                    }
                    result.PropertyLimit = propertyLimit;

                    //open fields limit
                    var CompanyOpenFieldsLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.COMPANY_OPEN_FIELDS_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (string.IsNullOrEmpty(CompanyOpenFieldsLimit)) CompanyOpenFieldsLimit = settings?.Where(x => x.SettingsKey == SettingResourceTypeEnum.GENERAL_OPEN_FIELDS_LIMIT.ToString().ToUpper())?.FirstOrDefault()?.Value;
                    if (!int.TryParse(CompanyOpenFieldsLimit, out int openFieldsLimit))
                    {
                        openFieldsLimit = 10; //default to 10 if nothing is set
                    }
                    result.OpenFieldsLimit = openFieldsLimit;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetApplicationSettings(): ", ex.Message));
            }

            var mediaLocations = new MediaLocations();
            mediaLocations.ImageMediaBaseUri = _configurationHelper.GetValueAsString("AppSettings:BaseUriImageMedia");
            mediaLocations.FileMediaBaseUri = _configurationHelper.GetValueAsString("AppSettings:BaseUriFileMedia");
            mediaLocations.VideoMediaBaseUri = _configurationHelper.GetValueAsString("AppSettings:BaseUriVideoMedia");
            //mediaLocations.MediaUploadLocation = _configurationHelper.GetValueAsString("AppSettings:BaseUriMediaUpload");

            result.MediaLocations = mediaLocations;

            result.AnalyticsLocationUri = _configurationHelper.GetValueAsString("AppSettings:AnalyticsLocationUri");

            return result;
        }

        public async Task<Features> GetFeatures(List<SettingResource> resourceSettings, int companyId)
        {
            await Task.CompletedTask;

            var tierEssentials = CheckFeatureValue(companyId: companyId, resourceSettings.Where(x => x.SettingsKey == SettingResourceTypeEnum.FEATURE_TIER_ESSENTIALS.ToString().ToUpper())?.FirstOrDefault()?.Value);
            var tierAdvanced = CheckFeatureValue(companyId: companyId, resourceSettings.Where(x => x.SettingsKey == SettingResourceTypeEnum.FEATURE_TIER_ADVANCED.ToString().ToUpper())?.FirstOrDefault()?.Value);
            var tierPremium = CheckFeatureValue(companyId: companyId, resourceSettings.Where(x => x.SettingsKey == SettingResourceTypeEnum.FEATURE_TIER_PREMIUM.ToString().ToUpper())?.FirstOrDefault()?.Value);

            var features = GetFeaturesDefaults(companyId: companyId);

            features.TierEssentials = tierEssentials;
            features.TierAdvanced = tierAdvanced;
            features.TierPremium = tierPremium;

            var overrideMatrixParts = _configurationHelper.GetValueAsBool("AppSettings:OverrideMatrixParts");
            //TODO Create Enum for all feature setting strings like "FEATURE_AUDITS"
            foreach (var resourceSetting in resourceSettings)
            {
                switch (resourceSetting.SettingsKey)
                {
                    case "FEATURE_AUDITS":
                        features.AuditsEnabled = tierEssentials || tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_CHECKLISTS":
                        features.ChecklistsEnabled = tierEssentials || tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKS":
                        features.TasksEnabled = tierEssentials || tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ACTIONS":
                        features.ActionsEnabled = tierEssentials || tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKTEMPLATEPROPERTIES":
                        features.TasksPropertyValueRegistrationEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKTEMPLATEPROPERTIES_CHECKLISTS":
                        features.ChecklistsPropertyValueRegistrationEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKTEMPLATEPROPERTIES_AUDITS":
                        features.AuditsPropertyValueRegistrationEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_CHECKLISTTEMPLATEPROPERTIES":
                        features.ChecklistsPropertyEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TRANSFERABLE_CHECKLISTS":
                        features.ChecklistsTransferableEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_AUDITTEMPLATEPROPERTIES":
                        features.AuditsPropertyEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ACTIONONTHESPOT":
                        features.ActionOnTheSportEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_COMMENT":
                        features.EasyCommentsEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_REQUIREPROOF":
                        features.RequiredProof = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_FACTORY_FEED":
                        features.FactoryFeedEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_EXPORTS":
                        features.ExportEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_EXPORTS_ADVANCED":
                        features.ExportAdvancedEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_MARKET":
                        features.MarketEnabled = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "MARKET_SAP":
                        features.MarketSapEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "MARKET_ULTIMO":
                        features.MarketUltimoEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_FIREBASELOGGING_CMS":
                        features.FirebaseLoggingCMSEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_EXPORTS_TASKPROPERTIES":
                        features.ExportTaskProperties = tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_REPORTING":
                        features.ReportsEnabled = tierEssentials || tierAdvanced || tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_WORKINSTRUCTIONS":
                        features.WorkInstructions = tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_SKILLASSESSMENTS":
                        features.SkillAssessments = tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_SKILLSMATRIX":
                        features.SkillMatrix = tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_SKILLSMATRIX_MANDATORYSKILLS":
                        features.SkillMatrixMandatorySkills = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_SKILLSMATRIX_OPERATIONALSKILLS":
                        features.SkillMatrixOperationalSkills = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_SKILLSMATRIX_OPERATIONALBEHAVIOUR":
                        features.SkillMatrixOperationalBehaviour = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ORDER_TASKS":
                        features.OrderTasks = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ORDER_CHECKLISTS":
                        features.OrderAudits = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ORDER_AUDITS":
                        features.OrderChecklists = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_USE_STATIC_AUDIT_STORAGE":
                        features.UseStaticAuditStorage = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_USE_STATIC_CHECKLIST_STORAGE":
                        features.UseStaticChecklistStorage = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_SUPPORT_CHAT":
                        features.EnableSupportChat = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_QRCODE":
                        features.QRCode = tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ADVANCEDSEARCH":
                        features.AdvancedSearchEnabled = tierPremium || CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_ANALYTICS":
                        features.AnalyticsEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKGENERATION_PERIODDAY":
                        features.TaskTypePeriodDayEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKGENERATION_DYNAMICDAY":
                        features.TaskTypeDynamicDayEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASK_MULTISKIP":
                        features.TaskMultiskipEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TASKGENERATION_GENERATION_OPTIONS":
                        features.TaskGenerationOptions = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_RUNNING_ASSESSMENTS_CMS":
                        features.SkillAssessmentsRunningInCms = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TAGS":
                        features.TagsEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_WORKINSTRUCTION_ITEM_ATTACHMENT_PDF":
                        features.WorkInstructionItemAttachmentPdf = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_WORKINSTRUCTION_ITEM_ATTACHMENT_LINK":
                        features.WorkInstructionItemAttachmentLink = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_TEMPLATE_SHARING":
                        features.TemplateSharingEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_AUDIT_TRAIL_DETAILS":
                        features.AuditTrailDetailsEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_FLATTEN_DATA":
                        features.FlattenDataEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_FLATTEN_DATA_FALLBACK":
                        features.FlattenDataFallbackEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_FLATTEN_DATA_SEARCH":
                        features.FlattenDataSearchEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_EXTENDED_USER_DETAILS":
                        features.UserExtendedDetailsEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS":
                        features.WorkInstructionsChangedNotificationsEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_MODIFY_OWN_COMPANY_SETTINGS_WORKINSTRUCTION":
                        features.EnableModificationOwnCompanySettingsWorkInstruction = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_MODIFY_OWN_COMPANY_SETTINGS_ASSESSMENT":
                        features.EnableModificationOwnCompanySettingsAssessment = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_MODIFY_OWN_COMPANY_SETTINGS_MATRIX":
                        features.EnableModificationOwnCompanySettingsMatrix = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;
                    case "FEATURE_MATRIX_CHANGED_SCORE_STANDARD":
                        features.EnableMatrixScoreStandard = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_CHECKLIST_STAGES":
                        features.ChecklistStagesEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_EXPORT_MATRIX_SKILL_USER":
                        features.ExportMatrixSkillUserEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "FEATURE_SYSTEMROLE_MANAGEMENT":
                        features.RoleManagementEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;

                    case "TECH_ATOSS_PROVISIONING":
                        features.AtossProvisioningEnabled = CheckFeatureValue(companyId: companyId, resourceSetting.Value);
                        break;
                }
            }

            if (overrideMatrixParts)
            {
                features.SkillMatrixMandatorySkills = features.SkillMatrix;
                features.SkillMatrixOperationalBehaviour = false;
                features.SkillMatrixOperationalSkills = features.SkillMatrix;
            }

            return features;
        }

        /// <summary>
        /// GetFeatures; GetFeatures get enabled features. Every feature will have it's own setting, and depending on implementation certain items will be retrieved.
        /// The basic setup will be based on the 'Tier' settings, and most settings will have a overrule or own setting. 
        /// A feature setting is usually based on the a list of company id's, or a specific setting with a company (other table)
        /// </summary>
        /// <param name="companyId">Customer (companyId)</param>
        /// <param name="userId">Specific user id (currently not implemented)</param>
        /// <returns>Feature object containing a list of true/false items.</returns>
        public async Task<Features> GetFeatures(int companyId, int? userId)
        {
            var resourceSettings = await GetSettingResources(companyid: companyId);
            return await GetFeatures(resourceSettings: resourceSettings, companyId: companyId);
        }

        /// <summary>
        /// GetFeaturesDefaults; Get a empty feature list. Used for filling later-on.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A empty feature list with default settings.</returns>
        private Features GetFeaturesDefaults(int companyId)
        {
            var features = new Features();

            //settings defaults based on logic or config settings
            features.ActionsEnabled = true;
            features.ActionCommentsEnabled = true;
            features.ActionOnTheSportEnabled = true;
            features.ActionPropertyEnabled = false;
            features.ActionPropertyValueRegistrationEnabled = false;
            features.AuditsEnabled = true;
            features.AuditsPropertyEnabled = false;
            features.AuditsPropertyValueRegistrationEnabled = false;
            features.ChecklistsEnabled = true;
            features.ChecklistsPropertyEnabled = false;
            features.ChecklistsPropertyValueRegistrationEnabled = false;
            features.ChecklistsConditionalEnabled = false;
            features.ChecklistsTransferableEnabled = true;
            features.EasyCommentsEnabled = true;
            features.ExportEnabled = true;
            features.ExportAdvancedEnabled = true;
            features.ExportTaskProperties = false;
            features.FactoryFeedEnabled = false;
            features.FirebaseLoggingCMSEnabled = true;
            features.MarketEnabled = false;
            features.RequiredProof = false;
            features.ReportsEnabled = true;
            features.TasksEnabled = true;
            features.TasksPropertyEnabled = false;
            features.OrderAudits = false;
            features.OrderChecklists = false;
            features.OrderTasks = false;
            features.TasksPropertyValueRegistrationEnabled = _configurationHelper.GetValueAsBoolBasedOnCompanyId(keyname: FeatureSettings.FEATURE_VALUE_REGISTRATION_TASKS_CONFIG_KEY, companyid: companyId);
            features.UserProfileManagementEnabled = true;
            features.UseStaticAuditStorage = false;
            features.UseStaticChecklistStorage = false;
            features.AnalyticsEnabled = false;

            return features;
        }

        /// <summary>
        /// CheckFeatureValue; Check feature methods is based on the primary feature logic (e.g. a list of company id's). 
        /// String with values is checked against the company id, if available true is returned. For some features the value 'ALL' can be used.
        /// If this is the case also true is returned.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="values">List of setting values (company id's comma seperated)</param>
        /// <returns>true/false depending on outcome.</returns>
        private bool CheckFeatureValue(int companyId, string values)
        {
            if (!string.IsNullOrEmpty(values))
            {
                if (values.ToUpper() == "ALL")
                {
                    return true;
                }
                else
                {
                    var Ids = values.Split(",");
                    return Ids.Contains(companyId.ToString());
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// GetUpdateChangesAsync; CheckUpdateChanges method will check if there are changes for a certain type of object in the database.
        /// Depending if there are changes a type and a number is returned.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp for checking range determination.</param>
        /// <returns>A list of object check items.</returns>
        public async Task<List<UpdateCheckItem>> GetUpdateChangesAsync(int companyId, int? userId = null, int? areaId = null, DateTime? timestamp = null)
        {
            var list = new List<UpdateCheckItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (timestamp.HasValue) parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                if (userId.HasValue) parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                if (areaId.HasValue) parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));

                using (dr = await _manager.GetDataReader("check_changes_v2", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {

                        if (Enum.TryParse(dr["check_type"].ToString(), true, out UpdateCheckTypeEnum possibleType))
                        {
                            UpdateCheckItem item = new()
                            {
                                NumberOfItems = Convert.ToInt32(dr["nr"]),
                                UpdateCheckType = possibleType,
                                Ids = ((int[])dr["ids"]).ToList()
                            };
                            list.Add(item);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetUpdateChangesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }
            return list;
        }

        /// <summary>
        /// GetUpdateChangesFeedAsync; CheckUpdateChanges specifically based on feeds method will check if there are changes for a certain type of object in the database.
        /// Depending if there are changes a type and a number is returned.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="timestamp">TimeStamp for checking range determination.</param>
        /// <returns>A list of object check items.</returns>
        public async Task<List<UpdateCheckItem>> GetUpdateChangesFeedAsync(int companyId, int? userId = null, int? areaId = null, DateTime? timestamp = null)
        {
            var list = new List<UpdateCheckItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (timestamp.HasValue) parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                if (userId.HasValue) parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                if (areaId.HasValue) parameters.Add(new NpgsqlParameter("@_areaid", areaId.Value));

                using (dr = await _manager.GetDataReader("check_changes_feed", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {

                        if (Enum.TryParse(dr["check_type"].ToString(), true, out UpdateCheckTypeEnum possibleType))
                        {
                            UpdateCheckItem item = new()
                            {
                                NumberOfItems = Convert.ToInt32(dr["nr"]),
                                UpdateCheckType = possibleType,
                                Ids = ((int[])dr["ids"]).ToList()
                            };
                            list.Add(item);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetUpdateChangesFeedAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }
            return list;
        }
        #endregion

        #region - language -
        /// <summary>
        /// GetLanguageResource; Gets a language resource based on a specific culture.
        /// </summary>
        /// <param name="culture">Culture, based on the culture format of .Net (e.g. isolanguage-ISOCOUNTRY)</param>
        /// <returns>A language resource containing the resources (texts) for the application.</returns>
        public async Task<LanguageResource> GetLanguageResource(string culture, ResourceLanguageTypeEnum resourceType = ResourceLanguageTypeEnum.APP)
        {
            var languageResource = new Models.Settings.LanguageResource();
            if (!string.IsNullOrEmpty(culture) && ApiSettings.VALID_LANGUAGE_CULTURES.Contains(culture.ToLower().Replace("-", "_")))
            {
                if (CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => x.Name == culture).Any() || CultureInfo.GetCultureInfo(culture) != null)
                {
                    var sp = "get_resource_language";
                    if (resourceType == ResourceLanguageTypeEnum.CMS)
                    {
                        sp = "get_resource_language_management";
                    }

                    culture = LanguageMapper.FromCulture(culture);

                    var cultureInfo = new CultureInfo(culture);
                    languageResource.Language = cultureInfo.DisplayName;
                    languageResource.LanguageCulture = cultureInfo.Name;
                    languageResource.LanguageIso = cultureInfo.ThreeLetterISOLanguageName;

                    string correctedculture = culture.Replace("-", "_").ToLower(); //correct the culture for db use.

                    string activeCultures = await _toolsManager.GetSupportedLanguages();

                    if (!activeCultures.Contains(correctedculture))
                    {
                        correctedculture = "en_us"; //default to english if language is not active.
                    }

                    NpgsqlDataReader dr = null;

                    try
                    {
                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                        parameters.Add(new NpgsqlParameter("@_culture", correctedculture));

                        using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                        {
                            while (await dr.ReadAsync())
                            {
                                var languageresourceitem = new LanguageResourceItem();
                                languageresourceitem.Description = dr["description"].ToString();
                                languageresourceitem.Guid = dr["resource_guid"].ToString();
                                languageresourceitem.ResourceKey = dr["resource_key"].ToString();
                                languageresourceitem.ResourceValue = dr["resource_value"].ToString();
                                languageResource.ResourceItems.Add(languageresourceitem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetLanguageResource(): ", ex.Message));

                        if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                    }
                    finally
                    {
                        if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                    }
                };

            }

            if (languageResource != null && !string.IsNullOrEmpty(languageResource.LanguageCulture) && languageResource.LanguageCulture.Contains('-'))
            {
                languageResource.LanguageCulture = languageResource.LanguageCulture.Replace('-', '_');
            }

            return languageResource;
        }

        /// <summary>
        /// GetLanguageResourceStatistics; Get statistics for language resource table. 
        /// </summary>
        /// <returns>Statistic list per culture how many fields are filled.</returns>
        public async Task<List<StatsItem>> GetLanguageResourceStatistics()
        {
            var languageResourceStats = new List<StatsItem>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("report_resource_language_stats", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var languageResourceStatsItem = new StatsItem();
                        languageResourceStatsItem.Title = dr["column_name"].ToString();
                        languageResourceStatsItem.Statistic = Convert.ToInt32(dr["value_count"]);

#pragma warning disable CS0168 // Variable is declared but never used
                        try
                        {
                            var currentCulture = CultureInfo.GetCultureInfo(dr["column_name"].ToString().Replace('_', '-')); // new CultureInfo(dr["column_name"].ToString().Replace('_','-'));
                            languageResourceStatsItem.SubTitle = string.Format("{0} ({1})", currentCulture.NativeName != null ? currentCulture.NativeName : currentCulture.EnglishName, currentCulture.Name);

                        }
                        catch (Exception ex)
                        {
                            //culture not available in standard culture
                            languageResourceStatsItem.SubTitle = languageResourceStatsItem.Title;

                        }
#pragma warning restore CS0168 // Variable is declared but never used

                        languageResourceStats.Add(languageResourceStatsItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetLanguageResource(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return languageResourceStats;
        }

        /// <summary>
        /// UpdateLanguageKeyAsync; Update language key. Value will be updated within the database. The culture will be used for determining the correct column.
        /// </summary>
        /// <param name="key">Key that needs to be updated.</param>
        /// <param name="culture"> Culture of that key that needs to be updated.</param>
        /// <param name="value">The new value (string)</param>
        /// <returns>true/false depends on successfulness</returns>
        public async Task<bool> UpdateLanguageKeyAsync(string key, string culture, string value, int companyid, int userid)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(culture))
            {
                if (CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => x.Name == culture).Any() || CultureInfo.GetCultureInfo(culture) != null)
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_key", key.ToUpper()));
                    parameters.Add(new NpgsqlParameter("@_culture", culture.Replace("-", "_")));
                    parameters.Add(new NpgsqlParameter("@_value", value));
                    var rowseffected = await _manager.ExecuteScalarAsync("change_resource_language_key_value", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                    if (Convert.ToInt32(rowseffected) > 0)
                    {
                        var mutated = new LanguageResourceItemCulture() { ResourceKey = key, ResourceValue = value, Culture = culture };
                        await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated.ToJsonFromObject(), Models.Enumerations.TableNames.resource_languages.ToString(), objectId: 0, userId: userid, companyId: companyid, description: "Changed language resource setting.");
                    }


                    return (Convert.ToInt32(rowseffected) > 0);
                }
            }
            return false;
        }

        /// <summary>
        /// UpdateDescriptionKeyAsync; Update a description of a key, used for management and development purposes.
        /// </summary>
        /// <param name="key">Key where the description needs to be updated.</param>
        /// <param name="value">The new value (string)</param>
        /// <returns>true/false depends on successfulness</returns>
        public async Task<bool> UpdateDescriptionKeyAsync(string key, string description)
        {
            if (!string.IsNullOrEmpty(key))
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_key", key.ToUpper()));
                parameters.Add(new NpgsqlParameter("@_description", description));
                var rowseffected = await _manager.ExecuteScalarAsync("change_resource_language_description", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                return (Convert.ToInt32(rowseffected) > 0);
            }
            return false;
        }

        /// <summary>
        /// AddLanguageKeyAsync; Add a new language key to the database
        /// </summary>
        /// <param name="key">Key that needs to be updated</param>
        /// <param name="description">Description of that key.</param>
        /// <returns>true/false depends on successfulness</returns>
        public async Task<bool> AddLanguageKeyAsync(string key, string description)
        {
            if (!string.IsNullOrEmpty(key))
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_key", key.ToUpper()));
                parameters.Add(new NpgsqlParameter("@_description", description));
                parameters.Add(new NpgsqlParameter("@_guid", Guid.NewGuid().ToString().Replace("-", "").Replace("_", "")));
                var rowseffected = await _manager.ExecuteScalarAsync("add_resource_language_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                return (Convert.ToInt32(rowseffected) > 0);
            }
            return false;
        }

        /// <summary>
        /// AddLanguageKeyAsync; Add a new language key to the database including the value
        /// </summary>
        /// <param name="key">Key that needs to be updated</param>
        /// <param name="description">Description of that key.</param>
        /// <param name="culture"> Culture of that key that needs to be updated.</param>
        /// <param name="value">The new value (string)</param>
        /// <returns>true/false depends on successfulness</returns>
        public async Task<bool> AddLanguageKeyAsync(string key, string description, string culture, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(culture))
            {
                if (CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => x.Name == culture).Any())
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_key", key.ToUpper()));
                    parameters.Add(new NpgsqlParameter("@_description", description));
                    parameters.Add(new NpgsqlParameter("@_guid", Guid.NewGuid().ToString().Replace("-", "").Replace("_", "")));
                    parameters.Add(new NpgsqlParameter("@_culture", culture));
                    parameters.Add(new NpgsqlParameter("@_value", value));
                    var rowseffected = await _manager.ExecuteScalarAsync("add_resource_language_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                    return (Convert.ToInt32(rowseffected) > 0);
                }
            }
            return false;
        }

        /// <summary>
        /// GetAvailableLanguagesForSettings; Get available languages for settings.
        /// </summary>
        /// <returns>return a list of items</returns>
        private async Task<List<string>> GetAvailableLanguagesForSettings()
        {
            List<string> availableLanguages = new[] { "nl-NL", "en-GB", "en-US", "de-DE", "es-ES", "pt-PT", "fr-FR" }.ToList();
            List<string> availableLanguagesFromDatabase = new List<string>();

            string supportLanguagesFromDb = await _toolsManager.GetSupportedLanguages();

            string[] supportedLanguagesShorts = new string[0];
            if (!string.IsNullOrEmpty(supportLanguagesFromDb))
            {
                supportedLanguagesShorts = supportLanguagesFromDb.Split(",");
            }

            foreach (var code in supportedLanguagesShorts)
            {
                var culture = CultureInfo.GetCultureInfo(code.Replace("_", "-"));
                if (culture != null)
                {
                    availableLanguagesFromDatabase.Add(culture.Name);
                }

            }

            return (availableLanguagesFromDatabase != null && availableLanguagesFromDatabase.Count > 1) ? availableLanguagesFromDatabase : availableLanguages;
        }
        #endregion

        #region - settting resources -
        public async Task<List<SettingResource>> GetFeatureSettingResources()
        {
            var output = new List<SettingResource>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _manager.GetDataReader("get_resource_settings", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var resourceitem = new SettingResource();
                        resourceitem.Description = dr["description"].ToString();
                        resourceitem.Id = Convert.ToInt32(dr["id"]);
                        resourceitem.Name = dr["name"].ToString();
                        resourceitem.SettingResourceType = (SettingResourceSettingsTypeEnum)Enum.ToObject(typeof(SettingResourceSettingsTypeEnum), Convert.ToInt32(dr["settingstype"]));
                        resourceitem.SettingsKey = dr["settingskey"].ToString();
                        resourceitem.Value = dr["settingvalue"].ToString();

                        output.Add(resourceitem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetSettingResources(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        /// <summary>
        /// GetSettingResources; Get all setting resources that are available. All setting resources can also be added when making use of the include variable.
        /// </summary>
        /// <returns>A list of setting resource items</returns>
        public async Task<List<SettingResource>> GetSettingResources(int companyid, string include = null)
        {
            var output = await GetFeatureSettingResources();

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.CompanySettings.ToString().ToLower())) output = await AppendCompanySettingResources(companyId: companyid, resources: output);

            }

            return output;
        }

        /// <summary>
        /// GetSettingResourceByKey; Get resource key setting.
        /// </summary>
        /// <param name="resourceKey">resource key based on settings.settingskey. (e.g. TECH_SOMETHINGSOMETHING )</param>
        /// <returns>returns a settings resource object containing all relevant data.</returns>
        public async Task<SettingResource> GetSettingResourceByKey(string resourceKey)
        {
            var output = new SettingResource();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_settingkey", resourceKey));

                using (dr = await _manager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var resourceitem = new SettingResource();
                        resourceitem.Description = dr["description"].ToString();
                        resourceitem.Id = Convert.ToInt32(dr["id"]);
                        resourceitem.Name = dr["name"].ToString();
                        resourceitem.SettingResourceType = (SettingResourceSettingsTypeEnum)Enum.ToObject(typeof(SettingResourceSettingsTypeEnum), Convert.ToInt32(dr["settingstype"]));
                        resourceitem.SettingsKey = dr["settingskey"].ToString();
                        resourceitem.Value = dr["settingvalue"].ToString();

                        output = resourceitem;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetSettingResourceByKey(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        //not async because then the method it cannot have out parameters
        public async Task<Tuple<bool, List<string>>> TryUpdateCompanySetting(int companyid, int userId, SettingResourceItemTrueFalse resourceSettingToUpdate)
        {
            var result = true;

            var messages = new List<string>();

            var resourceSetting = await GetSettingResourceByKey(resourceSettingToUpdate.ResourceKey);

            //move to manager
            if (resourceSettingToUpdate.ResourceValue == true)
            {
                //add to setting
                if (!resourceSetting.Value.Split(",").Contains(companyid.ToString()) && resourceSetting.Value != "ALL")
                {
                    var originalSettingValues = resourceSetting.Value.Split(',').ToList();

                    originalSettingValues.Add(companyid.ToString());

                    var newSettingValue = string.Join(',', originalSettingValues);

                    result &= await ChangeSettingResource(companyId: companyid, userId: userId, id: resourceSetting.Id, value: newSettingValue);
                }
                else
                {
                    messages.Add($"This feature setting is already turned on for this company. ({resourceSettingToUpdate.ResourceKey})");
                    result = false;
                }
            }
            else if (resourceSettingToUpdate.ResourceValue == false)
            {
                //remove from setting
                if (resourceSetting.Value == "ALL")
                {
                    messages.Add($"This feature setting is turned on for ALL companies. Unable to remove company from setting...");
                    result = false;
                }
                else if (resourceSetting.Value.Split(",").Contains(companyid.ToString()))
                {
                    var originalSettingValues = resourceSetting.Value.Split(',').ToList();

                    originalSettingValues.Remove(companyid.ToString());

                    var newSettingValue = string.Join(',', originalSettingValues);

                    result &= await ChangeSettingResource(companyId: companyid, userId: userId, id: resourceSetting.Id, value: newSettingValue);
                }
                else
                {
                    messages.Add($"This feature setting is already turned off for this company. ({resourceSettingToUpdate.ResourceKey})");
                    result = false;
                }
            }

            return new Tuple<bool, List<string>>(result, messages);
        }

        /// <summary>
        /// GetHasAccessToFeatureByCompany; Check if company has access to the feature.
        /// Features are based on setting resources and coupled by company_id; If feature value contains ALL then all companies may use this feature.
        /// </summary>
        /// <param name="companyId">companyId</param>
        /// <param name="featurekey">featurekey</param>
        /// <returns>true/false depending on outcome</returns>
        public async Task<bool> GetHasAccessToFeatureByCompany(int companyId, string featurekey)
        {
            var resource = await GetSettingResourceByKey(resourceKey: featurekey);
            if (resource != null && resource.Id > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(resource.Value) && (resource.Value == "ALL" || resource.Value.Split(",").Where(x => Convert.ToInt32(x) == companyId).FirstOrDefault() == companyId.ToString()))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetHasAccessToFeatureByCompany(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                    return false;
                }

            }
            return false;
        }

        /// <summary>
        /// GetIsSetInSetting; Checks if a certain value is set in the setting.
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="settingkey">Setting key to be retrieved.</param>
        /// <returns></returns>
        public async Task<bool> GetIsSetInSetting(string value, string settingkey)
        {
            var resource = await GetSettingResourceByKey(resourceKey: settingkey);
            if (resource != null && resource.Id > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(resource.Value) && (resource.Value == "ALL" || resource.Value.Split(",").Contains(value)))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetIsSetInSetting(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                    return false;
                }

            }
            return false;
        }

        /// <summary>
        /// GetSettingResources; Get all setting resources that are available for a specific company
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of Settings resource for a specific company.</returns>
        public async Task<List<SettingResource>> GetSettingResourcesForSettings(int companyid)
        {
            var output = new List<SettingResource>();

            output = await GetSettingResources(companyid: companyid);

            var companyitems = await GetSettingResourceItemForCompany(companyid: companyid);

            foreach (var item in output)
            {
                item.Value = companyitems.Where(x => x.ResourceId == item.Id)?.FirstOrDefault()?.Value ?? item.Value;
            }

            return output;
        }

        /// <summary>
        /// AppendCompanySettingResources; Add setting resource item of a specific company to a resource items based on the incoming list of items..
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="resources">Resource where company items need to be added.</param>
        /// <returns>Updated list of SettingResources.</returns>
        public async Task<List<SettingResource>> AppendCompanySettingResources(int companyId, List<SettingResource> resources)
        {
            var output = resources;

            var companyitems = await GetSettingResourceItemForCompany(companyid: companyId);

            foreach (var item in output)
            {
                item.ResourceItems = companyitems.Where(x => x.ResourceId == item.Id).ToList();
            }

            return output;
        }


        /// <summary>
        /// AddSettingResourceCompany; Add setting resource for a specific company.
        /// </summary>
        /// <param name="companyid">CompanyId of the company</param>
        /// <param name="setting">Setting. Note setting must have a settings resource id. </param>
        /// <returns>Id of just inserted item.</returns>
        public async Task<int> AddSettingResourceCompany(int companyid, SettingResourceItem setting)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", setting.CompanyId));
            parameters.Add(new NpgsqlParameter("@_description", setting.Description));
            parameters.Add(new NpgsqlParameter("@_resourceid", setting.ResourceId));
            parameters.Add(new NpgsqlParameter("@_value", setting.Value));
            var possibleid = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_resource_setting_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return possibleid;
        }

        /// <summary>
        /// ChangeSettingResourceCompany; Change setting
        /// </summary>
        /// <param name="companyid">CompanyId of the company</param>
        /// <param name="setting">Setting. Note setting must have a settings resource id. </param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ChangeSettingResourceCompany(int companyid, SettingResourceItem setting, bool encryptValue = false)
        {
            if(encryptValue)
            {
                setting.Value = _cryptography.Encrypt(setting.Value);
            }

            int rowseffected;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", setting.CompanyId));
            parameters.Add(new NpgsqlParameter("@_resourceid", setting.ResourceId));
            if (setting.ResourceId == 83)
            {
                var resourceSettingValue = _cryptography.Encrypt(setting.Value);
                parameters.Add(new NpgsqlParameter("@_value", resourceSettingValue));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_value", setting.Value));
            }
            if (setting.Id <= 0)
            {
                rowseffected = (int)await _manager.ExecuteScalarAsync("change_resource_setting_company_by_resource", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_id", setting.Id));
                rowseffected = (int)await _manager.ExecuteScalarAsync("change_resource_setting_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task<bool> ChangeSettingResourceHolding(int holdingId, SettingResourceItem setting, bool encryptValue = false)
        {
            if (encryptValue)
            {
                setting.Value = _cryptography.Encrypt(setting.Value);
            }

            int rowseffected;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_holdingid", setting.HoldingId));
            parameters.Add(new NpgsqlParameter("@_resourceid", setting.ResourceId));
            parameters.Add(new NpgsqlParameter("@_value", setting.Value));

            rowseffected = (int)await _manager.ExecuteScalarAsync("change_resource_setting_holding_by_resource", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

            return (rowseffected > 0);
        }

        /// <summary>
        /// ChangeSettingResource; Change settings resource
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> ChangeSettingResource(int companyId, int userId, int id, string value)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.resource_settings.ToString(), id);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_value", value));
            parameters.Add(new NpgsqlParameter("@_id", id));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_resource_setting", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.resource_settings.ToString(), id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.resource_settings.ToString(), objectId: id, userId: userId, companyId: companyId, description: "Changed resource setting.");
            }

            //try init ez feed if id is 14 (FEATURE_FACTORY_FEED)
            if (id == 14)
            {
                try
                {
                    var companyIds = value.Split(',').Select(v => Convert.ToInt32(v)).ToList();
                    foreach (var cId in companyIds)
                    {
                        var company = new Company();

                        NpgsqlDataReader dr = null;

                        //get company to see which user is the admin account
                        List<NpgsqlParameter> companyParameters = new List<NpgsqlParameter>();
                        companyParameters.Add(new NpgsqlParameter("@_companyid", cId));
                        companyParameters.Add(new NpgsqlParameter("@_id", cId));

                        using (dr = await _manager.GetDataReader("get_company", commandType: System.Data.CommandType.StoredProcedure, parameters: companyParameters))
                        {
                            while (await dr.ReadAsync())
                            {
                                company = CreateOrFillCompanyFromReader(dr, company: company);
                            }
                        }

                        //get feeds and add feeds for company if they dont exist
                        await TryInitializeEzFeed(cId, company.ManagerId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("GeneralManager.ChangeSettingResource(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
            }
            return (Convert.ToInt32(rowseffected) > 0);
        }

        /// <summary>
        /// GetSettingResourceItemForCompany; Get company resource specific items.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of setting resource items for a company.</returns>
      public async Task<List<SettingResourceItem>> GetSettingResourceItemForCompany(int companyid)
       {
            var output = new List<SettingResourceItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyid));

                using (dr = await _manager.GetDataReader("get_resource_settings_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var resourceitem = new SettingResourceItem();
                        resourceitem.Description = dr["description"].ToString();
                        resourceitem.Id = Convert.ToInt32(dr["id"]);
                        if (dr["resource_setting_id"] != DBNull.Value)
                        {
                            resourceitem.ResourceId = Convert.ToInt32(dr["resource_setting_id"]);
                        }
                
                        string encryptedValue = dr["value"].ToString();
                
                        // Only attempt decryption for Resource ID 83 (Ultimo API Token)
                        if (_configurationHelper.GetValueAsBool("AppSettings:EnableUltimoConnector") && 
                            resourceitem.ResourceId != null && 
                            resourceitem.ResourceId == 83) //83 is COMPANY_ULTIMO_API_TOKEN
                        {
                            try
                            {
                                resourceitem.Value = _cryptography.Decrypt(encryptedValue);
                            }
                            catch (Exception ex)
                            {
                                // If decryption fails, return empty string but don't crash the application
                                resourceitem.Value = string.Empty;
                                _logger.LogWarning("Failed to decrypt Ultimo API token for company {CompanyId}: {ErrorMessage}", 
                                    companyid, ex.Message);
                            }
                        }
                        else
                        {
                            // For all other resources, use the value as-is (no decryption)
                            resourceitem.Value = encryptedValue;
                        }
                
                        if (dr["company_id"] != DBNull.Value)
                        {
                            resourceitem.CompanyId = Convert.ToInt32(dr["company_id"]);
                        }

                        output.Add(resourceitem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetSettingResourceItemForCompany(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
       }

        /// <summary>
        /// GetSettingValueForCompanyByResourceId; Get a resource value for a specific item for a specific company. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>String containing a specific value of a resource setting.</returns>
        public async Task<string> GetSettingValueForCompanyByResourceId(int companyid, int resourcesettingid)
        {
            string output = string.Empty;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyid));
                parameters.Add(new NpgsqlParameter("@_resourcesettingid", resourcesettingid));

                var retrievedValue = await _manager.ExecuteScalarAsync("get_resource_settings_value_company_by_key", parameters: parameters);
                if (retrievedValue != null && retrievedValue != DBNull.Value)
                {
                    output = (string)retrievedValue;
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetSettingValueForCompanyByResourceId(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }
        public async Task<string> GetSettingValueForCompanyOrHoldingByResourceId(int companyid, int resourcesettingid, bool decryptValue = false)
        {
            string result = string.Empty;

            try
            {
                List<NpgsqlParameter> parameters = _manager.GetBaseParameters(companyId: companyid);
                parameters.Add(new NpgsqlParameter("@_resourcesettingid", resourcesettingid));

                var retrievedValue = await _manager.ExecuteScalarAsync("get_resource_settings_value_company_or_holding_by_key", parameters: parameters);
                if (retrievedValue != null && retrievedValue != DBNull.Value)
                {
                    result = (string)retrievedValue;
                    if (decryptValue)
                    {
                        result = _cryptography.Decrypt(result);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetSettingValueForCompanyOrHoldingByResourceId(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return result;
        }
        #endregion

        #region - announcement -
        /// <summary>
        /// GetAnnouncements; Get a list of announcements. Currently used in CMS on dashboard page and management portal.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A list of announcements.</returns>
        public async Task<List<Announcement>> GetAnnouncements(int companyId, AnnouncementTypeEnum announcementType = AnnouncementTypeEnum.ReleaseNotes, int limit = 0, int offset = 0)
        {
            var output = new List<Announcement>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_announcementtype", (int)announcementType));
                if (limit > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_limit", limit));
                }
                if (offset > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_offset", offset));
                }

                using (dr = await _manager.GetDataReader("get_announcements", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = new Announcement();
                        item.Description = dr["description"].ToString();
                        item.Id = Convert.ToInt32(dr["id"]);
                        if (dr["announcement_date"] != DBNull.Value)
                        {
                            item.Date = Convert.ToDateTime(dr["announcement_date"]);
                        }
                        item.Title = dr["title"].ToString();
                        item.AnnouncementType = (AnnouncementTypeEnum)Enum.ToObject(typeof(AnnouncementTypeEnum), Convert.ToInt32(dr["announcement_type"]));

                        output.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("GeneralManager.GetAnnouncements(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// Resolves a predefined date-range keyword into a CreatedFrom/CreatedTo period
        /// by executing the corresponding database stored procedure. 
        /// Supported range values include: last12days, last12weeks, last12months, and thisyear.
        /// Returns a tuple containing the calculated start and end timestamps, or null if
        /// the rangePeriod is invalid or the database returns no result.
        /// </summary>

        public async Task<(DateTime? start, DateTime? end)> GetCreatedPeriodByRangeAsync(int companyId, string rangePeriod)
        {
            string storedProcedure;
            switch (rangePeriod.ToLowerInvariant())
            {
                case "last12days":
                    storedProcedure = "get_last_12_days_period_by_timestamp";
                    break;

                case "last12weeks":
                    storedProcedure = "get_last_12_weeks_period_by_timestamp";
                    break;

                case "last12months":
                    storedProcedure = "get_last_12_months_period_by_timestamp";
                    break;

                case "thisyear":
                    storedProcedure = "get_this_years_12_months_period_by_timestamp";
                    break;

                default:
                    return (null, null); 
            }

            NpgsqlDataReader dr = null;

            try
            {
                var timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@_companyid", NpgsqlDbType.Integer) { Value = companyId },
                    new NpgsqlParameter("@_timestamp", NpgsqlDbType.Timestamp) { Value = timestamp }
                };

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: CommandType.StoredProcedure, parameters: parameters))
                {
                    if (await dr.ReadAsync())
                    {
                        DateTime? from = null;
                        DateTime? to = null;

                        if (dr.HasColumn("period_start") && dr["period_start"] != DBNull.Value)
                        {
                            from = Convert.ToDateTime(dr["period_start"]);
                        }

                        if (dr.HasColumn("period_end") && dr["period_end"] != DBNull.Value)
                        {
                            to = Convert.ToDateTime(dr["period_end"]);
                        }

                        return (from, to);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GeneralManager.GetCreatedPeriodByRangeAsync(): {Message}", ex.Message);

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY))
                {
                    this.Exceptions.Add(ex);
                }
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }
            }

            return (null, null);
        }


        /// <summary>
        /// AddAnnouncement; Add announcement to database.
        /// </summary>
        /// <param name="announcement">Announcement data to be added</param>
        /// <returns>id of inserted announcement.</returns>
        public async Task<int> AddAnnouncement(Announcement announcement)
        {
            await Task.CompletedTask;
            //"_id" int4, "_title" varchar, "_description" text, "_announcementdate" timestamp, "_announcementtype" int4
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_title", announcement.Title));
            parameters.Add(new NpgsqlParameter("@_description", announcement.Description));
            parameters.Add(new NpgsqlParameter("@_announcementdate", new DateTime(announcement.Date.Ticks)));
            parameters.Add(new NpgsqlParameter("@_announcementtype", (int)announcement.AnnouncementType));
            var possibleid = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_announcement", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return possibleid;
        }

        /// <summary>
        /// ChangeAnnouncement; Change a existing announcement
        /// </summary>
        /// <param name="announcementId">Id of the item to be changed</param>
        /// <param name="announcement">Announcement data to be added</param>
        /// <returns>true/false depending on rows effected.</returns>
        public async Task<bool> ChangeAnnouncement(int announcementId, Announcement announcement)
        {
            await Task.CompletedTask;
            //"_id" int4, "_title" varchar, "_description" text, "_announcementdate" timestamp, "_announcementtype" int4
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_title", announcement.Title));
            parameters.Add(new NpgsqlParameter("@_description", announcement.Description));
            parameters.Add(new NpgsqlParameter("@_announcementdate", new DateTime(announcement.Date.Ticks)));
            parameters.Add(new NpgsqlParameter("@_announcementtype", (int)announcement.AnnouncementType));
            parameters.Add(new NpgsqlParameter("@_id", announcementId));
            var rowseffected = await _manager.ExecuteScalarAsync("change_announcement", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            return (Convert.ToInt32(rowseffected) > 0);

        }

        public async Task<bool> SetAnnouncementActiveAsync(int companyId, int userId, int announcementId, bool isActive = true)
        {

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", announcementId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_announcement_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);
        }
        #endregion

        #region - other -
        /// <summary>
        /// DoTestCall; Test call for checking APM enabled structures. NOTE! needs to be moved to tools. 
        /// </summary>
        /// <param name="useError">Push error to APM yes or no</param>
        /// <returns>true</returns>
        public async Task<bool> DoTestCall(bool useError)
        {
            await Task.CompletedTask;
            return useError;
        }

        /// <summary>
        /// CreateOrFillCompanyFromReader; creates and fills a Company object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="company">Company object containing all data needed for updating the database. (DB: companies_company)</param>
        /// <returns>A filled Company object.</returns>
        private Company CreateOrFillCompanyFromReader(NpgsqlDataReader dr, Company company = null)
        {

            if (company == null) company = new Company();

            company.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                company.Description = dr["description"].ToString();
            }
            if (dr["manager_id"] != DBNull.Value)
            {
                company.ManagerId = Convert.ToInt32(dr["manager_id"]);
            }
            company.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                company.Picture = dr["picture"].ToString();
            }

            return company;
        }

        private async Task<bool> TryInitializeEzFeed(int companyId, int userId)
        {
            var feeds = await _feedManager.GetFeedAsync(companyId: companyId);
            if (feeds.Where(f => f.FeedType == FeedTypeEnum.MainFeed).FirstOrDefault() == null)
            {
                await _feedManager.AddFeedAsync(companyId: companyId,
                    userId: userId,
                    new Models.Feed.FactoryFeed()
                    {
                        Name = "Main Feed",
                        Description = "Main Feed",
                        Attachments = new List<string>(),
                        CompanyId = companyId,
                        DataJson = "",
                        FeedType = EZGO.Api.Models.Enumerations.FeedTypeEnum.MainFeed,
                        Items = new List<Models.Feed.FeedMessageItem>(),
                    });
            }

            if (feeds.Where(f => f.FeedType == FeedTypeEnum.FactoryUpdates).FirstOrDefault() == null)
            {
                await _feedManager.AddFeedAsync(companyId: companyId,
                    userId: userId,
                    new Models.Feed.FactoryFeed()
                    {
                        Name = "Factory Updates",
                        Description = "Factory Updates",
                        Attachments = new List<string>(),
                        CompanyId = companyId,
                        DataJson = "",
                        FeedType = EZGO.Api.Models.Enumerations.FeedTypeEnum.FactoryUpdates,
                        Items = new List<Models.Feed.FeedMessageItem>(),
                    });
            }

            return true;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_toolsManager.GetPossibleExceptions());
                listEx.AddRange(_feedManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }

        #endregion

    }
}
