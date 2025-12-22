using EZGO.Api.Models.Settings;

namespace WebApp.Models.Company
{
    public class CompanyFeaturesModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Features Features { get; set; }
        public bool TaskGenerationEnabled { get; set; }

    }
}
