using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Provisioner;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models.Provisioner;
using EZGO.Api.Models.Versions;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;


namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ProvisionerController : BaseController<ProvisionerController>
    {
        private readonly IProvisionerManager _manager;

        public ProvisionerController(IProvisionerManager provisionerManager, ILogger<ProvisionerController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = provisionerManager;
        }

        [Route("provisioner/ezgo")]
        [HttpPost]
        public async Task<IActionResult> PostProvisionerEzgo([FromBody] string contentData)
        {
            if(string.IsNullOrEmpty(contentData))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid content".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _manager.Provision(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), type:"ezgo", content:contentData); //return content now, now processing yet.

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [Route("provisioner/atoss")]
        [HttpPost]
        public async Task<IActionResult> PostProvisionerAtoss([FromBody] string contentData)
        {
            if (string.IsNullOrEmpty(contentData))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid content".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _manager.Provision(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), type: "atoss", content: contentData); //return content now, now processing yet.

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }
    }
}

