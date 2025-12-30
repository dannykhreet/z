using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Announcements;

namespace WebApp.ViewModels
{
    public class AnnouncementViewModel : BaseViewModel
    {

        public List<AnnouncementModel> Announcements { get; set; }
    }
}
