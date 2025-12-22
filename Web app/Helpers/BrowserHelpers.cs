using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Helpers
{
    public static class BrowserHelpers
    {
        public static string IsValidBrowser(HttpContext httpContext)
        {
            if(httpContext.Request.Headers["User-Agent"].ToString().ToUpper().Contains("TRIDENT"))
            {
                return "You are using an unsupported browser; Please us a modern up to date browser (Up to date versions of Chrome, Firefox, Edge, Safari).";
            }
            return "";
        }
    }
}
