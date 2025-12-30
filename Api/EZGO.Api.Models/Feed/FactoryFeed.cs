using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Feed
{
    /// <summary>
    /// FactoryFeed; Factory feed object. Used within the factory feed functionality. 
    /// </summary>
    public class FactoryFeed
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Attachments { get; set; }
        public string DataJson { get; set; }
        public FeedTypeEnum FeedType { get; set; }
        public List<FeedMessageItem> Items { get; set; }

    }
}
