using EZGO.Api.Models;
using EZGO.Api.Models.Setup;

namespace WebApp.Models.Company
{
    public class CompanyWithSettings : EZGO.Api.Models.Company
    {
        public SetupCompanySettings CompanySettings { get; set; }
    }
}
