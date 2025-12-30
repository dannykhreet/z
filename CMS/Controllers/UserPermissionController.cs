using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EZGO.Api.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Logic.Interfaces;
using WebApp.Models.User;
using WebApp.ViewModels;
using System.Text.RegularExpressions;
using System.Security.Claims;
using EZGO.Api.Models.Authentication;
using System.Linq;
using System;
using EZGO.Api.Models;
using EZGO.CMS.LIB.Interfaces;
using EZGO.CMS.LIB.Extensions;
using UserProfile = WebApp.Models.User.UserProfile;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using WebApp.Models;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using EZGO.Api.Models.Users;
using Amazon;

/// <summary>
/// ENTIRE PAGE AND STRUCTURE MUST BE REFACTORED
/// -> individual calls must be based on individual API calls not on collection calls and manually filtering on Id
/// -> not on all routes all data must be retrieved. Currently more it less it does. 
/// -> All double not user functionality and or views must be merged or removed so it works properly. 
/// -> All routes and views must have a full route in attribute and full location to view within the method!
/// </summary>
/// 
namespace WebApp.Controllers
{
    public class UserPermissionController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly string _apiBaseUrl;

        public UserPermissionController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _apiBaseUrl = configurationHelper.GetValueAsString("AppSettings:ApiUri");
        }

        // GET: UserPermission
        public async Task<ActionResult> Index()
        {
            var output = new UserPermissionViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();

            if (!((User.IsInRole("serviceaccount") || ((output.ApplicationSettings?.Features?.RoleManagementEnabled.HasValue == false || output.ApplicationSettings?.Features?.RoleManagementEnabled.Value == false) && User.IsInRole("manager")) || (output.ApplicationSettings?.Features?.RoleManagementEnabled.HasValue == true && output.ApplicationSettings?.Features?.RoleManagementEnabled.Value == true && User.IsInRole("usermanager")))))
            {
                return NoContent(); //no rights to part of site so return no content;
            }

            output.NewInboxItemsCount = await GetInboxCount();
            output.Locale = _locale;
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.Module = FilterViewModel.ApplicationModules.USERS;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "User overview";

            // current user
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();

            var arearesult = await _connector.GetCall(Logic.Constants.Task.GetTaskAreas);
            if (arearesult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }

            var result = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.UserProfiles = JsonConvert.DeserializeObject<List<UserProfile>>(result.Message);
            }

            return View(output);
        }

        [HttpPost]
        public async Task<ActionResult> LoadData()
        {
            //weird method??
            var model = new UserPermissionViewModel();
            model.NewInboxItemsCount = await GetInboxCount();
            model.Filter.Module = FilterViewModel.ApplicationModules.USERS;
            model.Locale = _locale;
            model.PageTitle = "User overview";
            model.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);

            var result = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                model.UserProfiles = JsonConvert.DeserializeObject<List<UserProfile>>(result.Message);
            }

            try
            {
                var draw = HttpContext.Request.Form["draw"].FirstOrDefault();

                // Skip number of Rows count
                var start = Request.Form["start"].FirstOrDefault();

                // Paging Length 10,20
                var length = Request.Form["length"].FirstOrDefault();

                // Sort Column Name
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();

                // Sort Column Direction (asc, desc)
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                // Search Value from (Search box)
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                //Paging Size (10, 20, 50,100)
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // getting all Customer data
                var users = (from user in model.UserProfiles
                             select user);
                //Sorting
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    // users = users.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                //Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    users = users.Where(m => m.FirstName == searchValue || m.LastName == searchValue || m.Role == searchValue);
                }

                //total number of rows counts
                recordsTotal = users.Count();
                //Paging
                var data = users.Skip(skip).Take(pageSize).ToList();
                //Returning Json Data
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region - user profile details -
        // GET: UserPermission/Details/5
        public async Task<ActionResult> Details(int id)
        {
            UserProfileViewModel output = new UserProfileViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();

            if (!((User.IsInRole("serviceaccount") || ((output.ApplicationSettings?.Features?.RoleManagementEnabled.HasValue == false || output.ApplicationSettings?.Features?.RoleManagementEnabled.Value == false) && User.IsInRole("manager")) || (output.ApplicationSettings?.Features?.RoleManagementEnabled.HasValue == true && output.ApplicationSettings?.Features?.RoleManagementEnabled.Value == true && User.IsInRole("usermanager")))))
            {
                return NoContent(); //no rights to part of site so return no content;
            }

            output.NewInboxItemsCount = await GetInboxCount();
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.Module = FilterViewModel.ApplicationModules.USERS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");

            //set specific boolean for display changes (show/not show certain parts.
            if (output.ApplicationSettings?.Features?.RoleManagementEnabled.HasValue == true && output.ApplicationSettings?.Features?.RoleManagementEnabled == true && (User.IsInRole("serviceaccount") || User.IsInRole("rolemanager")))
            {
                output.EnableRoleManagement = true;
            }

            if (output.ApplicationSettings?.Features?.UserExtendedDetailsEnabled.HasValue == true && output.ApplicationSettings?.Features?.UserExtendedDetailsEnabled.Value == true)
            {
                output.EnableExtendedUserManagement = true;
            }

            //getting areas
            var arearesult = await _connector.GetCall(Logic.Constants.Task.GetTaskAreas);
            if (arearesult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }

            if (id > 0)
            {
                string uri = string.Format(output.EnableRoleManagement ? string.Concat(Logic.Constants.User.UserProfileUrl, ",roles") : Logic.Constants.User.UserProfileUrl, id.ToString());
                var result = await _connector.GetCall(uri);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.UserProfile = JsonConvert.DeserializeObject<UserProfile>(result.Message);
                }

                if(output.ApplicationSettings?.Features?.UserExtendedDetailsEnabled == true && id > 0)
                {

                    var resultExt = await _connector.GetCall(string.Concat("/v1/userprofile/",id,"/extendeddetails"));
                    if (resultExt.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        output.UserExtendedDetails = JsonConvert.DeserializeObject<UserExtendedDetails>(resultExt.Message);
                    }
                } else
                {
                    output.UserExtendedDetails = new UserExtendedDetails();
                }

                if (output.UserProfile != null) output.UserProfile.UserExtendedDetails = output.UserExtendedDetails;

                 var profilesresult = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
                if (profilesresult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.UserProfiles = JsonConvert.DeserializeObject<List<UserProfile>>(profilesresult.Message);
                }

                //check if user has any actions
                var actionresultCreatedBy = await _connector.GetCall(string.Format(Logic.Constants.Action.GetActionsUserSpecificCreatedByUrl, id));
                if (actionresultCreatedBy.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.CurrentUserProfileHasActions = (JsonConvert.DeserializeObject<List<ActionsAction>>(actionresultCreatedBy.Message).Count > 0);
                }

                //check if user is assigned to any actions, if user already has actions check can be ignored.
                if (!output.CurrentUserProfileHasActions)
                {
                    var actionresultAssignedTo = await _connector.GetCall(string.Format(Logic.Constants.Action.GetActionsUserSpecificAssignedToUrl, id));
                    if (actionresultAssignedTo.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        output.CurrentUserProfileHasActions = (JsonConvert.DeserializeObject<List<ActionsAction>>(actionresultAssignedTo.Message).Count > 0);
                    }
                }
                //currentUserProfileHasActions


                if (_configurationHelper.GetValueAsBool("AppSettings:EnableAdvancedSecurityFeatures")) {
                    //check for tfa setup 
                    var tfaResult = await _connector.GetCall("/v1/tools/security/twofactor"); //TODO change make dynamic based on only generate tfa on own profile page. 
                    if (tfaResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        output.TwoFactorySetup = JsonConvert.DeserializeObject<TfaSetup>(tfaResult.Message);
                        output.TwoFactorySetup.Enabled = true;
                    }
                }
            }
            else
            {
                output.UserProfile = new UserProfile(); //create empty profile
            }

            if (output.UserProfile != null && output.UserProfile.AllowedAreas != null)
            {
                foreach (var area in output.UserProfile.AllowedAreas)
                {
                    var foundItem = FindArea(areas: output.Areas, id: area.Id);
                    if (foundItem != null && foundItem.Id > 0)
                    {
                        area.FullDisplayName = foundItem.FullDisplayName;
                    }
                }
            }

            if (output.UserProfile != null && output.UserProfile.DisplayAreas != null)
            {
                foreach (var area in output.UserProfile.DisplayAreas)
                {
                    var foundItem = FindArea(areas: output.Areas, id: area.Id);
                    if (foundItem != null && foundItem.Id > 0)
                    {
                        area.FullDisplayName = foundItem.FullDisplayName;
                    }
                }
            }

            if(output.UserProfile!=null)
            {
                if(User.GetProfile() != null && User.GetProfile().Company != null)
                {
                    output.SecurityKey = this.GetSecurityKey(string.Concat("USERPROFILE.PID.", output.UserProfile.Id, ".CID.", User.GetProfile().Company.Id));
                }

                output.UserProfile.ApplicationSettings = output.ApplicationSettings;
            }

            //sanity check, if user profile should be loaded, but isn't show no content. Either non-existing user is loaded or user is not active
            if (id > 0 && (output.UserProfile == null || output.UserProfile.Id == 0))
            {
                return NoContent(); //no rights to part of site so return no content;
            }

            return View("UserProfile", output);
        }

        #endregion

        #region - posts -
        [HttpPost]
        public async Task<ActionResult> Check([FromRoute] string type, [FromBody] string value)
        {
            if (type == "email")
            {
                var result = await CheckEmailExists(email: value);
                return StatusCode(result ? (int)HttpStatusCode.Conflict : (int)HttpStatusCode.OK, "Email already exists".ToJsonFromObject());
            };

            if (type == "username")
            {
                var result = await CheckUsernameExists(userName: value);
                return StatusCode(result ? (int)HttpStatusCode.Conflict : (int)HttpStatusCode.OK, "Username already exists".ToJsonFromObject());
            };

            return StatusCode((int)HttpStatusCode.NoContent);
        }


        //
        [HttpGet]
        [Route("/userpermission/generate/password")]
        public async Task<ActionResult> GeneratePassword()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, string.Concat(DateTime.Now.ToString("HHmm"), "EzF", string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(DateTime.Now.ToString("sssmmHHddMMyyyy"))).Select(x => x.ToString("X2"))).Substring(0, 6)));
        }

        [HttpPost]
        [Route("/userpermission/changepassword")]
        public async Task<ActionResult> ChangePassword([FromBody] WebApp.Models.Authentication.ChangePassword changedPasswords)
        {
            if (changedPasswords != null)
            {
                var SecurityKey = this.GetSecurityKey(string.Concat("USERPROFILE.PID.", changedPasswords.UserId, ".CID.", User.GetProfile().Company.Id));
                if (SecurityKey != changedPasswords.ValidationKey)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid password data.".ToJsonFromObject());
                }

                var passwordCorrect = await CheckPassword(changedPasswords.NewPassword);
                if (!passwordCorrect || changedPasswords.NewPassword != changedPasswords.NewPasswordValidation) return StatusCode((int)HttpStatusCode.Conflict, "Password is invalid.".ToJsonFromObject());

                if (await ChangeUserPassword(changedPasswords: changedPasswords))
                {
                    return StatusCode((int)HttpStatusCode.OK, "Ok");
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, "Error while trying to change password.");
                }
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("/userpermission/deleteuser")]
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUser deleteUser)
        {
            if (deleteUser != null && deleteUser.UserId > 0)
            {
                var SecurityKey = this.GetSecurityKey(string.Concat("USERPROFILE.PID.", deleteUser.UserId, ".CID.", User.GetProfile().Company.Id));
                if (SecurityKey != deleteUser.ValidationKey)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid user data.".ToJsonFromObject());
                }

                bool continueSetActive = false;
                if (deleteUser.SuccessorId.HasValue && deleteUser.SuccessorId.Value > 0)
                {
                    var setSuccessorResult = await _connector.PostCall(string.Concat("/v1/userprofile/setsuccessor/", deleteUser.UserId), deleteUser.SuccessorId.Value.ToJsonFromObject());
                    if (setSuccessorResult.StatusCode == HttpStatusCode.OK)
                    {
                        continueSetActive = true;
                    }
                    else
                    {
                        continueSetActive = false;
                    }
                }
                else
                {
                    continueSetActive = true;
                }

                if (continueSetActive)
                {
                    var setActiveResult = await _connector.PostCall(string.Concat("/v1/userprofile/setactive/", deleteUser.UserId), false.ToJsonFromObject()); ;
                    if (setActiveResult.StatusCode == HttpStatusCode.OK)
                    {
                        return StatusCode((int)HttpStatusCode.OK, "Ok");
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, setActiveResult.Message.ToJsonFromObject());
                    }
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, "Error while trying to remove user.".ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }


        [HttpPost]
        [Route("/userpermission/addchangeuser/{userid}")]
        public async Task<ActionResult> AddChangeUser([FromBody] UserProfile userprofile, [FromRoute] int? userid = null)
        {
            var settings = await this.GetApplicationSettings();

            if (userprofile != null)
            {
                if (userprofile.Id <= 0 && !CheckUsername(userprofile.UserName)) return StatusCode((int)HttpStatusCode.Conflict, "Username must contain at least 6 characters and not start or end with a space character.".ToJsonFromObject());

                if (userprofile.Id > 0 && !CheckChangeUsername(userprofile.UserName)) return StatusCode((int)HttpStatusCode.Conflict, "Username must contain at least 2 characters and can not start or end with a space character.".ToJsonFromObject());

                var validUserName = await CheckUsernameExists(userprofile.UserName, userprofile.Id > 0 ? userprofile.Id : new Nullable<int>());
                if (!validUserName) return StatusCode((int)HttpStatusCode.Conflict, "Username already exists or is invalid.".ToJsonFromObject());

                var validEmail = await CheckEmailExists(userprofile.Email, userprofile.Id > 0 ? userprofile.Id : new Nullable<int>());
                if (!validEmail) return StatusCode((int)HttpStatusCode.Conflict, "Email already exists or is invalid.".ToJsonFromObject());

                var validUPN = await CheckUPN(userprofile.UPN, userprofile.Id > 0 ? userprofile.Id : new Nullable<int>());
                if (!validUPN) return StatusCode((int)HttpStatusCode.Conflict, "Upn already exists or is invalid.".ToJsonFromObject());

                if(!string.IsNullOrEmpty(userprofile.SapPmUsername) && userprofile.SapPmUsername.Length > 12)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, "Sap Pm username was provided, but can only be up to 12 characters long.");
                }

                if (userid.HasValue && userprofile.Id != userprofile.Id)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid user data.".ToJsonFromObject());
                }

                var SecurityKey = this.GetSecurityKey(string.Concat("USERPROFILE.PID.", userprofile.Id, ".CID.", User.GetProfile().Company.Id));
                if (SecurityKey != userprofile.ValidationKey)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Invalid user data.".ToJsonFromObject());
                }

                //TODO create extension for convert
                EZGO.Api.Models.UserProfile profile = new EZGO.Api.Models.UserProfile();
                profile.FirstName = userprofile.FirstName;
                profile.LastName = userprofile.LastName;
                profile.Picture = userprofile.Picture;
                profile.Role = userprofile.Role;
                profile.UPN = userprofile.UPN;
                profile.Id = userprofile.Id;
                profile.Email = userprofile.Email;
                profile.UserName = userprofile.UserName;
                profile.IsTagManager = userprofile.IsTagManager;
                profile.SapPmUsername = userprofile.SapPmUsername;

                if(userprofile.Roles != null)
                {
                    profile.Roles = userprofile.Roles;
                }

                //START FIX FOR SELECTED SUBAREAS
                //get list of areas to check parent ids
                List<Area> areas = new List<Area>();
                var arearesult = await _connector.GetCall(Logic.Constants.Task.GetTaskAreas);
                if (arearesult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
                }

                //remove the areas from selected areas if it is a descendant from an already selected ara
                if (userprofile.AllowedAreas != null && userprofile.AllowedAreas.Count > 0)
                {
                    HashSet<AllowedAreaModel> areasToRemove = new HashSet<AllowedAreaModel>();
                    foreach (var selectedArea in userprofile.AllowedAreas)
                    {
                        RemoveSelectedChildren(selectedArea.Id, userprofile.AllowedAreas, areas, areasToRemove);
                    }

                    foreach (var areaToRemove in areasToRemove)
                    {
                        userprofile.AllowedAreas.Remove(areaToRemove);
                    }
                }
                //END FIX FOR SELECTED SUBAREAS

                if (userprofile.AllowedAreas != null && userprofile.AllowedAreas.Count > 0)
                {
                    profile.AllowedAreas = new List<EZGO.Api.Models.Basic.AreaBasic>();
                    foreach (var areaItem in userprofile.AllowedAreas)
                    {
                        profile.AllowedAreas.Add(new EZGO.Api.Models.Basic.AreaBasic() { Id = areaItem.Id });
                    }
                }

                if (userprofile.Id > 0)
                {
                    if (profile.AllowedAreas == null)
                    {
                        profile.AllowedAreas = new List<EZGO.Api.Models.Basic.AreaBasic>();
                    }

                    //update existing
                    var changeResult = await _connector.PostCall(string.Concat("/v1/userprofile/change/", profile.Id), profile.ToJsonFromObject());
                    if (changeResult.StatusCode == HttpStatusCode.OK)
                    {
                        if (settings?.Features?.UserExtendedDetailsEnabled == true && userprofile.UserExtendedDetails != null)
                        {
                            await SaveUserExtendedDetails(userId: profile.Id, userprofile.UserExtendedDetails);
                        }

                        if(settings?.Features?.RoleManagementEnabled == true && userprofile.Roles != null && userprofile.Role != "basic")
                        {
                            await SaveUserRoles(userProfile: profile);
                        }
                        
                        return StatusCode((int)HttpStatusCode.OK, "Ok");
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, changeResult.Message.ToJsonFromObject());
                    }
                }
                else
                {
                    var passwordCorrect = await CheckPassword(userprofile.Password);
                    if (!passwordCorrect || userprofile.Password != userprofile.PasswordConfirmation) return StatusCode((int)HttpStatusCode.Conflict, "Password is invalid.".ToJsonFromObject());

                    //add new
                    var result = await _connector.PostCall("/v1/userprofile/add", profile.ToJsonFromObject());
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var id = Convert.ToInt32(result.Message);
                        if (id > 0)
                        {
                            if(settings?.Features?.UserExtendedDetailsEnabled == true && userprofile.UserExtendedDetails != null)
                            {
                                await SaveUserExtendedDetails(userId: id, userprofile.UserExtendedDetails);
                            }

                            if (settings?.Features?.RoleManagementEnabled == true && userprofile.Roles != null && userprofile.Role != "basic") //only save roles (for now) with managers and shiftleaders
                            {
                                profile.Id = id;
                                await SaveUserRoles(userProfile: profile);
                            }

                            ChangePassword changedPassword = new ChangePassword();
                            changedPassword.NewPassword = userprofile.Password;
                            changedPassword.NewPasswordValidation = userprofile.PasswordConfirmation;
                            changedPassword.CompanyId = 0; //TODO fill, currently handled by API
                            changedPassword.UserId = id;

                            var changeResult = await ChangeUserPassword(changedPasswords: changedPassword);
                            if (changeResult == true)
                            {
                                return StatusCode((int)HttpStatusCode.OK, id.ToJsonFromObject());
                            }
                            else
                            {
                                return StatusCode((int)HttpStatusCode.Conflict, "User created, but password not saved.");
                            }
                        }
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.Conflict, result.Message.ToJsonFromObject());
                    }
                }
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="extendedDetails"></param>
        /// <returns></returns>
        private async Task<bool> SaveUserExtendedDetails (int userId ,UserExtendedDetails extendedDetails)
        {
            if(extendedDetails != null)
            {
                extendedDetails.UserId = userId;

                var result = await _connector.PostCall("/v1/userprofile/change/extendeddetails", extendedDetails.ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// SaveUserRoles; Save user roles, this will be called separately. Later on will be integrated when user management will be changed. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        private async Task<bool> SaveUserRoles(EZGO.Api.Models.UserProfile userProfile)
        {
            if (userProfile != null && userProfile.Id > 0 && userProfile.Roles != null) //only update existing users.
            {
                var result = await _connector.PostCall("/v1/userprofile/change/roles", userProfile.ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Recursively removes selected children from the selected areas
        /// This method was created as part of the fix for selected subareas, if the issue is fully resolved, this can be removed
        /// </summary>
        /// <param name="areaId">Id of area to select</param>
        /// <param name="selectedAreas">The list of selected areas</param>
        /// <param name="areas">List of Areas for complete data purposes</param>
        /// <param name="areasToRemove">Set of items to be filled with areas that can be removed</param>
        [NonAction]
        private void RemoveSelectedChildren(int areaId, List<AllowedAreaModel> selectedAreas, List<Area> areas, HashSet<AllowedAreaModel> areasToRemove)
        {
            //get data for the parent area
            var selectedAreaData = FindArea(areas, areaId);

            foreach (var child in selectedAreaData.Children)
            {
                //check if any of the children are selected
                var selectedChildren = selectedAreas.Where(selectedArea => selectedArea.Id == child.Id);
                foreach (var selectedChild in selectedChildren)
                {
                    //add any selected children to the list of items to be removed
                    areasToRemove.Add(selectedChild);
                }
                //also check children of children
                RemoveSelectedChildren(child.Id, selectedAreas, areas, areasToRemove);
            }
        }

        [HttpPost]
        [Route("/userpermission/details/upload")]
        public async Task<string> Upload(IFormCollection data)
        {
            foreach (IFormFile item in data.Files)
            {
                //var fileContent = item;
                if (item != null && item.Length > 0)
                {
                    // get a stream
                    using (var ms = new MemoryStream())
                    {
                        item.CopyTo(ms);
                        var fileBytes = ms.ToArray();

                        using var form = new MultipartFormDataContent();
                        using var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        form.Add(fileContent, "file", Path.GetFileName(item.FileName));

                        int mediaType = 14;//profile image

                        var endpoint = string.Format(Logic.Constants.Checklist.UploadPictureUrl, mediaType);

                        ApiResponse filepath = await _connector.PostCall(endpoint, form);
                        string output = filepath.Message;
                        if (data["filekind"] != "video")
                        {
                            output = filepath.Message.Replace("media/", "");
                        }
                        return output;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }
        #endregion

        /// <summary>
        /// FindArea; recursively find a area for data retrieval.
        /// </summary>
        /// <param name="areas"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [NonAction]
        private Area FindArea(List<Area> areas, int id)
        {
            if (areas.Where(x => x.Id == id).Any())
            {
                return areas.Where(x => x.Id == id).FirstOrDefault();
            }
            else
            {
                foreach (var item in areas)
                {
                    if (item.Children != null)
                    {
                        var foundItem = FindArea(item.Children, id);
                        if (foundItem != null)
                        {
                            return foundItem;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// ChangeUserPassword; Changes user password.
        /// </summary>
        /// <param name="changedPasswords">ChangedPassword object, containing the new data.</param>
        /// <returns>true/false depending on outcome.</returns>
        [NonAction]
        private async Task<bool> ChangeUserPassword(ChangePassword changedPasswords)
        {

            string posturl = string.Format(Logic.Constants.User.ChangePasswordUrl, changedPasswords.UserId);

            if (changedPasswords.NewPassword != changedPasswords.NewPasswordValidation)
            {
                return false;
            }

            try
            {
                var postoutput = await _connector.PostCall(posturl, changedPasswords.ToJsonFromObject());
                if (postoutput.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //todo add logging?
            }

            return false;
        }

        [NonAction]
        private async Task<bool> CheckUsernameExists(string userName, int? userId = null)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            //Following routes on the API can be used for checking properties (email, upn and username are supported)
            //[Route("userprofile/{userid}/check/{propertyname}")]
            //[Route("userprofile/check/{propertyname}")]

            var output = await _connector.PostCall(userId.HasValue ? string.Format("/v1/userprofile/{0}/check/username", userId.Value) : "/v1/userprofile/check/username", userName.ToJsonFromObject());
            if (output.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        [NonAction]
        private async Task<bool> CheckEmailExists(string email, int? userId = null)
        {
            if (string.IsNullOrEmpty(email)) return true; //email adres can be empty, so return true

            //Following routes on the API can be used for checking properties (email, upn and username are supported)
            //[Route("userprofile/{userid}/check/{propertyname}")]
            //[Route("userprofile/check/{propertyname}")]

            var output = await _connector.PostCall(userId.HasValue ? string.Format("/v1/userprofile/{0}/check/email", userId.Value) : "/v1/userprofile/check/email", email.ToJsonFromObject());
            if (output.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        [NonAction]
        private async Task<bool> CheckUPN(string upn, int? userId = null)
        {
            if (string.IsNullOrEmpty(upn)) return true; //email adres can be empty, so return true

            //Following routes on the API can be used for checking properties (email, upn and username are supported)
            //[Route("userprofile/{userid}/check/{propertyname}")]
            //[Route("userprofile/check/{propertyname}")]


            var output = await _connector.PostCall(userId.HasValue ? string.Format("/v1/userprofile/{0}/check/upn", userId.Value) : "/v1/userprofile/check/upn", upn.ToJsonFromObject());
            if (output.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }


        [NonAction]
        private async Task<bool> CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            var output = await _connector.GetCall(Logic.Constants.AppSettings.ApplicationSettingsUri);
            if (output.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var appsettings = JsonConvert.DeserializeObject<ApplicationSettings>(output.Message);
                var match = Regex.Match(password, appsettings.PasswordValidationRegEx).Success;
                return match;
            }

            return false;
        }

        [NonAction]
        private bool CheckUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            if (userName.FirstOrDefault().ToString() == " ") return false;

            if (userName.LastOrDefault().ToString() == " ") return false;

            if (userName.Length < 6) return false;

            return true;
        }

        [NonAction]
        private bool CheckChangeUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            if (userName.FirstOrDefault().ToString() == " ") return false;

            if (userName.LastOrDefault().ToString() == " ") return false;

            if (userName.Length < 2) return false;

            return true;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestChange(int id)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLatestUsersUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChanges(int id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingUsersUrl, id, limit, offset));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChangesByUser(int id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingByUserUrl, id, limit, offset));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }
    }
}