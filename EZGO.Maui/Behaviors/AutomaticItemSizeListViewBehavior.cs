using System;
using EZGO.Maui.Behaviors;
using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Behaviors
{
    public class AutomaticItemSizeListViewBehavior : BehaviorBase<SfListView>
    {
        public static readonly BindableProperty NumberOfItemsProperty =
            BindableProperty.Create(nameof(NumberOfItems), typeof(int), typeof(AutomaticItemSizeListViewBehavior), defaultValue: 80, defaultBindingMode: BindingMode.OneWay);

        public int NumberOfItems
        {
            get => (int)GetValue(NumberOfItemsProperty);
            set => SetValue(NumberOfItemsProperty, value);
        }

        protected override void OnAttachedTo(SfListView bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.SizeChanged += Bindable_SizeChanged;
        }

        private void Bindable_SizeChanged(object sender, EventArgs e)
        {
            AssociatedObject.ItemSize = (AssociatedObject.Height + AssociatedObject.ItemSpacing.Top - NumberOfItems * AssociatedObject.ItemSpacing.VerticalThickness) / NumberOfItems;
        }

        protected override void OnDetachingFrom(SfListView bindable)
        {
            bindable.SizeChanged -= Bindable_SizeChanged;
            base.OnDetachingFrom(bindable);

        }
    }
}

