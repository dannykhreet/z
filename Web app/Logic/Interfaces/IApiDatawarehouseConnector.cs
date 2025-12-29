using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Logic.Interfaces
{
    public interface IApiDatawarehouseConnector
    {
        Task<ApiResponse> GetCall(string url, string username, string password, string appid);
        Task<ApiResponse> PostCall(string url, string body, string username, string password, string appid);
    }
}
