using EZGO.Api.Interfaces.Data;
using EZGO.Api.Settings.Helpers;
using EZGO.WorkerService.Data.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.WorkerService.TaskGeneration.Utils
{
    /// <summary>
    /// WorkerServiceDatabaseLogger; Database logger for sending logging to a database for later usage.
    ///  NOTE! only for use with worker service!!
    /// </summary>
    public class WorkerServiceDatabaseLogger : ILogger
    {
        private IConfiguration _config;
        private IDatabaseLogWriter _logwriter;
        public WorkerServiceDatabaseLogger(IConfiguration configuration)
        {
            _config = configuration;
            this.CreateLogWriter();
        }

        private void CreateLogWriter()
        {
            try
            {
                //create log writer without DI;
                _logwriter = new DatabaseLogWriter(connectionHelper: new ConnectionHelper(Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT) != null ?
                                                                            Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT) :
                                                                            _config.GetConnectionString(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME)),
                                                                            configurationHelper: new ConfigurationHelper(_config));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            //TODO check loglevel and determan if needed (add params to constructor)
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var useDbLogger = true;
                string message = $"{formatter(state, exception)}";
                if (message.Length > 250)
                {
                    message = message.Substring(0, 254);
                }

                if ((exception != null && exception.Data.Contains("Execution Error")) || message.Contains("(DB)"))
                {
                    useDbLogger = false;
                }
                //db message is already logged in DB
                if (useDbLogger)
                {
                    string source = "";

                    //Create background task, fire and forget.
                    var runnableWriteTask = Task.Run(() => _logwriter.WriteToLog(message: message, type: logLevel.ToString().ToUpper(), eventid: eventId.Id.ToString(), eventname: eventId.Name, description: $"{formatter(state, exception)}", source: source).ConfigureAwait(false));
                    // runnableWriteTask.Start();
                }

            }
            catch (Exception ex)
            {
                var exc = ex;
                //ignore
            }
        }
    }
}
