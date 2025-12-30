using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Logic.Interfaces
{
    public interface IApplicationSettingsHelper
    {
        public Task<ApplicationSettings> GetApplicationSettings();
    }
}
