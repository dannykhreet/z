using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.Filters
{
    public class ValidTokenFilter : IAsyncAuthorizationFilter
    {
        readonly IApiConnector _connector;

        public ValidTokenFilter(IApiConnector connector)
        {
            _connector = connector;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var result = await _connector.PostCall(Logic.Constants.Authentication.CheckAuthentication, String.Empty);
            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var currenturl = context.HttpContext.Request.Path;
                await context.HttpContext.SignOutAsync();
                context.Result = new ChallengeResult();
            }
        }
    }
}