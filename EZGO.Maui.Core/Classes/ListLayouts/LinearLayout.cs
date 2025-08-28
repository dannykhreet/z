using System;
using EZGO.Maui.Core.Interfaces.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.ListLayouts
{
    public class LinearLayout : IListLayout
    {
        public LinearLayout(double itemSize = 110, int spanCount = 1, int itemSpacing = 7)
        {
            ItemSize = itemSize;
            SpanCount = spanCount;
            ItemSpacing = itemSpacing;
        }

        public LinearLayout(double itemSize, int spanCount, int itemSpacing, DataTemplate itemTemplate) : this(itemSize, spanCount, itemSpacing)
        {
            ItemTemplate = itemTemplate;
        }

        public double ItemSize { get; private set; }

        public int SpanCount { get; private set; }

        public int ItemSpacing { get; private set; }

        public DataTemplate ItemTemplate { get; private set; }
    }
}
