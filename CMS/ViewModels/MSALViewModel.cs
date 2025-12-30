using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels.Authentication
{
    public class MSALViewModel : BaseViewModel
    {
        public string ExternalRedirectUrl { get; set; }
    }
}
