using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Models.OpenFields
{
    public class UserValuesPropertyModel : NotifyPropertyChanged
    {
        public int Id { get; set; }
        public int? ChecklistId { get; set; }
        public int? AuditId { get; set; }
        public int PropertyId { get; set; }
        public int PropertyGroupId { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int TemplatePropertyId { get; set; }
        public string UserValueString { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public PropertyValueTypeEnum ValueTypeEnum { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
        public bool IsReadonly { get; set; } = false;
        public bool IsRequired { get; set; }

        public string TextInput
        {
            get
            {
                return UserValueString;
            }
            set
            {
                SetValue(value);
            }
        }

        private void SetValue(string value)
        {
            switch (ValueTypeEnum)
            {
                case PropertyValueTypeEnum.String:
                    if (UserValueString != value)
                    {
                        UserValueString = value;
                        if (UserValueString == null)
                            UserValueString = string.Empty;
                        RegisteredAt = DateTime.UtcNow;
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send(this, Constants.ValueChanged);
                        });
                    }

                    break;
                case PropertyValueTypeEnum.Boolean:
                    break;
                default:
                    break;
            }
        }

        public string GetFieldValue()
        {
            return UserValueString;
        }
    }
}
