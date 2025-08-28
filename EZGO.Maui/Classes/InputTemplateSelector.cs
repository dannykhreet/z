using System;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models.Tasks.Properties;

namespace EZGO.Maui.Classes
{
    public class InputTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SingleEntryTemplate { get; set; }

        public DataTemplate MeasureTimeInput { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is BaseTaskPropertyEditViewModel itemModel)
            {
                if (itemModel.Property.IsPlannedTimeProperty)
                    return MeasureTimeInput;

                switch (itemModel.Property.ValueType)
                {
                    // TODO implement
                    case PropertyValueTypeEnum.Boolean:
                        return null;
                    default:
                        return SingleEntryTemplate;
                }
            }

            return null;
        }
    }
}

