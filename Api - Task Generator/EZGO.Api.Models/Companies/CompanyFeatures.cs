using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Companies
{
    public class CompanyFeatures
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Features Features { get; set; }
        //specific technical implementations
        public bool TaskGenerationEnabled { get; set; }
    }
}
