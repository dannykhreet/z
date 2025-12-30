using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.WhiteLabel;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Export;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

//TODO split up, create seperate controllers for specific functionality
namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// GeneralController; contains all routes based on general functionality used by the App.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class GeneralController : BaseController<GeneralController>
    {
        #region - privates -
        private readonly IGeneralManager _manager;
        private readonly IMemoryCache _cache;
        #endregion

        #region - contructor(s) -
        public GeneralController(IGeneralManager manager, IMemoryCache cache, ILogger<GeneralController> logger, IConfigurationHelper configurationHelper,IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _cache = cache;
        }
        #endregion

        #region - GET routes for Menus -
        [Route("app/mainmenu")]
        [HttpGet]
        public async Task<IActionResult> GetMainMenu([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = false)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetMainMenuAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid, allowedonly.HasValue && allowedonly.Value ? await this.CurrentApplicationUser.GetAndSetUserIdAsync() : new Nullable<int>());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("app/mainmenu/statistics")]
        [HttpGet]
        public async Task<IActionResult> GetMainMenuStatistics([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = false)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetStatisticsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid, allowedonly.HasValue && allowedonly.Value ? await this.CurrentApplicationUser.GetAndSetUserIdAsync() : new Nullable<int>());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("app/sidemenu")]
        [HttpGet]
        public async Task<IActionResult> GetSideMenu([FromRoute] int? areaid)
        {
            //TODO needs to be moved to manager and separate code, currently just for setup reasons stubbed.
            var result = new Models.UI.SideMenu();
            result.MenuItems = new List<Models.UI.SideMenuItem>();
            result.MenuItems.Add(new Models.UI.SideMenuItem() { Title = "Actions", Description = "", Icon = "" });
            result.MenuItems.Add(new Models.UI.SideMenuItem() { Title = "Checklists", Description = "", Icon = "" });
            result.MenuItems.Add(new Models.UI.SideMenuItem() { Title = "Tasks", Description = "", Icon = "" });
            result.MenuItems.Add(new Models.UI.SideMenuItem() { Title = "Reports", Description = "", Icon = "" });
            result.MenuItems.Add(new Models.UI.SideMenuItem() { Title = "Audits", Description = "", Icon = "" });
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }
        #endregion

        #region - GET routes for settings and resources -
        [Route("app/settings")]
        [HttpGet]
        public async Task<IActionResult> GetApplicationSettings()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            //TODO needs to be moved to manager and seperate code, currently just for setup reasons stubbed.
            var result = await _manager.GetApplicationSettings(companyid: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [AllowAnonymous]
        [Route("app/resources/language")]
        [HttpGet]
        public async Task<IActionResult> GetLanguage([FromQuery] string language, [FromQuery] ResourceLanguageTypeEnum resourcetype = ResourceLanguageTypeEnum.APP)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = string.IsNullOrEmpty(language) ? await _manager.GetLanguageResource(culture: "en-US", resourceType: resourcetype) : await _manager.GetLanguageResource(culture: language, resourceType: resourcetype);

            if(result == null || (result != null && string.IsNullOrEmpty(result.Language)) || (result != null && result.ResourceItems?.Count() < 10)) {
                result = await _manager.GetLanguageResource(culture: "en-US", resourceType: resourcetype);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [AllowAnonymous]
        [Route("app/resources/language/file")]
        [HttpGet]
        public async Task<IActionResult> GetLanguageFile([FromQuery] string language, [FromQuery] string output, [FromQuery] ResourceLanguageTypeEnum resourcetype = ResourceLanguageTypeEnum.APP)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetLanguageResource(culture: language, resourceType: resourcetype);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            var fileResult = LanguageResourceFileGenerator.GenerateFileOutputIOS(result);

            return StatusCode((int)HttpStatusCode.OK, fileResult);
        }

        //[AllowAnonymous]
        [Route("app/resources/language/statistics")]
        [HttpGet]
        public async Task<IActionResult> GetLanguageStatistics()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result =  await _manager.GetLanguageResourceStatistics();

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }
        #endregion

        #region - post/get routes company settings -
        //TODO move to correct controller
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/{companyid}/settings/add")]
        [HttpPost]
        public async Task<IActionResult> AddCompanySetting([FromRoute] int companyid, [FromBody] SettingResourceItem resourceItem)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!resourceItem.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (companyid != resourceItem.CompanyId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDENCOMPANY_OBJECT.ToJsonFromObject());
            }
            var result = await _manager.AddSettingResourceCompany(companyid: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), setting: resourceItem);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result);

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/{companyid}/settings/change")]
        [HttpPost]
        public async Task<IActionResult> ChangeCompanySetting([FromRoute] int companyid, [FromBody] SettingResourceItem resourceItem)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!resourceItem.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                     userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                     messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (companyid != resourceItem.CompanyId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDENCOMPANY_OBJECT.ToJsonFromObject());
            }
            var result = await _manager.ChangeSettingResourceCompany(companyid: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), setting: resourceItem);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("holding/{holdingid}/settings/change")]
        [HttpPost]
        public async Task<IActionResult> ChangeHoldingSetting([FromRoute] int holdingid, [FromBody] SettingResourceItem resourceItem)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!resourceItem.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                     userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                     messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (holdingid != resourceItem.HoldingId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDENCOMPANY_OBJECT.ToJsonFromObject());
            }
            var result = await _manager.ChangeSettingResourceHolding(holdingId: holdingid, setting: resourceItem);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("company/{companyid}/settings/set")]
        public async Task<IActionResult> SetCompanySetting([FromRoute] int companyid, [FromBody] List<SettingResourceItemTrueFalse> resourceSettingsToUpdate)
        {
            var result = true;

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != companyid)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            //check whitelist of changeable settings
            foreach(var resourceSettingToUpdate in resourceSettingsToUpdate)
            {
                if (!Enum.TryParse<ChangeableCompanySettings>(resourceSettingToUpdate.ResourceKey, out var resourceKey))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, $"You aren't allowed to change this company setting. ({resourceSettingToUpdate.ResourceKey})".ToJsonFromObject());
                }
            }

            var messages = new List<string>();
            foreach (var resourceSettingToUpdate in resourceSettingsToUpdate)
            {
                var updateResult = await _manager.TryUpdateCompanySetting(companyid: companyid, 
                                                                            userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                                                            resourceSettingToUpdate: resourceSettingToUpdate);

                result &= updateResult.Item1;
                messages.AddRange(updateResult.Item2);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            if (result)
            {
                return StatusCode((int)HttpStatusCode.OK, result);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, messages.ToJsonFromObject());
            }
        }
        #endregion

        #region - GET routes for notifications -
        [Route("app/notifications")]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            //TODO needs to be moved to manager and separate code, currently just for setup reasons stubbed.
            var result = new Models.UI.DisplayNotification();
            result.UserId = -1;
            result.Notifications = new List<Models.UI.DisplayNotificationItem>();
            result.Notifications.Add(new Models.UI.DisplayNotificationItem() {Id = 1, Title = "New notification", Description = "Do something!" });
            result.Notifications.Add(new Models.UI.DisplayNotificationItem() {Id = 2, Title = "New notification 2", Description = "Do something! 2" });
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }
        #endregion

        #region - General update checks -
        [Route("updatecheck")]
        [HttpGet]
        public async Task<IActionResult> GetUpdates([FromQuery] string updatechecktype, [FromQuery] string fromdateutc, [FromQuery] int? areaid = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!string.IsNullOrEmpty(fromdateutc))
            {
                DateTime parsedfromdateutc;
                if (DateTime.TryParseExact(fromdateutc, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedfromdateutc) || DateTime.TryParseExact(fromdateutc, "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedfromdateutc))
                {
                    string runCheck = string.Format("_{0}_{1}_{2}", await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), await this.CurrentApplicationUser.GetAndSetUserIdAsync(), "GetUpdates");
                    if (ProtectionHelper.CheckRunningRequest(_cache, runCheck)) return StatusCode((int)HttpStatusCode.OK, (new List<UpdateCheckItem>()).ToJsonFromObject());

                    if (parsedfromdateutc < DateTime.Now.AddYears(-1)) return StatusCode((int)HttpStatusCode.OK, new List<UpdateCheckItem>().ToJsonFromObject()); //swallow request when date time is really smaller.
                    var resultbasedondate = await _manager.GetUpdateChangesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedfromdateutc, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid);

                    AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                    Agent.Tracer.CurrentSpan.End();

                    ProtectionHelper.RemoveRunningRequest(_cache, runCheck);
                    return StatusCode((int)HttpStatusCode.OK, (resultbasedondate).ToJsonFromObject());
                }
            }

            string runCheckNoParam = string.Format("_{0}_{1}_{2}", await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), await this.CurrentApplicationUser.GetAndSetUserIdAsync(), "GetUpdates");
            if (ProtectionHelper.CheckRunningRequest(_cache, runCheckNoParam)) return StatusCode((int)HttpStatusCode.OK, (new List<UpdateCheckItem>()).ToJsonFromObject());

            var result = await _manager.GetUpdateChangesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            ProtectionHelper.RemoveRunningRequest(_cache, runCheckNoParam);
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("updatecheckfeed")]
        [HttpGet]
        public async Task<IActionResult> GetUpdatesFeed([FromQuery] string updatechecktype, [FromQuery] string fromdateutc, [FromQuery] int? areaid = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!string.IsNullOrEmpty(fromdateutc))
            {
                DateTime parsedfromdateutc;
                if (DateTime.TryParseExact(fromdateutc, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedfromdateutc) || DateTime.TryParseExact(fromdateutc, "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedfromdateutc))
                {
                    string runCheck = string.Format("_{0}_{1}_{2}", await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), await this.CurrentApplicationUser.GetAndSetUserIdAsync(), "GetUpdatesFeed");
                    if (ProtectionHelper.CheckRunningRequest(_cache, runCheck)) return StatusCode((int)HttpStatusCode.OK, (new List<UpdateCheckItem>()).ToJsonFromObject());

                    if (parsedfromdateutc < DateTime.Now.AddYears(-1)) return StatusCode((int)HttpStatusCode.OK, new List<UpdateCheckItem>().ToJsonFromObject()); //swallow request when date time is really smaller.
                    var resultbasedondate = await _manager.GetUpdateChangesFeedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedfromdateutc, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid);

                    AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                    Agent.Tracer.CurrentSpan.End();

                    ProtectionHelper.RemoveRunningRequest(_cache, runCheck);
                    return StatusCode((int)HttpStatusCode.OK, (resultbasedondate).ToJsonFromObject());
                }
            }

            string runCheckNoParam = string.Format("_{0}_{1}_{2}", await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), await this.CurrentApplicationUser.GetAndSetUserIdAsync(), "GetUpdatesFeed");
            if (ProtectionHelper.CheckRunningRequest(_cache, runCheckNoParam)) return StatusCode((int)HttpStatusCode.OK, (new List<UpdateCheckItem>()).ToJsonFromObject());

            var result = await _manager.GetUpdateChangesFeedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            ProtectionHelper.RemoveRunningRequest(_cache, runCheckNoParam);
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        #endregion

        #region - whitelabel -
        [AllowAnonymous]
        [Route("app/labels/{label}")]
        [HttpGet]
        public async Task<IActionResult> GetWhiteLabelSettings([FromRoute] string label)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            await Task.CompletedTask;

            var lbl = new LabelSettings();

            switch (label)
            {
                case "pacman":
                    {
                        
                        lbl.DisplayTitle = "Pacman";
                        lbl.DisplayEntityName = "Pacman Inc.";
                        lbl.Icon = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/e37c11e5-c3bd-4f8d-8fd3-e856e0351e93.jpg";
                        lbl.Logo = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/deda826a-746f-4145-9d5b-96a794457bd6.png";
                        lbl.PrimaryColor = "#FFD10C";
                        lbl.SecondaryColor = "#1f3240";
                        lbl.TertiaryColor = "#FF1203";
                        lbl.BackgroundImage = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/lists/0/5ba20bbe-1941-4d5d-900d-269293712ae6.jpg";
                        lbl.ImageCarrousel = (new string[] { "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/0006ce37-e779-4d2b-a966-bee71323eed1.png",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/c4610940-49b3-4537-8cd7-2a7fced331c5.png",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/458da764-a301-4ef9-befe-03906f71c46f.png",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/07b80f18-f932-448a-93ea-a9b4adcce754.png",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/898d0617-011d-4da0-922d-063c28a70f23.png", 
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/0e0abfa4-b8e3-4762-8a46-d089b34bb8cb.png", 
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/dcd1ad8a-c2af-4a53-aa29-5446ad2654d7.png",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/3a2573cd-bdfc-4694-961b-d9236b06dd86.png"}).ToList();
                    }
                    
                    break;
                case "ezfactory":
                    {

                        lbl.DisplayTitle = "EZ GO";
                        lbl.DisplayEntityName = "EZ Factory";
                        lbl.Icon = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/07e5ab9d-278a-45aa-82c3-43fa1c201219.png";
                        lbl.Logo = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/b246476a-f8e7-408f-8e49-1e51f19fd555.png";
                        lbl.PrimaryColor = "#07488a";
                        lbl.SecondaryColor = "#1889c5";
                        lbl.TertiaryColor = "#8d8d8d";
                        lbl.BackgroundImage = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/730c81b4-eedd-4ae4-b6dd-7e5b0f8ba48f.jpg";
                        lbl.ImageCarrousel = (new string[] { "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/5eb77fc2-6c8d-44cd-99f0-dd93e1d29a77.jpg",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/8b02dfa1-e5d2-4bd3-b330-4f242b31570d.jpg",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/1e071147-7cb0-412b-8694-22506d262186.jpg",
                                                    "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/tasks/0/6b12200d-ce9f-4265-93d5-7b9ce797fb57.jpg"}).ToList();
                    }

                    break;
                default:
                    {
                        lbl.DisplayTitle = "EZ GO";
                        lbl.DisplayEntityName = "EZ Factory";
                        lbl.Icon = "https://ezfactory.nl/wp-content/uploads/2021/01/cropped-Logo-32x32.png";
                        lbl.Logo = "https://ezgo.testportal.ezfactory.nl/images/logo.png";
                        lbl.PrimaryColor = "#93C54B";
                        lbl.SecondaryColor = "#FF7804";
                        lbl.TertiaryColor = "#6c757d";
                        lbl.BackgroundImage = "https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/lists/0/ba8c3d3c-0e77-458d-a475-2d3188d1e2c7.jpg";
                        lbl.ImageCarrousel = (new string[] { "https://ezfactory.nl/wp-content/uploads/2020/10/factory-solution-1.jpg",
                                                 "https://ezfactory.nl/wp-content/uploads/2020/10/factory-solution-2.jpg",
                                                 "https://ezfactory.nl/wp-content/uploads/2020/10/factory-solution-3.jpg",
                                                 "https://ezfactory.nl/wp-content/uploads/2020/10/factory-solution-4.jpg"}).ToList();
                    }
                    break;
                    
            }

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (lbl).ToJsonFromObject());
        }
        #endregion

    }
}