using EZGO.Api.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using WebApp.Logic;
using WebApp.Models.Shared;

namespace WebApp.ViewModels
{
    public abstract class BaseViewModel
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        public Dictionary<string,string> CmsLanguage { get; set; }

        //Question: why required?
        [Required]
        public FilterViewModel Filter { get; set; }
        public string FiltersPreset { get; set; }
        public string PageTitle { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; } //TODO maybe create message type for return e.g. error, succes etc..
        public string ApiBaseUrl { get; set; }
        public string Locale { get; set; }
        public int NewComments { get; set; }
        public int NewInboxItemsCount { get; set; }

        /// <summary>
        /// IsAdminCompany; true/false, filled by checking if a users companyid is the configured (appsettings/AdministratorAdminCompany) administrator company.
        /// Based on this the UI will have extra or less features to specifically configuring general settings and specific company settings that the companies can not set them selfs.
        /// </summary>
        public bool IsAdminCompany { get; set; }

        /// <summary>
        /// ApplicationSettings; Get application settings, containing all information that is needed (access rights, default data) etc for display of several functionalities within the portal.
        /// </summary>
        public ApplicationSettings ApplicationSettings { get; set; }

        /// <summary>
        /// ApplicationVersion; Application version; Loaded from constructor
        /// </summary>
        public string ApplicationVersion { get; set; }
        
        /// <summary>
        /// Enable search filters
        /// </summary>
        public bool EnableSearchFilters { get; set; }

        /// <summary>
        /// Enabling Auditing filter;
        /// </summary>
        public bool EnablingAuditing { get; set; }

        /// <summary>
        /// SecurityKey; Can be used for validating data. 
        /// </summary>
        public string SecurityKey { get; set; }
        
        /// <summary>
        /// Enable on certain pages for extraction of json data of underlaying objects. Can be used by service users.
        /// </summary>
        public bool EnableJsonExtraction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the removal of objects is enabled.
        /// Normally this is only enabled for service users. 
        /// This parameter can be filled on different views if needed. 
        /// </summary>
        public bool EnableRemovalOfObject { get; set; }

        public BaseViewModel()
        {
            Filter = new FilterViewModel();
            ApplicationVersion = Startup.ApplicationVersion;
        }
    }
}
