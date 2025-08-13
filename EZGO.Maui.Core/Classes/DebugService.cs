using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EZGO.Maui.Core.Classes
{
    public static class DebugService
    {
        private static Dictionary<string, Stopwatch> times = new Dictionary<string, Stopwatch>();

        [Conditional("DEBUG")]
        public static void Start(string category)
        {
            Stopwatch st = GetStopwatch(category);
            st.Reset();
            st.Start();
        }

        private static Stopwatch GetStopwatch(string category)
        {
            Stopwatch st;
            if (times.TryGetValue(category, out Stopwatch value))
            {
                st = value;
            }
            else
            {
                st = new Stopwatch();
                times.Add(category, st);
            }

            return st;
        }

        [Conditional("DEBUG")]
        public static void Stop(string category)
        {
            Stopwatch st = GetStopwatch(category);
            st.Stop();
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string msg, string category)
        {
            Debug.WriteLine(msg, category);
        }

        [Conditional("DEBUG")]
        public static void WriteLineWithTime(string msg, string category)
        {
            Stopwatch st = GetStopwatch(category);
            Debug.WriteLine($"{msg}, time took: {st.ElapsedMilliseconds}ms", $"[{category}]\n\t");
        }
    }
}
