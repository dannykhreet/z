using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.ApiRequestHandlers
{
    public interface IApiRequestHandler : IDisposable
    {
        Task<T> HandleRequest<T>(string uri, bool refresh = false, bool isFromSyncService = false);
        Task<List<T>> HandleListRequest<T>(string uri, bool refresh = false, bool isFromSyncService = false);
        Task<HttpResponseMessage> HandlePostRequest<T>(string uri, T objectToPost, bool ignoreNullValues = false, bool saveRequest = true);
    }
}
