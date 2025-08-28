using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Core.Models.OpenFields
{
    public class TemplatePropertyModel : NotifyPropertyChanged
    {
        public int Id { get; set; }
        public int? CheklistTemplateId { get; set; }
        public int? AuditTemplateId { get; set; }
        public int PropertyId { get; set; }
        public int PropertyGroupId { get; set; }
        public string TitleDisplay { get; set; }
        public bool IsRequired { get; set; }
        public PropertyValueTypeEnum ValueType { get; set; }
        public int Index { get; set; }
    }
}
