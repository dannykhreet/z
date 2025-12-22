using EZGO.Api.Models;
using EZGO.Api.Models.Marketplace;
using EZGO.Api.Models.SapPm;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class ConfigController : BaseController
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public ConfigController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        #region - areas -
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            ConfigViewModel output = new ConfigViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.CONFIG; //TODO only init when needed, move to methods that need it
            output.PageTitle = "Company configuration";
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            output.EnableTaskgenerationManagement = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskGenerationManagement");

            var companyResult = _connector.GetCall(Constants.Company.GetCompanyWithShifts).GetAwaiter().GetResult();
            var rolesResult = _connector.GetCall("/v1/company/roles").GetAwaiter().GetResult();
            if (rolesResult.StatusCode == HttpStatusCode.OK)
            {
                output.CompanyRoles = JsonConvert.DeserializeObject<CompanyRoles>(rolesResult.Message);
            }
            else
            {
                output.CompanyRoles = new CompanyRoles();
            }
            if (companyResult.StatusCode == HttpStatusCode.OK)
            {
                output.Company = JsonConvert.DeserializeObject<CompanyModel>(companyResult.Message);
            }
            else
            {
                return Ok("Error during retrieval of company!");
            }

            var locationsEndpoint = "/v1/locations/children";
            var locationsResult = await _connector.GetCall(locationsEndpoint);
            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                output.SapPmFunctionalLocations = JsonConvert.DeserializeObject<List<SapPmLocation>>(locationsResult.Message);
            }

            output.Locale = _locale;
            output.DisableMutateArea = _configurationHelper.GetValueAsBool("AppSettings:DisableMutateArea");
            return View(output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.MarketPlace)]
        [HttpGet]
        public async Task<IActionResult> Integrations()
        {
            ConfigViewModel output = new ConfigViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.CONFIG; //TODO only init when needed, move to methods that need it
            output.PageTitle = "Company configuration";
            output.Locale = _locale;
            var companyResult = await _connector.GetCall(Constants.Company.GetCompanyWithShifts);
            var rolesResult = await _connector.GetCall("/v1/company/roles");
            //var marketResult = await _connector.GetCall("/v1/marketplace");
            output.CompanyRoles = JsonConvert.DeserializeObject<CompanyRoles>(rolesResult.Message);
            output.Company = JsonConvert.DeserializeObject<CompanyModel>(companyResult.Message);
            //output.MarketPlace = JsonConvert.DeserializeObject<List<MarketPlaceItem>>(marketResult.Message);

            //SAP, Microsoft office 365, Azure authentication, Ultimo, Realware, EZ-GO Web app, 
            output.MarketPlace = new List<MarketPlaceItem>(){
                new MarketPlaceItem
                {
                    Id = 1,
                    Name = "Ultimo",
                    Description = "Ultimo offers many rich functionalities as standard. For planning, monitoring, optimization and execution. For tracking all the required maintenance activities.",
                    Picture = "/images/placeholder.png"

                },
                new MarketPlaceItem
                {
                    Id = 2,
                    Name = "Microsoft office 365",
                    Description = "",
                    Picture = "/images/placeholder.png"
                },
                new MarketPlaceItem
                {
                    Id = 3,
                    Name = "Microsoft Azure Active Directory",
                    Description = "",
                    Picture = "/images/placeholder.png"
                },
                new MarketPlaceItem
                {
                    Id = 4,
                    Name = "Realware",
                    Description = "",
                    Picture = "/images/placeholder.png"
                },
                new MarketPlaceItem
                {
                    Id = 5,
                    Name = "EZ-GO Web Application",
                    Description = "",
                    Picture = "/images/placeholder.png"
                },
                new MarketPlaceItem
                {
                    Id = 6,
                    Name = "EZ-GO for iOS devices",
                    Description = "",
                    Picture = "/images/placeholder.png"
                },
                new MarketPlaceItem
                {
                    Id = 7,
                    Name = "EZ-GO for Android devices",
                    Description = "",
                    Picture = "/images/placeholder.png"
                }
            };

            output.DisableMutateArea = _configurationHelper.GetValueAsBool("AppSettings:DisableMutateArea");
            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/Config/Integrations.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.MarketPlace)]
        [HttpPost]
        [Route("/config/integrations/save")]
        public async Task<IActionResult> SaveIntegrations([FromBody] object configuration)
        {
            await Task.CompletedTask;
            if (configuration != null)
            {
                ApiResponse result = await _connector.PostCall("/v1/marketplace/config/save", configuration.ToString().ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return StatusCode((int)HttpStatusCode.OK, "Ok".ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Not Ok".ToJsonFromObject());
                }

            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [HttpGet]
        public async Task<String> GetAreas()
        {
            var result = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            return result.Message;
        }

        [HttpGet]
        public async Task<String> GetAreaTree()
        {
            var result = await _connector.GetCall(Logic.Constants.General.AreaList);
            return result.Message;
        }

        [HttpGet]
        [Route("/config/properties")]
        public async Task<String> GetProperties()
        {
            var result = await _connector.GetCall(Logic.Constants.General.PropertyList);

            List<EZGO.Api.Models.PropertyValue.Property> properties = result.Message.ToObjectFromJson<List<EZGO.Api.Models.PropertyValue.Property>>();
            List<EZGO.Api.Models.PropertyValue.Property> output = new List<EZGO.Api.Models.PropertyValue.Property>();

            if (_configurationHelper.GetValueAsInteger("AppSettings:PropertyStructureVersion") == 2)
            {
                List<EZGO.Api.Models.PropertyValue.Property> filteredproperties = properties.Where(x => x.PropertyGroupId == 6).ToList();

                // add basic values. 
                output.AddRange(properties.Where(x => x.PropertyGroupId == 1).ToList());

                /* Loop through implemented fieldtypes (see PropertyFieldTypeEnum) 
                 * 
                 *  SingleValue = 1,
                 *  Range = 2,
                 *  UpperLimit = 3,
                 *  LowerLimit = 4,
                 *  EqualTo = 5, 
                 * 
                 *  And add those as property to property collection. 
                 *  When the new IO is implemented the user can make this choice, but for now generate those for backwards compatibility with current UI. 
                 */

                for (var ftype = 1; ftype < 6; ftype++)
                {
                    for (var i = 0; i < filteredproperties.Count; i++)
                    {
                        var prop = filteredproperties[i].ToJsonFromObject().ToObjectFromJson<EZGO.Api.Models.PropertyValue.Property>();
                        prop.FieldType = (EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum)ftype;
                        output.Add(prop);
                    }
                }

                foreach (var item in output)
                {
                    if (item.FieldType == EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.Range)
                    {
                        item.Name = string.Concat(item.Name, " (~)");
                    }
                    if (item.FieldType == EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.UpperLimit) //smaller then
                    {
                        item.Name = string.Concat(item.Name, " (<)");
                    }
                    if (item.FieldType == EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.LowerLimit) //larger then
                    {
                        item.Name = string.Concat(item.Name, " (>)");
                    }
                    if (item.FieldType == EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.EqualTo)
                    {
                        item.Name = string.Concat(item.Name, " (=)");
                    }
                }

            }
            else
            {
                output = properties;
            }



            return output.ToJsonFromObject();
        }

        [HttpPost]
        public async Task<IActionResult> AddChangeArea([FromBody] Area area)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:DisableMutateArea"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Saving disabled".ToJsonFromObject());
            }

            if (area != null)
            {
                if (await this.CheckArea(area))
                {
                    if (area.ParentId.Value == 1)
                    {
                        area.ParentId = 0;
                    }
                    ApiResponse result = null;
                    if (area.Id > 0)
                    {
                        //update an area
                        area = await DetermineAndSetDefaultsArea(area: area);
                        result = await _connector.PostCall(string.Concat("/v1/area/change/", area.Id, "?fulloutput=true"), area.ToJsonFromObject());
                    }
                    else
                    {
                        //add new area
                        area = await DetermineAndSetDefaultsArea(area: area);
                        result = await _connector.PostCall("/v1/area/add?fulloutput=true", area.ToJsonFromObject());
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
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, "Area is not valid".ToJsonFromObject());
                }

            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveArea([FromBody] Area area)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:DisableMutateArea"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Saving disabled".ToJsonFromObject());
            }

            if (area != null && area.Id > 0)
            {
                var areaRelations = await GetAreaHasActiveRelationsAsync(area.Id);
                //only delete items that do not have any active relations
                if (!areaRelations.HasRelations())
                {
                    //set area inactive
                    ApiResponse result = await _connector.PostCall(string.Concat("/v1/area/setactive/", area.Id), false.ToJsonFromObject());
                    //TODO add check for status
                    return StatusCode((int)HttpStatusCode.OK, "Ok".ToJsonFromObject());

                }
                else
                {

                    return StatusCode((int)HttpStatusCode.Conflict, "Can not be removed due to active relations. This area contains active templates, shifts or sub areas.".ToJsonFromObject());
                }

            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("/config/areas/upload")]
        public async Task<string> upload(IFormCollection data)
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

                        int mediaType = 17;

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

            return string.Empty;

        }


        [HttpPost]
        [Route("/config/change/roles")]
        public async Task<IActionResult> ChangeRoles([FromBody] List<string> roles)
        {
            if (roles != null && roles.Count == 3)
            {
                //Roles are based on 3 values, they are posted based on a list of items (contains 3 items)
                var rolelist = roles.ToArray();
                var roleItem = new EZGO.Api.Models.CompanyRoles();
                roleItem.BasicDisplayName = rolelist[0];
                roleItem.ShiftLeaderDisplayName = rolelist[1];
                roleItem.ManagerDisplayName = rolelist[2];

                ApiResponse result = await _connector.PostCall("/v1/company/roles/change", roleItem.ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return StatusCode((int)HttpStatusCode.OK, "Ok".ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)result.StatusCode, "Roles are not saved".ToJsonFromObject());
                }

            }

            return StatusCode((int)HttpStatusCode.BadRequest, "Roles are empty or not complete.".ToJsonFromObject());
        }

        //_sapPmFunctionalLocations
        [HttpGet]
        [Route("/config/functionallocations")]
        public async Task<IActionResult> GetFunctionalLocations([FromQuery] int? functionalLocationId = null, [FromQuery] int parentLevel = 1)
        {
            var applicationSettings = await GetApplicationSettings();
            var locationsEndpoint = "/v1/locations/children";

            var uriParams = new List<string>();

            if (functionalLocationId != null && functionalLocationId > 0)
            {
                uriParams.Add("functionallocationid=" + functionalLocationId.Value);
            }

            if (uriParams.Count > 0)
            {
                locationsEndpoint += "?" + string.Join("&", uriParams);
            }

            var locationsResult = await _connector.GetCall(locationsEndpoint);

            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                var locations = JsonConvert.DeserializeObject<List<SapPmLocation>>(locationsResult.Message);
                var output = new List<SapPmLocationViewModel>();
                foreach(var location in locations)
                {
                    output.Add(new SapPmLocationViewModel()
                    {
                        SapPmFunctionalLocation = location,
                        ApplicationSettings = applicationSettings,
                        IndentationLevel = parentLevel+1
                    });
                }
                return PartialView("~/Views/Shared/SapFunctionalLocations/_functional_locations.cshtml", output);
            }
            else
            {
                return PartialView("~/Views/Shared/SapFunctionalLocations/_functional_locations.cshtml", null);
            }
        }

        //_sapPmFunctionalLocations
        [HttpGet]
        [Route("/config/functionallocations/search")]
        public async Task<IActionResult> SearchFunctionalLocations([FromQuery] string filterText = null)
        {
            var applicationSettings = await GetApplicationSettings();
            var locationsEndpoint = "/v1/locations/search";

            var uriParams = new List<string>();

            if (!string.IsNullOrEmpty(filterText))
            {
                uriParams.Add("searchtext=" + filterText);
            }
            else
            {
                return BadRequest("Please supply a functional location id");
            }

            locationsEndpoint += "?" + string.Join("&", uriParams);

            var locationsResult = await _connector.GetCall(locationsEndpoint);

            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                var locations = JsonConvert.DeserializeObject<List<SapPmLocation>>(locationsResult.Message);
                var output = new List<SapPmLocationViewModel>();
                foreach (var location in locations)
                {
                    output.Add(new SapPmLocationViewModel()
                    {
                        SapPmFunctionalLocation = location,
                        ApplicationSettings = applicationSettings,
                        IndentationLevel = 1
                    });
                }
                return PartialView("~/Views/Shared/SapFunctionalLocations/_functional_locations.cshtml", output);
            }
            else
            {
                return PartialView("~/Views/Shared/SapFunctionalLocations/_functional_locations.cshtml", null);
            }
        }

        #endregion

        #region - Area Org Chart -

        public async Task<JsonResult> Read()
        {
            ConfigViewModel output = new ConfigViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.CONFIG; //TODO only init when needed, move to methods that need it
            output.PageTitle = "Company configuration";
            output.Locale = _locale;

            var companyResult = _connector.GetCall(Logic.Constants.Company.GetCompanyWithShifts).GetAwaiter().GetResult();
            output.Company = JsonConvert.DeserializeObject<CompanyModel>(companyResult.Message);
            var applicationSettings = await this.GetApplicationSettings();

            string endpoint = Logic.Constants.Task.GetTaskAreasFlatten;
            var arearesult = await _connector.GetCall(endpoint);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }
            output.Areas.Insert(0, new Area
            {
                Id = 1,
                ParentId = 0,
                Name = output.Company.Name,
                FullDisplayName = "Company",
                Picture = output.Company.Picture,
            });
            var nodes = output.Areas.Select(p => new NodeModel
            {
                id = p.Id,
                pid = (p.ParentId == null ? 1 : p.ParentId),
                area = p.Name,
                location = p.FullDisplayName,
                location_shortened = p.FullDisplayName.Length > 40 ? string.Concat(p.FullDisplayName.Substring(0, 37), "...") : p.FullDisplayName,
                img = (string.IsNullOrEmpty(p.Picture) ? "/assets/img/normal_unavailable_image.png" : WebApp.Helpers.MediaHelpers.GetMediaImageUrl(applicationSettings, p.Picture)),
                tags = new List<string> { p.Id == 1 ? "rootMenu" : "" },
                funcLocationId = p.SapPmLocation?.Id,
                funcLocationName = p.SapPmLocation?.FunctionalLocationName,
                funcLocation = p.SapPmLocation?.FunctionalLocation,
            });

            return Json(new { nodes = nodes });
        }

        [HttpGet]
        public async Task<IActionResult> AreaRelationNumbers(int id)
        {
            if (id > 0)
            {
                var areaRelations = await GetAreaNumberActiveRelationsAsync(areaid: id);
                if (areaRelations != null)
                {
                    return StatusCode((int)HttpStatusCode.OK, areaRelations.ToJsonFromObject());
                }
                else
                {

                    return StatusCode((int)HttpStatusCode.OK, new EZGO.Api.Models.Relations.AreaActiveRelations().ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        public async Task<IActionResult> Areas(int? id)
        {
            List<Area> output = new List<Area>();
            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                //TODO log somewhere
                output = new List<Area>();
            }

            if (id != null)
            {
                output = GetAreaChildren(output, id);
            }

            return View("~/Views/Config/_workingareas.cshtml", output);
        }

        [NonAction]
        private List<Area> GetAreaChildren(List<Area> area, int? id)
        {
            List<Area> output = new List<Area>();
            foreach (Area item in area)
            {
                if (item.ParentId == id)
                {
                    output.Add(item);
                }

                output.AddRange(GetAreaChildren(item.Children, id));
            }
            return output;
        }

        /// <summary>
        /// GetAreaHasActiveRelationsAsync; Get area relations
        /// </summary>
        /// <param name="areaid"></param>
        /// <returns></returns>
        [NonAction]
        private async Task<EZGO.Api.Models.Relations.AreaActiveRelations> GetAreaHasActiveRelationsAsync(int areaid)
        {
            var result = await _connector.GetCall(string.Concat("/v1/area/relations/check/", areaid));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                EZGO.Api.Models.Relations.AreaActiveRelations relations = JsonConvert.DeserializeObject<EZGO.Api.Models.Relations.AreaActiveRelations>(result.Message);
                return relations;
            }

            return null;
        }

        /// <summary>
        /// GetAreaNumberActiveRelationsAsync; Get area relations
        /// </summary>
        /// <param name="areaid"></param>
        /// <returns></returns>
        [NonAction]
        private async Task<EZGO.Api.Models.Relations.AreaActiveRelations> GetAreaNumberActiveRelationsAsync(int areaid)
        {
            var result = await _connector.GetCall(string.Concat("/v1/area/relations/number/", areaid));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                EZGO.Api.Models.Relations.AreaActiveRelations relations = JsonConvert.DeserializeObject<EZGO.Api.Models.Relations.AreaActiveRelations>(result.Message);
                return relations;
            }

            return null;
        }

        /// <summary>
        /// CheckArea; Check area if valid for posting to API;
        /// </summary>
        /// <param name="area">Area that needs to be checked.</param>
        /// <returns></returns>
        [NonAction]
        private async Task<bool> CheckArea(Area area)
        {
            var output = true;
            if (area != null)
            {
                if (string.IsNullOrEmpty(area.Name))
                {
                    output = false;
                }

                if (area.ParentId.HasValue && area.ParentId.Value <= 0)
                {
                    output = false;
                }
            }
            else
            {
                output = false;
            }

            await Task.CompletedTask;

            return output;
        }

        /// <summary>
        /// DetermineAndSetDefaultsArea; Fill default values if needed.
        /// </summary>
        /// <param name="area">Area to be checked and filled.</param>
        /// <returns>Area containing defaults.</returns>
        [NonAction]
        private async Task<Area> DetermineAndSetDefaultsArea(Area area)
        {
            //NOTE most settings are handled by the API (e.g. level etc.)
            if (area.CompanyId <= 0) area.CompanyId = User.GetProfile().Company.Id;
            await Task.CompletedTask;
            return area;
        }
        #endregion

        #region - shifts -
        // GET: /<controller>/
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
        public async Task<IActionResult> Shifts()
        {
            ConfigViewModel output = new ConfigViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.CONFIG; //TODO only init when needed, move to methods that need it
            output.PageTitle = "Days, times and shifts";
            output.Locale = _locale;

            var companyResult = await _connector.GetCall(Logic.Constants.Company.GetCompanyWithShifts);
            output.Company = JsonConvert.DeserializeObject<CompanyModel>(companyResult.Message);

            string endpoint = Logic.Constants.Task.GetTaskAreasFlatten;
            var arearesult = await _connector.GetCall(endpoint);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }

            output.DisableMutateShifts = _configurationHelper.GetValueAsBool("AppSettings:DisableMutateShifts");
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            // current user
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            UserProfile currentUser = null;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                currentUser = JsonConvert.DeserializeObject<UserProfile>(userprofile);
                if (currentUser != null && currentUser.IsServiceAccount)
                {
                    return PartialView(output);
                }
                else
                {
                    return PartialView("~/Views/Config/ShiftsViewer.cshtml", output);
                }
            }
            return PartialView("~/Views/Config/ShiftsViewer.cshtml", output);
        }

        /// <summary>
        /// PostShifts; handle shifts
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
        public async Task<IActionResult> PostShifts([FromBody] ShiftsViewModel shifts)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:DisableMutateShifts"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Saving disabled".ToJsonFromObject());
            }

            if (shifts != null)
            {
                //TODO move to manager or related logic
                //TODO add urls to constants

                if (_configurationHelper.GetValueAsBool("AppSettings:EnableNewShifts"))
                {
                    if (shifts.AddedShifts != null && shifts.AddedShifts.Any())
                    {
                        foreach (var shift in shifts.AddedShifts)
                        {
                            await _connector.PostCall("/v1/shift/add", shift.ToJsonFromObject());
                        }
                    }
                }

                if (_configurationHelper.GetValueAsBool("AppSettings:EnableChangeShifts"))
                {
                    if (shifts.ChangedShifts != null && shifts.ChangedShifts.Any())
                    {
                        foreach (var shift in shifts.ChangedShifts)
                        {
                            if (shift.Id > 0)
                            {
                                await _connector.PostCall(string.Concat("/v1/shift/change/", shift.Id), shift.ToJsonFromObject());
                            }
                        }
                    }
                }

                if (_configurationHelper.GetValueAsBool("AppSettings:EnableDeleteShifts"))
                {
                    if (shifts.RemovedShifts != null && shifts.RemovedShifts.Any())
                    {
                        foreach (var shift in shifts.RemovedShifts)
                        {
                            if (shift.Id > 0)
                            {
                                await _connector.PostCall(string.Concat("/v1/shift/setactive/", shift.Id), false.ToJsonFromObject());
                            }
                        }
                    }
                }

                return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }


        }

        #endregion

        public IActionResult CompanyArea()
        {
            return PartialView("_workingareas_chart");
        }


    }
}
