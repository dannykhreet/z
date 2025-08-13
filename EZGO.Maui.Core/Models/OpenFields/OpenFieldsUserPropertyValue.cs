using System;
namespace EZGO.Maui.Core.Models.OpenFields
{
    public class OpenFieldsUserPropertyValue
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int TemplatePropertyId { get; set; }
        public string UserValueString { get; set; }
    }
}
