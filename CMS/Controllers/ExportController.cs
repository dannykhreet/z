using EZGO.CMS.LIB.Interfaces;
using EZGO.CMS.LIB.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Export;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using WebApp.Attributes;
using WebApp.Logic;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Exports)]
    public class ExportController : BaseController
    {
        private readonly ILogger<ExportController> _logger;
        private readonly IApiConnector _connector;
        public ExportController(ILogger<ExportController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;

        }

        //unused, not clear why we have this TODO REMOVE
        [Route("/export/checklistsandaudits")]
        [HttpPost]
        public async Task GetChecklistAndAuditsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklistsaudits_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var result = await _connector.GetCall(url: $"/v1/export/checklistsaudits/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={exportData.FromDate.Value.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={exportData.ToDate.Value.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }


        }

        [Route("/export/checklists")]
        [HttpPost]
        public async Task GetChecklistsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklists_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                //because data has timezone, start-, endtimestamp are also expected to have timestamp so no universal time
                var result = await _connector.GetCall(url: $"/v1/export/checklists/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/audits")]
        [HttpPost]
        public async Task GetAuditsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_audits_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }
                //because data has timezone, start-, endtimestamp are also expected to have timestamp so no universal time
                var result = await _connector.GetCall(url: $"/v1/export/audits/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/audittemplates")]
        [HttpPost]
        public async Task GetAuditTemplatesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_audittemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var result = await _connector.GetCall(url: $"/v1/export/audittemplates/{exportData.ExportType.ToLower()}/{companyid}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/checklisttemplates")]
        [HttpPost]
        public async Task GetChecklistTemplatesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_checklisttemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var result = await _connector.GetCall(url: $"/v1/export/checklisttemplates/{exportData.ExportType.ToLower()}/{companyid}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/workinstructiontemplates")]
        [HttpPost]
        public async Task GetWorkInstructionTemplatesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_workinstructiontemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var result = await _connector.GetCall(url: $"/v1/export/workinstructiontemplates/{exportData.ExportType.ToLower()}/{companyid}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/wichangenotifications")]
        [HttpPost]
        public async Task GetWorkInstructionChangeNotificationsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_wichangenotifications_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }
                //because data has timezone, start-, endtimestamp are also expected to have timestamp so no universal time
                var result = await _connector.GetCall(url: $"/v1/export/wichangenotifications/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/tasktemplates")]
        [HttpPost]
        public async Task GetTaskTemplatesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_templates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasktemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var result = await _connector.GetCall(url: $"/v1/export/tasktemplates/{exportData.ExportType.ToLower()}/{companyid}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/tasks")]
        [HttpPost]
        public async Task GetTasksExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasks_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_tasks_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/tasks/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/taskproperties")]
        [HttpPost]
        public async Task GetTaskPropertiesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/taskproperties/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/checklisttaskproperties")]
        [HttpPost]
        public async Task GetChecklistTaskPropertiesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/checklisttaskproperties/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/audittaskproperties")]
        [HttpPost]
        public async Task GetAuditTaskPropertiesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_taskproperties_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/audittaskproperties/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/actions")]
        [HttpPost]
        public async Task GetActionsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_actions_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/actions/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/comments")]
        [HttpPost]
        public async Task GetCommentsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_comments_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/comments/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/assessments")]
        [HttpPost]
        public async Task GetAssessmentsExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_assessments_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/assessments/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/assessmenttemplates")]
        [HttpPost]
        public async Task GetAssessmentTemplatesExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_assessmenttemplates_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var result = await _connector.GetCall(url: $"/v1/export/assessmenttemplates/{exportData.ExportType.ToLower()}/{companyid}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Route("/export/matrixskillscores")]
        [HttpPost]
        public async Task GetMatrixSkillScoresExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_matrixskillscores_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/matrix/skills/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/export/languageresources")]
        [HttpPost]
        public async Task GetLanguageExport([FromBody] ExportData exportData)
        {
            if (!this.IsAdminCompany)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_languageresources_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_languageresources_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var result = await _connector.GetCall(url: $"/v1/export/languageresources/{exportData.ExportType.ToLower()}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }


        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/export/languageimport")]
        [HttpPost]
        public async Task GetLanguageImport([FromBody] ExportData exportData)
        {
            if (!this.IsAdminCompany)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType))
            {
                exportData.ExportType = "XSLX";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_languageimport_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_languageimport_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var result = await _connector.GetCall(url: $"/v1/export/languageimport/{exportData.ExportType.ToLower()}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }


        }


        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/export/companyoverview")]
        [HttpPost]
        public async Task GetCompanyManagement([FromBody] ExportData exportData)
        {
            if (!this.IsAdminCompany)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "XSLX";
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_companyoverview_{DateTime.Now.ToString("yyyyMMddHHmm")}.csv");

                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_companyoverview_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                }

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/management/company/{exportData.ExportType.ToLower()}/?starttimestamp={fromDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }

        }

        [Route("/export/companyareas")]
        [HttpPost]
        public async Task GetCompanyActiveAreas([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "xlsx")
            {
                exportData.ExportType = "xlsx";
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("Content-Disposition", $"inline; filename=export_areas_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");

                var result = await _connector.GetCall(url: $"/v1/export/companyareas/{exportData.ExportType.ToLower()}/{companyid}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }

        }

        [Route("/export/auditinglog")]
        [HttpPost]
        public async Task GetAuditingLogExport([FromBody] ExportData exportData)
        {
            if (exportData == null)
            {
                exportData = new ExportData();
            }

            if (!exportData.FromDate.HasValue || exportData.FromDate.Value == DateTime.MinValue)
            {
                exportData.FromDate = DateTime.Now.AddDays(-7);
            }

            if (!exportData.ToDate.HasValue || exportData.ToDate.Value == DateTime.MinValue)
            {
                exportData.ToDate = DateTime.Now;
            }

            if (string.IsNullOrEmpty(exportData.ExportType) || exportData.ExportType.ToLower() != "csv")
            {
                exportData.ExportType = "csv";
            }

            if ((exportData.ToDate.Value - exportData.FromDate.Value).TotalDays > 60)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync("Time range to large, please select a smaller time range.");
                return;
            }

            var companyid = User.GetProfile()?.Company?.Id;
            if (companyid != null && companyid > 0)
            {
                if (exportData.ExportType.ToUpper() == "CSV")
                {
                    Response.ContentType = "text/csv";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_auditinglog_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv");
                }
                else
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add("Content-Disposition", $"inline; filename=export_auditinglog_company{companyid}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                }

                var fromDate = DateTimeOffset.MinValue;
                var toDate = DateTimeOffset.MinValue;

                if (exportData.FromDate != null)
                {
                    fromDate = GetDateTimeOffsetWithCompanyTimezone(exportData.FromDate.Value);
                }
                if (exportData.ToDate != null)
                {
                    toDate = GetDateTimeOffsetWithCompanyTimezone(exportData.ToDate.Value);
                }

                var result = await _connector.GetCall(url: $"/v1/export/auditinglog/{exportData.ExportType.ToLower()}/{companyid}?starttimestamp={fromDate.ToString("dd-MM-yyyy HH:mm:ss")}&endtimestamp={toDate.ToString("dd-MM-yyyy HH:mm:ss")}", Response.Body);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Response.Clear();
                    Response.StatusCode = (int)result.StatusCode;
                    await Response.WriteAsync("There was a issue getting you file, please contact the system administrator.");
                }
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("No rights for getting file.");
            }
        }

        [NonAction]
        public DateTimeOffset GetDateTimeOffsetWithCompanyTimezone(DateTime dateTime)
        {
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

            var tz = User.FindFirst(System.Security.Claims.ClaimTypes.Country)?.Value ?? "Europe/Amsterdam";
            TimeZoneInfo timezone = TZConvert.EzFindTimeZoneInfoById(tz);


            return new DateTimeOffset(dateTime, timezone.GetUtcOffset(dateTime));
        }
    }
}
