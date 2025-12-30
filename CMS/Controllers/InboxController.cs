//TODO refactor entire page, its a mess, remove initialization of object in controller, split models, move several data get point to separate methods, remove unused methods, add non action tag to non action methods, add route as attribute.
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.TemplateSharing)]
    public class InboxController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public InboxController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("/inbox")]
        public async Task<IActionResult> Index()
        {
            var output = await GetInboxItems();
            var currentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CurrentUser = currentUser;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Inbox";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.TAGS;
            output.Locale = _locale;
            output.ApplicationSettings = await GetApplicationSettings();

            foreach (var item in output.InboxItems)
            {
                item.TemplateType = item.Type switch
                {
                    ObjectTypeEnum.ChecklistTemplate => output.CmsLanguage?.GetValue(LanguageKeys.Checklist.ChecklistTemplateLabel, "checklist template") ?? "checklist template",
                    ObjectTypeEnum.TaskTemplate => output.CmsLanguage?.GetValue(LanguageKeys.Task.TaskTemplateLabel, "task template") ?? "task template",
                    ObjectTypeEnum.AuditTemplate => output.CmsLanguage?.GetValue(LanguageKeys.Audit.AuditTemplateLabel, "audit template") ?? "audit template",
                    ObjectTypeEnum.WorkInstructionTemplate => output.CmsLanguage?.GetValue(LanguageKeys.WorkInsctruction.WorkInsctructionLabel, "workinstruction") ?? "workinstruction",
                    ObjectTypeEnum.AssessmentTemplate => output.CmsLanguage?.GetValue(LanguageKeys.Skills.AssessmentTemplateLabel, "assessment template") ?? "assessment template",
                    _ => "unknown object",//should never happen
                };
            }

            return View(output);
        }

        [NonAction]
        private async Task<InboxViewModel> GetInboxItems()
        {
            var view = new InboxViewModel();
            var result = await _connector.GetCall(Constants.SharedTemplates.GetInboxItems);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var inboxItems = JsonConvert.DeserializeObject<List<InboxItemViewModel>>(result.Message);
                view.InboxItems = inboxItems;
            }

            return view;
        }


        [HttpPost]
        [Route("/inbox/delete/{id}")]
        public async Task<ActionResult> Reject(int id)
        {
            var endpoint = string.Format(Logic.Constants.Checklist.PostDeleteSharedChecklist, id);
            var result = await _connector.PostCall(endpoint, "false");
            return RedirectToAction("index", "inbox");
        }
    }
}
