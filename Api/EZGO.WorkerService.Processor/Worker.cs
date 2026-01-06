using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Processor;
using EZGO.Api.Interfaces.Provisioner;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EZGO.WorkerService.Processor
{
    /// <summary>

    /// </summary>
    public class Worker : BackgroundService
    {
        //services
        private readonly ILogger<Worker> _logger; //Logger, used for logging to ILogger (.net)
        private readonly IDatabaseAccessHelper _dataManager; //DataManager for retrieving data from DB
        private readonly IConfigurationHelper _configHelper; //ConfigurationHelper, retrieving data from config/env settings
        private readonly IConnectionHelper _connectionHelper; ///ConnectionHelper; Used for determining connection strings.
        private readonly IDatabaseLogWriter _databaseLogWriter; //DatabaseLogWriter; for writing data to logging table EZGO. 
        private readonly IProcessorManager _processorManager;

        //internal variables. 
        private bool _processorEnabled = false;
        private bool _debugToDbEnabled = false;
        private string _environment = "";
        private bool _flatteningRunning = false;
        private bool _demoModeOnly { get { return _configHelper.GetValueAsBool("AppSettings:DemoModeOnly"); } }

        public IServiceProvider _services { get; }


        /// <summary>
        /// Worker; Contructor + loading of config variables needed on startup. 
        /// </summary>
        public Worker(IServiceProvider services, ILogger<Worker> logger, IDatabaseAccessHelper dataManager, IProcessorManager processorManager, IConfigurationHelper configHelper, IConnectionHelper connectionHelper, IDatabaseLogWriter databaseLogWriter)
        {
            _logger = logger;
            _dataManager = dataManager;
            _configHelper = configHelper;
            _connectionHelper = connectionHelper;
            _services = services;
            _processorEnabled = _configHelper.GetValueAsBool("AppSettings:ProcessorEnabled");
            _debugToDbEnabled = _configHelper.GetValueAsBool("AppSettings:EnableDebugToDb");
            _environment = _configHelper.GetValueAsString("DOTNET_ENVIRONMENT");
            _databaseLogWriter = databaseLogWriter;
            _processorManager = processorManager;

        }

        /// <summary>
        /// ExecuteAsync; Main task for running worker service.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processor {version} | {enabled} started for {environment} at {time}", GetType().Assembly.GetName().Version.ToString(), _processorEnabled.ToString().ToLower(), _environment, DateTimeOffset.Now);
            await _processorManager.AddProcessorLogEvent(message: string.Format("Processor {0} | {1} started for {2} at {3} in demo mode {4}", GetType().Assembly.GetName().Version.ToString(), _processorEnabled.ToString().ToLower(), _environment, DateTimeOffset.Now, _demoModeOnly), eventName: "STARTUP", description: "Processor Started");

            await Task.CompletedTask;

            if (!_processorEnabled)
            {
                 await _processorManager.AddProcessorLogEvent(message: "Processor setting [ProcessorEnabled] is disabled.", description: "Processor disabled.");
            }

            //TO RUN ON STARTUP ENABLE CODE BELOW
            //if (_processorEnabled)
            //{
            //    _ = Task.Run(() => RunFlattening(cancellationToken: stoppingToken));
            //}

            //TODO add schedule structure when extending. 
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(60000, stoppingToken); //run every minute

                if (_processorEnabled && !_flatteningRunning) //export enabled through configuration. 
                {
                    if(DateTime.Now.Minute == 30) //run every 30th minute of an our (until scheduling is build-in
                    {
                        _ = Task.Run(() => RunFlattening(cancellationToken: stoppingToken));
                    } 
                   
                }

            }

            await Task.CompletedTask;

        }

        private async Task RunFlattening(CancellationToken cancellationToken)
        {
            var internalGuid = Guid.NewGuid().ToString();

            //already running or cancelation.
            if (cancellationToken.IsCancellationRequested || _flatteningRunning == true)
            {
                return;
            }

            try
            {
                using (var scope = _services.CreateScope())
                {
                    //create scoped manager for parallelism if needed.
                    var scopedManager = scope.ServiceProvider.GetRequiredService<IFlattenAutomatedManager>();
                    var result = true;

                    if (scopedManager != null)
                    {
                        try
                        {
                            _flatteningRunning = true;
                            result = await scopedManager.FlattenCurrentTemplatesAll();
                            if (!result) await scopedManager.AddFlattenerLogEvent(message: string.Format("> Flatten not successful {0}", internalGuid), description: "FlattenCurrentTemplatesAll not fully successful. One or more flatten methods failed.");
                        }
                        catch (Exception ex)
                        {
                            await scopedManager.AddFlattenerLogEvent(message: string.Format("> ERROR {0}", internalGuid), description: ex.Message);
                            _logger.LogInformation("Error occurred {0} - {1}", ex.Message, internalGuid);
                            _logger.LogError(exception: ex, message: ex.Message);
                        }
                        finally
                        {
                            _flatteningRunning = false;
                        }
                    }

                }
            } catch(Exception ex)
            {
                _logger.LogError(exception: ex, "Error RunFlattening");

            }

        }

    }  


}
