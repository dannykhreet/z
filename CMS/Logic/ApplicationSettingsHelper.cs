using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;

namespace WebApp.Logic
{
    public class ApplicationSettingsHelper : IApplicationSettingsHelper
    {
        private readonly IApiConnector _connector;
        private ApplicationSettings _applicationSettings;
        public ApplicationSettingsHelper(IApiConnector apiConnector)
        {
            _connector = apiConnector;
        }

        //TODO Use BaseController.GetApplicationSettings() instead, see references to this method
        public async Task<ApplicationSettings> GetApplicationSettings()
        {
            var applicationSettings = new ApplicationSettings();
            if(_applicationSettings != null && !string.IsNullOrEmpty(_applicationSettings.RunningEnvironment))
            {
                applicationSettings = _applicationSettings;
                return applicationSettings;
            }

            //TODO add caching.

            try
            {
                var applicationSettingsResult = await _connector.GetCall("/v1/app/settings");
                if (applicationSettingsResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    applicationSettings = applicationSettingsResult.Message.ToObjectFromJson<ApplicationSettings>();
                    _applicationSettings = applicationSettings;
                }
#pragma warning disable CS0168 // Do not catch general exception types
            } catch (Exception ex)
            {

            }
#pragma warning restore CS0168 // Do not catch general exception types

            return applicationSettings;
        }
    }
}
