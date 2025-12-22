using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApp.Filters;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Logic.Services;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(ValidTokenFilter)), Authorize(Roles = "manager,shift_leader")]
    public class BaseController : Controller
    {
        public const string AUTHORIZATION_ADMINISTRATOR_ROLES = "superuser,staff";

        private ApplicationSettings _applicationSettings;
        protected string _locale;

        protected readonly ILanguageService _language;
        protected readonly IConfigurationHelper _configurationHelper;
        protected readonly IApplicationSettingsHelper _applicationSettingsHelper;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IInboxService _inboxService;

        /// <summary>
        /// IsAdminCompany; true/false, filled by checking if a users companyid is the configured (appsettings/AdministratorAdminCompany) administrator company.
        /// Based on this the UI will have extra or less features to specifically configuring general settings and specific company settings that the companies can not set them selfs.
        /// </summary>
        public bool IsAdminCompany { get; set; }
        public bool IsOnCompanyVPN { get; set; }


        public BaseController(ILanguageService language, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService)
        {
            _language = language;
            _configurationHelper = configurationHelper;
            _applicationSettingsHelper = applicationSettingsHelper;
            _httpContextAccessor = httpContextAccessor;
            _inboxService = inboxService;

            _locale = GetCookie(Constants.General.LANGUAGE_COOKIE_STORAGE_KEY) ?? "en-US";

            IsAdminCompany = _configurationHelper.GetValueAsInteger("AppSettings:AdministratorAdminCompany") == httpContextAccessor.HttpContext.User.GetProfile()?.Company?.Id;
            try
            {
                IsOnCompanyVPN = new[] { "DISABLED", httpContextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"].ToString() }.Contains(_configurationHelper.GetValueAsString("AppSettings:AdministratorAdminCompany"));
#pragma warning disable CS0168 // Do not catch general exception types
            }
            catch (Exception ex)
            {
                IsOnCompanyVPN = false;
            }
            _inboxService = inboxService;
#pragma warning restore CS0168 // Do not catch general exception types

        }

        /// <summary>
        /// GetApplicationSettings; Get ApplicationSettings for use within controllers.
        /// </summary>
        /// <returns></returns>
        public async Task<ApplicationSettings> GetApplicationSettings()
        {
            if (_applicationSettings != null) return _applicationSettings;

            _applicationSettings = await _applicationSettingsHelper.GetApplicationSettings();
            return _applicationSettings;
        }

        /// <summary>
        /// set the cookie
        /// </summary>
        /// <param name="key">key (unique indentifier)</param>
        /// <param name="value">value to store in cookie object</param>
        /// <param name="expireTime">expiration time</param>
        public void SetCookie(string key, string value, int? expireTime = null)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Today.AddMonths(1);

            _httpContextAccessor?.HttpContext?.Response?.Cookies?.Append(key, value, option);
        }

        /// <summary>
        /// GetCookie; Get cookie value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetCookie(string key)
        {
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request != null && _httpContextAccessor.HttpContext.Request.Cookies != null)
            {
                return _httpContextAccessor.HttpContext.Request.Cookies[key];
            }
            else return string.Empty;
        }

        /// <summary>
        /// Delete the key
        /// </summary>
        /// <param name="key">Key</param>
        public void RemoveCookie(string key)
        {
            _httpContextAccessor?.HttpContext?.Response?.Cookies?.Delete(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetSecurityKey(string key)
        {
            var securityKey = Convert.ToBase64String(KeyDerivation.Pbkdf2(
             password: key,
             salt: Encoding.ASCII.GetBytes(Startup.ApplicationVersion),
             prf: KeyDerivationPrf.HMACSHA256,
             iterationCount: 100000,
             numBytesRequested: 256 / 8));
            return securityKey;
        }

        public async Task<int> GetInboxCount()
        {
            ApplicationSettings applicationSettings = await GetApplicationSettings();
            return applicationSettings?.Features?.TemplateSharingEnabled == true ? await _inboxService.GetSharedTemplatesCount() : 0;
        }
    }
}
