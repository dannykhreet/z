using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Models.Company;

namespace WebApp.ViewModels
{
    public class CompanyViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public List<CompanyShiftModel> Shifts { get; set; }
        public List<SettingResourceItem> Settings { get; set; }
        public UserProfile AdministratorUser { get; set; }
        public List<DatabaseTimezoneItem> Timezones { get; set; }
        public string CompanyLocale { get; set; }
        public List<Holding> Holdings { get; set; }
        public int HoldingId { get; set; }
        public string HoldingCompanySecurityGUID { get; set; }
        public List<int> HoldingUnitIds { get; set; }
        public List<HoldingUnit> HoldingUnits { get; set; }
        public List<UserProfile> Users { get; set; }
        public List<CompanyFeaturesModel> CompaniesFeatures { get; set; }
        public SetupCompanySettings CompanySettings { get; set; }
        public List<Country> Countries { get; set; }
        public string CountryIso2 { get; set; }
        /// <summary>
        /// DatawarehouseCompanyUser; Possible company datawarehouse user, these are users which have a specific account in the datawarehouse system
        /// </summary>
        public EZ.Api.DataWarehouse.Models.User DatawarehouseCompanyUser { get; set; }
        /// <summary>
        /// DatawarehouseCompanyUser; Possible holding datawarehouse user, these are users which have a specific account in the datawarehouse system based on the holding of the company
        /// </summary>
        public EZ.Api.DataWarehouse.Models.User DatawarehouseHoldingUser { get; set; }
        /// <summary>
        /// EnableDatawarehouseUserMutation: check if management of user is enabled on the environment. 
        /// </summary>
        public bool EnableDatawarehouseUserMutation { get; set; }

    }
}
