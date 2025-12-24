using EZGO.Api.Interfaces.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

//TODO determan if logger needs to be moved to other dll due to references.
namespace EZGO.Api.Utils.Logging
{
    /// <summary>
    /// DatabaseLogger; Database logger for sending logging to a database for later usage.
    /// </summary>
    public class DatabaseLogger : ILogger
    {
        private IDatabaseLogWriter _logwriter;

        private Dictionary<string, DateTime> _lastLogs = new Dictionary<string, DateTime>();

        private List<string> _messagesToThrottle = new List<string>()
        {
            "{PayloadSenderV2} Failed sending event. Events intake API absolute URL: https://apm.ezfactory.nl/intake/v2/events. APM Server response: status code: ServiceUnavailable, content: \r\n{\"accepted\":0,\"errors\":[{\"message\":\"queue is full\"}]}\r\n",
            "Warning: Unauthorized. User logged in on other device or user session has expired."
        };

        private TimeSpan _throttleDuration = TimeSpan.FromMinutes(10);

        public DatabaseLogger(IDatabaseLogWriter logwriter)
        {
            _logwriter = logwriter;
        }

        public bool IsEnabled(LogLevel logLevel) {
            //TODO check loglevel and determan if needed (add params to constructor)
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) {
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

                if((exception!=null && exception.Data.Contains("Execution Error")) || message.Contains("(DB)"))
                {
                    useDbLogger = false;
                }
                //db message is already logged in DB
                if(useDbLogger)
                {
                    string source = "";

                    //check if _messagesToLogLessOften contains message
                    //check if message has been logged in the last _throttleDuration TimeSpan (10 minutes)
                    if (_messagesToThrottle != null && _messagesToThrottle.Contains(message) && 
                        _lastLogs != null && _lastLogs.TryGetValue(message, out var messageLastLogged) && 
                        messageLastLogged != DateTime.MinValue && messageLastLogged.Add(_throttleDuration) > DateTime.UtcNow)
                    {
                        //dont log message
                        //maybe log something in console?
                    }
                    else
                    {
                        var runnableWriteTask = Task.Run(() => _logwriter.WriteToLog(message: message, type: logLevel.ToString().ToUpper(), eventid: eventId.Id.ToString(), eventname: eventId.Name, description: $"{formatter(state, exception)}", source: source).ConfigureAwait(false));

                        if (_lastLogs == null)
                        {
                            _lastLogs = new Dictionary<string, DateTime>();
                        }

                        _lastLogs[message] = DateTime.UtcNow;
                    }
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

