using System;
using EZGO.Maui.Core.Enumerations;

namespace EZGO.Maui.Core.Models.Actions
{
    public class ResourceModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Picture { get; set; }
        public ActionResourceType ActionResourceType { get; set; }        
    }
}
