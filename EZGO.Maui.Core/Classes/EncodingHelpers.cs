using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public class EncodingHelpers
    {
        public static string ForceUTF8(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Non-UFT8 character will be removed
            var result = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.WebName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                        ),
                    Encoding.UTF8.GetBytes(input)
                )
            );

            return result;
        }
    }
}
