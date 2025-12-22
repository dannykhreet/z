using EZGO.Api.Models.Stats;
using EZGO.CMS.LIB.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using WebApp.Models.Language;

namespace WebApp.ViewModels
{
    public class LanguageViewModel : BaseViewModel
    {
        public List<LanguageModel> Languages { get; set; }
        public List<StatsItem> Stats { get; set; }
        public LanguageModel Language { get; set; }
        //refactor, rebuild not using .net objects.
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> LanguageSelectorItems { get; set; }
        public LanguageViewModel()
        {

        }

    }
}
