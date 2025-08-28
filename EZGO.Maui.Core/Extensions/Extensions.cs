using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace EZGO.Maui.Core.Extensions
{
    public static class Extensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync().ConfigureAwait(false);
            //var dataAsString = await content.ReadAsAsync<string>().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(dataAsString, dateTimeZoneHandling: Newtonsoft.Json.DateTimeZoneHandling.Utc);
        }

        public static List<T> ReadAsJsonFromPath<T>(this List<T> list, string filepath)
        {
            try
            {
                // first time here, file does not exist.
                if (!File.Exists(filepath)) return new List<T>();

                using (StreamReader file = File.OpenText(filepath))
                {
                    string strjson = file.ReadToEnd();
                    return JsonSerializer.Deserialize<List<T>>(strjson);
                }
            }
            catch
            {
                return new List<T>();
            }
        }

        public static string RemoveSpecialCharacters(this string instance, char? replacementValue = null, params char[] validCharacters)
        {
            string validCharacterString = string.Empty;

            // Add whitelisted characters to the whiteList
            // These will be included in the regex
            // Use the escape method to ensure the correct characters
            if (validCharacters != null && validCharacters.Length > 0)
                validCharacterString = Regex.Escape(validCharacters.JoinString(item => item.ToString(), string.Empty));

            Regex regex = new Regex($@"([a-zA-Z0-9 {validCharacterString}]*)(.?)", RegexOptions.None);

            string returnValue = replacementValue == null
                ? regex.Matches(instance).Cast<Match>().JoinString(item => item.Groups[1].Value, string.Empty)
                : regex.Matches(instance).Cast<Match>().JoinString(item => item.Groups[1].Value + new string((char)replacementValue, item.Groups[2].Value.Length), string.Empty);


            return returnValue;
        }

        #region File Name safety

        static readonly char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        public static string ToSafeFileName(this string input, char replaceWith = '_')
        {
            var result = new string(input.Select(@char => invalidFileNameChars.Contains(@char) ? replaceWith : @char).ToArray());
            return result;
        }

        #endregion

        public static string JoinString<T>(this IEnumerable<T> enumeration, Func<T, string> toString, string separator)
        {
            return string.Join(separator, enumeration.Select(toString).ToArray());
        }

        public static string GetReadableString(this string input, string divider = "_", Boolean lower = true)
        {
            try
            {
                byte[] b = Encoding.GetEncoding(1251).GetBytes(input); // 8 bit characters
                input = Encoding.ASCII.GetString(b); // 7 bit characters

                Regex r0 = new Regex("<(.|\n)+?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                input = r0.Replace(input, " ");

                Regex r1 = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                input = r1.Replace(input, String.Empty);
                input = Regex.Replace(input, @"\s{2,}", " ").Trim();
                input = Regex.Replace(input, @"\s+", divider);
                if (lower) { input = input.ToLower(); }
                return input;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Replace variables like %@ to {i++}
        /// </summary>
        /// <param name="input">string</param>
        /// <param name="pattern">defaults to %@</param>
        /// <returns></returns>
        public static string ReplaceLanguageVariablesCumulative(this string input, string pattern = "%@")
        {
            try
            {
                System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(pattern);
                System.Text.RegularExpressions.MatchCollection matches = rx.Matches(input);
                int i = 0;
                if (matches.Count > 0)
                {
                    input = rx.Replace(input, match => { return string.Format("{{{0}}}", i++); });
                }

                return input;
            }
            catch
            {
                return input;
            }
        }

        public static double Clamp(this double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }

        public static bool IsNullOrDefault<T>(T value)
        {
            return object.Equals(value, default(T));
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source,
            Func<T, IEnumerable<T>> childrenSelector,
            Func<T, object> keySelector) where T : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (childrenSelector == null)
                throw new ArgumentNullException("childrenSelector");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            var stack = new Stack<T>(source);
            var dictionary = new Dictionary<object, T>();
            while (stack.Any())
            {
                var currentItem = stack.Pop();
                var currentkey = keySelector(currentItem);
                if (dictionary.ContainsKey(currentkey) == false)
                {
                    dictionary.Add(currentkey, currentItem);
                    var children = childrenSelector(currentItem);
                    if (children != null)
                    {
                        foreach (var child in children)
                        {
                            stack.Push(child);
                        }
                    }
                }

                yield return currentItem;
            }
        }

        /// <summary>
        /// This would flatten out a recursive data structure ignoring the loops. The end result would be an enumerable which enumerates all the
        /// items in the data structure regardless of the level of nesting.
        /// </summary>
        /// <typeparam name="T">Type of the recursive data structure</typeparam>
        /// <param name="source">Source element</param>
        /// <param name="childrenSelector">a function that returns the children of a given data element of type T</param>
        /// <param name="keySelector">a function that returns a key value for each element</param>
        /// <returns>a faltten list of all the items within recursive data structure of T</returns>
        public static IEnumerable<T> Flatten<T>(this T source,
            Func<T, IEnumerable<T>> childrenSelector,
            Func<T, object> keySelector) where T : class
        {
            return Flatten(new[] { source }, childrenSelector, keySelector);
        }

        /// <summary>
        /// Returns a boolean value that indicates whether or not the current instance is null, string.empty or only contains whitespace characters
        /// </summary>
        /// <param name="instance">The instance to verify</param>
        /// <returns>A boolean value</returns>
        public static bool IsNullOrWhiteSpace(this string instance)
        {
            return string.IsNullOrWhiteSpace(instance);
        }

        /// <summary>
        /// Returns a boolean value that indicates whether or not the current instance is null or string.empty
        /// </summary>
        /// <param name="instance">The instance to verify</param>
        /// <returns>A boolean value</returns>
        public static bool IsNullOrEmpty(this string instance)
        {
            return string.IsNullOrEmpty(instance);
        }

        /// <summary>
        /// Replaces each format item in a specified string with the text equivalent of a corresponding object's value.
        /// </summary>
        /// <param name="instance">The format string</param>
        /// <param name="arguments">The arguments</param>
        /// <returns>The formatted string</returns>
        public static string Format(this string instance, params object[] arguments)
        {
            if (instance.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(instance));

            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            return string.Format(instance, arguments);
        }

        public static bool IsTimeBetween(this DateTime dateTime, TimeSpan start, TimeSpan end)
        {
            bool result;

            TimeSpan now = dateTime.TimeOfDay;

            if (start < end)
                result = start <= now && now <= end;
            else
                result = !(end < now && now < start);

            return result;
        }

        public static bool IsDateBetween(this DateTime dateTime, DateTime start, DateTime end)
        {
            return dateTime >= start && dateTime < end;
        }

        public static string ToApiString(this RecurrencyTypeEnum recurrency)
        {
            if (recurrency == RecurrencyTypeEnum.NoRecurrency)
                return "no recurrency";
            return recurrency.ToString().ToLower();
        }

        public static RecurrencyTypeEnum GetRecurrencyType(this TaskRecurrency recurrency)
        {
            if (recurrency.RecurrencyType == "no recurrency")
                return RecurrencyTypeEnum.NoRecurrency;
            try
            {
                return (RecurrencyTypeEnum)Enum.Parse(typeof(RecurrencyTypeEnum), recurrency.RecurrencyType, true);
            }
            catch
            {
                return default;
            }
        }
        public static string GetValue(this IDictionary<string, string> resource, string key)
        {
            return resource.GetValue(key, null);
        }

        public static string GetValue(this IDictionary<string, string> resource, string key, string defaultValue)
        {
            if (resource.TryGetValue(key, out string value) == false)
                value = defaultValue;
            // If the key is in dictionary but it's empty (translation has not yet been added)
            else if (string.IsNullOrEmpty(value))
                // Set the default one from code
                value = defaultValue;

            return value;
        }

        public static string Translate(this DayOfWeek day)
        {
            var dayStr = day.ToString();
            return Statics.LanguageDictionary.GetValue($"WEEKDAY_{dayStr.ToUpper()}", dayStr);
        }

        public static Schedule Clone(this Schedule source)
        {
            return new Schedule()
            {
                Date = source.Date,
                Day = source.Day,
                EndDate = source.EndDate,
                IsOncePerMonth = source.IsOncePerMonth,
                IsOncePerWeek = source.IsOncePerWeek,
                Month = source.Month,
                MonthRecurrencyType = source.MonthRecurrencyType,
                StartDate = source.StartDate,
                Week = source.Week,
                WeekDay = source.WeekDay,
                Weekday0 = source.Weekday0,
                Weekday1 = source.Weekday1,
                Weekday2 = source.Weekday2,
                Weekday3 = source.Weekday3,
                Weekday4 = source.Weekday4,
                Weekday5 = source.Weekday5,
                Weekday6 = source.Weekday6,
                WeekDayNumber = source.WeekDayNumber,
            };
        }

        public static StepModel Clone(this StepModel source)
        {
            return new StepModel()
            {
                Description = source.Description,
                Id = source.Id,
                Index = source.Index,
                Picture = source.Picture,
                TaskTemplateId = source.TaskTemplateId,
                Video = source.Video,
                VideoThumbnail = source.VideoThumbnail
            };
        }

        public static string ToApiString(this TaskStatusEnum status)
        {
            if (status == TaskStatusEnum.NotOk)
                return "not ok";

            return status.ToString().ToLower();
        }
    }
}