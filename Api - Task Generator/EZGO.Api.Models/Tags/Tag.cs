using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models.Tags
{
    public class Tag
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string GroupGuid { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool? IsSeachable { get; set; }
        public bool? IsSystemTag { get; set; }
        public bool? IsHoldingTag { get; set; }
        public string ColorCode { get; set; }
        public string IconName { get; set; }
        public string IconStyle { get; set; }
        public List<TagableObjectEnum> AllowedOnObjectTypes { get; set; }
        public List<TagableObjectEnum> UsedInTemplateTypes { get; set; }
        public bool? IsInUse { get; set; }
        public string Translation { get; set; }
        public bool? UseTranslation { get; set; }
    }
}
