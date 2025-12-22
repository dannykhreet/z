using EZGO.Api.Models.Marketplace;
using EZGO.CMS.LIB.Email;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.MarketPlace)]
    public class MarketplaceController : BaseController
    {
        private readonly ILogger<MarketplaceController> _logger;
        private readonly ILanguageService _languageService;

        public MarketplaceController(ILogger<MarketplaceController> logger, ILanguageService language, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _languageService = language;
        }

        public async Task<IActionResult> Index()
        {
            var output = new MarketplaceViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Checklist overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.MARKETPLACE;
            output.Locale = _locale;
            output.MarketPlaceItems = GetMarketPlaceItems();
            output.ApplicationSettings = await this.GetApplicationSettings();
            return PartialView(output);
        }

        [NonAction]
        public List<MarketPlaceItem> GetMarketPlaceItems()
        {
            return new List<MarketPlaceItem>()
            {
                new MarketPlaceItem
                {
                    Id = 1,
                    Name = "SAP Plant Maintenance",
                    Description = "Share actions between EZ-GO and SAP Plant Maintenance module.",
                    Picture = "images/marketplace/Sap.png"
                },
                new MarketPlaceItem
                {
                    Id = 3,
                    Name = "Azure Active Directory",
                    Description = "Authenticate your existing user base on the EZ-GO Platform.",
                    Picture = "images/marketplace/Azure%20active.png"
                },
                new MarketPlaceItem
                {
                    Id = 5,
                    Name = "EZ-GO for Android",
                    Description = "Use the EZ-GO platform from any Android enabled device.",
                    Picture = "images/marketplace/Play%20store.png"
                },
                new MarketPlaceItem
                {
                    Id = 7,
                    Name = "EZ-GO for Web browsers",
                    Description = "Use the EZ-GO Platform from any internet enabled device by using a modern browser.",
                    Picture = "images/marketplace/pwa.png"
                },
                new MarketPlaceItem
                {
                    Id = 8,
                    Name = "EZ-GO for iOS",
                    Description = "Use the EZ-GO platform from any iOS enabled device.",
                    Picture = "images/marketplace/App%20store.png"
                },
                new MarketPlaceItem
                {
                    Id = 9,
                    Name = "Ultimo connector",
                    Description = "Share actions between EZ-GO and the Ultimo module.",
                    Picture = "images/marketplace/Ultimo.png"
                },
                new MarketPlaceItem
                {
                    Id = 10,
                    Name = "Microsoft Power BI",
                    Description = "Visualise all EZ-GO important data in well organised dashboards using Power BI.",
                    Picture = "images/marketplace/Power%20Bi.png"
                },
                new MarketPlaceItem
                {
                    Id = 11,
                    Name = "Action on the spot",
                    Description = "Act on deviations from standards with the action management module.",
                    Picture = "images/marketplace/Action%20on%20the%20spot.png"
                },
                new MarketPlaceItem
                {
                    Id = 12,
                    Name = "Advanced package",
                    Description = "Get more out of the EZ-GO platform with the Advanced Package. This includes features as; Open Fields, Value Registration and more.",
                    Picture = "images/marketplace/Advanced%20Package.png"
                },
                new MarketPlaceItem
                {
                    Id = 13,
                    Name = "Automatic E-mail Notifications",
                    Description = "Receive e-mail notifications when critical issues are reported via the EZ-GO platform, so you can intervene or support to fix the issue.",
                    Picture = "images/marketplace/Automatic%20Email.png"
                },
                new MarketPlaceItem
                {
                    Id = 14,
                    Name = "EZ-Feed",
                    Description = "With EZ Feed you keep everyone on the shop floor updated concerning important matters and changes, which improves the involvement of operators and team leaders.",
                    Picture = "images/marketplace/EZ-Feed.png"
                },
                new MarketPlaceItem
                {
                    Id = 15,
                    Name = "Open Fields",
                    Description = "Add important information to checklists or audits with the open field feature. Think of batch codes, barcode numbers, article numbers and etc.",
                    Picture = "images/marketplace/Open%20Fields.png"
                },
                new MarketPlaceItem
                {
                    Id = 16,
                    Name = "Skill assessments",
                    Description = "With a skills matrix, you have insight in where to develop your team. " +
                    "They can take skills assessments in missing skills, record the results in the digital skills matrix and the overview is immediately up-to-date.",
                    Picture = "images/marketplace/Skills%20Assesments.png"
                },
                new MarketPlaceItem
                {
                    Id = 17,
                    Name = "Skills Matrix",
                    Description = "A skills matrix shows which skills a team has and which are still missing. This makes scheduling and training easy.",
                    Picture = "images/marketplace/Skills%20Matrix.png"
                },
                new MarketPlaceItem
                {
                    Id = 18,
                    Name = "Value Registration",
                    Description = "Register and save important values, like temperature, pressure or pH value with the EZ-GO platform. ",
                    Picture = "images/marketplace/Value%20Registration.png"
                },
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(string emailFrom, string requestInfoTitle, string requestInfoDescription)
        {
            var userProfile = User.GetProfile();

            if (emailFrom.Equals(string.Empty))
                return BadRequest();

            string body = $"<table style=\"border: 0; padding: 5px\">" +
                $"<tr><td align=\"right\"><strong>Company name:</strong></td><td>{userProfile.Company.Name}</td></tr> " +
                $"<tr><td align=\"right\"><strong>Company id:</strong></td><td>{userProfile.Company.Id}</td></tr>" +
                $"<tr><td align=\"right\"><strong>User name:</strong></td><td>{userProfile.FirstName} {userProfile.LastName}</td></tr>" +
                $"<tr><td align=\"right\"><strong>User email:</strong></td><td>{emailFrom}</td></tr>" +
                $"<tr><td align=\"right\"><strong>Requested for:</strong></td><td>{requestInfoTitle}</td></tr>" +
                $"<tr><td align=\"right\"><strong>Description:</strong></td><td>{requestInfoDescription}</td></tr>" +
                $"</table>";

            await Task.CompletedTask;

            AmazonSesClient sesClient = new AmazonSesClient();
            if (sesClient.SendMail("hi@ezfactory.nl", null, "Marketplace information request", body))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Mail not send, error occurred.");

            }

        }
    }
}
