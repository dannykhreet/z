using System;
using EZGO.Maui.Core.Interfaces.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.ListLayouts
{
    public class GridLayout : IListLayout
    {
        public GridLayout(double itemSize = 200, int spanCount = 3, int itemSpacing = 10)
        {
            ItemSize = itemSize;
            SpanCount = spanCount;
            ItemSpacing = itemSpacing;
        }

        public GridLayout(double itemSize, int spanCount, int itemSpacing, DataTemplate itemTemplate) : this(itemSize, spanCount, itemSpacing)
        {
            ItemTemplate = itemTemplate;
        }

        public double ItemSize { get; private set; }
        public int SpanCount { get; private set; }
        public int ItemSpacing { get; private set; }
        public DataTemplate ItemTemplate { get; private set; }
    }
}
