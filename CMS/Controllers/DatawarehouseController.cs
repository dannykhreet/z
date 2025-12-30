using EZGO.Api.Models.Tools;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Data;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;
using System.Threading.Tasks;
using EZGO.Api.Models;
using System.Net;
using System.Runtime.InteropServices;
using EZ.Api.DataWarehouse.Models;
using System.Linq;
using System.Text;
using Humanizer;
using EZGO.Api.Models.Stats;

namespace WebApp.Controllers
{
    public class DatawarehouseController : BaseController
    {
        private readonly ILogger<DatawarehouseController> _logger;
        private readonly IApiConnector _connector;
        private readonly IApiDatawarehouseConnector _datawarehouseConnector;
        private readonly ILanguageService _languageService;
    //    private readonly IConfigurationHelper _configurationHelper;

        public DatawarehouseController(ILogger<DatawarehouseController> logger, IApiConnector connector, IApiDatawarehouseConnector datawarehouseConnector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _datawarehouseConnector = datawarehouseConnector;
            _languageService = language;
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse")]
        public async Task<IActionResult> Index()
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var output = new DatawarehouseViewModel();

                    output.IsAdminCompany = this.IsAdminCompany;
                    output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                    output.Filter.Module = FilterViewModel.ApplicationModules.DATAWAREHOUSE;
                    output.ApplicationSettings = await this.GetApplicationSettings();

                    return View("~/Views/Datawarehouse/Index.cshtml", output);
                }
            } 

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/tools")]
        public async Task<IActionResult> IndexTools()
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var output = new DatawarehouseToolsViewModel();

                    output.IsAdminCompany = this.IsAdminCompany;
                    output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                    output.Filter.Module = FilterViewModel.ApplicationModules.DATAWAREHOUSE;
                    output.ApplicationSettings = await this.GetApplicationSettings();

                    var companiesResult = await _connector.GetCall(@"/v1/companies");
                    if (companiesResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                    }

