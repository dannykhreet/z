using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Models.OpenFields;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IOpenTextFields
    {
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }
        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }
        public int Id { get; set; }
    }
}
