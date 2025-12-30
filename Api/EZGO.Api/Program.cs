using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.Configuration.SystemsManager;
using EZGO.Api.Utils.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EZGO.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder based on full implementation and config. 
            //If not working (e.g. any of the 3rd party functionalities don't function, basic hostbuilder will be run without those. 
            try
            {
                CreateHostBuilder(args, false).Build().Run();
            }
            catch (Exception ex)
            {
                //logger.LogError(exception: ex, message: string.Format("Error: {0}", ex.Message));
                Debug.WriteLine(string.Format("Error: {0}", ex.Message));

                //Run basic API config, no 3rd party for now
                CreateHostBuilder(args, true).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, bool forceBasic) =>
            Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddEnvironmentVariables();
                var env = builderContext.HostingEnvironment;
                if (env.IsEnvironment("LocalDevelopment"))
                {
                    //Add user sercrets structure for local development (will load on default with development)
                    config.AddUserSecrets<Startup>();
                }

                if (forceBasic == false)
                {
                    try
                    {
                        //Get configuration for specific environment to check if AWS SYSTEM MANAGER needs to be added. 
                        IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                        IConfigurationRoot localConfiguration = builder.Build();
                        var sectionValue = localConfiguration.GetSection("AppSettings:AWSSystemManagerEnabled").Value;

                        if (sectionValue != null && Convert.ToBoolean(sectionValue))
                        {
                            ISystemsManagerConfigurationSource configurationSource = new SystemsManagerConfigurationSource();
                            //configurationSource.Optional.
                            //Add system manager (AWS) structure for retrieving parameters from parameter store 
                            config.AddSystemsManager(
                                configSource =>
                                {
                                    configSource.Path = $"/ez-api/{env.EnvironmentName}";
                                    configSource.ReloadAfter = TimeSpan.FromMinutes(10);
                                    configSource.OnLoadException += exceptionContext =>
                                    {
                                        //on load exception, note not always works.
                                        Console.WriteLine(exceptionContext.Exception.Message);
                                        Debug.WriteLine(string.Format("Error: {0}", exceptionContext.Exception.Message));
                                    };
                                });
                            // path: $"/ez-api/{env.EnvironmentName}", reloadAfter: TimeSpan.FromMinutes(10));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("Error: {0}", ex.Message));
                        Console.WriteLine(ex.Message);
                    }
                }

                /*
                 *  For checking the configuration of a other state locally use the following webbuilder part:
                 *
                        var env = builderContext.HostingEnvironment;
                        if (env.IsEnvironment("Test"))
                        {
                            config.AddUserSecrets<Startup>();
                        }
                */
            }).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.AddServerHeader = false;
                });
                webBuilder.UseStartup<Startup>();
                // Enable for local longer request if needed;
                //webBuilder.ConfigureKestrel(options =>
                //{
                //    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(60);
                //});
            });
    }
}

