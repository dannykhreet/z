using EZGO.Api.Models.Versions;
using System.Collections.Generic;

namespace WebApp.ViewModels
{
    public class VersionOverviewViewModel : BaseViewModel
    {
        public List<VersionApp> Versions { get; set; }
        public VersionApp newVersion { get; set; }
    }
}
