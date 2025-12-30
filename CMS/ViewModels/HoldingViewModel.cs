using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Setup;
using System.Collections.Generic;
using WebApp.Logic;

namespace WebApp.ViewModels
{
    public class HoldingViewModel : BaseViewModel
    {
        public int HoldingId { get; set; }
        public Holding Holding { get; set; }
        public List<SettingResourceItem> Settings { get; set; }
        public List<DatabaseTimezoneItem> Timezones { get; set; }
        public string HoldingCompanySecurityGUID { get; set; }
        public List<int> HoldingUnitIds { get; set; }
        public List<HoldingUnit> HoldingUnits { get; set; }
        public List<Country> Countries { get; set; }

        public List<Company> Companies { get; set; }
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
