using System;
namespace EZGO.Maui.Classes
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        public string Text { get; set; }

        /// <param name="serviceProvider">The service that provides the value.</param>
        /// <summary>Returns the object created from the markup extension.</summary>
        /// <returns>The object</returns>
        /// <remarks>To be added.</remarks>
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return EZGO.Maui.Core.Extensions.TranslateExtension.GetValueFromDictionary(Text);
        }
    }
}

