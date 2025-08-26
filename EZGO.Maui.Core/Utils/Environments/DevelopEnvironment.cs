using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Core.Utils.Environments
{
    public class DevelopEnvironment : BaseEnvironment
    {
        public DevelopEnvironment() : base()
        {
        }

        public override Dictionary<string, string> PopulateEnvironment()
        {
            var dic = base.PopulateEnvironment();

            dic.TryAdd("ApiBaseUrl", "https://ezgo.accapi.ezfactory.nl/");
            dic.TryAdd("EnvironmentIdentifier", "A");

            return dic;
        }
    }
}
