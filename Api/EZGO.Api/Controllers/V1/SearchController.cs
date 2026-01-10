using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class SearchController : BaseController<SearchController>
    {
        private readonly ISearchManager _manager;

        public SearchController(ISearchManager manager, ILogger<SearchController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser,configurationHelper)
        {
            _manager = manager;
        }

        //search/tasktemplates
        [Route("search/tasktemplates")]
        [HttpGet]
        public async Task<IActionResult> SearchTaskTemplates([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.TaskTemplates, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters) ;

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }


        //search/checklisttemplates
        [Route("search/checklisttemplates")]
        [HttpGet]
        public async Task<IActionResult> SearchChecklistTemplates([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.ChecklistTemplates, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //search/audittemplates
        [Route("search/audittemplates")]
        [HttpGet]
        public async Task<IActionResult> SearchAuditTemplates([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.AuditTemplates, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //search/workinstructiontemplates
        [Route("search/workinstructiontemplates")]
        [HttpGet]
        public async Task<IActionResult> SearchWorkInstructionTemplates([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.WorkInstructionTemplates, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //search/assessmenttemplates
        [Route("search/assessmenttemplates")]
        [HttpGet]
        public async Task<IActionResult> SearchAssessmentTemplates([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.AssessmentTemplates, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }


        //search/comments
        [Route("search/comments")]
        [HttpGet]
        public async Task<IActionResult> SearchComments([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.Comments, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //search/actions
        [Route("search/actions")]
        [HttpGet]
        public async Task<IActionResult> SearchActions([FromQuery] string searchvalue, [FromQuery] string sort, [FromQuery] string direction, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters(searchvalue, sort, direction, limit, offset);

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.Actions, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //search/users
        [Route("search/users")]
        [HttpGet]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.Users, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("search/all")]
        [HttpGet]
        public async Task<IActionResult> SearchAll([FromQuery] string searchvalue, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var filters = GetSearchFilters();

            var result = await _manager.GetSearchResultAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchType: Models.Enumerations.SearchTypeEnum.All, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //search/tasks
        //search/checklists
        //search/audits
        //search/assessments

        private SearchFilters GetSearchFilters(string searchValue = null, string sort = null, string direction = null, int? limit = null, int? offset = null)
        {
            var output = new SearchFilters()
            {
                SearchValue = searchValue,
                SortColumn = sort,
                SortDirection = direction,
                Limit = limit,
                OffSet = offset
            };

            return output;
        }
    }
}
