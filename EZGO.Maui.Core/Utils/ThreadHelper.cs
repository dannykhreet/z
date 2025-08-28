using System;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.ApplicationModel;

namespace EZGO.Maui.Core.Utils
{
    public static class ThreadHelper
    {
        public static void IsRunningOnMainThread(string name)
        {
            if (MainThread.IsMainThread)
            {
                DebugService.WriteLine($"{name} - on the main thread", "[ThreadHelper]");
            }
            else
            {
                DebugService.WriteLine($"{name} - not on the main thread", "[ThreadHelper]");
            }
        }
    }
}

