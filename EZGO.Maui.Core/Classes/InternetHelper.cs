using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Helper class for internet/connection related functionality.
    /// </summary>
    public static class InternetHelper
    {
        private static bool httpClientInitialized;
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri(Constants.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(5),
        };

        public static bool HasNetworkAccess => Connectivity.NetworkAccess.Equals(NetworkAccess.Internet);

        /// <summary>
        /// Determines whether the user has a internet and API connection asynchronous.
        /// </summary>
        /// <param name="ignoreToken">if set to <c>true</c> their will be no check if a token is present.</param>
        /// <returns>
        ///   <c>true</c> if the user has a internet and API connection; otherwise, <c>false</c>.
        /// </returns>
        public static async Task<bool> HasInternetAndApiConnectionAsync(bool ignoreToken = false)
        {
            bool result = false;

            try
            {
                if ((ignoreToken || !Settings.Token.IsNullOrEmpty()) && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    if (!httpClientInitialized)
                    {
                        httpClient.DefaultRequestHeaders.Accept.Clear();

                        httpClientInitialized = true;
                    }

                    HttpResponseMessage response = await httpClient.GetAsync("health").ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        result = JsonSerializer.Deserialize<bool>(content);
                    }

                }
            }
            catch (Exception exception)
            {
                //Crashes.TrackError(exception);
            }

            return result;
        }

        /// <summary>
        /// Determines whether the user has a internet and API connection asynchronous, but ignores the token.
        /// This one is needed, because if you pass the method above as a Func argument, you can't pass the method parameter.
        /// </summary>
        /// <returns>
        ///    <c>true</c> if the user has a internet and API connection; otherwise, <c>false</c>.
        /// </returns>
        public static async Task<bool> HasInternetAndApiConnectionIgnoreTokenAsync()
        {
            return await HasInternetAndApiConnectionAsync(true);
        }

        public static async Task<DateTime> GetServerTimeUtcAsync()
        {
            DateTime result = DateTime.MinValue;
            try
            {
                if (!httpClientInitialized)
                {
                    httpClient.BaseAddress = new Uri(Constants.ApiBaseUrl);
                    httpClient.Timeout = TimeSpan.FromSeconds(5);
                    httpClient.DefaultRequestHeaders.Accept.Clear();

                    httpClientInitialized = true;
                }

                HttpResponseMessage response = await httpClient.GetAsync("health/apitime");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<DateTime>(content, dateTimeZoneHandling: DateTimeZoneHandling.Utc);
                    StringBuilder sb = new StringBuilder($"DateTimes Info:\nBase result: {result}\n\t");
                    var local = LocalDateTime.FromDateTime(result);
                    sb.Append($"Local time from result: {local}\n\t");

                    var date = LocalDateTime.FromDateTime(result, CalendarSystem.Gregorian);
                    sb.Append($"Date from result using GregorianCalendar: {date}\n\t");
                    var currentTimezons = TimeZoneInfo.Local;
                    sb.Append($"Current Timezone: {currentTimezons.DisplayName}\n\t");

                    var baseOffset = currentTimezons.BaseUtcOffset;
                    sb.Append($"Base offset: {baseOffset}\n\t");

                    var utcOffset = currentTimezons.GetUtcOffset(result);
                    sb.Append($"Utc offset: {utcOffset}\n\t");

                    var zoned = ZonedDateTime.FromDateTimeOffset(result);
                    sb.Append($"ZonedDateTime: {zoned}\n\t");

                    var zonedLocal = ZonedDateTime.FromDateTimeOffset(DateTimeOffset.Parse(date.ToString()));
                    sb.Append($"ZonedDateTime: {zoned}\n\t");
                    var timezone = DateTimeZone.ForOffset(Offset.FromTimeSpan(baseOffset));
                    sb.Append($"Timezone for offset: {timezone}\n\t");
                    Debug.WriteLine(sb.ToString());
                }
            }
            catch { }
            return result;
        }

        public static async Task<bool> HasInternetConnection()
        {
            bool result = false;
            try
            {
                var current = Connectivity.NetworkAccess;

                if (current.Equals(NetworkAccess.Internet))
                {
                    HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("health").ConfigureAwait(false);

                    result = httpResponseMessage.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                //Debugger.Break();
                result = false;
            }

            return result;
        }
    }
}
