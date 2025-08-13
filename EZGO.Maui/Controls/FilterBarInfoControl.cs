using System;
using EZGO.Maui.Classes;

namespace EZGO.Maui.Controls
{
    public class FilterBarInfoControl : StackLayout
    {
        public static readonly BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCountProperty), typeof(int), typeof(FilterBarInfoControl), propertyChanged: OnItemsCountPropertyChanged, defaultValue: 0);

        private static void OnItemsCountPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as FilterBarInfoControl;
            obj.TotalTaskCount.Text = $"{obj.ItemsCount}";
        }

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            set
            {
                SetValue(ItemsCountProperty, value);
                OnPropertyChanged();
            }
        }

        public FilterBarInfoControl()
        {
            HorizontalOptions = LayoutOptions.CenterAndExpand;
            Orientation = StackOrientation.Horizontal;
            var textColor = ResourceHelper.GetValueFromResources<Color>("DarkerGreyColor");

            TotalTaskText = new Label
            {
                Text = Core.Extensions.TranslateExtension.GetValueFromDictionary("TOTAL_TASKS_COUNT_TEXT"),
                TextColor = textColor
            };

            TotalTaskCount = new Label
            {
                TextColor = textColor,
                Text = $"{ItemsCount}",
            };
            Children.Add(TotalTaskText);
            Children.Add(TotalTaskCount);
        }

        private Label TotalTaskText;

        private Label TotalTaskCount;
    }
}

