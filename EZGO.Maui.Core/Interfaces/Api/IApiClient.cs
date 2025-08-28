using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Api
{
    public interface IApiClient : IDisposable
    {
        Task<List<T>> GetAllAsync<T>(string action);

        Task<T> GetAsync<T>(string action);

        Task<HttpResponseMessage> PostAsync(string action, StringContent data);
        Task<HttpResponseMessage> PostAsync<T>(string action, T data);
    }
}
