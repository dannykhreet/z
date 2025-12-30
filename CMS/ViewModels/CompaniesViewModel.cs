using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Company;
using WebApp.Models.Settings;

namespace WebApp.ViewModels
{
    public class CompaniesViewModel : BaseViewModel
    {
        public List<Company> Companies { get; set; }
        public List<DatabaseTimezoneItem> Timezones { get; set; }
        public List<SettingModel> Settings { get; set; }
        public List<Holding> Holdings { get; set; }
        public Dictionary<int, List<string>> CompanySettings { get; set; }
        public List<CompanyFeaturesModel> CompaniesFeatures { get; set; }
        public bool EnableCompanyManagement { get; set; }
        public bool EnableHoldingManagement { get; set; }
    }
}
