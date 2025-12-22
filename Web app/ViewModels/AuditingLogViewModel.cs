using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class AuditingLogViewModel : BaseViewModel
    {
        public string ItemType { get; set; }
        public int TemplateId { get; set; }
    }
}
