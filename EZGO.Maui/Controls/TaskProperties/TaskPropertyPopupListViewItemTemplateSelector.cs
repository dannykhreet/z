using System;
namespace EZGO.Maui.Controls.TaskProperties
{
    public class TaskPropertyPopupListViewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EmptyDataTemplate { get; set; }

        public DataTemplate PropertyDataTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate result;

            if (item == null)
                result = EmptyDataTemplate;
            else
                result = PropertyDataTemplate;

            return result;
        }
    }
}

