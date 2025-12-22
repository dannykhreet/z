using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Settings;
using WebApp.ViewModels;

namespace WebApp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public List<SettingModel> Settings { get; set; }
        public List<Company> Companies { get; set; }
        public string ConnectionIP { get; set; }
    }
}
