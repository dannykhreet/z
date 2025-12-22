using EZGO.Api.Models;
using EZGO.Api.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class RawViewModel : BaseViewModel
    {
        public RawData Data { get; set; }
        public List<Company> Companies { get; set; }

        public DateTime ChoosenStartDateTime { get; set; }
        public DateTime ChoosenEndDateTime { get; set; }
        public string ChoosenRawViewerType { get; set; }

        public int ChoosenCompanyId { get; set; }
    }
}
