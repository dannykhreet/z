using EZGO.Api.Models.Basic;
using System.Collections.Generic;

namespace WebApp.ViewModels
{
    public class TemplateSharingViewModel
    {
        public Dictionary<string, string> CmsLanguage { get; set; }
        public List<CompanyBasic> Companies { get; set; }
    }
}
