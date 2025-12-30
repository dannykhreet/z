using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Company;

namespace WebApp.ViewModels
{
    public class ShiftsViewModel
    {
        public List<CompanyShiftModel> Shifts { get; set; }

        public List<CompanyShiftModel> ChangedShifts { get; set; }

        public List<CompanyShiftModel> RemovedShifts { get; set; }

        public List<CompanyShiftModel> AddedShifts { get; set; }

    }
}
