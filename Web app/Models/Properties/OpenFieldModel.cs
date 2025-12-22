using System;
namespace WebApp.Models.Properties
{
    public class OpenFieldModel
    {
        public int Id { get; set; }
        public int TemplatePropertyId { get; set; }
        public string TitleDisplay { get; set; }
        public string UserValueString { get; set; }
        public bool? IsRequired { get; set; }

    }
}
