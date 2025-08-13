using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Utils.Environments
{
    public class ProductionEnvironment : BaseEnvironment
    {
        public ProductionEnvironment()
        {
        }

        public override Dictionary<string, string> PopulateEnvironment()
        {
            var dic = base.PopulateEnvironment();

            dic.TryAdd("ApiBaseUrl", "https://connect.ezfactory.nl/");
            dic.TryAdd("EnvironmentIdentifier", "P");

            return dic;
        }
    }
}
