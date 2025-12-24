using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Versions
{
    public class VersionApp
    {
        public int Id { get; set; }
        public string AppName { get; set; }
        public string AppVersionInternal { get; set; }
        public string AppVersion { get; set; }
        public string OctopusVersion { get; set; }
        public string Platform { get; set; }
        public bool IsValidated { get; set; }
        public bool? IsLive { get; set; }
        public bool? IsCurrentActiveVersion { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
