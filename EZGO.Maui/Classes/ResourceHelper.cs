using System;
namespace EZGO.Maui.Classes
{
    public class ResourceHelper
    {
        private ResourceHelper()
        {
        }

        public static T GetValueFromResources<T>(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value))
            {
                return GetResource<T>(value);
            }

            return default(T);
        }

        public static T GetValueFromLocal<T>(ResourceDictionary resources, string key)
        {
            if (resources.TryGetValue(key, out var value))
            {
                return GetResource<T>(value);
            }

            return default(T);
        }

        private static T GetResource<T>(object value)
        {
            if (value is OnPlatform<string> onPlatform)
            {
                var resource = onPlatform.Platforms.Where(x => x.Platform.First() == DeviceInfo.Platform.ToString()).FirstOrDefault();

                if (resource == null) return default(T);

                return (T)resource.Value;
            }
            else if (value is OnPlatform<double> onPlatformDouble)
            {
                var resource = onPlatformDouble.Platforms.Where(x => x.Platform.First() == DeviceInfo.Platform.ToString()).FirstOrDefault();

                if (resource == null) return default(T);

                return (T)resource.Value;
            }
            else if (value is object color)
            {
                return (T)color;
            }

            return default(T);
        }
    }
}