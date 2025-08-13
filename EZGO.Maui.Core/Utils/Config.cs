using System;
using EZGO.Maui.Core.Utils.Environments;

namespace EZGO.Maui.Core.Utils
{
    public class Config
    {
        private Config() { }

        private static BaseEnvironment environment;

        public static Env CurrentEnv;

        public static void SetEnv()
        {
#if USE_ACCEPTANCE
            Set(Env.DEVELOP);
#elif USE_PRODUCTION
            Set(Env.PRODUCTION);
#elif USE_TEST
            Set(Env.TEST);
#else
#error Build configuration must define which environment to use
#endif
        }

        private static void Set(Env env)
        {
            CurrentEnv = env;

            environment = env switch
            {
                Env.DEVELOP => new DevelopEnvironment(),
                Env.PRODUCTION => new ProductionEnvironment(),
                Env.TEST => new TestEnvironment(),
                _ => new DevelopEnvironment(),
            };
        }

        public static void UpdateSettings()
        {
            environment.UpdateEnv();
        }

        public static string GetData(string key)
        {
            return environment.GetValue(key);
        }
    }

    public enum Env
    {
        DEVELOP, PRODUCTION, TEST,
    }
}
