using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Models.Requests;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using itemsQueue = EZGO.Maui.Classes.QueueManager<EZGO.Maui.Core.Models.Requests.RequestModel>;

namespace EZGO.Maui.Core.Classes
{
    public class RequestHelper
    {
        private static RequestHelper request;

        private RequestHelper()
        {
        }

        public static RequestHelper Instance()
        {
            if (request == null)
            {
                request = new RequestHelper();
            }

            return request;
        }

        public async Task<RequestModel> ReadRequest()
        {
            return await itemsQueue.DequeueItemAsync();
        }

        public RequestModel PeekRequest()
        {
            return itemsQueue.PeekItem();
        }

        public async Task AddRequest(string key, RequestModel content)
        {
            if (!itemsQueue.Contains(content))
                await itemsQueue.EnqueueItemAsync(content);
        }

        public bool HasAny()
        {
            return itemsQueue.HasItems();
        }
    }
}