                    var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings");
                    if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                    }


                    return View("~/Views/Datawarehouse/IndexTools.cshtml", output);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/tools/import")]
        public async Task<IActionResult> IndexToolsImport()
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var output = new DatawarehouseToolsViewModel();

                    output.IsAdminCompany = this.IsAdminCompany;
                    output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                    output.Filter.Module = FilterViewModel.ApplicationModules.DATAWAREHOUSE;
                    output.ApplicationSettings = await this.GetApplicationSettings();

                    var companiesResult = await _connector.GetCall(@"/v1/companies");
                    if (companiesResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                    }

                    var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings");
                    if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                    }


                    return View("~/Views/Datawarehouse/ToolsDataImport.cshtml", output);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/tools/statistics")]
        public async Task<IActionResult> IndexToolsStatistics()
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var output = new DatawarehouseToolsViewModel();

                    output.IsAdminCompany = this.IsAdminCompany;
                    output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                    output.Filter.Module = FilterViewModel.ApplicationModules.DATAWAREHOUSE;
                    output.ApplicationSettings = await this.GetApplicationSettings();

                    var companiesResult = await _connector.GetCall(@"/v1/companies");
                    if (companiesResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
                    }

                    var companiesHoldingsResult = await _connector.GetCall(@"/v1/company/holdings");
                    if (companiesHoldingsResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.Holdings = companiesHoldingsResult.Message.ToObjectFromJson<List<Holding>>();
                    }


                    return View("~/Views/Datawarehouse/ToolsStatistics.cshtml", output);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/useroverview")]
        public async Task<IActionResult> UserOverview([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if(_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var output = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;

                    var usersResult = await _datawarehouseConnector.GetCall(@"/data/management/users", username: output.UserName, password: output.Password, appid: output.AppId);
                    if (usersResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        output.Users = usersResult.Message.ToObjectFromJson<List<EZ.Api.DataWarehouse.Models.User>>();
                        output.Users = output.Users.OrderBy(x => x.Username).ToList();
                    }

                    return PartialView("~/Views/Datawarehouse/_users.cshtml", output);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/user/{holdingid}/{companyid}")]
        public async Task<IActionResult> GetUserForDetails(int holdingid, int companyid)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    //build input for retrieval of user
                    var input = new EZ.Api.DataWarehouse.Models.User();
                    input.CompanyId = holdingid;
                    input.HoldingId = companyid;
                    //possible output
                    var possibleOutput = new EZ.Api.DataWarehouse.Models.User();

                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/user", input.ToJsonFromObject(),username: _configurationHelper.GetValueAsString("DW_USER"), password: _configurationHelper.GetValueAsString("DW_PWD"), appid: _configurationHelper.GetValueAsString("DW_APPID"));
                    if (usersResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        possibleOutput = usersResult.Message.ToObjectFromJson<EZ.Api.DataWarehouse.Models.User>();
                    } else
                    {
                        return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                    }

                    return StatusCode((int)HttpStatusCode.OK, possibleOutput.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/setip")]
        public async Task<IActionResult> SetIP([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/setuseripranges", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/setmodules")]
        public async Task<IActionResult> SetModules([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/setusermodules", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/setbikey")]
        public async Task<IActionResult> SetBiKey([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/setuserbikey", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/setversion")]
        public async Task<IActionResult> SetVersion([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/setuserversion", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/createuser")]
        public async Task<IActionResult> CreateUser([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/createuser", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/updateuser")]
        public async Task<IActionResult> UpdateUser([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/updateuser", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/updateuser/all")]
        public async Task<IActionResult> UpdateUserAll([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {

                    var dwUserCollectionViewModel = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;

                    var usersResult = await _datawarehouseConnector.GetCall(@"/data/management/users", username: dwUserCollectionViewModel.UserName, password: dwUserCollectionViewModel.Password, appid: dwUserCollectionViewModel.AppId);
                    if (usersResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        dwUserCollectionViewModel.Users = usersResult.Message.ToObjectFromJson<List<EZ.Api.DataWarehouse.Models.User>>();
                        dwUserCollectionViewModel.Users = dwUserCollectionViewModel.Users.OrderBy(x => x.Username).ToList();
                    }

                    var output = new StringBuilder();

                    foreach(var dwUser in dwUserCollectionViewModel.Users)
                    {

                        var usersUpdateResult = await _datawarehouseConnector.PostCall(@"/data/management/updateuser", dwUser.ToJsonFromObject(), username: dwUserCollectionViewModel.UserName, password: dwUserCollectionViewModel.Password, appid: dwUserCollectionViewModel.AppId);
                        if(usersUpdateResult.StatusCode == HttpStatusCode.OK)
                        {
                            output.AppendLine(string.Format("Updated: H{0}-C{1} [{2}]", dwUser.HoldingId, dwUser.CompanyId, usersUpdateResult.Message));
                        } else
                        {
                            output.AppendLine(string.Format("Not Updated: H{0}-C{1} [{2}]", dwUser.HoldingId, dwUser.CompanyId, usersUpdateResult.Message));
                        }
                    }

                    return StatusCode((int)HttpStatusCode.OK, output.ToString().ToJsonFromObject());

                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }
        //management/updateuser

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/dropuser")]
        public async Task<IActionResult> DropUser([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/dropuser", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/generatecreateuser")]
        public async Task<IActionResult> GenerateCreateUser([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;

                    DatawarehouseCustomer dwCustomer = new DatawarehouseCustomer();
                    if(incoming.User.Id > 0)
                    {
                        dwCustomer.DeterminedId = incoming.User.Id;
                    }
                    dwCustomer.Username = incoming.User.Username;
                    dwCustomer.Password = incoming.User.Password;
                    dwCustomer.AppId = incoming.User.AppId;
                    dwCustomer.IpValidationList = incoming.User.IpValidationList;
                    dwCustomer.Modules = incoming.User.Modules;
                    dwCustomer.CompanyId = incoming.User.CompanyId.Value;
                    dwCustomer.HoldingId = incoming.User.HoldingId.Value;
                    dwCustomer.Version = incoming.User.Version;
                    dwCustomer.BIKey = incoming.User.BIKey;

                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/generate/newcustomer", dwCustomer.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }


        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/generateupdateuser")]
        public async Task<IActionResult> GenerateUpdateUser([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;

                    DatawarehouseCustomer dwCustomer = new DatawarehouseCustomer();
                    if (incoming.User.Id > 0)
                    {
                        dwCustomer.DeterminedId = incoming.User.Id;
                    }
                    dwCustomer.Username = incoming.User.Username;
                    dwCustomer.Password = incoming.User.Password;
                    dwCustomer.AppId = incoming.User.AppId;
                    dwCustomer.IpValidationList = incoming.User.IpValidationList;
                    dwCustomer.Modules = incoming.User.Modules;
                    dwCustomer.CompanyId = incoming.User.CompanyId.Value;
                    dwCustomer.HoldingId = incoming.User.HoldingId.Value;

                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/generate/updatecustomer", dwCustomer.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/generatedropuser")]
        public async Task<IActionResult> GenerateDropUser([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;

                    DatawarehouseCustomer dwCustomer = new DatawarehouseCustomer();
                    if (incoming.User.Id != 0)
                    {
                        dwCustomer.DeterminedId = incoming.User.Id;
                    }
                    dwCustomer.Username = incoming.User.Username;
                    dwCustomer.Password = incoming.User.Password;
                    dwCustomer.AppId = incoming.User.AppId;
                    dwCustomer.IpValidationList = incoming.User.IpValidationList;
                    dwCustomer.Modules = incoming.User.Modules;
                    dwCustomer.CompanyId = incoming.User.CompanyId.Value;
                    dwCustomer.HoldingId = incoming.User.HoldingId.Value;

                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/generate/dropcustomer", dwCustomer.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/bulk/base")]
        public async Task<IActionResult> SetBulkModified([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _connector.PostCall(string.Format("/v1/tools/bulk/base/{0}/{1}", incoming.User.HoldingId, incoming.User.CompanyId), string.Empty);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/userstructures")]
        public async Task<IActionResult> CheckUserStructures([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/userstructures", incoming.User.ToJsonFromObject(), username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToString());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/migrations/retrieve")]
        public async Task<IActionResult> MigrationsRetrieve([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/migrations/retrieve", "", username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);        
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToString());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/migrations/execute")]
        public async Task<IActionResult> MigrationsExecute([FromBody] DatawarehouseViewModel datawarehouseViewModel)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var incoming = datawarehouseViewModel == null ? new DatawarehouseViewModel() : datawarehouseViewModel;
                    var usersResult = await _datawarehouseConnector.PostCall(@"/data/management/migrations/execute", "", username: incoming.UserName, password: incoming.Password, appid: incoming.AppId);
                    return StatusCode((int)usersResult.StatusCode, usersResult.Message.ToString());
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/migrations/manual")]
        public async Task<IActionResult> MigrationsManaulImportExecute([FromBody] AutomatedDataFilter datafilter)
        {
            
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    if(datafilter != null)
                    {
                        if(string.IsNullOrEmpty(datafilter.ProcedureName))
                        {
                            /*
                             * not used, no date dependence.
                                    "export_data_dw_area_overview",
                                    "export_data_dw_shift_overview",
                                    "export_data_dw_company_overview",
                                    "export_data_dw_tag_overview",
                                    "export_data_dw_tag_items_overview",
                             
                             */
                            string[] procedures = {"export_data_dw_audit_items_actions_overview",
                                                        "export_data_dw_audit_items_comments_overview",
                                                        "export_data_dw_audit_items_overview",
                                                        "export_data_dw_audit_items_properties_overview",
                                                        "export_data_dw_audit_openfields_properties_overview",
                                                        "export_data_dw_audit_overview",
                                                        "export_data_dw_checklist_items_actions_overview",
                                                        "export_data_dw_checklist_items_comments_overview",
                                                        "export_data_dw_checklist_items_overview",
                                                        "export_data_dw_checklist_items_properties_overview",
                                                        "export_data_dw_checklist_openfields_properties_overview",
                                                        "export_data_dw_checklist_overview",
                                                        "export_data_dw_task_actions_overview",
                                                        "export_data_dw_task_comments_overview",
                                                        "export_data_dw_task_overview",
                                                        "export_data_dw_task_properties_overview",
                                                        "export_data_dw_action_overview",
                                                        "export_data_dw_comment_overview",
                                                        "export_data_dw_assessment_overview",
                                                        "export_data_dw_assessment_instruction_overview",
                                                        "export_data_dw_assessment_instruction_items_overview",
                                                        "export_data_dw_action_tags_overview",
                                                        "export_data_dw_comment_tags_overview",
                                                        "export_data_dw_checklist_items_tags_overview",
                                                        "export_data_dw_checklist_tags_overview",
                                                        "export_data_dw_audit_items_tags_overview",
                                                        "export_data_dw_audit_tags_overview",
                                                        "export_data_dw_task_tags_overview",
                                                        "export_data_dw_assessment_tags_overview",
                                                        "export_data_dw_assessment_instruction_tags_overview",
                                                        "export_data_dw_assessment_instruction_items_tags_overview",
                                                        "export_data_dw_pictureproof_overview",
                                                        "export_data_dw_checklist_items_pictureproof_overview",
                                                        "export_data_dw_audit_items_pictureproof_overview",
                                                        "export_data_dw_task_pictureproof_overview",
                                                        "export_data_dw_task_linked_overview"};

                            StringBuilder messages = new StringBuilder();

                            foreach(var sp in procedures)
                            {
                                datafilter.ProcedureName = sp;
                                var dataResult = await _connector.PostCall("/v1/export/datawarehouse/manual", datafilter.ToJsonFromObject());
                                messages.AppendLine(string.Format("{0}: {1}[{2}]", sp, dataResult.StatusCode.ToString(), dataResult.Message.ToString()));
                            }
                            return StatusCode((int)HttpStatusCode.OK, messages.ToString());
                        } else
                        {
                            var dataResult = await _connector.PostCall("/v1/export/datawarehouse/manual", datafilter.ToJsonFromObject());
                            return StatusCode((int)dataResult.StatusCode, dataResult.Message.ToString());
                        }
                     
                    } else
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, "Filter not valid".ToJsonFromObject());
                    }
                   
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpGet]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/datawarehouse/statistics/{holdingid}/{companyid}/{statreference}")]
        public async Task<IActionResult> RetrieveStatisticsDW([FromRoute] string statreference, [FromRoute] int holdingid, [FromRoute] int companyid)
        {

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDatawarehouseManagement"))
            {
                if (this.IsAdminCompany)
                {
                    var results = new List<StatisticsData>();

                    if (statreference.Contains(","))
                    {
                        foreach(string statref in statreference.Split(","))
                        {
                            var result = await _connector.GetCall($"/v1/statistics/datawarehouse/{holdingid}/{companyid}/{statref}");
                            results.Add(result.Message.ToObjectFromJson<StatisticsData>());
                        }
                    } else
                    {
                        //statistics/datawarehouse/{holdingid}/{companyid}/{statreference}
                        var result = await _connector.GetCall($"/v1/statistics/datawarehouse/{holdingid}/{companyid}/{statreference}");
                        results.Add(result.Message.ToObjectFromJson<StatisticsData>());
                    }

                    return StatusCode(200, results);

                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        #region - direct mutations from company / holding -
        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/createdatawarehouseuser")]
        [Route("/holding/createdatawarehouseuser")]
        public async Task<IActionResult> CreateDatawarehouseUser([FromBody] EZ.Api.DataWarehouse.Models.User dwUser)
        {
            if (IsAdminCompany)
            {
                if (dwUser != null && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_USER")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_PWD")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_APPID")))
                {
                    var result = await _datawarehouseConnector.PostCall(@"/data/management/creategenerateuser", dwUser.ToJsonFromObject(), username: _configurationHelper.GetValueAsString("DW_USER"), password: _configurationHelper.GetValueAsString("DW_PWD"), appid: _configurationHelper.GetValueAsString("DW_APPID"));
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
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [HttpPost]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/company/deletedatawarehouseuser")]
        [Route("/holding/deletedatawarehouseuser")]
        public async Task<IActionResult> RemoveDatawarehouseUser([FromBody] EZ.Api.DataWarehouse.Models.User dwUser)
        {
            if (IsAdminCompany)
            {
                if (dwUser != null && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_USER")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_PWD")) && !string.IsNullOrEmpty(_configurationHelper.GetValueAsString("DW_APPID")))
                {
                    var result = await _datawarehouseConnector.PostCall(@"/data/management/setuserinactive", dwUser.ToJsonFromObject(), username: _configurationHelper.GetValueAsString("DW_USER"), password: _configurationHelper.GetValueAsString("DW_PWD"), appid: _configurationHelper.GetValueAsString("DW_APPID"));
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
                    return StatusCode((int)HttpStatusCode.NoContent);
                }
            }

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }
        #endregion

    }

}
