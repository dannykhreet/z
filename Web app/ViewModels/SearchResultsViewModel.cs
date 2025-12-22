using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class SearchResultsViewModel : BaseViewModel
    {
        public List<EZGO.Api.Models.Search.SearchResult> SearchResults { get; set; }
        public string DetailsUrlPart { get; set; }
    }
}
