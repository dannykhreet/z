using Amazon.S3.Model;
using System;
using System.Collections.Generic;

namespace WebApp.Models.Shared
{
    public class ExtractionModel
    {
        public int TemplateId { get; set; }
        public List<ExtractionModel.VersionModel> Versions { get; set; }
        public string ExtractionUriPart { get; set; }
        public class VersionModel
        {
            public DateTime CreatedOn { get; set; }
            public string Version { get; set; }

        }
    }
}
