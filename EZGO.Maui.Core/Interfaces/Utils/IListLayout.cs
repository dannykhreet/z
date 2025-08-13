using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IListLayout
    {
        public double ItemSize { get; }
        public int SpanCount { get; }
        public int ItemSpacing { get; }
        public DataTemplate ItemTemplate { get; }
    }
}
