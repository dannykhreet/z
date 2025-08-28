using System;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;

namespace EZGO.Maui.Core.Models.Tasks.Properties
{
    public class PropertyTaskTemplateModel : PropertyTaskTemplate
    {
        public PropertyFieldTypeEnum FieldType { get; set; }

        public PropertyTaskTemplateModel()
        {
        }
    }
}
