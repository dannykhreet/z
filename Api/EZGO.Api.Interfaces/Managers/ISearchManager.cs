using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ISearchManager
    {
        Task<List<SearchResult>> GetSearchResultAsync(int companyId, SearchTypeEnum searchType, int? userId = null, SearchFilters? filters = null, string include = null);
        List<Exception> GetPossibleExceptions();
    }
}
