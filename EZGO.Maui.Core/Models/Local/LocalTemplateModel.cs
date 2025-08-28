using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Local
{
    public class LocalTemplateModel : IOpenTextFields
    {
        public DateTime? StartedAt { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<LocalTaskTemplateModel> TaskTemplates { get; set; } = new List<LocalTaskTemplateModel>();
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }
        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }
    }
}
