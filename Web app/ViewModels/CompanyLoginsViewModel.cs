using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.General;
using System.Collections.Generic;
using WebApp.Models.Settings;

namespace WebApp.ViewModels
{
    public class CompanyLoginsViewModel : BaseViewModel
    {
        public int CompanyId { get; set; }
        public int HoldingId { get; set; }
        public List<Company> Companies { get; set; }
        public List<Holding> Holdings { get; set; }
        public List<DatabaseTimezoneItem> Timezones { get; set; }
        public RawData Data { get; set; }
        public string JsDataOutput { get; set;  }
    }

}
