using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public static class LiveCycleHelper
    {
        public static string GetAliveObjects(object obj)
        {
            var type = obj.GetType();
            var declearingType = type.DeclaringType;
            StringBuilder stringBuilder = new StringBuilder($"{type.Name} alive objects\n\r");
            stringBuilder.Append($"DeclaringType: {declearingType?.FullName}\n");
            var properties = type.GetProperties();
            var fileds = type.GetFields();
            var runtimeEvents = type.GetRuntimeEvents();
            var events = type.GetEvents();

            stringBuilder.Append("Properties\n");
            foreach (var item in properties)
            {
                var propertyValue = item.GetValue(obj);
                AppendObjectInfo(stringBuilder, item, propertyValue);
            }

            foreach (var item in fileds)
            {
                var filedValue = item.GetValue(obj);
                AppendObjectInfo(stringBuilder, item, filedValue);
            }

            foreach (var item in runtimeEvents)
            {
                var eventMethodSource = item.GetAddMethod();
                AppendObjectInfo(stringBuilder, item, eventMethodSource);
            }

            foreach (var item in events)
            {
                var eventMethodSource = item.GetAddMethod();
                AppendObjectInfo(stringBuilder, item, eventMethodSource);
            }

            return stringBuilder.ToString();
        }

        private static void AppendObjectInfo<T>(StringBuilder stringBuilder, T item, object filedValue) where T : MemberInfo
        {
            if (filedValue != null)
            {
                stringBuilder.Append($"{item.Name}: {filedValue}\r\n");
            }
        }
    }
}
