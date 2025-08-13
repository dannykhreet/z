using System;
using EZGO.Maui.Core.Interfaces.Utils;
using Foundation;
using UIKit;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class TextMeterService : ITextMeter
    {
        public Tuple<double, double> MeasureTextSize(string text, double fontSize, string fontName = null)
        {
            text ??= "";
            var nsText = new NSString(text);
            var boundSize = new SizeF((float)fontSize, float.MaxValue);

            var lettersList = fontName.Where(x => x == $"{x}".ToUpper().ToCharArray().First()).ToList();
            var index = fontName.LastIndexOf(lettersList.Last());

            fontName = fontName.Insert(index, "-");

            var attributes = new UIStringAttributes
            {
                Font = UIFont.FromName(fontName, (float)fontSize)
            };

            var textBounds = nsText.GetSizeUsingAttributes(attributes);

            return new Tuple<double, double>((double)textBounds.Width, (double)textBounds.Height);
        }

        public Tuple<double, double> MeasureTextSize(string text, double width, double fontSize, string fontName = null)
        {
            text ??= "";
            var nsText = new NSString(text);
            var boundSize = new SizeF((float)width, float.MaxValue);

            var lettersList = fontName.Where(x => x == $"{x}".ToUpper().ToCharArray().First()).ToList();
            var index = fontName.LastIndexOf(lettersList.Last());

            fontName = fontName.Insert(index, "-");

            var attributes = new UIStringAttributes
            {
                Font = UIFont.FromName(fontName, (float)fontSize)
            };

            var options = NSStringDrawingOptions.UsesFontLeading |
                NSStringDrawingOptions.UsesLineFragmentOrigin;

            var textBounds = nsText.GetBoundingRect(boundSize, options, attributes, null);

            return new Tuple<double, double>((double)textBounds.Width, (double)textBounds.Height);
        }
    }
}

