using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Generation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace EZGO.WorkerService.TaskGeneration
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ITaskGenerationManager _taskGenerationManager;
        private readonly IConfigurationHelper _configHelper;
        private readonly IHostEnvironment _hostingEnvironment;
        private int _lastRunTimeNumber;
        private List<int> _timeNumbers;
        private string _timeType = "hour"; //supported types hour, minute; Hour will be the default.
        private bool run = false;
        private bool running = false;
        private int _looptimeInMs;
        private bool _enabled = true;
        private DateTime _lastRunTime = DateTime.MinValue;
        private DateTime _lastSettingTime = DateTime.MinValue;
        private bool _initFirstRun = false;


        public Worker(ITaskGenerationManager taskGenerationManager, ILogger<Worker> logger, IConfigurationHelper configurationHelper, IHostEnvironment env)
        {
            _logger = logger;
            _taskGenerationManager = taskGenerationManager;
            _configHelper = configurationHelper;
            _hostingEnvironment = env;
            _timeNumbers = GetTimeNumbersFromConfig();
            _timeType = GetTimeTypeFromConfig();
            _looptimeInMs = GetLoopTimeInMsFromConfig();
            _lastSettingTime = DateTime.Now;
            _enabled = GetGenerationActiveFromConfiguration();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (_looptimeInMs <= 1000) _looptimeInMs = 1000; //default to 1000

            _logger.LogInformation("TaskGeneration Worker {version}|{enabled} running {env} started at: {time}", GetType().Assembly.GetName().Version.ToString(), _enabled.ToString().ToLower(), _hostingEnvironment.EnvironmentName, DateTimeOffset.Now);
            await _taskGenerationManager.AddGenerationLogEvent(message: string.Format("TaskGeneration Worker {2}|{3} running {0} started at: {1}", _hostingEnvironment.EnvironmentName, DateTimeOffset.Now, GetType().Assembly.GetName().Version.ToString(), _enabled.ToString().ToLower()));

            while (!stoppingToken.IsCancellationRequested)
            {
                if((!running && !run) || (!_initFirstRun)) await InitiateOrUpdateSettings();

                if(_enabled)
                {
                    run = false; //set run to false

                    if (_timeType.ToUpper() == "MINUTES" || _timeType.ToUpper() == "MINUTE")
                    {
                        if (_timeNumbers.Contains(DateTime.Now.Minute) && ((_lastRunTime.Hour != DateTime.Now.Hour && _lastRunTimeNumber == DateTime.Now.Minute) || (_lastRunTime.Hour == DateTime.Now.Hour && _lastRunTimeNumber != DateTime.Now.Minute) || _lastRunTimeNumber != DateTime.Now.Minute))
                        {
                            _lastRunTime = DateTime.Now;
                            _lastRunTimeNumber = _lastRunTime.Minute;
                            await _taskGenerationManager.AddGenerationLogEvent(message: string.Format(">> Fire minute timer: {0}", DateTimeOffset.Now));
                            run = true; //start run!
                        }
                    }
                    else
                    {
                        if (_timeNumbers.Contains(DateTime.Now.Hour) && ((_lastRunTime.Day != DateTime.Now.Day && _lastRunTimeNumber == DateTime.Now.Hour) || (_lastRunTime.Day == DateTime.Now.Day && _lastRunTimeNumber != DateTime.Now.Hour) || _lastRunTimeNumber != DateTime.Now.Hour))
                        {
                            _lastRunTime = DateTime.Now;
                            _lastRunTimeNumber = _lastRunTime.Hour;
                            await _taskGenerationManager.AddGenerationLogEvent(message: string.Format(">> Fire hour timer: {0}", DateTimeOffset.Now));
                            run = true; //start run!
                        }
                    }

                    if (run && !running)
                    {
                        running = true;
                        if (_configHelper.GetValueAsBool(Api.Settings.TaskGenerationSettings.TASKGENERATION_ACTIVE_CONFIG_KEY))
                        {
                            await _taskGenerationManager.AddGenerationLogEvent(message: string.Format(">> RUN STARTED!! {0}", DateTimeOffset.Now));
                            try
                            {
                                var outcome = await _taskGenerationManager.GenerateAll(stoppingToken: stoppingToken);
                                await _taskGenerationManager.AddGenerationLogEvent(message: string.Format("TaskGeneration succes: {0}", outcome), eventId: 200);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(exception: ex, string.Concat("Error occurred", ex.Message));
                            }
                        }
                        running = false;
                    }

                    run = false;
                }

                await Task.Delay(_looptimeInMs, stoppingToken);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                await _taskGenerationManager.AddGenerationLogEvent(message: "TaskGeneration stopped due token");
                _logger.LogInformation("TaskGeneration stopped due token");
            }
        }

        /// <summary>
        /// GetTimeNumbers; Get a list of time numbers (e.g. full hours, minutes)
        /// </summary>
        /// <returns>A list of numbers.</returns>
        private List<int> GetTimeNumbersFromConfig()
        {
            var items = _configHelper.GetValueAsString(Api.Settings.TaskGenerationSettings.RUNNABLE_TIME_NUMBER_CONFIG_KEY);
            var numbers = (items.Split(",")).Select(x => Convert.ToInt32(x)).ToList();
            return numbers;
        }

        /// <summary>
        /// GetTimeTypeFromConfig;
        /// </summary>
        /// <returns></returns>
        private string GetTimeTypeFromConfig()
        {
            var output = _configHelper.GetValueAsString(Api.Settings.TaskGenerationSettings.RUNNABLE_TIME_TYPE_CONFIG_KEY);
            return output;
        }

        /// <summary>
        /// GetLoopTimeInMsFromConfig;
        /// </summary>
        /// <returns></returns>
        private int GetLoopTimeInMsFromConfig()
        {
            var output = _configHelper.GetValueAsInteger(Api.Settings.TaskGenerationSettings.TASKGENERATION_AUTOMATED_LOOP_TIMEOUT_CONFIG_KEY);
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool GetGenerationActiveFromConfiguration()
        {
            var output = _configHelper.GetValueAsBool(Api.Settings.TaskGenerationSettings.TASKGENERATION_ACTIVE_CONFIG_KEY);
            return output;
        }


        /// <summary>
        /// InitiateOrUpdateSettings;
        /// </summary>
        private async Task InitiateOrUpdateSettings()
        {
           
            if (_lastSettingTime < DateTime.Now.AddMinutes(-5) || !_initFirstRun)
            {
                await _taskGenerationManager.AddGenerationLogEvent(message: string.Format("InitiateOrUpdateSettings : {0}", DateTime.Now));

                _lastSettingTime = DateTime.Now;
                //_lastSettingTimeNumber = DateTime.Now.Minute;
                var retrievedTimeType = await _taskGenerationManager.GetRunnableType();
                if (!string.IsNullOrEmpty(retrievedTimeType))
                {
                    _timeType = retrievedTimeType;
                }

                if (string.IsNullOrEmpty(_timeType))
                {
                    _timeType = GetTimeTypeFromConfig();
                }
                var retrievedTimeNumbers =  _timeType.ToUpper() == "MINUTES" || _timeType.ToUpper() == "MINUTE" ?
                                            await _taskGenerationManager.GetRunnableMinutes() :
                                            await _taskGenerationManager.GetRunnableHours();

                if (retrievedTimeNumbers != null && retrievedTimeNumbers.Any())
                {
                    _timeNumbers = retrievedTimeNumbers;
                }

                if (_timeNumbers == null && !_timeNumbers.Any())
                {
                    _timeNumbers = GetTimeNumbersFromConfig();
                }

                _looptimeInMs = GetLoopTimeInMsFromConfig();

                if (_timeType.ToUpper() == "HOURS" || _timeType.ToUpper() == "MINUTES" || _timeType.ToUpper() == "HOUR" || _timeType.ToUpper() == "MINUTE") { _enabled = true; } else { _enabled = false; }

                _enabled = GetGenerationActiveFromConfiguration();

                _initFirstRun = true;

            }

        }

    }
}
