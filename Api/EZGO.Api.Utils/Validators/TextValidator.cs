using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EZGO.Api.Utils.Validators
{
    public static class TextValidator
    {
        /// <summary>
        /// Strip all possible rogue new line data (e.g. \r \n)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripRogueNewLineDataFromText(string text)
        {
            var output = text;
            if (!string.IsNullOrEmpty(output))
            {
                output = output.Replace("\r", string.Empty)
                               .Replace("\n", string.Empty);

            }
            return output;
        }

        /// <summary>
        /// Strip all possible rogue data (e.g. html, script other tags)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripRogueDataFromText(string text, bool ignoreEmailChar = false)
        {
            var output = text;
            if(!string.IsNullOrEmpty(output))
            {
                output = Regex.Replace(output, "<script[^>]*?>.*?</script>", String.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase); // Strip out javascript 
                output = Regex.Replace(output, "<[\\/\\!]*?[^<>]*?>", String.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase); // Strip out HTML tags. 
                output = Regex.Replace(output, "<style[^>]*?>.*?</style>", String.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase); // Strip style tags properly 
                output = Regex.Replace(output, "<![\\s\\S]*?--[ \\t\\n\\r]*>", String.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase); // Strip multi-line comments 
                output = output.Replace("javascript:", ""); // just kill possible js direct code structures if still available. Note this will make a mess of the data if available.
                output = output.Replace("vbscript:", ""); // just kill possible vbscript direct code structures if still available. Note this will make a mess of the data if available.                                        
                output = output.Replace("<", "˂"); // replace lesser than (U+003C) with arrow left (U+02C2) 
                output = output.Replace(">", "˃");// replace greater than (U+003E) with arrow right (U+02C3) 
                output = output.Replace("+", "＋");// replace + with ＋ (U+FF0B)
                output = output.Replace("-", "−");// replace - with − (U+2212)
                output = output.Replace("=", "꞊");// replace = with ꞊ (U+2261)
                if(!ignoreEmailChar) output = output.Replace("@", "﹫");// replace @ with ﹫ (U+FE6B)
            }
            return output;
        }
    }
}