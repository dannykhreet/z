using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class AuditingDetailsFullViewModel: BaseViewModel
    {
        public string ObjectType { get; set; }
        public List<AuditingDetailsViewModel> ItemChangeDetails { get; set; }
    }
}
