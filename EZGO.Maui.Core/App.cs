using Autofac;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core
{
    /// <summary>
    /// New application class.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Application" />
    public class App : Application
    {
        public static IServiceProvider Container { get; protected set; }

        /// <summary>
        /// The builder that builds the dependency container
        /// </summary>
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public static void SetApiUrl()
        {
            Statics.ApiUrl = $"{Constants.ApiBaseUrl}v1/";
            if (!Constants.ApiBaseUrl.Equals("https://ezgo.testapi.ezfactory.nl/") &&
                !Constants.ApiBaseUrl.Equals("https://ezgo.accapi.ezfactory.nl/"))
            {
                SentrySdk.Close();
            }
        }

        /// <summary>
        /// Application developers override this method to perform actions when the application starts.
        /// </summary>
        protected override async void OnStart()
        {
            Initialize();
            //AppCenter.Start("android=d5743f63-5dda-4bb0-91a8-50b71a9faad2;" + "ios=0ebca781-9a60-4017-8a22-db2bf8d6f560",
            //typeof(Analytics), typeof(Crashes));

            //#if DEBUG
            //            await Crashes.SetEnabledAsync(false);
            //            await Analytics.SetEnabledAsync(false);
            //#endif
            base.OnStart();
        }

        /// <summary>
        /// Initializes the application.
        /// </summary>
        protected virtual void Initialize()
        {
            RegisterDependencies();
        }

        /// <summary>
        /// Registers the dependencies.
        /// </summary>
        protected virtual void RegisterDependencies()
        {

        }
    }
}
