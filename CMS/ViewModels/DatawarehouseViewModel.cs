using EZGO.Api.Models;
using System.Collections.Generic;
using System.Web;

namespace WebApp.ViewModels
{
    public class DatawarehouseViewModel : BaseViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AppId { get; set; }
        public List<EZ.Api.DataWarehouse.Models.User> Users { get; set; }
        public EZ.Api.DataWarehouse.Models.User User { get; set; }
    }

    public class DatawarehouseToolsViewModel : BaseViewModel
    {
        public List<Company> Companies { get; set; }

        public List<Holding> Holdings { get; set; }
    }
}
