using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHtmlString(this Color color)
        {
            int red = (int)(color.Red * 255);
            int green = (int)(color.Green * 255);
            int blue = (int)(color.Blue * 255);
            int alpha = (int)(color.Alpha * 255);
            string hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", red, green, blue, alpha);
            return hex;
        }
    }
}
