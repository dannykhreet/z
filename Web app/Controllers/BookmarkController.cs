using EZGO.Api.Models;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.Controllers
{
    public class BookmarkController : BaseController
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public BookmarkController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("bookmark/createorretrieve/{bookmarkType}/{objectType}/{objectId}")]
        public async Task<IActionResult> CreateOrRetrieve(int bookmarkType, int objectType, int objectId)
        {
            //parameters are bookmarkType, objectType, objectId in this order
            var postUri = string.Format(Logic.Constants.Bookmark.CreateOrRetrieve, bookmarkType, objectType, objectId);
            var bookmarkResult = await _connector.PostCall(postUri, "");
            
            if (bookmarkResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var bookmark = JsonConvert.DeserializeObject<Bookmark>(bookmarkResult.Message);

                return Ok(bookmark);
            }

            return BadRequest();
        }
    }
}
