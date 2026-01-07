using System.Collections.Generic;
using WebApp.Models.Settings;
using WebApp.Models.Skills;

namespace WebApp.ViewModels
{
    public class CompanySettingsViewModel : BaseViewModel
    {
        public List<SettingModel> Settings { get; set; }
        public SkillMatrixLegendConfiguration LegendConfiguration { get; set; }
    }
}
