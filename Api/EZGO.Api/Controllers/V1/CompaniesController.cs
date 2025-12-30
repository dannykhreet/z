using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Setup;
using EZGO.Api.Models.Stats;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Cache;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// CompaniesController; contains all routes based on companies.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class CompaniesController :  BaseController<CompaniesController>
    {
        #region - private(s) -
        private readonly ICompanyManager _manager;
        private readonly IUserManager _userManager;
        private readonly IMemoryCache _cache;
        #endregion

        #region - constructor(s) -
        public CompaniesController(IMemoryCache memoryCache, ICompanyManager manager,IUserManager userManager, IConfigurationHelper configurationHelper, ILogger<CompaniesController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _userManager = userManager;
            _cache = memoryCache;
        }
        #endregion

        #region - GET routes company -
        //Only returnes list based on own code
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("companies")]
        [HttpGet]
        public async Task<IActionResult> GetCompanies([FromQuery] string include)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCompaniesAsync(include:include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)] //TODO check if needed
        [Route("company/{companyid}")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyById([FromRoute]int companyid, [FromQuery] string include)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!CompanyValidators.CompanyIdIsValid(companyid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
                if (!CompanyValidators.HasAccessToCompanyId(userCompanyId, companyid))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCompanyAsync(companyId: companyid, getCompanyId: companyid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)] 
        [Route("company/{companyid}/statistics")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyStatistics([FromRoute] int companyid)
        {

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!CompanyValidators.CompanyIdIsValid(companyid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
                if (!CompanyValidators.HasAccessToCompanyId(userCompanyId, companyid))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
                }
            }

            CompanyStatistics result = null;

            var cacheKey = CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyCompanyStatisticCheck, companyid);

            if (_cache.TryGetValue(cacheKey, out result))
            {
                if (result != null) return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            result = await _manager.GetCompanyStatistics(companyId: companyid);

            if (result.CompanyBasicStatistics.Any() ||
                result.ChecklistStatistics.Any() ||
                result.AuditsStatistics.Any() ||
                result.TasksStatistics.Any() ||
                result.TasksOkStatistics.Any() ||
                result.TasksSkippedStatistics.Any() ||
                result.TasksNotOkStatistics.Any() ||
                result.AssessmentsStatistics.Any() ||
                result.ActionCreatedStatistics.Any() ||
                result.ActionDueAtStatistics.Any() ||
                result.CommentCreatedStatistics.Any() ||
                result.TaskTemplateStatistics.Any() ||
                result.ChecklistTemplateStatistics.Any() ||
                result.AuditTemplateStatistics.Any() ||
                result.AssessmentTemplateStatistics.Any() ||
                result.WorkInstructionTemplateStatistics.Any())
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSettings.CacheTimeDefaultLongInSeconds) });

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("company")]
        [HttpGet]
        public async Task<IActionResult> GetCompany([FromQuery] string include)
        {

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            var result = await _manager.GetCompanyAsync(companyId: userCompanyId, getCompanyId: userCompanyId, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("company/roles")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyRoles()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCompanyRolesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Gets a list of basic comany objects that are in the same holding as the company of the user
        /// </summary>
        /// <returns>List of CompanyBasic objects</returns>
        [Route("company/holding/companies")]
        [HttpGet]
        public async Task<IActionResult> GetCompaniesInSameHolding()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            int holdingId = await _manager.GetCompanyHoldingIdAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            List<CompanyBasic> comaniesInHolding = await _manager.GetCompaniesInHoldingAsync(holdingId: holdingId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (comaniesInHolding).ToJsonFromObject());
        }

        /// <summary>
        /// Gets a list of basic comany objects that are in the same holding as the company of the user
        /// </summary>
        /// <returns>List of CompanyBasic objects</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("company/holding/features/templatesharing/companies")]
        [HttpGet]
        public async Task<IActionResult> GetCompaniesInSameHoldingWithTemplateSharingEnabled()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            int holdingId = await _manager.GetCompanyHoldingIdAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            List<CompanyBasic> comaniesInHolding = await _manager.GetCompaniesInHoldingWithTemplateSharingEnabledAsync(holdingId: holdingId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (comaniesInHolding).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("companiesfeatures")]
        [HttpGet]
        public async Task<IActionResult> GetCompaniesFeatures()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCompaniesFeaturesAsync();

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/all/statistics")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyStatisticsAll()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            CompanyStatistics result = null;

            var cacheKey = CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyCompanyAllStatisticCheck);

            if (_cache.TryGetValue(cacheKey, out result))
            {
                if (result != null) return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            result = await _manager.GetCompanyStatisticsAll();

            if (result.CompanyBasicStatistics.Any() ||
                result.ChecklistStatistics.Any() ||
                result.AuditsStatistics.Any() ||
                result.TasksStatistics.Any() ||
                result.TasksOkStatistics.Any() ||
                result.TasksSkippedStatistics.Any() ||
                result.TasksNotOkStatistics.Any() ||
                result.AssessmentsStatistics.Any() ||
                result.ActionCreatedStatistics.Any() ||
                result.ActionDueAtStatistics.Any() ||
                result.CommentCreatedStatistics.Any() ||
                result.TaskTemplateStatistics.Any() ||
                result.ChecklistTemplateStatistics.Any() ||
                result.AuditTemplateStatistics.Any() ||
                result.AssessmentTemplateStatistics.Any() ||
                result.WorkInstructionTemplateStatistics.Any())
                _cache.Set(cacheKey, result, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSettings.CacheTimeDefaultLongInSeconds) });

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - POST routes company -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/add")]
        [HttpPost]
        public async Task<IActionResult> AddCompany([FromBody]Company company)
        {
            if (!company.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var result = await _manager.AddCompanyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), company:company);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/change/{companyid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeCompany([FromRoute]int companyid, [FromBody]Company company)
        {
            if (!company.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (!CompanyValidators.CompanyIdIsValid(companyid) || companyid != company.Id)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            if(await _manager.CheckCompany(name: company.Name, companyId: companyid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ALREADY_EXISTS.ToJsonFromObject());
            }

            var result = await _manager.ChangeCompanyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), changedCompanyId: companyid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), company: company);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/setactive/{companyid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTask([FromRoute]int companyid, [FromBody] object isActive)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!CompanyValidators.CompanyIdIsValid(companyid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            var result = await _manager.SetCompanyActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), inactiveCompanyId: companyid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/remove/{companyid}")]
        [HttpPost]
        public async Task<IActionResult> RemoveCompanyFromSystem([FromRoute] int companyid, [FromBody] SetupCompany company)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!CompanyValidators.CompanyIdIsValid(companyid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!company.CompanyId.HasValue || !CompanyValidators.CompanyIdIsValid(company.CompanyId.Value))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (companyid != company.CompanyId.Value)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            var result = await _manager.RemoveCompany(company: company, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            if (result)
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Company deletion not complete.".ToJsonFromObject());
            }
          
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("company/roles/change")]
        [HttpPost]
        public async Task<IActionResult> ChangeCompanyRole([FromBody] CompanyRoles roles)
        {
            if (!roles.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetCompanyRolesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), roles: roles);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/setup")]
        [HttpPost]
        public async Task<IActionResult> SetupCompany([FromBody] SetupCompany company)
        {
            if (!company.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var validationResult = CompanyValidators.SetupCompanyIsValidAndOrSetDefaults(company: company);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, (validationResult).ToJsonFromObject());
            }

            var userNameAllreadyFound = await _userManager.CheckUserName(userName: company.PrimaryUserName);
            if (userNameAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Username already exists.".ToJsonFromObject());
            }

            if (await _manager.CheckCompany(name: company.Name, companyId: 0))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ALREADY_EXISTS.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.CreateCompany(company: company, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/setup/companysettings/{companyid}")]
        [HttpPost]
        public async Task<IActionResult> SetupCompanySettings([FromRoute] int companyid, [FromBody] SetupCompanySettings companySettings)
        {
            if (!companySettings.ValidateAndClean(companyid: companyid, messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetupCompanySettings(companySettings: companySettings, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            if (result)
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            else
            {
                return BadRequest(false);
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/setup/validate")]
        [HttpPost]
        public async Task<IActionResult> ValidateSetupCompany([FromBody] SetupCompany company)
        {
            if (!company.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var validationResult = CompanyValidators.SetupCompanyIsValidAndOrSetDefaults(company: company);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, (validationResult).ToJsonFromObject());
            }

            var userNameAllreadyFound = await _userManager.CheckUserName(userName: company.PrimaryUserName);
            if (userNameAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Username already exists.".ToJsonFromObject());
            }

            if (await _manager.CheckCompany(name: company.Name, companyId: 0))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ALREADY_EXISTS.ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (true).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/setup/generateserviceaccount")]
        [HttpPost]
        public async Task<IActionResult> GenerateSetupCompanyServiceAccount([FromBody] SetupCompany company)
        {
            if (!company.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = string.Empty; // await _manager.CreateServiceAccount(company: company, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - holdings -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/holdings")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyHoldings([FromQuery] string include)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetHoldings(include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/holding/{holdingid}")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyHoldings([FromRoute] int holdingid, [FromQuery] string include)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetHolding(holdingId: holdingid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/holdings/save")]
        [HttpPost]
        public async Task<IActionResult> SaveCompanyHolding([FromBody] Holding holding)
        {
            if (!holding.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            bool result = false;
            
            if(holding.Id > 0)
            {
                var resultChange = await _manager.ChangeHoldingAsync(holdingId: holding.Id, holding: holding);

                if (resultChange)
                {
                    result = true;
                } else
                {
                    result = false;
                }
            } else
            {
                var resultAdd = await _manager.AddHoldingAsync(holding: holding);
                if (resultAdd > 0)
                {
                    holding.Id = resultAdd;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }

            var response = await _manager.GetHolding(holdingId: holding.Id, include: "holdingunits");

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if(result)
            {
                return StatusCode((int)HttpStatusCode.OK, (response).ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, (response).ToJsonFromObject());
            }
            
        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/holding/{holdingid}/unit/save")]
        [HttpPost]
        public async Task<IActionResult> SaveCompanyHolding([FromRoute] int holdingid, [FromBody] HoldingUnit holdingunit)
        {
            if (!holdingunit.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                             messages: out var possibleMessages))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            bool result = false;

            if (holdingunit.Id > 0)
            {
                var resultChange = await _manager.ChangeHoldingUnitAsync(holdingId: holdingid, holdingUnit: holdingunit);

                if (resultChange)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                var resultAdd = await _manager.AddHoldingUnitAsync(holdingUnit: holdingunit);
                if (resultAdd > 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }

            var response = await _manager.GetHolding(holdingId: holdingid, include: "holdingunits");

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result)
            {
                return StatusCode((int)HttpStatusCode.OK, (response).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, (response).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("company/holding/{holdingid}/statistics")]
        [HttpGet]
        public async Task<IActionResult> GetHoldingStatistics([FromRoute] int holdingid)
        {

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!CompanyValidators.CompanyIdIsValid(holdingid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, CompanyValidators.MESSAGE_COMPANY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            CompanyStatistics result = null;

            var cacheKey = CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyHoldingStatisticCheck, holdingid);

            if (_cache.TryGetValue(cacheKey, out result))
            {
                if (result != null) return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            result = await _manager.GetHoldingStatistics(holdingId: holdingid);

            if (result.CompanyBasicStatistics.Any() ||
                result.ChecklistStatistics.Any() ||
                result.AuditsStatistics.Any() ||
                result.TasksStatistics.Any() ||
                result.TasksOkStatistics.Any() ||
                result.TasksSkippedStatistics.Any() ||
                result.TasksNotOkStatistics.Any() ||
                result.AssessmentsStatistics.Any() ||
                result.ActionCreatedStatistics.Any() ||
                result.ActionDueAtStatistics.Any() ||
                result.CommentCreatedStatistics.Any() ||
                result.TaskTemplateStatistics.Any() ||
                result.ChecklistTemplateStatistics.Any() ||
                result.AuditTemplateStatistics.Any() ||
                result.AssessmentTemplateStatistics.Any() ||
                result.WorkInstructionTemplateStatistics.Any())
                _cache.Set(cacheKey, result, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSettings.CacheTimeDefaultLongInSeconds) });

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        #endregion

        #region - health checks -
        /// <summary>
        /// GetCompaniesHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("companies/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetCompaniesHealth()
        {
            try
            {
                var result = await _manager.GetCompaniesAsync();

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                if (result.Any() && result.Count > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());
                }
            }
            catch
            {

            }
            return StatusCode((int)HttpStatusCode.Conflict, false.ToJsonFromObject());
        }
        #endregion

    }
}