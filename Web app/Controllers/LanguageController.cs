using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EZGO.Api.Models.Stats;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using EZGO.CMS.LIB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Helpers;
using WebApp.Logic.Interfaces;
using WebApp.Models.Language;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    /// <summary>
    /// LanguageController; Updating keys (language keys)
    /// For adding new flags to the system (normally only active language have a flag displayed on overview and details use: 
    ///     https://en.wikipedia.org/wiki/Gallery_of_sovereign_state_flags
    ///     Save -> Open in editor -> Resize to 640px with (height set to scaling based on the width) -> Save as PNG, use culture short as name. 
    /// </summary>
    public class LanguageController : BaseController
    {
        private readonly ILogger<LanguageController> _logger;
        private readonly IApiConnector _connector;
        LanguageViewModel output;

        public LanguageController(ILogger<LanguageController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            output = new LanguageViewModel();
            output.CmsLanguage = language.GetLanguageDictionaryAsync(_locale).Result;
            output.PageTitle = "Languages";
            output.Filter.Module = FilterViewModel.ApplicationModules.APPLANGUAGES;
            output.IsAdminCompany = this.IsAdminCompany;

        }

        //// GET: /<controller>/
        //public async Task<IActionResult> Index(string locale = "en-GB")
        //{
        //    var endpoint = string.Format(Logic.Constants.Language.GetLanguageKeys, locale);
        //    var result = await _connector.GetCall(endpoint);
        //    if (result.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        output.Language = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
        //        output.Language.ResourceItems = output.Language.ResourceItems.OrderBy(x => x.ResourceKey).ToList();
        //    }

        //    output.ApplicationSettings = await this.GetApplicationSettings();
        //    return View(output);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index(LanguageViewModel input)
        //{
        //    if (input == null || !ModelState.IsValid)
        //    {
        //        RedirectToAction("Index");
        //    }
        //    else
        //    {


        //    }

        //    output = input;

        //    await Task.CompletedTask;

        //    output.ApplicationSettings = await this.GetApplicationSettings();
        //    return View(output);
        //}

        //public async Task<IActionResult> Keys(string locale = "en-GB")
        //{
        //    var endpoint = string.Format(Logic.Constants.Language.GetLanguageKeys, locale);
        //    var result = await _connector.GetCall(endpoint);
        //    if (result.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        output.Language = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
        //        output.Language.ResourceItems = output.Language.ResourceItems.OrderBy(x => x.ResourceKey).ToList();
        //    }

        //    return PartialView("~/Views/CmsLanguage/_language_keys.cshtml", output);
        //}

        //#region Edit details

        //[HttpGet]
        //public async Task<IActionResult> EditResource(string locale, string id)
        //{
        //    LanguageResourceItemModel output = new LanguageResourceItemModel();

        //    var endpoint = string.Format(Logic.Constants.Language.GetLanguageKeys, locale);
        //    var result = await _connector.GetCall(endpoint);
        //    if (result.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        var myresult = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
        //        if (myresult != null)
        //        {
        //            output = myresult.ResourceItems.FirstOrDefault(x => x.Guid == id);
        //            if (output != null) { output.Locale = locale; }
        //        }
        //    }

        //    return PartialView("~/Views/CmsLanguage/_modal_edit_language_key.cshtml", output);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<LanguageResourceItemModel> EditResource(LanguageResourceItemModel input)//, IFormCollection collection)
        //{
        //    LanguageResourceItemModel model = null;
        //    if (input != null && ModelState.IsValid)
        //    {
        //        if (!string.IsNullOrWhiteSpace(input.Locale))
        //        {
        //            var endpoint = string.Format(Logic.Constants.Language.GetLanguageKeys, input.Locale);
        //            var result = await _connector.GetCall(endpoint);
        //            if (result.StatusCode == System.Net.HttpStatusCode.OK)
        //            {
        //                var myresult = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
        //                if (myresult != null)
        //                {
        //                    var item = myresult.ResourceItems.FirstOrDefault(x => x.Guid == input.Guid);
        //                    if (item != null)
        //                    {
        //                        if (item.Description != input.Description || item.ResourceValue != input.ResourceValue)
        //                        {
        //                            bool success = false;
        //                            if (item.Description != input.Description)
        //                            {
        //                                var postresult = await _connector.PostCall(string.Format(Logic.Constants.Language.UpdateLanguageKeyDescription, item.ResourceKey), input.Description.ToJsonFromObject());
        //                                if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
        //                                {
        //                                    item.Description = input.Description;
        //                                    success = true;
        //                                }
        //                            }
        //                            if (item.ResourceValue != input.ResourceValue && !string.IsNullOrEmpty(input.ResourceValue))
        //                            {
        //                                var postresult = await _connector.PostCall(string.Format(Logic.Constants.Language.UpdateLanguageKeyValue, item.ResourceKey, input.Locale.Replace("-","_").ToLowerInvariant()), input.ResourceValue.ToJsonFromObject());
        //                                if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
        //                                {
        //                                    item.ResourceValue = input.ResourceValue;
        //                                    success = true;
        //                                }
        //                                else
        //                                    success = false;
        //                            }
        //                            model = success ? item : null;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return model;
        //}

        //#endregion

        #region Create key

        //[HttpGet]
        //public async Task<IActionResult> CreateResource(string locale)
        //{
        //    LanguageResourceItemModel output = new LanguageResourceItemModel();

        //    if (!string.IsNullOrWhiteSpace(locale))
        //    {
        //        output = new LanguageResourceItemModel
        //        {
        //            Locale = locale,
        //            Guid = System.Guid.NewGuid().ToString("N")
        //        };
        //    }

        //    await Task.CompletedTask;

        //    return PartialView("~/Views/CmsLanguage/_modal_new_language_key.cshtml", output);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<LanguageResourceItemModel> CreateResource(LanguageResourceItemModel input)
        {
            LanguageResourceItemModel model = null;

            if (input != null && ModelState.IsValid)
            {
                //var postresult = await _connector.PostCall(string.Format(Logic.Constants.Language.CreateLanguageKey, input.ResourceKey), input.Description.ToJsonFromObject());
                //if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
                //{
                //    model = input;
                //}
            }
            await Task.CompletedTask;
            return model;
        }

        #endregion

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("/languages/overview")]
        public async Task<IActionResult> Overview()
        {
            output.Languages = await _language.GetLanguageItems();

            var result = await _connector.GetCall("v1/app/resources/language/statistics");
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Stats = JsonConvert.DeserializeObject<List<StatsItem>>(result.Message);
            } else
            {
                output.Stats = new List<StatsItem>();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/Language/Overview.cshtml", output);
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("/languages/{locale}")]
        [Route("/languages/{locale}/{type}")]
        public async Task<IActionResult> Retrieve([FromRoute]string locale, [FromQuery]string localecompare, [FromRoute] string type)
        {
            if(string.IsNullOrEmpty(type))
            {
                type = "app";
            }

            List<string> languagesToCompare = new List<string>();
            if (!string.IsNullOrEmpty(localecompare)) {
              
                if(localecompare.Contains(","))
                {
                    languagesToCompare = localecompare.Split(',').ToList();
                } else
                {
                    languagesToCompare.Add(localecompare);
                }

            } 
            if (!languagesToCompare.Contains("en-us")) { languagesToCompare.Add("en-us"); }

            var endpoint = string.Format(Logic.Constants.Language.GetLanguageKeys, locale);
            if(type == "cms")
            {
                endpoint = string.Concat(endpoint, "&resourcetype=2");
            }
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.Language = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
                if (output.Language.ResourceItems != null)
                {
                    output.Language.ResourceItems = output.Language.ResourceItems.OrderBy(x => x.ResourceKey).ToList();
                }
                
            }

            output.Languages = await _language.GetLanguageItems();

            if (languagesToCompare != null && languagesToCompare.Any() && output.Languages != null && output.Languages.Any())
            {
                foreach(var language in languagesToCompare)
                {
                    var endpointCompare = string.Format(Logic.Constants.Language.GetLanguageKeys, language);
                    if (type == "cms")
                    {
                        endpointCompare = string.Concat(endpointCompare, "&resourcetype=2");
                    }
                    var resultCompare = await _connector.GetCall(endpointCompare);
                    if (resultCompare.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resultCompare.Message))
                    {
                        var retrievedLanguage = JsonConvert.DeserializeObject<LanguageModel>(resultCompare.Message);
                        if(output.Languages.Where(x => x.LanguageCulture.ToLower() == retrievedLanguage.LanguageCulture.ToLower()).FirstOrDefault() != null)
                        {
                            output.Languages.Where(x => x.LanguageCulture.ToLower() == retrievedLanguage.LanguageCulture.ToLower()).FirstOrDefault().ResourceItems = retrievedLanguage.ResourceItems.OrderBy(x => x.ResourceKey).ToList();
                        }
                    }
                }
               
            }
            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/Language/Details.cshtml", output);
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("/languages/{locale}/{key}/{guid}")]
        public async Task<IActionResult> UpdateKey([FromRoute] string locale, [FromRoute] string key, [FromRoute] string guid, [FromBody] string value)
        {
            var output = true; 

            if(string.IsNullOrEmpty(value))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Value is empty.".ToJsonFromObject());
            }

            var postresult = await _connector.PostCall(string.Format(Logic.Constants.Language.UpdateLanguageKeyValue, key, locale.Replace("-", "_").ToLowerInvariant()), value.ToJsonFromObject());
            if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output = true;
            }
            else
            {
                output = false;
            }
  
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }
    }
}
