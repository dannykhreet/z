using EZGO.Api.Models.Tools;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.Controllers
{
    [AllowAnonymous]
    public class HealthController : Controller
    {

        private readonly ILogger<HealthController> _logger;
        private readonly IApiConnector _connector;
        private readonly IConfigurationHelper _configHelper;


        public HealthController(ILogger<HealthController> logger, IConfigurationHelper configHelper, IApplicationSettingsHelper applicationSettingsHelper,
            IApiConnector connector)
        {
            // DI
            _logger = logger;
            _connector = connector;
            _configHelper = configHelper;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("health")]
        public async Task<IActionResult> GetItWorksCheck()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, "ok".ToJsonFromObject());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("health/api")]
        public async Task<IActionResult> GetItWorksCheckApi()
        {
            var result = await _connector.GetCall("/health");
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message) && result.Message == "true")
            {
                return StatusCode((int)HttpStatusCode.OK, "ok".ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.BadRequest, false.ToJsonFromObject());

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/health/apiversion")]
        public async Task<IActionResult> GetItWorksCheckApiVersion()
        {
            var result = await _connector.GetCall("/health/version");
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)HttpStatusCode.OK, string.Concat(_configHelper.GetValueAsString("AppSettings:ApiUri"), " - ", result.Message).ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.BadRequest, false.ToJsonFromObject());

        }

        //[HttpGet]
        //[Route("/health/routes")]
        //[Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        //public async Task<RootResultModel> GetRoutes()
        //{
            
        //    var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Where(
        //        ad => ad.AttributeRouteInfo != null).Select(ad => new RouteModel
        //        {
        //            Name = ad.AttributeRouteInfo.Template,
        //            Method = ad.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.First(),
        //        }).ToList();

        //    var res = new RootResultModel
        //    {
        //        Routes = routes.OrderBy(x => x.Name).ToList()
        //    };

        //    await Task.CompletedTask;


        //    return res;
        //}

       

        //

    }
}
