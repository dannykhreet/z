using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class SearchController : BaseController
    {
        #region - privates / constants -
        public const int MAX_NR_OF_DYNAMIC_ITEMS = 10;
        public const int START_NR_OF_ITEMS = 10;

        private readonly ILogger<SearchController> _logger;
        private readonly IApiConnector _connector;
        private readonly IHttpContextAccessor _context;
        #endregion

        #region - constructor(s) -
        public SearchController(ILogger<SearchController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _context = httpContextAccessor;
        }
        #endregion

        [Route("/search")]
        public async Task<IActionResult> Search()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, "{result}".ToJsonFromObject());
        }

        [Route("/search/tasktemplates")]
        public async Task<IActionResult> SearchTaskTemplates()
        {
            var output = new SearchResultsViewModel();
            output.DetailsUrlPart = "task";

            string uri = Logic.Constants.Search.SearchTaskTemplatsUrl;
            output.SearchResults = await GetSearchResultsAsync(uri);

            return PartialView("~/Views/Search/_search_overview_results.cshtml", output);
        }

        [Route("/search/checklisttemplates")]
        public async Task<IActionResult> SearchChecklistTemplates()
        {
            var output = new SearchResultsViewModel();
            output.DetailsUrlPart = "checklist";

            string uri = Logic.Constants.Search.SearchChecklistTemplatsUrl;
            output.SearchResults = await GetSearchResultsAsync(uri);

            return PartialView("~/Views/Search/_search_overview_results.cshtml", output);
        }

        [Route("/search/audittemplates")]
        public async Task<IActionResult> SearchAuditTemplates()
        {
            var output = new SearchResultsViewModel();
            output.DetailsUrlPart = "audit";

            string uri = Logic.Constants.Search.SearchAuditTemplatsUrl;
            output.SearchResults = await GetSearchResultsAsync(uri);

            return PartialView("~/Views/Search/_search_overview_results.cshtml", output);
        }

        [Route("/search/workinstructiontemplates")]
        public async Task<IActionResult> SearchWorkInstructionTemplates()
        {
            var output = new SearchResultsViewModel();
            output.DetailsUrlPart = "audit";

            string uri = Logic.Constants.Search.SearchWorkInstructionTemplatsUrl;
            output.SearchResults = await GetSearchResultsAsync(uri);

            return PartialView("~/Views/Search/_search_overview_results.cshtml", output);
        }

        private async Task<List<EZGO.Api.Models.Search.SearchResult>> GetSearchResultsAsync(string uri)
        {
            var output = new List<EZGO.Api.Models.Search.SearchResult>();
            var searchResult = await _connector.GetCall(uri);
            if (searchResult.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    output = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Search.SearchResult>>(searchResult.Message);
                }
                catch
                {
                    //TODO log somewhere
                    output = new List<EZGO.Api.Models.Search.SearchResult>(); //reset to empty collection if something goes wrong.
                }
            }
            return output;
        }
    }
}
