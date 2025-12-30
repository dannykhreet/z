using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class CompanyStatisticViewModel : BaseViewModel
    {
        public int CompanyId { get; set; }
        public int HoldingId { get; set; }
        public List<Company> Companies { get; set; }
        public List<Holding> Holdings { get; set; }
    }
}
