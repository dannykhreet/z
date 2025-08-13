using System;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using EZGO.Maui.Core.Interfaces.Utils;
using Rect = Android.Graphics.Rect;
using View = Android.Views.View;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class TextMeterService : ITextMeter
    {
        private Typeface textTypeface;

        public Tuple<double, double> MeasureTextSize(string text, double fontSize, string fontName = null)
        {
            var textView = new TextView(global::Android.App.Application.Context);
            textView.Typeface = GetTypeface(fontName);
            textView.SetText(text, TextView.BufferType.Normal);
            textView.SetTextSize(ComplexUnitType.Px, (float)fontSize);

            var bounds = new Rect();

            textView.Paint.GetTextBounds(text, 0, text.Length, bounds);

            return new Tuple<double, double>((double)bounds.Width(), (double)bounds.Height());
        }

        public Tuple<double, double> MeasureTextSize(string text, double width, double fontSize, string fontName = null)
        {
            text ??= "";
            var textView = new TextView(global::Android.App.Application.Context);
            textView.Typeface = GetTypeface(fontName);
            textView.SetText(text, TextView.BufferType.Normal);
            textView.SetTextSize(ComplexUnitType.Px, (float)fontSize);

            int widthMeasureSpec = View.MeasureSpec.MakeMeasureSpec(
                (int)width, MeasureSpecMode.AtMost);

            int heightMeasureSpec = View.MeasureSpec.MakeMeasureSpec(
                0, MeasureSpecMode.Unspecified);

            textView.Measure(widthMeasureSpec, heightMeasureSpec);

            return new Tuple<double, double>((double)textView.MeasuredWidth, (double)textView.MeasuredHeight);
        }

        private Typeface GetTypeface(string fontName)
        {
            if (fontName == null)
            {
                return Typeface.Default;
            }

            if (textTypeface == null)
            {
                textTypeface = Typeface.Create(fontName, TypefaceStyle.Normal);
            }

            return textTypeface;
        }
    }
}

