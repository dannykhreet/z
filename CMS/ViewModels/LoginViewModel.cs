using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string LoginName { get; set; }

        [Required]
        public string LoginPassword { get; set; }

        public string ApiConnectionKey { get; set; }

        public bool IsValid { get; set; }

        public bool LoggedIn { get; set; }

        public bool UseApiKey { get; set; }

        public bool UseExternalLogin { get; set; }
        public string Message { get; set; }

        public System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Languages { get; set; }
        public string Locale { get; set; }
        public Dictionary<string, string> CmsLanguage { get; set; }
    }
}
