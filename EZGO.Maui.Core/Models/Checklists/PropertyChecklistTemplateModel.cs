using System;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;

namespace EZGO.Maui.Core.Models.Checklists
{
    public class PropertyChecklistTemplateModel : PropertyChecklistTemplate
    {
        public PropertyFieldTypeEnum FieldType { get; set; }
    }
}
