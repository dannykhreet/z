using EZGO.Api.Models.Marketplace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class MarketplaceViewModel : BaseViewModel
    {
        public MarketplaceViewModel()
        {
        }

        public List<MarketPlaceItem> MarketPlaceItems { get; set; }
    }
}
