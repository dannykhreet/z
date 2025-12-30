using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Action;

namespace WebApp.ViewModels
{
    public class ActionModelViewModel : ActionModel
    {
        public string StartDate { get; set; }
        public string Duedate { get; set; }
        public string Author { get; set; }
    }
}
