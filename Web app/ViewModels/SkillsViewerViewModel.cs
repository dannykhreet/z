using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class SkillsViewerViewModel
    {
        public int UserId { get; set; }
        public List<int> participants { get; set; }
    }
}
