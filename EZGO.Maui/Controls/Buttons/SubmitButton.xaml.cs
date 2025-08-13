using System;
using System.Collections.Generic;
using System.Windows.Input;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons
{
    public partial class SubmitButton : SfButton
    {
        public readonly static BindableProperty IsLoadingProperty = BindableProperty.Create(nameof(IsLoadingProperty), typeof(bool), typeof(SubmitButton), propertyChanged: OnIsLoadingPropertyChanged, defaultValue: false);

        private static void OnIsLoadingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as SubmitButton;
            obj.IsEnabled = !obj.IsLoading;
        }

        public readonly static BindableProperty TextSizeProperty = BindableProperty.Create(nameof(TextSize), typeof(string), typeof(SubmitButton), propertyChanged: OnTextSizePropertyChanged, defaultValue: "Small");

        private static void OnTextSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var obj = bindable as SubmitButton;
            var fontConverter = new FontSizeConverter();
            double size = (double)fontConverter.ConvertFromInvariantString(obj.TextSize);
        }

        public static readonly BindableProperty ButtonTextProperty = BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(SubmitButton));

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public SubmitButton()
        {
            InitializeComponent();
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set
            {
                SetValue(IsLoadingProperty, value);
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string TextSize
        {
            get => (string)GetValue(TextSizeProperty);
            set => SetValue(TextSizeProperty, value);
        }
    }
}
