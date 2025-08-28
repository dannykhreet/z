using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EZGO.Maui.Core.Utils
{
    /// <summary>
    /// This utility will show the embedded resources
    /// TODO: Leave out when compiling for production
    /// </summary>
    public class EmbeddedResourcesDebug
    {
        private static EmbeddedResourcesDebug _instance;

        public static EmbeddedResourcesDebug Debug
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EmbeddedResourcesDebug();
                }

                return _instance;
            }
        }

        public void Run()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(EmbeddedResourcesDebug)).Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                System.Diagnostics.Debug.WriteLine("found resource: " + res);
            }
        }
    }
}
