using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Models.Language;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;
using System.Net;
using WebApp.Logic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Controllers
{
    public class CmsLanguageController : BaseController
    {
        private readonly ILogger<LanguageController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;


        public CmsLanguageController(ILogger<LanguageController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;


        }

    //    // GET: /<controller>/
    //    public async Task<IActionResult> Index(string locale = null)
    //    {
    //        LanguageViewModel output;
    //        output = new LanguageViewModel();
    //        output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
    //        output.PageTitle = "Languages";
    //        output.Filter.Module = FilterViewModel.ApplicationModules.CMSLANGUAGES;
    //        output.IsAdminCompany = this.IsAdminCompany;

    //        List<SelectListItem> selectorResult = await _language.GetLanguageSelectorItems();
    //        selectorResult ??= new List<SelectListItem>();
    //        output.LanguageSelectorItems = selectorResult;


    //        locale ??= _locale;

    //        var endpoint = string.Format(Logic.Constants.CmsLanguage.GetLanguageKeys, locale.ToLowerInvariant());
    //        var result = await _connector.GetCall(endpoint);
    //        if (result.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            output.Language = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
    //            output.Language.ResourceItems = output.Language.ResourceItems.OrderBy(x => x.ResourceKey).ToList();
    //        }

    //        output.ApplicationSettings = await this.GetApplicationSettings();
    //        return View(output);
    //    }

    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Index(LanguageViewModel input)
    //    {
    //        LanguageViewModel output;

    //        if (input == null || !ModelState.IsValid)
    //        {
    //            RedirectToAction("Index");
    //        }
    //        else
    //        {

    //        }
    //        output = input;
    //        output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
    //        output.PageTitle = "Languages";
    //        output.Filter.Module = FilterViewModel.ApplicationModules.CMSLANGUAGES;
    //        output.IsAdminCompany = this.IsAdminCompany;
    //        output.ApplicationSettings = await this.GetApplicationSettings();
    //        return View(output);
    //    }

    //    public async Task<IActionResult> Keys(string locale)
    //    {
    //        LanguageViewModel output;
    //        output = new LanguageViewModel();
    //        output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
    //        output.PageTitle = "Languages";
    //        output.Filter.Module = FilterViewModel.ApplicationModules.CMSLANGUAGES;
    //        output.IsAdminCompany = this.IsAdminCompany;

    //        var endpoint = string.Format(Logic.Constants.CmsLanguage.GetLanguageKeys, locale);
    //        var result = await _connector.GetCall(endpoint);
    //        if (result.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            output.Language = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
    //            output.Language.ResourceItems = output.Language.ResourceItems.OrderBy(x => x.ResourceKey).ToList();
    //        }

    //        return PartialView("~/Views/CmsLanguage/_language_keys.cshtml", output);
    //    }

    //    #region Edit details

    //    [HttpGet]
    //    public async Task<IActionResult> EditResource(string locale, string id)
    //    {
    //        LanguageResourceItemModel output = new LanguageResourceItemModel();

    //        var endpoint = string.Format(Logic.Constants.CmsLanguage.GetLanguageKeys, locale);
    //        var result = await _connector.GetCall(endpoint);
    //        if (result.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var myresult = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
    //            if (myresult != null)
    //            {
    //                output = myresult.ResourceItems.FirstOrDefault(x => x.Guid == id);
    //                if (output != null) { output.Locale = locale; }
    //            }
    //        }

    //        return PartialView("~/Views/CmsLanguage/_modal_edit_language_key.cshtml", output);
    //    }

    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<LanguageResourceItemModel> EditResource(LanguageResourceItemModel input)//, IFormCollection collection)
    //    {
    //        LanguageResourceItemModel model = null;
    //        if (input != null && ModelState.IsValid)
    //        {
    //            if (!string.IsNullOrWhiteSpace(input.Locale))
    //            {
    //                var endpoint = string.Format(Logic.Constants.CmsLanguage.GetLanguageKeys, input.Locale);
    //                var result = await _connector.GetCall(endpoint);
    //                if (result.StatusCode == System.Net.HttpStatusCode.OK)
    //                {
    //                    var myresult = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
    //                    if (myresult != null)
    //                    {
    //                        var item = myresult.ResourceItems.FirstOrDefault(x => x.Guid == input.Guid);
    //                        if (item != null)
    //                        {
    //                            if (item.Description != input.Description || item.ResourceValue != input.ResourceValue)
    //                            {
    //                                bool successDescription = false;
    //                                bool successValue = false;
    //                                bool descriptionRun = false;
    //                                bool valueRun = false;

    //                                if (item.Description != input.Description)
    //                                {
    //                                    var postresult = await _connector.PostCall(string.Format(Logic.Constants.CmsLanguage.UpdateLanguageKeyDescription, item.ResourceKey), input.Description.ToJsonFromObject());
    //                                    if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
    //                                    {
    //                                        item.Description = input.Description;
    //                                        successDescription = true;

    //                                    }
    //                                    descriptionRun = true;
    //                                }
    //                                if (item.ResourceValue != input.ResourceValue && !string.IsNullOrEmpty(input.ResourceValue))
    //                                {
    //                                    var postresult = await _connector.PostCall(string.Format(Logic.Constants.CmsLanguage.UpdateLanguageKeyValue, item.ResourceKey, input.Locale.Replace("-", "_").ToLowerInvariant()), input.ResourceValue.ToJsonFromObject());
    //                                    if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
    //                                    {
    //                                        item.ResourceValue = input.ResourceValue;
    //                                        successValue = true;
    //                                    }
    //                                    valueRun = true;
    //                                }
    //                                model = successValue && valueRun || successDescription && descriptionRun ? item : null;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        return model;
    //    }

    //    #endregion

    //    #region Create key

    //    [HttpGet]
    //    public async Task<IActionResult> CreateResource(string locale)
    //    {
    //        LanguageResourceItemModel output = new LanguageResourceItemModel();

    //        if (!string.IsNullOrWhiteSpace(locale))
    //        {
    //            output = new LanguageResourceItemModel
    //            {
    //                Locale = locale,
    //                Guid = Guid.NewGuid().ToString("N"),
    //                ResourceKey = "CMS_"
    //            };
    //        }

    //        await Task.CompletedTask;

    //        return PartialView("~/Views/CmsLanguage/_modal_new_language_key.cshtml", output);
    //    }

    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<LanguageResourceItemModel> CreateResource(LanguageResourceItemModel input)
    //    {
    //        LanguageResourceItemModel model = null;

    //        if (input != null && ModelState.IsValid)
    //        {
    //            if (!string.IsNullOrWhiteSpace(input.Locale))
    //            {
    //                input.ResourceKey = input.ResourceKey.ToUpperInvariant();
    //                if (!input.ResourceKey.StartsWith("CMS")) { input.ResourceKey = string.Format("{0}_{1}", "CMS", input.ResourceKey); }
    //                //var postresult = await _connector.PostCall(string.Format(Logic.Constants.CmsLanguage.CreateLanguageKey, input.ResourceKey), input.Description.ToJsonFromObject());
    //                //if (postresult.StatusCode == System.Net.HttpStatusCode.OK)
    //                //{
    //                //    model = input;
    //                //}
    //            }
    //        }
    //        await Task.CompletedTask;
    //        return model;
    //    }

    //    #endregion

    //    #region - tools -
    //    [HttpPost]
    //    [Route("/language/reinit/{locale}")]
    //    public async Task<IActionResult> ReInitLanguageSet(string locale)
    //    {
    //        Dictionary<string, string> language = null;

    //        var endpoint = string.Format(Logic.Constants.CmsLanguage.GetLanguageKeys, locale);
    //        var result = await _connector.GetCall(endpoint);
    //        if (result.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var languageResult = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
    //            language = languageResult.ResourceItems.ToDictionary(r => r.ResourceKey, r => r.ResourceValue);

    //            if (Statics.Languages.ContainsKey(locale))
    //            {
    //                Statics.Languages[locale] = language;
    //            }
    //            else
    //            {
    //                Statics.Languages.Add(locale, language);
    //            }

    //            return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
    //        }

    //        return StatusCode((int)HttpStatusCode.BadRequest, "".ToJsonFromObject());
    //    }

    //    #endregion
    }
}
