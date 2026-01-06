using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Processor;
using EZGO.Api.Interfaces.Provisioner;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Processor;
using EZGO.Api.Logic.Processor.Base;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EZGO.Api.Logic.Processor
{
    /// <summary>
    /// ProcessorManager; Main manager for processing data; Specific functionalities will have their own manager depending on the functionality. 
    /// </summary>
    public class ProcessorManager : BaseManager<ProcessorManager>, IProcessorManager
    {
        private readonly IDatabaseAccessHelper _dbmanager;
        private readonly IConfigurationHelper _configHelper;
        private readonly IConnectionHelper _connectionHelper;
        private List<string> messages = new List<string>();

        private bool _demoModeOnly { get { return _configHelper.GetValueAsBool("AppSettings:DemoModeOnly"); } }


        public ProcessorManager(IDatabaseAccessHelper dbmanager, IConfigurationHelper configHelper, IConnectionHelper connectionHelper, ILogger<ProcessorManager> logger) : base(logger)
        {
            _dbmanager = dbmanager;
            _configHelper = configHelper;
            _connectionHelper = connectionHelper;
        }

        #region - logging -
        /// <summary>
        /// AddProvisionerLogEvent; Adds item to provisioner log. 
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="eventId">Possible event id</param>
        /// <param name="type">Type of message</param>
        /// <param name="eventName">Possible event name</param>
        /// <param name="description">Description, containing more details information if available. </param>
        /// <returns>true/false (will mostly be ignored, but can be used if needed.)</returns>
        public async Task<bool> AddProcessorLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "")
        {
            if (_configHelper.GetValueAsBool("AppSettings:EnableDbLogging"))
            {
                try
                {
                    var source = _configHelper.GetValueAsString("AppSettings:ApplicationName");

                    var parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message.Length > 255 ? message.Substring(0, 254) : message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventId.ToString()));
                    parameters.Add(new NpgsqlParameter("@_eventname", eventName));

                    if (string.IsNullOrEmpty(source))
                    {
                        parameters.Add(new NpgsqlParameter("@_source", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_source", source));
                    }
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    var output = await _dbmanager.ExecuteScalarAsync("add_logging_processor", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    _logger.LogError(exception: ex, message: string.Concat("ProcessorManager.AddProcessorLogEvent(): ", ex.Message));
                }
                finally
                {

                }
            }
            return true;
        }
        #endregion

    }
}
