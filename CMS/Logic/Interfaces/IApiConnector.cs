using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Logic.Interfaces
{
    public interface IApiConnector
    {
        Task<ApiResponse> GetCall(string url);
        Task<ApiResponse> GetCall(string url, string token);
        Task<ApiResponse> PostCall(string url, string body);
        Task<ApiResponse> GetCall(string url, Stream stream);
        Task<ApiResponse> PostCall(string url, MultipartFormDataContent objectBody);
        Task<ApiResponse> PostTokenCall(string url, string token, string jsonBody = null);
        Task<HttpStatusCode> CheckConnectorAuthorized();
    }
}
