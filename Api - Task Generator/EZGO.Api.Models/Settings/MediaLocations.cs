using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    public class MediaLocations
    {
        public string ImageMediaBaseUri { get; set; }
        public string VideoMediaBaseUri { get; set; }
        public string FileMediaBaseUri { get; set; }
        public string MediaUploadLocation { get; set; }
    }
}
