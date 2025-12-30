using System.Collections.Generic;
using WebApp.Models.Settings;

namespace WebApp.ViewModels
{
    public class CompanySettingsViewModel : BaseViewModel
    {
        public List<SettingModel> Settings { get; set; }
    }
}
