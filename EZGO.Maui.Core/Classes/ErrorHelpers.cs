using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public static class ErrorHelpers
    {
        public static string ToNumberedList(this List<string> list, string itemSeparator = "\r\n")
        {
            if (list == null || list.Any() == false)
                return string.Empty;

            return string.Join(itemSeparator, Enumerable.Range(0, list.Count()).Select(i => $"{i + 1}. {list[i]}"));
        }
    }
}
