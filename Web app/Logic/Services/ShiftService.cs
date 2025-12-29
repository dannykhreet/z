using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Models.Company;
using WebApp.Models.Shift;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebApp.Logic.Services
{
    public class ShiftService : IShiftService
    {
        private async Task<List<CompanyShiftModel>> GetDemoShifts()
        {
            string strJson = await File.ReadAllTextAsync("JsonFiles\\shifts.json");
            var shifts = JsonSerializer.Deserialize<List<CompanyShiftModel>>(strJson);
            return shifts;
        }

        public async Task<Dictionary<DayOfWeek, List<SelectListItem>>> GetSelectLists()
        {
            var shifts = await GetDemoShifts();
            return shifts.GroupBy(x => x.Weekday).ToDictionary(g => (DayOfWeek)g.Key, g => new List<SelectListItem>(g.Select(shift => new ShiftSelectItem(shift).Item).ToList()));
        }
    }
}
