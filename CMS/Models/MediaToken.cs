using Newtonsoft.Json;
using System;

namespace WebApp.Models
{
    public class MediaToken
    {
        public string AccessKeyId { get; set; }
        public DateTime Expiration { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
    }
}
