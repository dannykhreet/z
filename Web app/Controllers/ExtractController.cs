using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using System.Security.Claims;
using WebApp.Models.User;
using EZGO.CMS.LIB.Extensions;

namespace WebApp.Controllers
{
    public class ExtractController : BaseController
    {
        private readonly ILogger<ExtractController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public ExtractController(ILogger<ExtractController> logger, IApiConnector connector, ILanguageService language, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [HttpGet]
        [Route("/extract/checklisttemplate/{id}")]
        public async Task GetChecklistTemplate([FromRoute] int id)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                if (retrievedUser == null || !retrievedUser.IsServiceAccount)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.WriteAsync("");
                }
                else
                {
                    var result = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetChecklistTemplateDetails, id));
                    Response.Clear();
                    Response.ContentType = "application/json";

                    if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {

                        Response.StatusCode = (int)result.StatusCode;
                        await Response.WriteAsync(result.Message);
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        await Response.WriteAsync("{}");
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("");
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        [HttpGet]
        [Route("/extract/audittemplate/{id}")]
        public async Task GetAuditTemplate([FromRoute] int id)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                if (retrievedUser == null || !retrievedUser.IsServiceAccount)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.WriteAsync("");
                } else
                {
                    var result = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetAuditTemplatesDetailUrl, id));
                    Response.Clear();
                    Response.ContentType = "application/json";

                    if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {
                       
                        Response.StatusCode = (int)result.StatusCode;
                        await Response.WriteAsync(result.Message);
                    } else
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        await Response.WriteAsync("{}");
                    }   
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("");
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        [HttpGet]
        [Route("/extract/tasktemplate/{id}")]
        public async Task GetTaskTemplate([FromRoute] int id)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                if (retrievedUser == null || !retrievedUser.IsServiceAccount)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.WriteAsync("");
                }
                else
                {
                    var result = await _connector.GetCall(string.Format(Logic.Constants.Task.GetTaskTemplateDetailUrl, id));
                    Response.Clear();
                    Response.ContentType = "application/json";

                    if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {

                        Response.StatusCode = (int)result.StatusCode;
                        await Response.WriteAsync(result.Message);
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        await Response.WriteAsync("{}");
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("");
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        [HttpGet]
        [Route("/extract/assessmenttemplate/{id}")]
        public async Task GetAssessmentTemplate([FromRoute] int id)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                if (retrievedUser == null || !retrievedUser.IsServiceAccount)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.WriteAsync("");
                }
                else
                {
                    var result = await _connector.GetCall(string.Format(Logic.Constants.Assessments.GetAssessmentTemplateForCreation, id));
                    Response.Clear();
                    Response.ContentType = "application/json";

                    if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {

                        Response.StatusCode = (int)result.StatusCode;
                        await Response.WriteAsync(result.Message);
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        await Response.WriteAsync("{}");
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("");
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        [HttpGet]
        [Route("/extract/workinstructiontemplate/{id}")]
        [Route("/extract/workinstruction/{id}")]
        public async Task GetWorkinstruction([FromRoute] int id)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                if (retrievedUser == null || !retrievedUser.IsServiceAccount)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.WriteAsync("");
                }
                else
                {
                    var result = await _connector.GetCall(string.Format(Logic.Constants.WorkInstructions.WorkInstructionTemplateDetailsUrl, id));
                    Response.Clear();
                    Response.ContentType = "application/json";

                    if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {

                        Response.StatusCode = (int)result.StatusCode;
                        await Response.WriteAsync(result.Message);
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        await Response.WriteAsync("{}");
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("");
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        #region - template versions -
        [HttpGet]
        [Route("/extract/{templatetype}/{templateid}/version/{version}")]
        public async Task GetTemplateVersion([FromRoute]int templateid, [FromRoute] string templatetype, [FromRoute]string version)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                if (retrievedUser == null || !retrievedUser.IsServiceAccount)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.WriteAsync("");
                }
                else
                {
                    var result = await _connector.GetCall(string.Format("/v1/export/{2}/{0}/version/{1}", templateid, version, templatetype));
                    Response.Clear();
                    Response.ContentType = "application/json";

                    if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {

                        Response.StatusCode = (int)result.StatusCode;
                        await Response.WriteAsync(result.Message);
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.OK;
                        await Response.WriteAsync("{}");
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await Response.WriteAsync("");
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        #endregion
    }
}
