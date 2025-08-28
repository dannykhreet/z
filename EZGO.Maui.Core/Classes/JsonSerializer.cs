using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization;
using NodaTime.Serialization.JsonNet;

namespace EZGO.Maui.Core.Classes
{
    public class JsonSerializer
    {
        private const string _debCat = "[Serializer]";
        private static JsonSerializerSettings settings = new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        private JsonSerializer()
        {
        }

        public static string Serialize(object? value, DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Unspecified, bool ignoreNullValues = false)
        {
            if (ignoreNullValues)
                settings.NullValueHandling = NullValueHandling.Ignore;
            else
                settings.NullValueHandling = NullValueHandling.Include;

            settings.DateTimeZoneHandling = dateTimeZoneHandling;
            var strData = JsonConvert.SerializeObject(value, settings);
            //Debug.WriteLine($"\n\tSerialized Data: {strData}", _debCat);
            return strData;
        }

        public static string Serialize<T>(object? value, bool ignoreNullValues = false)
        {
            var strData = JsonConvert.SerializeObject(value, settings);
            //Debug.WriteLine($"\n\tSerialized Data: {strData}", _debCat);
            return strData;
        }

        public static T Deserialize<T>(string data, DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Unspecified)
        {
            try
            {

                settings.DateTimeZoneHandling = dateTimeZoneHandling;
                var deserializedData = JsonConvert.DeserializeObject<T>(data, settings);
                //Debug.WriteLine($"\n\tDeserialized Data: {deserializedData}", _debCat);
                return deserializedData;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return default(T);
            }
        }
    }
}
