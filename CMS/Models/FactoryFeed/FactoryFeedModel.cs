using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;

namespace WebApp.Models.FactoryFeed
{
    public class FactoryFeedModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Attachments { get; set; }
        public string DataJson { get; set; }
        public FeedTypeEnum FeedType { get; set; }
        public List<FactoryFeedItemModel> Items { get; set; }
    }
}
