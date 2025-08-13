using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Utils.Environments
{
    public class TestEnvironment : BaseEnvironment
    {
        public TestEnvironment() : base()
        {
        }

        public override Dictionary<string, string> PopulateEnvironment()
        {
            var dic = base.PopulateEnvironment();

            dic.TryAdd("ApiBaseUrl", "https://ezgo.testapi.ezfactory.nl/");
            dic.TryAdd("EnvironmentIdentifier", "T");

            return dic;
        }
    }
}
