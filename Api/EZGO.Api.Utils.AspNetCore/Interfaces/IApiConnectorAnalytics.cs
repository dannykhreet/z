using EZGO.Api.Models.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Utils
{
    public interface IApiConnectorAnalytics
    {
        Task<ApiResponse> PostCall(string url, string body);
    }
}
