using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Helpers
{
    public static class Extensions
    {
        public static string GetValue(this IDictionary<string, string> resource, string key, string defaultValue)
        {
            string value = defaultValue;
            if (resource != null)
            {
                if (resource.TryGetValue(key, out value) == false)
                    value = defaultValue;
            }

            return value;
        }
    }
}
