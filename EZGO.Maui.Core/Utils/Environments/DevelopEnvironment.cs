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
            // dic.TryAdd("ApiBaseUrl", "https://6w6dvwth-56864.euw.devtunnels.ms/");
            dic.TryAdd("ApiBaseUrl", "https://localhost:56864/");
            dic.TryAdd("EnvironmentIdentifier", "A");

            return dic;
        }
    }
}
