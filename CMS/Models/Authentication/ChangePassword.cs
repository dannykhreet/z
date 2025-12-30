using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Authentication
{
    public class ChangePassword : EZGO.Api.Models.Authentication.ChangePassword
    {
        public string ValidationKey { get; set; }
    }
}
