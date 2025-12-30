using Amazon.SimpleSystemsManagement.Model;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.General
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class MigrationsController : BaseController<MigrationsController>
    {

        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IFlattenAutomatedManager _flattenAutomatedManager;

        #region - constructor(s) -
        public MigrationsController(IDataMigrationManager dataMigrationManager, IConfigurationHelper configurationHelper, IFlattenAutomatedManager flattenAutomatedManager, ILogger<MigrationsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _dataMigrationManager = dataMigrationManager;
            _flattenAutomatedManager = flattenAutomatedManager;
        }
        #endregion

        /// <summary>
        /// MigrateChecklistsToStatic; Migrate existing data to static table. Currently capped at 100 per run due to load. (hard coded). Can only be used when logged in with EZ-Administrator account.
        /// </summary>
        /// <param name="companyid">CompanyId if that needs to be migrated</param>
        /// <returns>true/false or not found.</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/makestatic/checklists/{companyid}")]
        public async Task<IActionResult> MigrateChecklistsToStatic(int companyid)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableChecklistStaticMigration"))
            {
                var result = await _dataMigrationManager.MigrationChecklistsToStaticAsync(companyId: companyid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

                AppendCapturedExceptionToApm(_dataMigrationManager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Module not found or disabled.".ToJsonFromObject());
            }

        }

        /// <summary>
        /// MigrateAuditsToStatic; Migrate existing data to static table. Currently capped at 100 per run due to load. (hard coded). Can only be used when logged in with EZ-Administrator account.
        /// </summary>
        /// <param name="companyid">CompanyId if that needs to be migrated</param>
        /// <returns>true/false or not found.</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/makestatic/audits/{companyid}")]
        public async Task<IActionResult> MigrateAuditsToStatic(int companyid)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableAuditStaticMigration"))
            {
                var result = await _dataMigrationManager.MigrationAuditsToStaticAsync(companyId: companyid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

                AppendCapturedExceptionToApm(_dataMigrationManager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Module not found or disabled.".ToJsonFromObject());
            }

        }

        #region - fixes action -
        /// <summary>
        /// FixDataActionCommentActionResolved; Fixed the action comment that were missing and not send. This route needs to be called with an administrator user and called until no-more items are updated. 
        /// When this happens it should return a 410 (resource gone) and as message 0. Until then it should return an OK and result he number of inserted records. Records are added by 100 items per call. 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/actioncomment/fixresolved")]
        public async Task<IActionResult> FixDataActionCommentActionResolved()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDataFixActionComments"))
            {
                var result = await _dataMigrationManager.ActionCommentCorrectionForActionResolvedIssue();

                AppendCapturedExceptionToApm(_dataMigrationManager.GetPossibleExceptions());

                if (result > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
                } else
                {
                    return StatusCode((int)HttpStatusCode.Gone, result.ToJsonFromObject());
                }
                
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Module not found or disabled.".ToJsonFromObject());
            }

        }

        #endregion

        /// <summary>
        /// This is a fix for auditing records of audit templates where the audit template was deleted (set inactive).
        /// Fixes original object json in data auditing records where the original object json is not the audit template.
        /// Fixes 100 records, if the returned value is less than 100, that means all targetted records are now fixed.
        /// Can be run again but would have no effect and return 0.
        /// </summary>
        /// <returns>Number of affected records</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/auditing/fixdatalog/audittemplates")]
        public async Task<IActionResult> FixDataAuditingLogForAuditTemplates()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDataAuditingFixForAudittemplates"))
            {
                var result = await _dataMigrationManager.DataAuditingCorrectionForAuditTemplateSetActive();

                AppendCapturedExceptionToApm(_dataMigrationManager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Module not found or disabled.".ToJsonFromObject());
            }

        }

        /// <summary>
        /// This is a fix for auditing records of work instruction items where the original object was never logged.
        /// Fills original object json in data auditing records where the original object json is empty with the mutated object of the previous recorded mutation.
        /// Fixes 100 records, if the returned value is less than 100, that means all targetted records are now fixed.
        /// Can be run again but would have no effect and return 0.
        /// </summary>
        /// <returns>Number of affected records</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/auditing/fixdatalog/workinstructiontemplateitems")]
        public async Task<IActionResult> FixDataAuditingLogForWorkInstructionTemplateItems()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableDataAuditingFixForWorkInstructionTemplateItems"))
            {
                var result = await _dataMigrationManager.DataAuditingCorrectionForWorkInstructionTemplateItems();

                AppendCapturedExceptionToApm(_dataMigrationManager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Module not found or disabled.".ToJsonFromObject());
            }

        }

        #region - flatten process manual -
        /// <summary>
        /// Run flattenprocess manually for a company, this is an fall back method in case of automatic process doesn't function propertly. 
        /// </summary>
        /// <returns>Number of affected records</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/flattentemplates/{companyid}")]
        public async Task<IActionResult> MigrationFlattenCompanyTemplates(int companyid)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var result = await _flattenAutomatedManager.FlattenCurrentTemplatesAll(companyId: companyid);

            AppendCapturedExceptionToApm(_flattenAutomatedManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());

        }

        /// <summary>
        /// Run flattenprocess manually for a company, this is an fall back method in case of automatic process doesn't function propertly. 
        /// </summary>
        /// <returns>Number of affected records</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("migrations/flattentemplates/{templatetype}/{companyid}")]
        public async Task<IActionResult> MigrationFlattenCompanyTemplatesPerType(int companyid, TemplateTypeEnum templatetype)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var result = await _flattenAutomatedManager.FlattenCurrentTemplatesType(companyId: companyid, templateType: templatetype);

            AppendCapturedExceptionToApm(_flattenAutomatedManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());

        }
        #endregion

    }
}
