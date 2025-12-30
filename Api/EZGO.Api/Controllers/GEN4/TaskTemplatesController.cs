using Elastic.Apm.Api;
using Elastic.Apm;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Security;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Security.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using EZGO.Api.Utils.Json;

namespace EZGO.Api.Controllers.GEN4
{
    /// <summary>
    /// TasksController; contains all routes based on tasks.
    /// Can be used for GEN4 or new/optimized implementations on existing clients.
    /// </summary>
    [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    [Route(ApiSettings.VERSION_GEN4_BASE_API_ROUTE)]
    [ApiController]
    public class TaskTemplatesController : BaseController<TasksController>
    {
        #region - privates -
        private readonly ITaskManager _manager;
        #endregion

        #region - contructor(s) -
        public TaskTemplatesController(ITaskManager manager, IConfigurationHelper configurationHelper, ILogger<TasksController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        #region - GET routes tasktemplates -
        [Route("tasktemplates")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplates([FromQuery] string filterText, [FromQuery] string roles, [FromQuery] string recurrencytypes, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] bool? videosAdded, [FromQuery] string? areaids, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] TaskTypeEnum? tasktype, [FromQuery] RoleTypeEnum? role, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] RecurrencyTypeEnum? recurrencytype = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new TaskFilters()
            {
                AreaIds = string.IsNullOrEmpty(areaids) ? null : areaids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                FilterAreaType = filterareatype,
                TaskType = tasktype,
                RecurrencyType = recurrencytype,
                Role = role,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASKTEMPLATES_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded,
                VideosAdded = videosAdded,
                RecurrencyTypes = string.IsNullOrEmpty(recurrencytypes) ? null : recurrencytypes.Split(",").Select(id => (RecurrencyTypeEnum)Convert.ToInt32(id)).ToList(),
            };
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion
    }
}
