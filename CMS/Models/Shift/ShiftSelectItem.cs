using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models.Company;

namespace WebApp.Models.Shift
{
    public class ShiftSelectItem
    {
        private bool _Selected;
        private CompanyShiftModel _Shift;

        public ShiftSelectItem(CompanyShiftModel shift, List<int> shiftIds, bool preselect = true)
        {
            shiftIds ??= new List<int>();
            _Shift = shift;
            _Selected = preselect ? shiftIds.Any(x => x == shift.Id) : false;
        }

        public SelectListItem Item { get => new SelectListItem { Value = _Shift.Id.ToString(), Text = String.Format(@"{0:hh\:mm} - {1:hh\:mm}", _Shift.StartTime, _Shift.EndTime), Selected = _Selected }; }
    }
}
