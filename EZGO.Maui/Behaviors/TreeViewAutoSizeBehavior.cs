using System;
using Syncfusion.Maui.TreeView;

namespace EZGO.Maui.Behaviors
{
    public class TreeViewAutoSizeBehavior : Behavior<SfTreeView>
    {
        public static readonly BindableProperty ZeroLevelNodeHeightProperty = BindableProperty.Create("ZeroLevelNodeHeight", typeof(int), typeof(TreeViewAutoSizeBehavior), 100, BindingMode.TwoWay);

        public int ZeroLevelNodeHeight
        {
            get => (int)GetValue(ZeroLevelNodeHeightProperty);
            set => SetValue(ZeroLevelNodeHeightProperty, value);
        }

        protected override void OnAttachedTo(SfTreeView bindable)
        {
            bindable.QueryNodeSize += Bindable_QueryNodeSize;
            base.OnAttachedTo(bindable);
        }

        private void Bindable_QueryNodeSize(object sender, QueryNodeSizeEventArgs e)
        {
            // Returns item height based on the content loaded.
            if (e.Node.Level == 0)
            {
                e.Height = ZeroLevelNodeHeight;
            }
            else
            {
                e.Height = e.GetActualNodeHeight();
            }
            e.Handled = true;
        }
        protected override void OnDetachingFrom(SfTreeView bindable)
        {
            bindable.QueryNodeSize -= Bindable_QueryNodeSize;
            base.OnDetachingFrom(bindable);
        }
    }
}

