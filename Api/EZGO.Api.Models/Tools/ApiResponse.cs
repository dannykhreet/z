using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}
