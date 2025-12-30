using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Search
{
    public class SearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SearchCount> Counts { get; set; }
        public List<SearchLinkedInformation> LinkedInformation { get; set; }
        public string AreaPath { get; set; }
        public string Picture { get; set; }
        public string Type { get; set; }
        public string Role { get; set; }
        public SearchTypeEnum SearchType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}
