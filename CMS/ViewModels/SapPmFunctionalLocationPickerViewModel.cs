using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Settings;
using System.Collections.Generic;

namespace WebApp.ViewModels
{
    public class SapPmFunctionalLocationPickerViewModel
    {
        public List<SapPmLocation> SapPmLocations { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public bool? DisableMutateArea { get; set; }
    }
}
