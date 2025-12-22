using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Action;
using WebApp.Models.Comment;
using WebApp.Models.User;
using WebApp.ViewModels;

/// <summary>
/// ENTIRE PAGE AND STRUCTURE MUST BE REFACTORED
/// -> individual calls must be based on individual API calls not on collectioncalls and manually filtering on Id
/// -> not on all routes all data must be retrieved. Currenlty more it less it does. 
/// -> All double not user functioanlity and or views must be merged or removed so it works properly. 
/// -> All routes and views must have a full route in attribute and full location to view within the method!
/// </summary>

namespace WebApp.Controllers
{
    public class ActionController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ActionController(ILogger<HomeController> logger,
            IApiConnector connector,
            ILanguageService language,
            IWebHostEnvironment hostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IConfigurationHelper configurationHelper,
            IApplicationSettingsHelper applicationSettingsHelper,
            IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            // DI
            _logger = logger;
            _connector = connector;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [Route("/action")]
        public async Task<IActionResult> Index([FromQuery(Name = "filter")] string filter)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            //output = await buildActions();
            output.Tags.TagGroups = await GetTagGroups();
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.TagGroups = await this.GetTagGroupsForFilter();

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;

            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

            // current user
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                output.CurrentUser = JsonConvert.DeserializeObject<Models.User.UserProfile>(userprofile);
                output.Filter.CurrentUser = output.CurrentUser;
            }

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resultUsers.Message))
            {
                output.Filter.ActionUsers = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.UserProfile>>(resultUsers.Message)).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            }

            var resultAreas = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            if (resultAreas.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resultAreas.Message))
            {
                output.Filter.ActionAreas = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.Area>>(resultAreas.Message)).OrderBy(x => x.FullDisplayName).ToList();
            }

            switch (filter)
            {
                case "unviewed":
                    var filterobject = new
                    {
                        actiontype = "action",
                        unviewedcomments = new string[] { "unviewed" },
                        involvement = "my"
                    };
                    output.FiltersPreset = filterobject.ToJsonFromObject();
                    break;
                case "open":
                    var filterobject2 = new
                    {
                        actionstatus = new string[] { "overdue", "unresolved" },
                        actiontype = "action"
                    };
                    output.FiltersPreset = filterobject2.ToJsonFromObject();
                    break;
                case "action":
                    var filterobject3 = new
                    {
                        actiontype = "action"
                    };
                    output.FiltersPreset = filterobject3.ToJsonFromObject();
                    break;
                case "comment":
                    var filterobject4 = new
                    {
                        actiontype = "comment"
                    };
                    output.FiltersPreset = filterobject4.ToJsonFromObject();
                    break;
                default:
                    break;
            }

            output.UseTaskIdFiltering = false;

            return View(output);
        }

        [HttpGet]
        [Route("/action/getactions")]
        public async Task<IActionResult> GetActions([FromQuery] string filterText, [FromQuery] string tagids, [FromQuery] string status, [FromQuery] string involvement, [FromQuery] string assignedUserIds, [FromQuery] string assignedAreaIds, [FromQuery] string createdFrom, [FromQuery] string createdTo, [FromQuery] string resolvedFrom, [FromQuery] string resolvedTo, [FromQuery] string overdueFrom, [FromQuery] string overdueTo, [FromQuery] bool? hasUnviewedComments, [FromQuery] int? parentAreaId, [FromQuery] string type, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] int? taskId)
        {
            ActionViewModel output = new ActionViewModel();

            //get actions from api
            var uriParams = new List<string>();
            if (!string.IsNullOrEmpty(filterText))
            {
                uriParams.Add("filtertext=" + System.Web.HttpUtility.UrlEncode(filterText));
            }

            if (!string.IsNullOrEmpty(tagids))
            {
                uriParams.Add("tagids=" + tagids);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var statuses = status.Split(',');
                foreach (var actionStatus in statuses)
                {
                    if (actionStatus == "resolved")
                    {
                        uriParams.Add("isresolved=" + true.ToString().ToLower());
                    }
                    else if (actionStatus == "unresolved")
                    {
                        uriParams.Add("isunresolved=" + true.ToString().ToLower());
                    }
                    else if (actionStatus == "overdue")
                    {
                        uriParams.Add("isoverdue=" + true.ToString().ToLower());
                    }
                }
            }

            if (!string.IsNullOrEmpty(involvement))
            {
                //started
                //involved
                //started or involved (my actions)
                //all actions (dont set filter)
                if (involvement == "all")
                {
                    //do nothing (add no parameter)
                }
                else if (involvement == "started")
                {
                    uriParams.Add("createdbyid=" + User.GetProfile().Id.ToString());
                }
                else if (involvement == "involved")
                {
                    uriParams.Add("assigneduserid=" + User.GetProfile().Id.ToString());
                }
                else if (involvement == "my")
                {
                    uriParams.Add("createdByOrAssignedToMe=" + true.ToString().ToLower());
                }
            }

            if (!string.IsNullOrEmpty(assignedUserIds))
            {
                uriParams.Add("assigneduserids=" + assignedUserIds.ToString());
            }

            if (!string.IsNullOrEmpty(assignedAreaIds))
            {
                uriParams.Add("assignedareaids=" + assignedAreaIds.ToString());
            }

            if (hasUnviewedComments.HasValue)
            {
                uriParams.Add("hasunviewedcomments=" + hasUnviewedComments.ToString().ToLower());
            }

            if (!string.IsNullOrEmpty(createdFrom) && !string.IsNullOrEmpty(createdTo))
            {
                uriParams.Add("createdfrom=" + createdFrom);
                uriParams.Add("createdto=" + createdTo);
            }

            if (!string.IsNullOrEmpty(resolvedFrom) && !string.IsNullOrEmpty(resolvedTo))
            {
                uriParams.Add("resolvedfrom=" + resolvedFrom);
                uriParams.Add("resolvedto=" + resolvedTo);
            }

            if (!string.IsNullOrEmpty(overdueFrom) && !string.IsNullOrEmpty(overdueTo))
            {
                uriParams.Add("overduefrom=" + overdueFrom);
                uriParams.Add("overdueto=" + overdueTo);
            }

            if (taskId != null)
            {
                uriParams.Add("taskId=" + taskId.ToString());
            }

            //parentAreaId
            if (parentAreaId > 0)
            {
                uriParams.Add("parentAreaId=" + parentAreaId);
            }

            //limit
            if (limit > 0)
            {
                uriParams.Add("limit=" + limit);
            }

            //offset
            if (offset > 0)
            {
                uriParams.Add("offset=" + offset);
            }

            if (output.ActionList == null)
            {
                output.ActionList = new List<ActionModel>();
            }

            if (output.CommentList == null)
            {
                output.CommentList = new List<CommentModel>();
            }

            // fill the actionslist
            if (type == "action")
            {
                string endpoint = @"/v1/actions?include=mainparent,tags";

                endpoint += "&" + string.Join("&", uriParams);

                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.ActionList = JsonConvert.DeserializeObject<List<ActionModel>>(result.Message);
                    //output.ActionList = SortActions(actionList);
                }
            }
            else if (type == "comment")
            {
                string endpoint = @"/v1/comments?include=mainparent,tags";

                endpoint += "&" + string.Join("&", uriParams);

                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.CommentList = JsonConvert.DeserializeObject<List<CommentModel>>(result.Message);
                    output.CommentList ??= new List<CommentModel>();

                    if (output.CommentList.Any())
                    {
                        output.CommentList = output.CommentList.OrderByDescending(x => x.CreatedAt).ToList();

                        output.ActionList.AddRange(output.CommentList.Select(x => x.ToAction()));
                        //output.ActionList = SortActions(output.ActionList);
                    }
                }

            }

            // current user
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                output.CurrentUser = JsonConvert.DeserializeObject<Models.User.UserProfile>(userprofile);
                output.Filter.CurrentUser = output.CurrentUser;
            }

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Filter.ActionUsers = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.UserProfile>>(resultUsers.Message)).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            }

            var resultAreas = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            if (resultAreas.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Filter.ActionAreas = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.Area>>(resultAreas.Message)).OrderBy(x => x.FullDisplayName).ToList();
            }

            // comments preparation
            output._ChatViewModel = new ChatViewModel { CurrentUser = output.CurrentUser, CmsLanguage = output.CmsLanguage, Locale = _locale };


            output.ApplicationSettings = await this.GetApplicationSettings();

            foreach (var action in output.ActionList)
            {
                action.ApplicationSettings = output.ApplicationSettings;
            }

            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;

            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

            return View("_overview", output);
        }

        //note: keep parameters exactly the same as getactions for convenience
        [HttpGet]
        [Route("/action/getactioncounts")]
        public async Task<IActionResult> GetActionCounts([FromQuery] string filterText, [FromQuery] string tagids, [FromQuery] string status, [FromQuery] string involvement, [FromQuery] string assignedUserIds, [FromQuery] string assignedAreaIds, [FromQuery] string createdFrom, [FromQuery] string createdTo, [FromQuery] string resolvedFrom, [FromQuery] string resolvedTo, [FromQuery] string overdueFrom, [FromQuery] string overdueTo, [FromQuery] bool? hasUnviewedComments, [FromQuery] int? parentAreaId, [FromQuery] string type, [FromQuery] int? taskId)
        {
            //get actioncounts from api
            var uriParams = new List<string>();
            if (!string.IsNullOrEmpty(filterText))
            {
                uriParams.Add("filtertext=" + System.Web.HttpUtility.UrlEncode(filterText));
            }

            if (!string.IsNullOrEmpty(tagids))
            {
                uriParams.Add("tagids=" + tagids);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var statuses = status.Split(',');
                foreach (var actionStatus in statuses)
                {
                    if (actionStatus == "resolved")
                    {
                        uriParams.Add("isresolved=" + true.ToString().ToLower());
                    }
                    else if (actionStatus == "unresolved")
                    {
                        uriParams.Add("isunresolved=" + true.ToString().ToLower());
                    }
                    else if (actionStatus == "overdue")
                    {
                        uriParams.Add("isoverdue=" + true.ToString().ToLower());
                    }
                }
            }

            if (!string.IsNullOrEmpty(involvement))
            {
                //started
                //involved
                //started or involved (my actions)
                //all actions (dont set filter)
                if (involvement == "all")
                {
                    //do nothing (add no parameter)
                }
                else if (involvement == "started")
                {
                    uriParams.Add("createdbyid=" + User.GetProfile().Id.ToString());
                }
                else if (involvement == "involved")
                {
                    uriParams.Add("assigneduserid=" + User.GetProfile().Id.ToString());
                }
                else if (involvement == "my")
                {
                    uriParams.Add("createdByOrAssignedToMe=" + true.ToString().ToLower());
                }
            }

            if (!string.IsNullOrEmpty(assignedUserIds))
            {
                uriParams.Add("assigneduserids=" + assignedUserIds.ToString());
            }

            if (!string.IsNullOrEmpty(assignedAreaIds))
            {
                uriParams.Add("assignedareaids=" + assignedAreaIds.ToString());
            }

            if (hasUnviewedComments.HasValue)
            {
                uriParams.Add("hasunviewedcomments=" + hasUnviewedComments.ToString().ToLower());
            }

            if (!string.IsNullOrEmpty(createdFrom) && !string.IsNullOrEmpty(createdTo))
            {
                uriParams.Add("createdfrom=" + createdFrom);
                uriParams.Add("createdto=" + createdTo);
            }

            if (!string.IsNullOrEmpty(resolvedFrom) && !string.IsNullOrEmpty(resolvedTo))
            {
                uriParams.Add("resolvedfrom=" + resolvedFrom);
                uriParams.Add("resolvedto=" + resolvedTo);
            }

            if (!string.IsNullOrEmpty(overdueFrom) && !string.IsNullOrEmpty(overdueTo))
            {
                uriParams.Add("overduefrom=" + overdueFrom);
                uriParams.Add("overdueto=" + overdueTo);
            }

            //parentAreaId
            if (parentAreaId > 0)
            {
                uriParams.Add("parentAreaId=" + parentAreaId);
            }

            // fill the actionslist
            if (type == "action")
            {
                string endpoint = @"/v1/actions_counts";
                if (uriParams.Count > 0)
                {
                    endpoint += "?" + string.Join("&", uriParams);
                }
                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var stats = JsonConvert.DeserializeObject<ActionCountStatistics>(result.Message);
                    return Ok(stats.TotalCount);
                }
            }
            else if (type == "comment")
            {
                string endpoint = @"/v1/comments_counts";

                if (uriParams.Count > 0)
                {
                    endpoint += "?" + string.Join("&", uriParams);
                }

                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var stats = JsonConvert.DeserializeObject<CommentCountStatistics>(result.Message);
                    return Ok(stats.TotalCount);
                }
            }

            return BadRequest();
        }

        // GET: Action/Task/12345/63739
        [Route("/action/taskactions/{taskid}/{templateid?}/{goBack}")]
        public async Task<IActionResult> TaskActions(int taskid, bool goBack)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            output.Locale = _locale;

            var actionList = new List<ActionsAction>();

            string actionsEndpoint = string.Format(Logic.Constants.Action.GetTaskActionsUrl, taskid.ToString());

            var actionsResult = await _connector.GetCall(actionsEndpoint);
            if (actionsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.ActionList = JsonConvert.DeserializeObject<List<ActionModel>>(actionsResult.Message);

                if (output.ActionList.Any())
                {
                    output.ActionList = output.ActionList.OrderByDescending(x => x.CreatedAt).ToList();
                    output.ActionList = SortActions(output.ActionList);
                }
            }

            output.GoBack = goBack;

            // WebApp.Models.Comment Comments
            string endpoint = string.Format(Logic.Constants.Comment.GetTaskCommentsUrl, taskid.ToString());
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CommentList = JsonConvert.DeserializeObject<List<CommentModel>>(result.Message);
                output.CommentList ??= new List<CommentModel>();

                if (output.CommentList.Any())
                {
                    output.CommentList = output.CommentList.OrderByDescending(x => x.CreatedAt).ToList();
                    output.ActionList.AddRange(output.CommentList.Select(x => x.ToAction()));
                    output.ActionList = SortActions(output.ActionList);
                }
            }

            output.TaskId = taskid;
            output.ApplicationSettings = await this.GetApplicationSettings();
            foreach (var action in output.ActionList)
            {
                action.ApplicationSettings = output.ApplicationSettings;
            }

            output.UseTaskIdFiltering = true;
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

            return View("Index", output);
        }

        [Route("/action/comment/{id}/{taskid}")]
        public async Task<ActionResult> Comment(int id, int taskid)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            output = await buildActions();
            output.Locale = _locale;

            string endpoint = string.Format(Logic.Constants.Comment.GetTaskCommentsUrl, taskid.ToString());
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CommentList = JsonConvert.DeserializeObject<List<CommentModel>>(result.Message);
                output.CommentList ??= new List<CommentModel>();

                if (output.CommentList.Any())
                {
                    output.CurrentComment = output.CommentList.FirstOrDefault(x => x.Id == id);
                }
            }
            output.TaskId = taskid;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;
            return View(output);
        }

        [Route("/action/comment/{id}")]
        public async Task<ActionResult> GetComment(int id)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            output = await buildActions();
            output.Locale = _locale;

            string endpoint = string.Format(Logic.Constants.Comment.GetCommentUrl, id.ToString());
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentComment = JsonConvert.DeserializeObject<CommentModel>(result.Message);
            }
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;
            return View("Comment", output);
        }

        // GET: Action/Details/5
        [Route("/action/details/{id}/{taskid?}")]
        public async Task<ActionResult> Details(int id, int? taskid)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

            // current user
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                output.CurrentUser = JsonConvert.DeserializeObject<Models.User.UserProfile>(userprofile);
                output.Filter.CurrentUser = output.CurrentUser;
            }

            // comments preparation
            output._ChatViewModel = new ChatViewModel { CurrentUser = output.CurrentUser, CmsLanguage = output.CmsLanguage, Locale = _locale };

            ActionModel myAction = null;
            var actionResult = await _connector.GetCall(string.Format(Logic.Constants.Action.GetActionDetails, id));
            if (actionResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                myAction = JsonConvert.DeserializeObject<ActionModel>(actionResult.Message);
            }

            if (myAction != null)
            {
                output.CurrentAction = myAction;
                output._ChatViewModel.CurrentActionId = id;
                output._ChatViewModel.ActionIsResolved = output.CurrentAction.IsResolved;
                output._ChatViewModel._CommentsViewModel = new CommentsViewModel { CurrentUser = output.CurrentUser, Locale = _locale };
                output._ChatViewModel._CommentsViewModel.ApplicationSettings = await this.GetApplicationSettings();
                output._ChatViewModel._CommentsViewModel.Comments = output.CurrentAction.Comments?.OrderBy(x => x.CreatedAt).ToList() ?? new List<ActionCommentModel>();
                output._ChatViewModel.UnviewedCount = output.CurrentAction.UnviewedCommentNr;
                output.Tags.itemId = myAction.Id;

                var userresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
                if (userresult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output._ChatViewModel._CommentsViewModel.Resources = JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(userresult.Message);
                    output._ChatViewModel._CommentsViewModel.Resources ??= new List<Models.User.UserProfile>();
                    if (output._ChatViewModel._CommentsViewModel.Resources.Any() && output._ChatViewModel._CommentsViewModel.Comments.Any())
                    {
                        output._ChatViewModel._CommentsViewModel.Comments = output._ChatViewModel._CommentsViewModel.CommentsWithImages();
                    }
                }

                if (myAction.SapPmNotificationConfig != null && myAction.SapPmNotificationConfig.FunctionalLocationId > 0)
                {
                    var locationEndpoint = "/v1/location/" + myAction.SapPmNotificationConfig.FunctionalLocationId;

                    var locationsResult = await _connector.GetCall(locationEndpoint);

                    if (locationsResult.StatusCode == HttpStatusCode.OK)
                    {
                        output.SelectedLocation = JsonConvert.DeserializeObject<SapPmLocation>(locationsResult.Message);
                    }
                }
            }
            else
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            output.TaskId = taskid.HasValue ? taskid.Value : 0;

            if (output.CurrentAction != null && output.CurrentAction.UnviewedCommentNr > 0)
            {
                await SetAllCommentsViewed(id);
            }

            if (output.CurrentUser != null) /*&& output.CurrentUser.IsServiceAccount)*/
            {
                //retrieve checklist templates, audit templates and task templates

                var responseChecklists = await _connector.GetCall(@"/v1/checklisttemplates?limit=0");
                if (responseChecklists.StatusCode == HttpStatusCode.OK)
                {
                    output.ChecklistTemplates = JsonConvert.DeserializeObject<List<ChecklistTemplate>>(responseChecklists.Message);
                    output.ChecklistTemplates = output.ChecklistTemplates.OrderBy(t => t.Id).ToList();
                }

                var responseAudits = await _connector.GetCall(@"/v1/audittemplates?limit=0");
                if (responseAudits.StatusCode == HttpStatusCode.OK)
                {
                    output.AuditTemplates = JsonConvert.DeserializeObject<List<AuditTemplate>>(responseAudits.Message);
                    output.AuditTemplates = output.AuditTemplates.OrderBy(t => t.Id).ToList();
                }

                var responseTasks = await _connector.GetCall(@"/v1/tasktemplates?limit=0");
                if (responseTasks.StatusCode == HttpStatusCode.OK)
                {
                    output.TaskTemplates = JsonConvert.DeserializeObject<List<TaskTemplate>>(responseTasks.Message);
                    output.TaskTemplates = output.TaskTemplates.OrderBy(t => t.Id).ToList();
                }
            }

            return View(output);
        }

        [Route("/action/comments/{actionid}/{count?}")]
        public async Task<ActionResult> GetComments(int actionid, int count = 0)
        {
            string endpoint = string.Format(Logic.Constants.Action.GetActionComments, actionid.ToString());
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                CommentsViewModel model = new CommentsViewModel
                {
                    Comments = JsonConvert.DeserializeObject<List<ActionCommentModel>>(result.Message),
                    CurrentUser = new Models.User.UserProfile { Id = User.GetProfile().Id },// output.CurrentUser,
                    Locale = _locale
                };

                model.ApplicationSettings = await this.GetApplicationSettings();
                model.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

                if (model.Comments.Any())
                {
                    if (model.Comments.Count > count)
                    {
                        var userresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
                        if (userresult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            model.Resources = JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(userresult.Message);
                            model.Resources ??= new List<Models.User.UserProfile>();
                            if (model.Resources.Any() && model.Comments.Any())
                            {
                                model.Comments = model.CommentsWithImages();
                                model.Comments = model.Comments?.OrderBy(x => x.CreatedAt).ToList();
                            }
                        }

                        await SetAllCommentsViewed(actionid);
                        return PartialView("~/Views/Action/_comments.cshtml", model);
                    }
                    return PartialView("~/Views/Action/_comments.cshtml", model);
                }
            }
            return NoContent();
        }

        [HttpPost]
        [Route("/action/postcomment")]
        public async Task<JsonResult> PostComment([FromBody] ActionCommentModel input)
        {
            if (ModelState.IsValid)
            {
                string endpoint = string.Format(Logic.Constants.Action.PostActionComment);
                var result = await _connector.PostCall(endpoint, input.ToJsonFromObject());
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Json(input.Comment);
                }
            }
            return null;
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("/action/setresolved/{actionid}")]
        public async Task<ActionResult> SetResolved(int actionid)
        {
            if (User.IsInRole("manager") || User.IsInRole("shift_leader"))
            {
                string endpoint = string.Format(Logic.Constants.Action.SetActionResolved, actionid);
                var result = await _connector.PostCall(endpoint, (true).ToJsonFromObject());
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //adding actioncomment is done in api nowadays so this can be removed:
                    //await SetCommentsForAction(null, null, true, actionid);

                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Details", new { id = actionid });
        }

        [Route("/action/settask")]
        public async Task<ActionResult> ChangeLinkAndPostActionComment(int actionid, int taskid)
        {
            if (actionid > 0 && taskid > 0)
            {
                //set task action if taskid > 0 and actionid > 0
                string endpoint = string.Format("/v1/action/settask/{0}", actionid);
                var result = await _connector.PostCall(endpoint, (taskid).ToJsonFromObject());

                //post action comment stating that the linked item has been changed
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    string endpoint2 = string.Format(Logic.Constants.Action.PostActionComment);


                    ActionCommentModel actionComment = new ActionCommentModel
                    {
                        ActionId = actionid,
                        Comment = $"Action/task link has been changed.",
                        UserId = User.GetProfile().Id
                    };
                    var result2 = await _connector.PostCall(endpoint2, actionComment.ToJsonFromObject());
                    if (result2.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return Ok();
                    }
                }
            }

            return BadRequest();
        }

        [Route("/action/new/{id?}")]
        [Route("/action/edit/{id?}")]
        public async Task<ActionResult> New(int id)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;
            output.Tags.TagGroups = await GetTagGroups();

            // current user
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                output.CurrentUser = JsonConvert.DeserializeObject<Models.User.UserProfile>(userprofile);
            }

            if (id != 0)
            {
                ActionModel myAction = null;
                var actionResult = await _connector.GetCall(string.Format(Logic.Constants.Action.GetActionDetails, id));
                if (actionResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    myAction = JsonConvert.DeserializeObject<ActionModel>(actionResult.Message);
                }

                if (myAction != null)
                {
                    output.EditAction = new ActionEditViewModel(myAction);
                    output.EditAction.AlreadySentToSapPm = myAction.IsResolved;
                    output.Tags.SelectedTags = myAction.Tags;
                    output.Tags.itemId = myAction.Id;
                }

            }

            if (output.ApplicationSettings.Features.MarketSapEnabled.HasValue && output.ApplicationSettings.Features.MarketSapEnabled.Value == true)
            {
                var notificationSettingsResult = await _connector.GetCall("/v1/notifications/options");
                if (notificationSettingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.NotificationOptions = JsonConvert.DeserializeObject<SapPmNotificationOptions>(notificationSettingsResult.Message);
                }
            }

            output.EditAction ??= new ActionEditViewModel(new ActionModel() { DueDate = DateTime.Today.AddDays(1) });

            var result = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(result.Message);
                output.EditAction.ResourcesUsers = users.Select(x => x.ToBasic()).OrderBy(x => x.Name).ToList();
            }

            var resultAreas = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            if (resultAreas.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var areas = JsonConvert.DeserializeObject<List<AreaBasicModel>>(resultAreas.Message);
                output.EditAction.ResourcesAreas = areas.OrderBy(x => x.NamePath).ToList();
            }

            var locationsEndpoint = "/v1/locations/children";
            var locationsResult = await _connector.GetCall(locationsEndpoint);
            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                output.SapPmFunctionalLocations = JsonConvert.DeserializeObject<List<SapPmLocation>>(locationsResult.Message);
            }

            output.EditAction.ResourcesUsers ??= new List<UserBasicModel>();
            output.EditAction.CmsLanguage = output.CmsLanguage;
            output.EditAction.ApplicationSettings = output.ApplicationSettings;
            return View(output);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("/action/new/{id?}")]
        [Route("/action/edit/{id?}")]
        public async Task<ActionResult> New(ActionModel input, IFormCollection collection)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;
            output.Tags.TagGroups = await GetTagGroups();

            var users = new List<Models.User.UserProfile>();
            var result = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                users = JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(result.Message);
            }

            var areas = new List<AreaBasicModel>();
            var resultAreas = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            if (resultAreas.StatusCode == System.Net.HttpStatusCode.OK)
            {
                areas = JsonConvert.DeserializeObject<List<AreaBasicModel>>(resultAreas.Message);
            }

            var basicusers = users?.Select(x => x.ToBasic()).OrderBy(x => x.Name).ToList();
            basicusers ??= new List<UserBasicModel>();

            List<int> resources = (StringValues.IsNullOrEmpty(collection["resources"]) ? new StringValues() : collection["resources"]).Select(x => int.Parse(x)).ToList();
            if (resources.Any())
            {
                input.AssignedUsers = new List<UserBasicModel>();
                resources.ForEach(x =>
                {
                    var resource = basicusers.FirstOrDefault(b => b.Id == x);
                    if (resource != null)
                    {
                        input.AssignedUsers.Add(resource);
                    }
                });
            }

            List<int> resourcesAreas = (StringValues.IsNullOrEmpty(collection["resourcesareas"]) ? new StringValues() : collection["resourcesareas"]).Select(x => int.Parse(x)).ToList();
            if (resourcesAreas.Any())
            {
                input.AssignedAreas = new List<AreaBasicModel>();
                resourcesAreas.ForEach(x =>
                {
                    var resource = areas.FirstOrDefault(b => b.Id == x);
                    if (resource != null)
                    {
                        input.AssignedAreas.Add(resource);
                    }
                });
            }

            if (!StringValues.IsNullOrEmpty(collection["Tags"]))
            {
                var tagIds = collection["Tags"].ToString().Split(',');
                input.Tags = new List<Tag>();
                foreach (var tag in tagIds)
                {
                    input.Tags.Add(new Tag() { Id = int.Parse(tag) });
                }
                output.Tags.SelectedTags = input.Tags;
            }

            if (!string.IsNullOrEmpty(collection["SendToUltimo"]))
            {
                input.SendToUltimo = Convert.ToBoolean((collection["SendToUltimo"].ToString() == "true,false" || collection["SendToUltimo"].ToString() == "true") ? "true" : "false");
            }

            if (!string.IsNullOrEmpty(collection["SendToSapPm"]))
            {
                input.SendToSapPm = Convert.ToBoolean((collection["SendToSapPm"].ToString() == "true,false" || collection["SendToSapPm"].ToString() == "true") ? "true" : "false");

                if (string.IsNullOrEmpty(collection["SelectedFunctionalLocation"]))
                    ModelState.AddModelError("SelectedFunctionalLocation", "FunctionalLocation is required when sending to SAP PM");

                if (string.IsNullOrEmpty(collection["MaintPriority"]))
                    ModelState.AddModelError("MaintPriority", "MaintPriority is required when sending to SAP PM");

                if (string.IsNullOrEmpty(collection["NotificationType"]))
                    ModelState.AddModelError("NotificationType", "NotificationType is required when sending to SAP PM");

                if (string.IsNullOrEmpty(collection["NotificationTitle"]))
                    ModelState.AddModelError("NotificationTitle", "NotificationTitle is required when sending to SAP PM");

                if (ModelState["SelectedFunctionalLocation"]?.Errors?.Any() != true || 
                    ModelState["MaintPriority"]?.Errors?.Any() != true || 
                    ModelState["NotificationType"]?.Errors?.Any() != true || 
                    ModelState["NotificationTitle"]?.Errors?.Any() != true)
                {
                    input.SapPmNotificationConfig = new SapPmNotificationConfig()
                    {
                        FunctionalLocationId = Convert.ToInt32(collection["SelectedFunctionalLocation"]),
                        MaintPriority = Convert.ToString(collection["MaintPriority"]),
                        Notificationtype = Convert.ToString(collection["NotificationType"]),
                        NotificationTitle = Convert.ToString(collection["NotificationTitle"])
                    };
                }
            }

            if (ModelState.IsValid)
            {
                if (input.CompanyId == 0)
                {
                    input.CompanyId = User.GetProfile().Company.Id;
                }

                if (input.CreatedById == 0)
                {
                    input.CreatedById = User.GetProfile().Id;
                }

                var apiModel = input.ToApiModel().ToJsonFromObject();
                if (input.Id == 0)
                {
                    var myresult = await _connector.PostCall(string.Format(Logic.Constants.Action.PostNewAction), apiModel);
                    if (myresult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        input.Id = int.Parse(myresult.Message);
                    }
                }
                else
                {
                    ActionModel myAction = new ActionModel();

                    if (input.Id > 0)
                    {
                        var actionResult = await _connector.GetCall(string.Format(Logic.Constants.Action.GetActionDetails, input.Id));
                        if (actionResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            myAction = JsonConvert.DeserializeObject<ActionModel>(actionResult.Message);
                        }
                    }

                    output.CurrentAction = myAction;
                    output.Tags.itemId = myAction.Id;

                    if (!output.CurrentAction.IsResolved)
                    {
                        var actionresult = await _connector.PostCall(string.Format(Logic.Constants.Action.PostChangeAction, input.Id), apiModel);
                        await SetCommentsForAction(output?.CurrentAction ?? input, input);
                    }
                }

                return RedirectToAction("Index");
            }
            else
            {
                if (ModelState.ContainsKey("DueDate"))
                {
                    if (ModelState["DueDate"].Errors.Any())
                    {
                        string valMsg = string.Empty;
                        output.CmsLanguage?.TryGetValue(LanguageKeys.Action.EnsureFutureDateValidator, out valMsg);
                        if (!string.IsNullOrEmpty(valMsg))
                        {
                            ModelState["DueDate"].Errors.Clear();
                            ModelState.AddModelError("DueDate", valMsg);
                        }
                    }
                }
            }

            if (output.ApplicationSettings.Features.MarketSapEnabled.HasValue && output.ApplicationSettings.Features.MarketSapEnabled.Value == true)
            {
                var notificationSettingsResult = await _connector.GetCall("/v1/notifications/options");
                if (notificationSettingsResult.StatusCode == HttpStatusCode.OK)
                {
                    output.NotificationOptions = JsonConvert.DeserializeObject<SapPmNotificationOptions>(notificationSettingsResult.Message);
                }
            }

            var locationsEndpoint = "/v1/locations/children";
            var locationsResult = await _connector.GetCall(locationsEndpoint);
            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                output.SapPmFunctionalLocations = JsonConvert.DeserializeObject<List<SapPmLocation>>(locationsResult.Message);
            }

            output.EditAction = new ActionEditViewModel(input);
            output.EditAction.ResourcesAreas = areas.OrderBy(x => x.NamePath).ToList();
            output.EditAction.ResourcesUsers = basicusers;
            output.EditAction.CmsLanguage = output.CmsLanguage;
            output.EditAction.ApplicationSettings = output.ApplicationSettings;
            return View(output);
        }


        //_sapPmFunctionalLocations
        [HttpGet]
        [Route("/action/functionallocations")]
        public async Task<IActionResult> GetFunctionalLocations([FromQuery] string filterText = null, [FromQuery] int? functionalLocationId = null)
        {
            var locationsEndpoint = "/v1/locations/search";

            var uriParams = new List<string>();

            if (!string.IsNullOrEmpty(filterText))
            {
                uriParams.Add("searchText=" + System.Web.HttpUtility.UrlEncode(filterText));
            }

            if (functionalLocationId != null && functionalLocationId > 0)
            {
                uriParams.Add("functionalLocationId=" + functionalLocationId.Value);
            }

            locationsEndpoint += "?" + string.Join("&", uriParams);

            var locationsResult = await _connector.GetCall(locationsEndpoint);

            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                var locations = JsonConvert.DeserializeObject<List<SapPmLocation>>(locationsResult.Message);
                return PartialView("_sapPmFunctionalLocations", locations);
            }
            else
            {
                return PartialView("_sapPmFunctionalLocations", null);
            }
        }


        //_sapPmFunctionalLocations
        [HttpGet]
        [Route("/action/functionallocation/{functionalLocationId}")]
        public async Task<IActionResult> GetFunctionalLocationById([FromRoute] int? functionalLocationId = null)
        {
            var locationEndpoint = "/v1/location/";

            var uriParams = new List<string>();

            if (functionalLocationId != null && functionalLocationId > 0)
            {
                locationEndpoint += functionalLocationId.Value;
            }

            var locationsResult = await _connector.GetCall(locationEndpoint);

            if (locationsResult.StatusCode == HttpStatusCode.OK)
            {
                var location = JsonConvert.DeserializeObject<SapPmLocation>(locationsResult.Message);
                return Ok(location);
            }
            else
            {
                return StatusCode((int)locationsResult.StatusCode, "SAP PM functional location not found");
            }
        }


        [HttpPost]
        [RequestSizeLimit(52428800)]
        [Route("/action/new/media")]
        public async Task<string> PostMedia(IFormCollection collection)
        {
            string result = string.Empty;

            foreach (IFormFile item in collection.Files)
            {
                //var fileContent = item;
                if (item != null && item.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        item.CopyTo(ms);
                        var fileBytes = ms.ToArray();

                        using var form = new MultipartFormDataContent();

                        using var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                        form.Add(fileContent, "file", Path.GetFileName(item.FileName));
                        //form.Add(new StreamContent(item.OpenReadStream()), "File", item.FileName);

                        int.TryParse(collection["actionid"], out int id);

                        var endpoint = string.Format(Logic.Constants.Action.UploadPictureUrl, (int)MediaStorageTypeEnum.Actions, id);
                        switch (collection["target"])
                        {
                            case "video":
                                endpoint = string.Format(Logic.Constants.Action.UploadVideoUrl, (int)MediaStorageTypeEnum.Actions, id);
                                break;
                        }

                        ApiResponse filepath = await _connector.PostCall(endpoint, form);
                        string output = JsonConvert.DeserializeObject<string>(filepath.Message);

                        switch (collection["target"])
                        {
                            case "video":
                                break;
                            default:
                                output = output.Replace("media/", "");
                                break;
                        }

                        return output;
                    }
                }
            }
            return result;
        }

        [NonAction]
        private async Task SetCommentsForAction(ActionModel modifiedAction, ActionModel originalAction, bool IsResolved = false, int actionid = 0)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;

            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

            //this key doesnt even exist
            string changedItems = output.CmsLanguage.GetValue(LanguageKeys.Action.WasEditedTitle, "The following items of this action have been changed:"); //use ACTION_EDITED_TITLE instead?
            List<string> items = new List<string>();
            string item = string.Empty;

            if (modifiedAction != null && originalAction != null)
            {
                if (actionid == 0) { actionid = originalAction.Id; }
                if (!modifiedAction.Comment.Equals(originalAction.Comment))
                {
                    //this key doesnt even exist
                    item = output.CmsLanguage.GetValue(LanguageKeys.Action.CommentChangedTitle, "Comment");
                    items.Add(item);
                }

                if (!modifiedAction.Description.Equals(originalAction.Description))
                {
                    //this key doesnt even exist
                    item = output.CmsLanguage.GetValue(LanguageKeys.Action.DescriptionChangedTitle, "Description");
                    items.Add(item);
                }

                IEnumerable<int> oldUserIds = originalAction.AssignedUsers.Select(user => user.Id);
                IEnumerable<int> newUserIds = modifiedAction.AssignedUsers.Select(user => user.Id);

                if (oldUserIds.Union(newUserIds).Any(w => !(oldUserIds.Contains(w) && newUserIds.Contains(w))))
                {
                    //this key doesnt even exist
                    item = output.CmsLanguage.GetValue(LanguageKeys.Action.ResourcesChangedTitle, "Resources");
                    items.Add(item);
                }

                if (!modifiedAction.DueDate.Date.Equals(originalAction.DueDate.Date))
                {
                    //this key doesnt even exist
                    item = output.CmsLanguage.GetValue(LanguageKeys.Action.DueDateChangedTitle, "Due date");
                    items.Add(item);
                }



                List<string> oldMediaUrls = originalAction.Images ?? new List<string>();

                if (originalAction.Videos != null)
                    oldMediaUrls.AddRange(originalAction.Videos);

                List<string> newMediaUrls = modifiedAction.Images ?? new List<string>();

                if (modifiedAction.Videos != null)
                    newMediaUrls.AddRange(modifiedAction.Videos);

                if (oldMediaUrls.Union(newMediaUrls).Any(item => !(oldMediaUrls.Contains(item) && newMediaUrls.Contains(item))))
                {
                    //this key doesnt even exist
                    item = output.CmsLanguage.GetValue(LanguageKeys.Action.MediaChangedTitle, "Media");
                    items.Add(item);
                }
            }

            if (IsResolved)
            {
                //this key doesnt even exist
                item = output.CmsLanguage.GetValue(LanguageKeys.Action.IsResolvedChangedTitle, "Completed"); //use BASE_TEXT_COMPLETED instead?
                items.Add(item);
            }

            if (items.Any())
            {
                string comment = $"{changedItems} {items.Aggregate((a, b) => $"{a} , {b}")}";

                ActionCommentModel actionComment = new ActionCommentModel
                {
                    ActionId = actionid,
                    Comment = comment,
                    UserId = User.GetProfile().Id
                };

                string endpoint = string.Format(Logic.Constants.Action.PostActionComment);
                var result = await _connector.PostCall(endpoint, actionComment.ToJsonFromObject());
            }
        }

        [NonAction]
        private async Task<ActionViewModel> buildActions(int id = 0)
        {
            // ViewModel
            ActionViewModel output = new ActionViewModel();

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = _language.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;

            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.ACTIONS;

            // fill the actionslist
            var result = await _connector.GetCall(Logic.Constants.Action.GetActionsUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var actionList = JsonConvert.DeserializeObject<List<ActionModel>>(result.Message);
                output.ActionList = SortActions(actionList);
            }

            // get all comments and attach
            var comments = await _connector.GetCall(Logic.Constants.Action.GetAllComments);
            if (comments.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var commentList = JsonConvert.DeserializeObject<List<ActionCommentModel>>(comments.Message);
                commentList ??= new List<ActionCommentModel>();
                if (commentList.Any())
                {
                    output.ActionList = AddCommentsToActions(output.ActionList, commentList);
                }
            }

            var assignedusers = await _connector.GetCall(Logic.Constants.Action.GetAssignedUsers);
            if (assignedusers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var assigneduserlist = JsonConvert.DeserializeObject<List<AssignedUserModel>>(assignedusers.Message);
                assigneduserlist ??= new List<AssignedUserModel>();
                if (assigneduserlist.Any())
                {
                    output.ActionList = AddAssignedUsersToActions(output.ActionList, assigneduserlist);
                }
            }

            var assignedareas = await _connector.GetCall(Logic.Constants.Action.GetAssignedAreas);
            if (assignedareas.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var assignedarealist = JsonConvert.DeserializeObject<List<AssignedAreaModel>>(assignedareas.Message);
                assignedarealist ??= new List<AssignedAreaModel>();
                if (assignedarealist.Any())
                {
                    output.ActionList = AddAssignedAreasToActions(output.ActionList, assignedarealist);
                }
            }

            // current user
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                output.CurrentUser = JsonConvert.DeserializeObject<Models.User.UserProfile>(userprofile);
                output.Filter.CurrentUser = output.CurrentUser;
            }

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Filter.ActionUsers = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.UserProfile>>(resultUsers.Message)).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            }

            var resultAreas = await _connector.GetCall(Logic.Constants.General.AreaFlatList);
            if (resultAreas.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Filter.ActionAreas = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.Area>>(resultAreas.Message)).OrderBy(x => x.FullDisplayName).ToList();
            }

            // comments preparation
            output._ChatViewModel = new ChatViewModel { CurrentUser = output.CurrentUser, CmsLanguage = output.CmsLanguage, Locale = _locale };

            // get current action if id != 0
            if (id != 0)
            {
                output.CurrentAction = output.ActionList.FirstOrDefault(x => x.Id == id);
                output.Tags.SelectedTags = output.CurrentAction.Tags;
                output.Tags.itemId = output.CurrentAction.Id;
            }

            return output;
        }

        [NonAction]
        private static List<ActionModel> SortActions(IEnumerable<ActionModel> actions)
        {
            List<ActionModel> sortedActions = new List<ActionModel>();

            // unresolved by latest unread comment
            IOrderedEnumerable<ActionModel> actionsWithUnreadMessages = actions.Where(action => (action.UnviewedCommentNr) > 0 && !action.IsResolved || action.ActionType == EZGO.CMS.LIB.Enumerators.ActionTypeEnum.comment)
                .OrderByDescending(action => action.Comments.Max(comment => comment.ModifiedAt));

            // unresolved by due date
            IOrderedEnumerable<ActionModel> uncompletedActions = actions.Where(action => (action.UnviewedCommentNr) == 0 && !action.IsResolved && action.ActionType != EZGO.CMS.LIB.Enumerators.ActionTypeEnum.comment)
                .OrderBy(action => action.DueDate);

            // resolved by latest comment thenby due date
            IOrderedEnumerable<ActionModel> completedActions = actions.Where(action => action.IsResolved && action.ActionType != EZGO.CMS.LIB.Enumerators.ActionTypeEnum.comment)
                .OrderByDescending(action => action.Comments.Max(comment => comment.ModifiedAt)).ThenByDescending(action => action.DueDate);

            if (actionsWithUnreadMessages.Any())
                sortedActions.AddRange(actionsWithUnreadMessages);

            if (uncompletedActions.Any())
                sortedActions.AddRange(uncompletedActions);

            if (completedActions.Any())
                sortedActions.AddRange(completedActions);

            return sortedActions;
        }

        [NonAction]
        private static List<ActionModel> AddCommentsToActions(IEnumerable<ActionModel> actions, List<ActionCommentModel> comments)
        {
            if (actions != null)
            {
                foreach (ActionModel action in actions)
                {
                    action.Comments = comments.Where(item => item.ActionId == action.Id).ToList();
                    action.Comments ??= new List<ActionCommentModel>();
                }
            }

            return actions.ToList();
        }

        [NonAction]
        private static List<ActionModel> AddAssignedUsersToActions(IEnumerable<ActionModel> actions, List<AssignedUserModel> users)
        {
            foreach (ActionModel action in actions)
            {
                action.AssignedUsers = users.Where(item => item.ActionId == action.Id).Select(x => new UserBasicModel { Id = x.Id, Name = x.Name, Picture = x.Picture }).ToList();
                action.AssignedUsers ??= new List<UserBasicModel>();
            }

            return actions.ToList();
        }

        [NonAction]
        private static List<ActionModel> AddAssignedAreasToActions(IEnumerable<ActionModel> actions, List<AssignedAreaModel> areas)
        {
            foreach (ActionModel action in actions)
            {
                action.AssignedAreas = areas.Where(item => item.ActionId == action.Id).Select(x => new AreaBasicModel { Id = x.Id, Name = x.Name, FullDisplayName = x.Name, NamePath = x.NamePath }).ToList();
                action.AssignedAreas ??= new List<AreaBasicModel>();
            }

            return actions.ToList();
        }

        [NonAction]
        private async Task SetAllCommentsViewed(int actionid)
        {
            string endpoint = string.Format(Logic.Constants.Action.SetActionCommentsViewed, actionid);
            var result = await _connector.PostCall(endpoint, string.Empty);
        }

        [NonAction]
        private async Task<List<TagGroup>> GetTagGroups()
        {
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTagGroups);
            var tagGroups = new List<TagGroup>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                tagGroups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                //filter tags to only include tags that are allowed on actions
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Action))
                ).ToList());
            }

            return tagGroups;
        }

        [NonAction]
        private async Task<List<TagGroup>> GetTagGroupsForFilter()
        {
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTagGroups);
            var tagGroups = new List<TagGroup>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                tagGroups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                //filter tags to only include tags that are allowed on actions
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Action))).ToList());
            }

            return tagGroups;
        }
    }
}
