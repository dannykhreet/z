using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Marketplace
{
    /// <summary>
    /// MarketPlaceItem; Used for the customer market place where a customer can setup certain connections and items. 
    /// </summary>
    public class MarketPlaceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }
        public List<string> Attachments { get; set; }
        public string SystemKey { get; set; }
        public int? SettingIdReference { get; set; }
        public string ExternalUrl { get; set; }
        /// <summary>
        /// JSON; dynamic will be different for each item, e.g. will contain fields like Key, User etc.
        /// </summary>
        public string ConfigurationTemplate { get; set; }
        /// <summary>
        /// JSON; configuration as saved with a company
        /// </summary>
        public string Configuration { get; set; }
    }
}
