using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.SapPmConnector;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;

namespace EZGO.WorkerService.SapPM
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfigurationHelper _configHelper;
        private readonly IDatabaseLogWriter _databaseLogWriter;
        public IServiceProvider _services { get; }

        private bool _connectorEnabled = false;
        private bool _demoModeOnly { get { return _configHelper.GetValueAsBool("AppSettings:DemoModeOnly"); } }

        private string _environment = "development";

        public Worker(IServiceProvider services, ILogger<Worker> logger, IConfigurationHelper configurationHelper, IDatabaseLogWriter databaseLogWriter)
        {
            _logger = logger;
            _configHelper = configurationHelper;
            _services = services;
            _connectorEnabled = _configHelper.GetValueAsBool("AppSettings:ConnectorEnabled");
            _environment = _configHelper.GetValueAsString("DOTNET_ENVIRONMENT");
            _databaseLogWriter = databaseLogWriter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sap PM Worker {version} | Started for {environment} at {time}", GetType().Assembly.GetName().Version.ToString(), _environment, DateTimeOffset.Now);


            if (!_connectorEnabled)
            {
                _logger.LogInformation("Sap PM Worker [ConnectorEnabled] is disabled.");

            }

            if (_connectorEnabled)
            {
                await generateMissingSapUsernames();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(60000, stoppingToken); //run every minute
                    _logger.LogInformation("Sap PM Worker | Worker running at: {time}", DateTimeOffset.Now);

                    if (DateTime.Now.Minute == 0 && (DateTime.Now.Hour % 4) == 0) // run every 4 hours at the top of the hour.
                    {
                        await RunSapPmLocationsProcessing(stoppingToken);
                    }

                    await RunSapPMNotificationsProcessing(stoppingToken);

                }
            }
          
        }

        private async Task generateMissingSapUsernames()
        {
            _logger.LogInformation("Sap PM Worker | SAP username generation started at: {time}", DateTimeOffset.Now);
            try
            {
                using (var scope = _services.CreateScope())
                {
                    // Resolve services from the scope
                    var dbManager = scope.ServiceProvider.GetRequiredService<IDatabaseAccessHelper>();
                    // run query to generate missing SAP usernames
                    await dbManager.ExecuteScalarAsync(procedureNameOrQuery: "UPDATE profiles_user p SET sap_pm_username = replace(upper(concat(left(first_name, 3), left(last_name, 9))), ' ', '') FROM resource_settings r WHERE r.id = 20 AND p.company_id = ANY(string_to_array(r.settingvalue, ',')::int[]) AND p.sap_pm_username IS NULL AND p.is_active", commandType: System.Data.CommandType.Text);                    
                    
                    _logger.LogInformation("Sap PM Worker | SAP username generation finished at: {time}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sap PM Worker | Error during SAP username generation at: {time}", DateTimeOffset.Now);
            }

        }

        private async Task RunSapPMNotificationsProcessing(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sap PM Worker | Notification processing started at: {time}", DateTimeOffset.Now);
            try
            {
                using (var scope = _services.CreateScope())
                {
                    // Resolve services from the scope
                    var sapPmConnectionManager = scope.ServiceProvider.GetRequiredService<ISapPmConnectionManager>();
                    // Call the method to process notifications
                    await sapPmConnectionManager.SendNotificationMessagesToSapPM();
                    _logger.LogInformation("Sap PM Worker | Notification processing completed at: {time}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sap PM Worker | Error during notification processing at: {time}", DateTimeOffset.Now);
            }
        }

        private async Task RunSapPmLocationsProcessing(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sap PM Worker | Location processing started at: {time}", DateTimeOffset.Now);
            try
            {
                using (var scope = _services.CreateScope())
                {
                    // Resolve services from the scope
                    var sapPmConnectionManager = scope.ServiceProvider.GetRequiredService<ISapPmConnectionManager>();
                    // Call the method to synchronize functional locations
                    await sapPmConnectionManager.SynchFunctionalLocations();
                    _logger.LogInformation("Sap PM Worker | Location processing completed at: {time}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sap PM Worker | Error during Location processing at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
