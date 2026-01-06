using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    public class DashboardsController : BaseController<DashboardsController>
    {
        #region - privates -
        private readonly IDashboardsManager _manager;
        #endregion

        #region - contructor(s) -
        public DashboardsController(IDashboardsManager manager, ILogger<DashboardsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        #region - CMS Dashboard Specific -

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardStatistics(bool usestatictotals = false, bool useannouncements = false, bool usecompanyoverview = false, bool usecompletedaudits = false, bool usecompletedchecklists = false, bool usecompletedtasks = false, bool useactions = false)
        {

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var filters = new DashboardFilters() { UseStatisticsTotals = usestatictotals, UseAnnouncements = useannouncements, UseCompanyOverview = usecompanyoverview, UseCompletedAudits = usecompletedaudits, UseCompletedChecklists = usecompletedchecklists, UseCompletedTasks = usecompletedtasks, UseActions = useactions };

            var result = await _manager.GetDashboard(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters); //Add dashboard object with several datasets

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard/announcements")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardAnnouncements(bool useannouncements = false)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var filters = new DashboardFilters() { UseStatisticsTotals = false, UseAnnouncements = useannouncements, UseCompanyOverview = false, UseCompletedAudits = false, UseCompletedChecklists = false, UseCompletedTasks = false, UseActions = false };

            var result = await _manager.GetDashboardAnnouncements(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard/completed")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardCompletedItems(bool usecompletedaudits = false, bool usecompletedchecklists = false, bool usecompletedtasks = false, bool useactions = false)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var filters = new DashboardFilters() { UseStatisticsTotals = false, UseAnnouncements = false, UseCompanyOverview = false, UseCompletedAudits = usecompletedaudits, UseCompletedChecklists = usecompletedchecklists, UseCompletedTasks = usecompletedtasks, UseActions = useactions };

            var result = await _manager.GetDashboardCompletedItems(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard/completed/audits")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardCompletedAudits()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetDashboardCompletedAudits(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard/completed/checklists")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardCompletedChecklists()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetDashboardCompletedChecklists(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard/completed/tasks")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardCompletedTasks()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetDashboardCompletedTasks(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("dashboard/actions")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardActions()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetDashboardActions(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        #endregion
    }
}
