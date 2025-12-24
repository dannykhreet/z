using System.Collections.Generic;
using WebApp.Models.Settings;
using WebApp.Models.Skills;

namespace WebApp.ViewModels
{
    public class CompanySettingsViewModel : BaseViewModel
    {
        public List<SettingModel> Settings { get; set; }

        /// <summary>
        /// Skills Matrix Legend configuration for customizable skill level display
        /// </summary>
        public SkillMatrixLegendConfiguration LegendConfiguration { get; set; }

        /// <summary>
        /// Indicates if the current user can edit the legend (team leaders and managers only)
        /// </summary>
        public bool CanEditLegend { get; set; }
    }
}
