using EZGO.Api.Models.Tools;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class RawSchedulerViewModel : BaseViewModel
    {
            public CalendarSchedule Schedule { get; set; }
            public List<Company> Companies { get; set; }
            public DateTime ChoosenStartDateTime { get; set; }
            public DateTime ChoosenEndDateTime { get; set; }

    }
}
