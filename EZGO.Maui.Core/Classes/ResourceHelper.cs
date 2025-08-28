using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public static class ResourceHelper
    {
        public static T GetApplicationResource<T>(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out object val))
            {
                return (T)val;
            }

            return default(T);
        }
    }
}
