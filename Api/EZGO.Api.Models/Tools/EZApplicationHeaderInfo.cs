using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class EZApplicationHeaderInfo
    {
        public string App { get; set; }
        public string OperatingSystem { get; set; }
        public string Version { get; set; }
        public string Language { get; set; }
        public string UserAgent { get; set; }
    }
}
