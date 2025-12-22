//TODO refactor entire page, its a mess, remove initialization of object in controller, split models, move several data get point to separate methods, remove unused methods, add non action tag to non action methods, add route as attribute.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Audit;
using WebApp.Models.Checklist;
using WebApp.Models.Properties;
using WebApp.Models.Settings;
using WebApp.Models.Shift;
using WebApp.Models.Task;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tags)]
    public class TagsController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public TagsController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("/tags")]
        public async Task<IActionResult> Index()
        {
            var currentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            TagGroupsViewModel output = new TagGroupsViewModel();

            if (currentUser.IsTagManager == true)
            {
                output = await GetAllTagGroups();
            }
            else
            {
                output = await GetTagGroups();
            }

            output.NewInboxItemsCount = await GetInboxCount();
            output.CurrentUser = currentUser;
            output.Tags = await GetTags();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Tag group configuration";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.TAGS;
            output.Locale = _locale;
            output.ApplicationSettings = await GetApplicationSettings();
            output.EnableTagGroupTranslation = _configurationHelper.GetValueAsBool("AppSettings:EnableTagGroupTranslation");

            return PartialView(output);
        }

        [NonAction]
        private async Task<TagGroupsViewModel> GetTagGroups()
        {
            var view = new TagGroupsViewModel();
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTagGroups);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var groups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                view.TagGroups = groups;
            }

            return view;
        }

        [NonAction]
        private async Task<TagGroupsViewModel> GetAllTagGroups()
        {
            var view = new TagGroupsViewModel();
            var result = await _connector.GetCall(Logic.Constants.Tags.GetAllTagGroups);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var groups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                view.TagGroups = groups;
            }

            return view;
        }

        [NonAction]
        private async Task<List<Tag>> GetTags()
        {
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTags);
            var tags = new List<Tag>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                tags = JsonConvert.DeserializeObject<List<Tag>>(result.Message);
            }

            return tags;
        }

        [Route("taggroups/change")]
        [HttpPost]
        public async Task<string> UpdateTagGroups(List<TagGroup> tagGroups)
        {
            var url = Logic.Constants.Tags.UpdateTagGroups;
            var jsonTag = tagGroups.ToJsonFromObject();
            var result = await _connector.PostCall(url, jsonTag);

            return result.Message;
        }

        [Route("tags/add")]
        [HttpPost]
        public async Task<string> AddTag(Tag tag)
        {
            var url = Logic.Constants.Tags.AddTag;
            var jsonTag = tag.ToJsonFromObject();
            var result = await _connector.PostCall(url, jsonTag);

            return result.Message;
        }

        [Route("tags/change")]
        [HttpPost]
        public async Task<string> UpdateTag(Tag tag)
        {
            string url = Logic.Constants.Tags.UpdateTag + tag.Id;
            var jsonTag = tag.ToJsonFromObject();
            var result = await _connector.PostCall(url, jsonTag);

            return result.Message;
        }

        [Route("tag/delete")]
        [HttpPost]
        public async Task<string> DeleteTag(Tag tag)
        {
            string url = Logic.Constants.Tags.DeleteTag + tag.Id;
            var result = await _connector.PostCall(url, "false");

            return result.Message;
        }
    }
}
