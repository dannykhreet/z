using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tools;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Export;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.General
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Exports)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ExportController : BaseController<ExportController>
    {
        #region - privates -
        private readonly IExportingManager _exportingManager;
        private readonly IGeneralManager _generalManager;
        private readonly IFlattenAutomatedManager _flattenAutomatedManager;
        #endregion

        #region - constructor(s) -
        public ExportController(ILogger<ExportController> logger, IGeneralManager generalManager, IApplicationUser applicationUser, IExportingManager reportingmanager, IFlattenAutomatedManager flattenAutomatedManager, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper: configurationHelper)
        {
            _exportingManager = reportingmanager;
            _generalManager = generalManager;
            _flattenAutomatedManager = flattenAutomatedManager;
        }
        #endregion

        #region - combination exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklistsaudits/xlsx/{companyid}")]
        public async Task GetChecklistsAudits([FromRoute]int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetChecklistAuditOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklistaudit_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklistaudittemplates/xlsx/{companyid}")]
        public async Task GetChecklistsAuditsTemplates([FromRoute]int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetChecklistAuditTemplatesOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype)) ;

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklistaudit_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - checklists -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklists/xlsx/{companyid}")]
        public async Task GetChecklists([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetChecklistOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklist_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklists/csv/{companyid}")]
        public async Task GetChecklistsCSV([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetChecklistOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklist_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklistitems/csv/{companyid}")]
        public async Task GetChecklistItemsCSV([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetChecklistOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklist_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[1], Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklisttemplates/xlsx/{companyid}")]
        public async Task GetChecklistsTemplates([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetChecklistTemplatesOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklisttemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - audits -

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/audits/xlsx/{companyid}")]
        public async Task GetAudits([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetAuditOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_audit_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/audits/csv/{companyid}")]
        public async Task GetAuditsCSV([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetAuditOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_audit_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/audititems/csv/{companyid}")]
        public async Task GetAuditItemsCSV([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetAuditOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_audit_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[1], Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/audittemplates/xlsx/{companyid}")]
        public async Task GetAuditsTemplates([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetAuditTemplatesOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_audittemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - tasks / tasktemplates -
        //GetTaskTemplateOverview
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/tasktemplates/csv/{companyid}")]
        public async Task GetTasksTemplateCsv([FromRoute]int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var dt = await _exportingManager.GetTaskTemplateDetailsOverviewByCompanyAsync(companyId:companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_templates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(dt, Response.Body);

                //cleanup
                dt.Rows.Clear();
                dt.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/tasktemplates/xlsx/{companyid}")]
        public async Task GetTasksTemplateXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                var ds = await _exportingManager.GetTaskTemplateOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasktemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: false);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/tasks/xlsx/{companyid}")]
        public async Task GetTasksXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                var ds = await _exportingManager.GetTaskOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasktemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - workinstructiontemplates -

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/workinstructiontemplates/xlsx/{companyid}")]
        public async Task GetWorkInstructionTemplates([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetWorkInstructionTemplatesOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_workinstructiontemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        #endregion

        #region - work instruction template change notifications - 

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/wichangenotifications/xlsx/{companyid}")]
        public async Task GetWorkInstructionChangeNotificationsExcelExport([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { }
                ;

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { }
                ;

                var ds = await _exportingManager.GetWorkInstructionChangeNotificationsOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_workinstructiontemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }


        #endregion

        #region - assessmenttemplates -

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/assessmenttemplates/xlsx/{companyid}")]
        public async Task GetAssessmentTemplates([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetAssessmentTemplatesOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_assessmenttemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - completed assessments - 
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/assessments/xlsx/{companyid}")]
        public async Task GetAssessments([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetAssessmentOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_assessments_company_{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        #endregion

        #region - action comments -
        //

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/actioncomments/xlsx/{companyid}")]
        public async Task GetActionComments([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetActionCommentOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_actionscomments_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/actions/xlsx/{companyid}")]
        public async Task GetActions([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetActionOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_actions_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/comments/xlsx/{companyid}")]
        public async Task GetComments([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {

            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                //_logger.LogInformation("Export - Retrieve data");
                var ds = await _exportingManager.GetCommentOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));
                //_logger.LogInformation("Export - Retrieve data - Done");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_comments_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                //_logger.LogInformation("Export - Generating XLSX");
                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);
                //_logger.LogInformation("Export - Generating XLSX - Done");
                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
        #endregion

        #region - configuration
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/companyareas/xlsx/{companyid}")]
        public async Task CompanyActiveAreas([FromRoute] int companyid)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
               
                var ds = await _exportingManager.GetCompanyActiveAreas(companyid: companyid);

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_areas_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        #endregion

        #region - property exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/taskproperties/xlsx/{companyid}")]
        public async Task GetTasksPropertiesXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                var ds = await _exportingManager.GetTaskPropertyOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/checklisttaskproperties/xlsx/{companyid}")]
        public async Task GetChecklistPropertiesXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                var ds = await _exportingManager.GetTaskChecklistPropertyOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/audittaskproperties/xlsx/{companyid}")]
        public async Task GetAuditPropertiesXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                var ds = await _exportingManager.GetTaskAuditPropertyOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskauditproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - auditing log exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/auditinglog/csv/{companyid}")]
        public async Task GetAuditingLogCsv([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                var ds = await _exportingManager.GetAuditingLogOverviewAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_auditinglog_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - management exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/management/company/xlsx")]
        public async Task GetCompanyManagementOverviewXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp)
        {

            if(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetManagementCompanyOverview(from: parsedstarttimestamp, to: parsedendtimestamp);

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_company_overview_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

            await ExcelWriter.WriteFromDataTableAsync(ds, Response.Body, includeHeaders: true);

            ds.Dispose();
        }
        #endregion

        #region - language exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/languageresources/xlsx")]
        public async Task GetLanguageResourcesXslx()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var ds = await _exportingManager.GetLanguageResourcesAsync();

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("Content-Disposition", $"inline; filename=language_resources_ezgo_app_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

            await ExcelWriter.WriteFromDataTableAsync(ds, Response.Body, includeHeaders: true);

            ds.Dispose();

        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/languageimport/csv")]
        public async Task GetLanguageImportXslx()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var dt = await _exportingManager.GetLanguageImportQueriesAsync();

            Response.ContentType = "text/csv";
            Response.Headers.Add("Content-Disposition", $"inline; filename=language_importqueries_ezgo_app_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv");

            await CsvWriter.WriteFromDataTable(dt, Response.Body);

            dt.Rows.Clear();
            dt.Dispose();

        }
        #endregion

        #region - customer specific exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/customerspecific/tasks/xlsx/{companyid}")]
        public async Task GetTasksCustomerSpecificXslx([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                DateTime parsedstarttimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                DateTime parsedendtimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };


                var ds = await _exportingManager.GetTaskOverviewCustomerSpecificAsync(companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasktemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                ds.Tables.Clear();
                ds.Dispose();
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }

        #endregion

        #region - automated exports -

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/holding/tasks/xslx/{holdingid}")]
        [Route("export/automated/company/tasks/xslx/{companyid}")]
        public async Task GetAutomatedTaskExport([FromRoute] int companyid, [FromRoute] int holdingid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedTaskExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp);

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_company_tasks_overview_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

            await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);

            ds.Dispose();

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/holding/tasks/csv/{holdingid}")]
        [Route("export/automated/company/tasks/csv/{companyid}")]
        public async Task GetAutomatedTaskExportCSV([FromRoute] int companyid, [FromRoute] int holdingid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] string datatype = "overview")
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedTaskExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, dataType: datatype);

            Response.ContentType = "text/csv";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasks_{datatype}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

            await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);

            ds.Tables.Clear();
            ds.Dispose();

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/holding/checklists/xslx/{holdingid}")]
        [Route("export/automated/company/checklists/xslx/{companyid}")]
        public async Task GetAutomatedChecklistExport([FromRoute] int companyid, [FromRoute] int holdingid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedChecklistExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp);

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_company_checklists_overview_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

            await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);

            ds.Dispose();

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/holding/checklists/csv/{holdingid}")]
        [Route("export/automated/company/checklists/csv/{companyid}")]
        public async Task GetAutomatedChecklistExportCSV([FromRoute] int companyid, [FromRoute] int holdingid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] string datatype = "overview")
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedChecklistExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, dataType: datatype);

            Response.ContentType = "text/csv";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklists_{datatype}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

            await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);

            ds.Tables.Clear();
            ds.Dispose();

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/holding/audits/xslx/{holdingid}")]
        [Route("export/automated/company/audits/xslx/{companyid}")]
        public async Task GetAutomatedAuditExport([FromRoute] int companyid, [FromRoute] int holdingid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedAuditExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp);

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_company_audits_overview_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

            await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);

            ds.Dispose();

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/holding/audits/csv/{holdingid}")]
        [Route("export/automated/company/audits/csv/{companyid}")]
        public async Task GetAutomatedAuditExportCSV([FromRoute] int companyid, [FromRoute] int holdingid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] string datatype = "overview")
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedAuditExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, dataType: datatype);

            Response.ContentType = "text/csv";
            Response.Headers.Add("Content-Disposition", $"inline; filename=export_audits_{datatype}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

            await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);

            ds.Tables.Clear();
            ds.Dispose();

        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/schedule")]
        public async Task<IActionResult> GetAutomatedSchedule()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "".ToJsonFromObject());
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return StatusCode((int)HttpStatusCode.Forbidden, "".ToJsonFromObject());
            }

            // var ds = await _exportingManager.GetAutomatedAuditExportsOverview(holdingId: holdingid, companyId: companyid, from: parsedstarttimestamp, to: parsedendtimestamp);

            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.Headers.Add("Content-Disposition", $"inline; filename=export_company_audits_overview_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

            //await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);
            await Task.CompletedTask;
            //ds.Dispose();

            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());

        }

        #endregion

        #region - automated exporter exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/automated/atoss/holding/{module}/{type}/{holdingid}")]
        [Route("export/automated/atoss/company/{module}/{type}/{companyid}")]
        [Route("export/automated/atoss/holding/{module}/{datatype}/{type}/{holdingid}")]
        [Route("export/automated/atoss/company/{module}/{datatype}/{type}/{companyid}")]
        public async Task GetAutomatedAtossExport([FromRoute] int companyid, [FromRoute] int holdingid, [FromRoute] string module, [FromRoute] string type, [FromQuery] string companyids)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableAutomatedExportFunctionality"))
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var dt = await _exportingManager.GetAutomatedExportAtoss(holdingId: holdingid, companyId: companyid, module: module, (!string.IsNullOrEmpty(companyids) ? companyids.Split(",").Select(x => Convert.ToInt32(x)).ToList() : null));

            if (type == "xslx")
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_atoss_{module}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataTableAsync(dt, Response.Body, includeHeaders: true);
            }
            else if (type == "csv")
            {
                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_atoss_{module}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(dt, Response.Body);
            }
            else if (type == "json")
            {
                Response.ContentType = "text/json";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_atoss_{module}_{DateTime.Now.ToString("yyyyMMddHHmm")}.json");
                await Response.WriteAsync(SerializeObjectNoCache(dt));
            }

            dt.Clear();
            dt.Dispose();

        }

        #endregion

        #region - automated datawarehouse exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/datawarehouse/holding/{module}/{type}/{holdingid}")]
        [Route("export/datawarehouse/company/{module}/{type}/{companyid}")]
        [Route("export/datawarehouse/holding/{module}/{datatype}/{type}/{holdingid}")]
        [Route("export/datawarehouse/company/{module}/{datatype}/{type}/{companyid}")]
        public async Task GetAutomatedDatawarehouseExport([FromRoute] int companyid, [FromRoute] int holdingid, [FromRoute] string module, [FromRoute] string type, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null, [FromRoute] string datatype = null)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableAutomatedExportFunctionality"))
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var ds = await _exportingManager.GetAutomatedDatawarehouseExport(holdingId: holdingid, companyId: companyid, module: module, from: parsedstarttimestamp, to: parsedendtimestamp, dataType: datatype);

            if(type == "xslx")
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_dw_{companyid}_{module}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);
            } else if (type == "csv")
            {
                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_dw_{companyid}_{module}_{datatype}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);
            } else if (type == "json") {
                Response.ContentType = "text/json";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_dw_{companyid}_{module}_{datatype}_{DateTime.Now.ToString("yyyyMMddHHmm")}.json");
                await Response.WriteAsync(SerializeObjectNoCache(ds));
            }

            ds.Tables.Clear();
            ds.Dispose();

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("export/datawarehouse/manual")]
        public async Task<IActionResult> AutomatedDatawarehouseExportManual(AutomatedDataFilter filter)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableAutomatedExportFunctionality"))
            {
                return StatusCode((int)HttpStatusCode.NoContent, "Functionality not available.".ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden (A.0001)".ToJsonFromObject());
            }

            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden (A.0002)".ToJsonFromObject());
            }

            var outcome = await _exportingManager.ExportToDatawarehouse(holdingId: filter.HoldingId, companyId: filter.CompanyId, storedProcedureName: filter.ProcedureName, fromTime: filter.StartAt.Value, toTime: filter.EndAt.Value);
            return StatusCode(outcome ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Conflict, outcome.ToJsonFromObject());

        }
        #endregion

        #region - automated data collection (project cobra) exports -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/datacollection/company/{module}/{type}/{companyid}")]
        public async Task GetAutomatedDataCollectionExport([FromRoute] int companyid, [FromRoute] string type, [FromQuery] string timestamp,[FromRoute] string module)
        {
            if(!_configurationHelper.GetValueAsBool("AppSettings:EnableAutomatedExportFunctionality"))
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!GetIsValidIP())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(timestamp) && DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            var ds = await _exportingManager.GetAutomatedDataCollectionExport(companyId: companyid, module: module, from: parsedstarttimestamp);

            if (type == "xslx")
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_dc_{companyid}_{module}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body, includeHeaders: true);
            }
            else if (type == "csv")
            {
                Response.ContentType = "text/csv";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_dc_{companyid}_{module}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                await CsvWriter.WriteFromDataTable(ds.Tables[0], Response.Body);
            }
            else if (type == "json")
            {
                Response.ContentType = "text/json";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_dc_{companyid}_{module}_{DateTime.Now.ToString("yyyyMMddHHmm")}.json");
                await Response.WriteAsync(SerializeObjectNoCache(ds));
            }

            ds.Tables.Clear();
            ds.Dispose();

        }
        #endregion

        #region - custom serializer for datasets -

        [NonAction]
        public static string SerializeObjectNoCache<T>(T obj, JsonSerializerSettings settings = null)
        {
            settings = settings ?? new JsonSerializerSettings();
            bool reset = (settings.ContractResolver == null);
            if (reset)
                // To reduce memory footprint, do not cache contract information in the global contract resolver.
                settings.ContractResolver = new DefaultContractResolver();
            try
            {
                return JsonConvert.SerializeObject(obj, settings);
            }
            finally
            {
                if (reset)
                    settings.ContractResolver = null;
            }
        }

        #endregion

        #region - flattened version exports -
        //TODO, check if needs to be moved to own controller later-on. For now these are considered exports. 

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/{templatetype}/{templateid}/versions")]
        public async Task<IActionResult> ExportFlattendedTemplateVersionList([FromRoute] string templatetype, [FromRoute] int templateid)
        {
            TemplateTypeEnum parsedTemplatType;
            if (!Enum.TryParse(value: templatetype, ignoreCase: true, out parsedTemplatType)) {
                return StatusCode((int)HttpStatusCode.BadRequest, "Incorrect template type".ToJsonFromObject());
            }

            if(templateid <= 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid template id".ToJsonFromObject());
            }

            //SortedList<DateTime, string> t = new SortedList<DateTime, string>();
            //t.Add(DateTime.Now, "V12384328128383");
            //t.Add(DateTime.Now.AddDays(-1), "V12384328128383");
            //t.Add(DateTime.Now.AddDays(-2), "V12384328128383");
            //t.Add(DateTime.Now.AddDays(-3), "V12384328128383");

            var outcome = await _flattenAutomatedManager.RetrieveVersionsList(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), templateId: templateid, templateType: parsedTemplatType);

            AppendCapturedExceptionToApm(_flattenAutomatedManager.GetPossibleExceptions());
            
            return StatusCode((int)HttpStatusCode.OK, outcome.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/{templatetype}/{templateid}/version/{version}")]
        public async Task<IActionResult> ExportFlattendedTemplateVersion([FromRoute] string templatetype, [FromRoute] int templateid, [FromRoute] string version)
        {
            TemplateTypeEnum parsedTemplatType;
            if (!Enum.TryParse(value: templatetype, ignoreCase: true, out parsedTemplatType))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Incorrect template type".ToJsonFromObject());
            }

            if (templateid <= 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid template id".ToJsonFromObject());
            }

            if(string.IsNullOrEmpty(version))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Version not valid".ToJsonFromObject());
            }

            var outcome = await _flattenAutomatedManager.RetrieveVersionJson(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), templateId: templateid, templateType: parsedTemplatType, version: version);

            AppendCapturedExceptionToApm(_flattenAutomatedManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, outcome);
        }

        #endregion

        #region - matrix exports -

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("export/matrix/skills/xlsx/{companyid}")]
        public async Task GetMatrixSkillTemplateConnection([FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            var userCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            if (userCompanyId == companyid)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyid, featurekey: "FEATURE_EXPORT_MATRIX_SKILL_USER")) {
                    DateTime parsedstarttimestamp = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

                    DateTime parsedendtimestamp = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

                    var ds = await _exportingManager.GetMatrixSkillsUserOverviewByCompanyAsync(companyid: companyid, from: parsedstarttimestamp, to: parsedendtimestamp, timespanInDays: Convert.ToInt32(timespantype));

                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_workinstructiontemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                    await ExcelWriter.WriteFromDataSetAsync(ds, Response.Body);

                    ds.Tables.Clear();
                    ds.Dispose();
                } else
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

        }
        #endregion

        #region - internal-
        //TODO move to base and implement in more items
        [NonAction]
        private bool GetIsValidIP()
        {
            return (CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")));
        }
        #endregion

    }
}