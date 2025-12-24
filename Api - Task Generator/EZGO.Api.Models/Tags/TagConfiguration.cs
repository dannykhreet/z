using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tags
{
    public class TagConfiguration
    {
        public string ColorCode { get; set; }
        public string IconName { get; set; }
        public string IconStyle { get; set; }
        public List<TagableObjectEnum> AllowedOnObjectTypes { get; set; }
        public bool? UseTranslation { get; set; }
    }
}
