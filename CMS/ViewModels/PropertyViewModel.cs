using System;
using System.Linq;

namespace WebApp.ViewModels
{
    public enum PropertyFieldTypeEnum
    {
        Custom = 0,
        SingleValue = 1,
        Range = 2,
        UpperLimit = 3,
        LowerLimit = 4,
        EqualTo = 5,
        UpperLimitEqualTo = 6, //not yet implemented
        LowerLimitEqualTo = 7 //not yet implemented
    }

    
    public class PropertyViewModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public int PropertyId { get; set; }
        public int PropertyValueId { get; set; }
        public int TemplateId { get; set; }
        public string PrimaryValue { get; set; }
        public string SecondaryValue { get; set; }
        public string TitleDisplay { get; set; } //title text
        public string PropertyValueDisplay { get; set; }  //footer text
        public bool IsRequired { get; set; } = false;
        public PropertyViewModel() 
        {
        }
    }
}
