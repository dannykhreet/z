using EZGO.Api.Models;
using EZGO.Api.Models.Marketplace;
using EZGO.Api.Models.SapPm;
using System.Collections.Generic;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class SapPmLocationViewModel : BaseViewModel
    {
        public SapPmLocationViewModel()
        {
        }

        public SapPmLocation SapPmFunctionalLocation { get; set; }
        public int IndentationLevel { get; set; }
    }
}
