using System;
using System.Net.Http;
using EZGO.Maui.Core.Interfaces.Utils;
using Newtonsoft.Json;

namespace EZGO.Maui.Core.Models.Requests
{
    public class RequestModel : IQueueableItem
    {
        public string Uri { get; set; }
        public string ContentAsString { get; set; }
        public Guid LocalGuid { get; }

        public RequestModel()
        {
            LocalGuid = Guid.NewGuid();
        }

        [JsonConstructor]
        public RequestModel(Guid localGuid)
        {
            LocalGuid = localGuid;
        }
    }
}
