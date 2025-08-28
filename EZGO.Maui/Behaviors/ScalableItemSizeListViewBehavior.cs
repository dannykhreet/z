using System;
using EZGO.Maui.Behaviors;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Behaviors
{
    public class ScalableItemSizeListViewBehavior : BehaviorBase<SfListView>
    {
        /// <summary>
        /// The number of items that should be visible
        /// </summary>
        public double NumberOfItemsVisible { get; set; }

        protected override void OnAttachedTo(SfListView bindable)
        {
            base.OnAttachedTo(bindable);
            AssociatedObject.PropertyChanged += ScalableItemSizeListViewBehavior_PropertyChanged;
        }

        protected override void OnDetachingFrom(SfListView bindable)
        {
            base.OnDetachingFrom(bindable);
            AssociatedObject.PropertyChanged -= ScalableItemSizeListViewBehavior_PropertyChanged;
        }

        private void ScalableItemSizeListViewBehavior_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SfListView.Height))
            {
                if (AssociatedObject.Height == -1 || NumberOfItemsVisible == 0)
                    return;

                AssociatedObject.ItemSize = AssociatedObject.Height / NumberOfItemsVisible - AssociatedObject.ItemSpacing.VerticalThickness;
            }
        }
    }
}

