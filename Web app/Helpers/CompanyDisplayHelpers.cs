using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Helpers
{
    public static class CompanyDisplayHelpers
    {
        public static string CompanyDisplayList(string companyIds, string divider, List<Company> companies)
        {
            var ouput = companyIds;
            var defaultDivider = string.IsNullOrEmpty(divider) ? "," : divider;

            if(!string.IsNullOrEmpty(companyIds) && companies != null && companies.Count > 0)
            {
                var companyIdCollection = companyIds.Split(defaultDivider);
                var sb = new StringBuilder();
                foreach(var id in companyIdCollection)
                {
                    if (id == "ALL")
                    {
                        sb.AppendFormat("[{0}]", "ALL");
                    } else
                    {
                        sb.AppendFormat("[{0}] ", companies.Where(x => x.Id.ToString() == id)?.FirstOrDefault()?.Name);
                    }

                }

                ouput = sb.ToString();
                sb.Clear();
                sb = null;
            }

            return ouput;
        }
    }
}
