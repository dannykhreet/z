using EZGO.Api.Models;
using EZGO.Api.Models.PropertyValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TasksTemplateTaskStatusModel
    {
        public string Status { get; set; }
        public int CompanyId { get; set; }
        public int TemplateId { get; set; }
        public List<PropertyUserValue> PropertyUserValues { get; set; }
        public PictureProof PictureProof { get; set; }
        public Signature Signature { get; set; }
        public long Id { get; set; }
    }
}
