using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ApiTests.Helpers
{
    /// <summary>
    /// JsonExtensions; JSON related extensions.
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// ToJsonFromObject; Creates a JSON string from a .net object. The Text.Json serializer (.net3.1) is used for serializing.
        /// </summary>
        /// <param name="obj">The object that needs to be converted.</param>
        /// <param name="useCamelCasingProperties">Use the camel JsonNamingPolicy.</param>
        /// <param name="useIgnoreNullValues">Ignores null values in JSON.</param>
        /// <returns>A JSON string based of the object that was supplied.</returns>
        public static string ToJsonFromObject(this object obj, bool useCamelCasingProperties = false, bool useIgnoreNullValues = true)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            if (useCamelCasingProperties)
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }

            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// ToObjectFromJson; Creates a object from a JSON string based on the T (type) of the object that is needed.
        /// </summary>
        /// <typeparam name="T">Type of the object that needs to be generated.</typeparam>
        /// <param name="objString">The string containing the JSON structure.</param>
        /// <returns>A object of type T.</returns>
        public static T ToObjectFromJson<T>(this string objString)
        {
            return (T)JsonSerializer.Deserialize(objString, typeof(T));
        }
    }
}
