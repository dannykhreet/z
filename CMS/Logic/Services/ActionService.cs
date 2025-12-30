using EZGO.Api.Models.Stats;
using EZGO.CMS.LIB.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Action;
using WebApp.Models.User;

namespace WebApp.Logic.Services
{
    public class ActionService : IActionService
    {
        private readonly IApiConnector _connector;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActionService(IApiConnector connector, IHttpContextAccessor httpContextAccessor)
        {
            _connector = connector;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> MyCommentsCount()
        {
            var CurrentUser = new UserProfile();
            var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            if (!string.IsNullOrWhiteSpace(userprofile))
            {
                CurrentUser = JsonConvert.DeserializeObject<UserProfile>(userprofile);
            }
            if (CurrentUser != null)
            {
                

                var resultrelatedstatistics = await _connector.GetCall(@"/v1/actioncomments/unread");
                if (resultrelatedstatistics.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    List<ActionCommentViewedStatsItem> actionCommentViewedStatsItems = resultrelatedstatistics.Message.ToObjectFromJson<List<ActionCommentViewedStatsItem>>();
                    return actionCommentViewedStatsItems.Where(x => x.CommentsNotViewedNr > 0).Count();
                }
            }
            return 0;
        }
    }
}
