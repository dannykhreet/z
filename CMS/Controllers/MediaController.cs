using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using System.Net;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class MediaController : BaseController
    {
        private readonly ILogger<MediaController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public MediaController(ILogger<MediaController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        #region - media token -
        [Authorize]
        [HttpGet]
        [Route("/fetchmediatoken")]
        public async Task<IActionResult> FetchMediaToken()
        {
            var response = await _connector.PostCall(url: "/v1/authentication/fetchmediatoken", body:"".ToJsonFromObject());
            if(response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                MediaToken token = response.Message.ToObjectFromJson<MediaToken>();
                if(token != null && !string.IsNullOrEmpty(token.AccessKeyId) && !string.IsNullOrEmpty(token.SecretAccessKey) && !string.IsNullOrEmpty(token.SessionToken))
                {
                    return StatusCode((int)HttpStatusCode.OK, token.ToJsonFromObject());
                }
               
            }

            return StatusCode((int)HttpStatusCode.Unauthorized);
        }
        #endregion
    }
}
