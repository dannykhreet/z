using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class NodeModel
    {
        public int id { get; set; }
        public int? pid { get; set; }
        public string area { get; set; }
        public string location { get; set; }
        public string location_shortened { get; set; }
        public string img { get; set; }
        public List<string> tags { get; set; }
        public int? funcLocationId { get; set; }
        public string funcLocationName { get; set; }
        public string funcLocation { get; set; }
    }
}
