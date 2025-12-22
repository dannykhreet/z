using EZGO.Api.Models;
using EZGO.Api.Models.Marketplace;
using EZGO.Api.Models.SapPm;
using System.Collections.Generic;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public partial class ConfigViewModel : BaseViewModel
    {
        public ConfigViewModel()
        {
        }

        public List<Area> Areas { get; set; }

        public CompanyModel Company { get; set; }

        public EZGO.Api.Models.CompanyRoles CompanyRoles { get; set; }

        public List<MarketPlaceItem> MarketPlace { get; set; }

        public List<SapPmLocation> SapPmFunctionalLocations { get; set; }

        public bool DisableMutateArea { get; set;}
        public bool DisableMutateShifts { get; set; }
        public bool EnableTaskgenerationManagement { get; set; }
    }
}
