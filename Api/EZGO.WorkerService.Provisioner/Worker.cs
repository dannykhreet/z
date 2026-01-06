using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Math;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Provisioner;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Users;
using EZGO.WorkerService.Provisioner.Objects;
using EZGO.WorkerService.Provisioner.Utils;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EZGO.WorkerService.Provisioner
{
    /// <summary>

    /// </summary>
    public class Worker : BackgroundService
    {
        //services
        private readonly ILogger<Worker> _logger; //Logger, used for logging to ILogger (.net)
        private readonly IDatabaseAccessHelper _dataManager; //DataManager for retrieving data from DB
        private readonly IConfigurationHelper _configHelper; //ConfigurationHelper, retrieving data from config/env settings
        private readonly IServiceProvider _provider; //ServiceProvider, parent provider for creating scoped execution of managers.
        private readonly IConnectionHelper _connectionHelper; ///ConnectionHelper; Used for determining connection strings.
        private readonly IDatabaseLogWriter _databaseLogWriter; //DatabaseLogWriter; for writing data to logging table EZGO. 
        private readonly IProvisionerManager _manager;

        //internal variables. 
        private bool _provisionerEnabled = false;
        private bool _debugToDbEnabled = false;
        private string _environment = "";
        private bool _demoModeOnly { get { return _configHelper.GetValueAsBool("AppSettings:DemoModeOnly"); } }

        private List<int> _validCompanyIds = new List<int>();
        private List<int> _validHoldingIds = new List<int>();
        //variables for certain functionalities (mailers, connectors etc)
        private Mailer _mailer = new Mailer();

        //variables for schedules. 
        public static ConcurrentDictionary<string, List<ProvisionerScheduleSettings>> Schedule = new ConcurrentDictionary<string, List<ProvisionerScheduleSettings>>();
        private readonly int[] _updateSchedule = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };

        //service provider for creating scopes. 
        public IServiceProvider _services { get; }


        /// <summary>
        /// Worker; Contructor + loading of config variables needed on startup. 
        /// </summary>
        public Worker(IServiceProvider services, ILogger<Worker> logger, IProvisionerManager provisionManager, IDatabaseAccessHelper dataManager, IConfigurationHelper configHelper, IConnectionHelper connectionHelper, IDatabaseLogWriter databaseLogWriter)
        {
            _logger = logger;
            _dataManager = dataManager;
            _configHelper = configHelper;
            _connectionHelper = connectionHelper;
            _services = services;
            _provisionerEnabled = _configHelper.GetValueAsBool("AppSettings:ProvisionerEnabled");
            _debugToDbEnabled = _configHelper.GetValueAsBool("AppSettings:EnableDebugToDb");
            _environment = _configHelper.GetValueAsString("DOTNET_ENVIRONMENT");
            _databaseLogWriter = databaseLogWriter;
            _manager = provisionManager;

        }

        /// <summary>
        /// ExecuteAsync; Main task for running worker service.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Provisioner {version} | {enabled} started for {environment} at {time}", GetType().Assembly.GetName().Version.ToString(), _provisionerEnabled.ToString().ToLower(), _environment, DateTimeOffset.Now);
            await _manager.AddProvisionerLogEvent(message: string.Format("Provisioner {0} | {1} started for {2} at {3} in demo mode {4}", GetType().Assembly.GetName().Version.ToString(), _provisionerEnabled.ToString().ToLower(), _environment, DateTimeOffset.Now, _demoModeOnly), eventName:"STARTUP", description:"Provisioner Started");

            Init();

            await CreateSchedule();

            await CreateValidHoldingCompanyCollections();

            if (!_provisionerEnabled)
            {
                await _manager.AddProvisionerLogEvent(message: "Provisioner setting [ProvisionerEnabled] is disabled.", description: "Provisioner disabled.");
            }

            //for running on start-up, force run
            if(_provisionerEnabled)
            {
                var scheduleKeyStart = DateTime.Now.ToUniversalTime().ToString("HHmm");
                if (_environment.ToLower() == "production")
                {
                    _ = Task.Run(() => RunProvisioning(currentScheduleKey: scheduleKeyStart, schedule: new ProvisionerScheduleSettings() { CompanyId = 0, HoldingId = 34, RetrievalSFTP = true, SettingSFTPLocation = "SFTP_REF_LOCATION", SettingSFTPPassword = "SFTP_REF_PASSWORD", SettingSFTPUserName = "SFTP_REF_USERNAME", ScheduleType = "atoss" }, cancellationToken: stoppingToken));
                }
                else
                {
                    _ = Task.Run(() => RunProvisioning(currentScheduleKey: scheduleKeyStart, schedule: new ProvisionerScheduleSettings() { CompanyId = 0, HoldingId = 1, RetrievalSFTP = false, SettingSFTPLocation = "SFTP_REF_LOCATION", SettingSFTPPassword = "SFTP_REF_PASSWORD", SettingSFTPUserName = "SFTP_REF_USERNAME", ScheduleType = "atoss" }, cancellationToken: stoppingToken));

                }
            }

            while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(60000, stoppingToken); //run every minute
                    //var scheduleKey = DateTime.Now.ToUniversalTime().ToString("HHmm");
                    var scheduleKey = DateTime.Now.ToString("HHmm");// -> enable for local run and certain config not correct (seeing config is utc)

                    foreach (var item in Schedule[scheduleKey])
                    {
                        if (_provisionerEnabled) //export enabled through configuration. 
                        {
                            _ = Task.Run(() => RunProvisioning(currentScheduleKey: scheduleKey, schedule: item, cancellationToken: stoppingToken));
                        } else
                        {
                            await _manager.AddProvisionerLogEvent(message: "Provisioner setting [ProvisionerEnabled] is disabled.", description: "Provisioner disabled. Scheduled run not completed.");
                        }

                    }

                    //run schedule updates
                    var now = DateTime.Now;
                    if (_updateSchedule.Contains(now.Hour) && (now.Minute == 23 || now.Minute == 53)) //check every half hour. 
                    {
                        await CreateSchedule();

                        await CreateValidHoldingCompanyCollections();

                        if (!_provisionerEnabled)
                        {
                            await _manager.AddProvisionerLogEvent(message: "Provisioner setting [ProvisionerEnabled] is disabled.", description: "Provisioner disabled.");

                        }
                    }
                }

            await Task.CompletedTask;

        }

        /// <summary>
        /// Init(); Pre-fill schedule dictionary with fields.
        /// </summary>
        private void Init()
        {
            for (var h = 0; h < 24; h++)
            {
                var key = "";
                for (var m = 0; m < 60; m++)
                {
                    key = string.Concat(h.ToString("00"), m.ToString("00"));
                    Schedule.TryAdd(key, new List<ProvisionerScheduleSettings>());
                }
            }
        }

        private async Task<bool> CreateSchedule()
        {
            //Will be dynamically be filled in next release. For MVP this will be forcably be filled based on config.
            var scheduleSettings = new List<ProvisionerScheduleSettings>();
            //Scheduling for specific customer, will be made dynamic later on. 
           
            if(_environment.ToLower() =="production")
            {
                scheduleSettings.Add(new ProvisionerScheduleSettings() { CompanyId = 0, HoldingId = 34, RetrievalSFTP = true, SettingSFTPLocation = "SFTP_REF_LOCATION", SettingSFTPPassword = "SFTP_REF_PASSWORD", SettingSFTPUserName = "SFTP_REF_USERNAME", ScheduleType = "atoss" });

            } else if (_environment.ToLower() == "localdevelopment")
            {
                scheduleSettings.Add(new ProvisionerScheduleSettings() { CompanyId = 0, HoldingId = 1, RetrievalSFTP = true, SettingSFTPLocation = "SFTP_REF_LOCATION", SettingSFTPPassword = "SFTP_REF_PASSWORD", SettingSFTPUserName = "SFTP_REF_USERNAME", ScheduleType = "atoss" });
            }
            else
            {
                scheduleSettings.Add(new ProvisionerScheduleSettings() { CompanyId = 0, HoldingId = 1, RetrievalSFTP = false, SettingSFTPLocation = "SFTP_REF_LOCATION", SettingSFTPPassword = "SFTP_REF_PASSWORD", SettingSFTPUserName = "SFTP_REF_USERNAME", ScheduleType = "atoss" });
            }
            //dummy scheduling, will be replaced with perm. scheduling and later based on dynamic config. 
            Schedule["0900"] = scheduleSettings;

            return true;
        }

        /// <summary>
        /// CreateValidHoldingCompanyCollections(); Get list of holdings/companies to be running.
        /// Will fill private variables used in worker.
        /// </summary>
        /// <returns>true/false depending on outcome</returns>
        private async Task<bool> CreateValidHoldingCompanyCollections()
        {
            await _manager.AddProvisionerLogEvent(message: "Provisioner CreateValidHoldingCompanyCollections()");
            _validCompanyIds = await GenerationCompanyIds();
            _validHoldingIds = await GenerationHoldingIds();

            return true;
        }

        /// <summary>
        /// GenerationCompanyIds; Get a list of company ids to start sync,
        /// </summary>
        /// <returns>A list of generation company ids</returns>
        private async Task<List<int>> GenerationCompanyIds()
        {
            var output = await GetResourceSettingIds(settingKey: "TECH_COMPANY_USER_PROVISIONING");
            return output;
        }

        /// <summary>
        /// GenerationHoldingIds; Get a list of holding ids to start sync,
        /// </summary>
        /// <returns>A list of generation holding ids</returns>
        private async Task<List<int>> GenerationHoldingIds()
        {
            var output = await GetResourceSettingIds(settingKey: "TECH_HOLDING_USER_PROVISIONING");
            return output;
        }


        /// <summary>
        /// GetResourceSettingIds; Get resource ids for specific setting (can be any id contains in setting, used for holding, company id
        /// </summary>
        /// <param name="settingKey">Key of sertting to be retrieved</param>
        /// <returns>List of numbers represeting ids</returns>
        private async Task<List<int>> GetResourceSettingIds(string settingKey)
        {
            var output = new List<int>();

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", settingKey));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dataManager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var ids = dr["settingvalue"].ToString();
                            if (ids != null)
                            {
                                output = ids.Split(",").Where(x => int.TryParse(x, out int num)).Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("EZExporter.GetResourceSettingIds(): ", ex.Message));

            }

            return output;
        }

        private async void RunProvisioning(string currentScheduleKey, ProvisionerScheduleSettings schedule, CancellationToken cancellationToken)
        {
            var internalGuid = Guid.NewGuid().ToString();


            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using (var scope = _services.CreateScope())
            {
                //create scoped manager for paralellism if needed.
                var scopedManager = scope.ServiceProvider.GetRequiredService<IProvisionerManager>();

                //check if holding is enabled
                if (schedule.HoldingId > 0 && !_validHoldingIds.Contains(schedule.HoldingId))
                {
                    await scopedManager.AddProvisionerLogEvent(message: "Holding not in provisioner settings.", description: string.Format("Run not started for ({0}).", schedule.HoldingId));

                    return;
                }
                //check if company is enabled
                if (schedule.CompanyId > 0 && !_validCompanyIds.Contains(schedule.CompanyId))
                {
                    await scopedManager.AddProvisionerLogEvent(message: "Company not in provisioner settings.", description: string.Format("Run not started for ({0}).", schedule.CompanyId));

                    return;
                }

                await scopedManager.AddProvisionerLogEvent(message: string.Format("> Start Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: schedule.ToString());

                try
                {
                    string retrievedContent = ""; //replace with sftp retrieval

                    if (schedule.RetrievalSFTP)
                    {
                        SftpConnector sftpConnector = new SftpConnector(SFTPLocation: _configHelper.GetValueAsString(schedule.SettingSFTPLocation), SFTPUser: _configHelper.GetValueAsString(schedule.SettingSFTPUserName), SFTPPassword: _configHelper.GetValueAsString(schedule.SettingSFTPPassword));

                        string fileName = (schedule.ScheduleType == "atoss" ? "MASTERDATA" : ""); //send masterdata

                        var outputSftp = sftpConnector.GetFileDynamic(fileName, out retrievedContent);
                        if(outputSftp)
                        {
                            await scopedManager.AddProvisionerLogEvent(message: string.Format("> Output SFTP Content Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: string.Format("Output content lenght: {0}", retrievedContent.Count().ToString()));

                        }
                        else
                        {
                            if(retrievedContent.StartsWith("Error"))
                            {
                                await scopedManager.AddProvisionerLogEvent(message: string.Format("> ERROR Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: retrievedContent);

                            } else
                            {
                                await scopedManager.AddProvisionerLogEvent(message: string.Format("> ERROR Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: "Error occurred, possible issue with FTP connection.");

                            }
                            retrievedContent = string.Empty; //clear data, to disable further processing so no resources are needed. 

                        }
                    }

                    //Enable for local tests
                    //if (_environment.ToLower() != "production") //only on test
                    //{
                    //    //overwrite content with test data for testing purposes (will be replaced later on)
                    //    retrievedContent = "Personeelsnummer;Voornaam;Achternaam;Datum indiensttreding;leave_date;Company_ID;E-mailadres;Inactief\n2017;;;02-01-2023;;33;;nee\nTESTDAG4;Dagdienst PT no TVT;31000441;01-01-2024;;31;;nee\n2016;Randstad;Aanvraag;02-01-2023;;33;;nee\n2013;EL;ACCOUNT TESTEN;02-01-2023;;33;;nee\n1002;Lotte;ACCOUNT TESTEN;01-01-2023;;32;;nee\n1000;Marieke;ACCOUNT TESTEN;02-01-2023;;32;;ja";
                    //    schedule.ScheduleType = "atoss";
                    //}
                    //retrievedContent = string.Empty;

                    if (!string.IsNullOrEmpty(retrievedContent)) {
                        if(schedule.HoldingId > 0)
                        {
                            await scopedManager.ProvisionByHolding(holdingId: schedule.HoldingId, schedule.ScheduleType, retrievedContent);
                        } else
                        {
                            await scopedManager.ProvisionByCompany(companyId: schedule.CompanyId, schedule.ScheduleType, retrievedContent);
                        }
                    
                    } else
                    {
                        await scopedManager.AddProvisionerLogEvent(message: string.Format("> Retrieved Content Empty :{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: string.Empty);

                    }


                } catch (Exception ex)
                {
                    await scopedManager.AddProvisionerLogEvent(message: string.Format("> ERROR Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: ex.Message);
                    _logger.LogInformation("Error occurred {0} - {1}", ex.Message, internalGuid);
                    _logger.LogError(exception: ex, message: ex.Message);

                }


                await scopedManager.AddProvisionerLogEvent(message: string.Format("> End Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: schedule.ToString());

            }
        }

    }  


}
