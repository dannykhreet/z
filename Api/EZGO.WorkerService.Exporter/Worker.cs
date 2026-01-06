using DocumentFormat.OpenXml.Math;
using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Exporting;
using EZGO.Api.Models.Export;
using EZGO.Api.Models.Users;
using EZGO.WorkerService.Exporter.Objects;
using EZGO.WorkerService.Exporter.Utils;
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


namespace EZGO.WorkerService.Exporter
{
    /// <summary>
    /// Worker; Export worker. 
    /// Export working is used for exporting data to other locations (external) this can be email, sftp, etc. 
    /// How the exporter is based on a configuration (located in the DB either with an holding or a company, with resource id 44 in the settings tables)
    /// The exporter will check 2 resource settings if certain companies are enabled/holdings are enabled for usage with the exporter and use the configuration to determine what needs to be done. 
    /// On start (and periodically) the exporter will buildup a schedule, every minute that schedule is checked if something needs to be done. 
    /// Schedules that need to be started will be used when more than one on a scheduled timeset will run in parallel (therefor make sure the code can handle this and functionalities are scoped if needed).
    /// 
    /// Example config: 
    ///   //TODO ADD EXAMPLE
    /// 
    /// Schedule is based on UTC. (no leading 0 in config due to interpret issues, schedule will be parsed correctly)
    /// 
    /// The schedule is used to determine what needs to be done, within the schedule a configuration (can be multiple) is available and the configuration determined what needs specifically to be done. 
    /// Within the export worker there are 3 parts
    /// - retrieval of data
    /// - converting of data
    /// - delivering of data
    /// 
    /// Retrieval is usually handled by methods that either return a dataset or datatable. 
    /// Converting of data is based on the format that needs to be converted
    /// Delivering of data can be done by uploading files, emailing and or related. 
    /// 
    /// One or two of those steps can be merged due to validations or other reasons. 
    /// 
    /// Retrieval parameters within the configuration are based on the following naming scheme:
    /// - RetrievalXXXXXX per example RetrievalScoreDataAtoss for retrieving score data for the ATOSS system.
    /// 
    /// Converting parameters within the configuration are based on the following naming scheme: 
    /// - OutputXXXXX per example: OutputCSV for generating an CSV file.
    /// 
    /// Delivering of data is based on the following naming scheme
    /// - DeliveryXXXXX per example: DeliverySFTP for delivering to an SFTP location.
    /// 
    /// Per part separate configuration can be added for determining all data and formatting that needs to be done. 
    /// These include naming certain items, location, references to variables etc. These are determined per configuration item and will wildly differ per type/part.
    /// 
    /// Every configuration runs in a separate scoped thread, multiple configurations can run at once. 
    /// 
    /// NOTE! currently SFTP is configured and enabled for delivery. Other methods are not yet enabled.
    /// </summary>
    public class Worker : BackgroundService
    {
        //services
        private readonly ILogger<Worker> _logger; //Logger, used for logging to ILogger (.net)
        private readonly IDatabaseAccessHelper _dataManager; //DataManager for retrieving data from DB
        private readonly IConfigurationHelper _configHelper; //ConfigurationHelper, retrieving data from config/env settings
        private readonly IServiceProvider _provider; //ServiceProvider, parent provider for creating scoped execution of managers.
        private readonly IAutomatedExportingManager _manager; //Main manager for retrieving data sets from db.
        private readonly IConnectionHelper _connectionHelper; ///ConnectionHelper; Used for determining connection strings.
        private readonly IDatabaseLogWriter _databaseLogWriter; //DatabaseLogWriter; for writing data to logging table EZGO. 

        //internal variables. 
        private bool _exportEnabled = false;
        private bool _debugToDbEnabled = false;
        private string _environment = "";

        private List<int> _validCompanyIds = new List<int>();
        private List<int> _validHoldingIds = new List<int>();
        //variables for certain functionalities (mailers, connectors etc)
        private Mailer _mailer = new Mailer();

        //variables for schedules. 
        public static ConcurrentDictionary<string, List<ExportScheduleSettings>> Schedule = new ConcurrentDictionary<string, List<ExportScheduleSettings>>();
        private readonly int[] _updateSchedule = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };

        //service provider for creating scopes. 
        public IServiceProvider _services { get; }


        /// <summary>
        /// Worker; Contructor + loading of config variables needed on startup. 
        /// </summary>
        public Worker(IServiceProvider services, ILogger<Worker> logger, IDatabaseAccessHelper dataManager, IConfigurationHelper configHelper, IConnectionHelper connectionHelper, IDatabaseLogWriter databaseLogWriter, ILogger<AutomatedExportingManager> loggerDb, IServiceProvider provider, IAutomatedExportingManager exportManager)
        {
            _logger = logger;
            _dataManager = dataManager;
            _configHelper = configHelper;
            _provider = provider;
            _manager = exportManager;
            _connectionHelper = connectionHelper;
            _services = services;
            _exportEnabled = _configHelper.GetValueAsBool("AppSettings:ExportEnabled");
            _debugToDbEnabled = _configHelper.GetValueAsBool("AppSettings:EnableDebugToDb");
            _environment = _configHelper.GetValueAsString("DOTNET_ENVIRONMENT");
            _databaseLogWriter = databaseLogWriter;

        }

        /// <summary>
        /// ExecuteAsync; Main task for running worker service.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Exporter {version} | {enabled} started for {environment} at {time}", GetType().Assembly.GetName().Version.ToString(), _exportEnabled.ToString().ToLower(), _environment, DateTimeOffset.Now);
            await _manager.AddExportLogEvent(message: string.Format("Exporter {0} | {1} started for {2} at {3}", GetType().Assembly.GetName().Version.ToString(), _exportEnabled.ToString().ToLower(), _environment, DateTimeOffset.Now), eventName:"STARTUP", description:"Exporter Started");

            Init();

            await CreateSchedule();

            await LogScheme();

            await CreateValidHoldingCompanyCollections();

            if (!_exportEnabled)
            {
                await _manager.AddExportLogEvent(message: "Exporter setting [ExportEnabled] is disabled.", description: "Exporter disabled.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(60000, stoppingToken); //run every minute
                var scheduleKey = DateTime.Now.ToUniversalTime().ToString("HHmm");
                //var scheduleKey = DateTime.Now.ToString("HHmm"); -> enable for local run and certain config not correct (seeing config is utc)
                foreach (var item in Schedule[scheduleKey])
                {
                    if (_exportEnabled) //export enabled through configuration. 
                    {
                        _ = Task.Run(() => RunGeneration(currentScheduleKey: scheduleKey, schedule: item, cancellationToken: stoppingToken));
                    } 

                }

                //run schedule updates
                var now = DateTime.Now;
                if (_updateSchedule.Contains(now.Hour) && (now.Minute == 13 || now.Minute == 43)) //check every half hour. 
                {
                    await CreateSchedule();

                    await LogScheme();

                    await CreateValidHoldingCompanyCollections();

                    if (!_exportEnabled)
                    {
                        await _manager.AddExportLogEvent(message: "Exporter setting [ExportEnabled] is disabled.", description: "Exporter disabled.");

                    }
                }
            }


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
                    Schedule.TryAdd(key, new List<ExportScheduleSettings>());
                }
            }
        }

        /// <summary>
        /// RunGeneration; Main generation call, used for starting and running data exports. This call needs to be a separate task to run properly. 
        /// </summary>
        /// <param name="currentScheduleKey">Key (time) of scheduled item</param>
        /// <param name="schedule">The schedule (what needs to be done)</param>
        /// <param name="cancellationToken">Token for cancelation when shutting down.</param>
        private async void RunGeneration(string currentScheduleKey, ExportScheduleSettings schedule, CancellationToken cancellationToken)
        {
            var internalGuid = Guid.NewGuid().ToString();

            
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using (var scope = _services.CreateScope())
            {
                var scopedManager = scope.ServiceProvider.GetRequiredService<IAutomatedExportingManager>();
                //check if holding is enabled
                if (schedule.HoldingId > 0 && !_validHoldingIds.Contains(schedule.HoldingId))
                {
                    await scopedManager.AddExportLogEvent(message: "Holding not in exporter settings.", description: string.Format("Run not started for ({0}).", schedule.HoldingId));

                    return;
                }
                //check if company is enabled
                if (schedule.CompanyId > 0 && !_validCompanyIds.Contains(schedule.CompanyId))
                {
                    await scopedManager.AddExportLogEvent(message: "Company not in exporter settings.", description: string.Format("Run not started for ({0}).", schedule.CompanyId));

                    return;
                }

                await scopedManager.AddExportLogEvent(message: string.Format("> Start Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: schedule.ToString());

                try
                {
                    if (CheckGenerationRunning(scheduleKey: currentScheduleKey, currentSchedule: schedule) == false)
                    {
                        UpdateGenerationStartSchedule(scheduleKey: currentScheduleKey, currentSchedule: schedule);

                        if (schedule.ExportSchedule != null)
                        {
                            var datasets = new List<DataSet>(); //every result is mapped to a dataset, so for example creating excells can be setup a bit easer. 

                            /* Data Retrieval */
                            if (schedule.RetrievalUserDataAtoss == true)
                            {
                                await scopedManager.AddExportLogEvent(message: string.Format("> UserDataAtoss Start Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));

                                DataSet dsUserData = new DataSet(schedule.OutputNameUserDataAtoss.ToUpper()); //create dataset as wrapper for the data. 
                                DataTable dtUserData = await scopedManager.GetAutomatedUserExportForAtoss(holdingId: schedule.HoldingId, companyId: schedule.CompanyId, validHoldingIds: _validHoldingIds, validCompanyIds: _validCompanyIds);
                                dtUserData.TableName = schedule.OutputNameUserDataAtoss.ToUpper();
                                dsUserData.Tables.Add(dtUserData);   //retrieve data
                                datasets.Add(dsUserData);

                                if (_debugToDbEnabled)
                                {
                                    await scopedManager.AddExportLogEvent(message: string.Format("> dsUserData({2} - {3}) {0} - {1}", DateTime.Now.ToString(), internalGuid, dsUserData.Tables.Count, dsUserData.Tables.Count > 0 ? dsUserData.Tables[0].Rows.Count : 0));
                                }
                             

                                await scopedManager.AddExportLogEvent(message: string.Format("> UserDataAtoss End Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));
                            }

                            if(schedule.RetrievalScoreDataAtoss == true)
                            {
                                await scopedManager.AddExportLogEvent(message: string.Format("> ScoreDataAtoss Start Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));

                                DataSet dsScoreData = new DataSet(schedule.OutputNameScoreDataAtoss.ToUpper()); //create dataset as wrapper for the data. 
                                DataTable dtScoreData = await scopedManager.GetAutomatedScoreDataExportForAtoss(holdingId: schedule.HoldingId, companyId: schedule.CompanyId, validHoldingIds: _validHoldingIds, validCompanyIds: _validCompanyIds);
                                dtScoreData.TableName = schedule.OutputNameScoreDataAtoss.ToUpper();
                                dsScoreData.Tables.Add(dtScoreData);   //retrieve data
                                datasets.Add(dsScoreData);

                                if (_debugToDbEnabled)
                                {
                                    await scopedManager.AddExportLogEvent(message: string.Format("> dsScoreData({2} - {3}) {0} - {1}", DateTime.Now.ToString(), internalGuid, dsScoreData.Tables.Count, dsScoreData.Tables.Count > 0 ? dsScoreData.Tables[0].Rows.Count : 0));
                                }

                                await scopedManager.AddExportLogEvent(message: string.Format("> ScoreDataAtoss End Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));
                            }

                            if (schedule.RetrievalMasterDataAtoss == true)
                            {
                                await scopedManager.AddExportLogEvent(message: string.Format("> MasterDataAtoss Start Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));

                                DataSet dsMasterData = new DataSet(schedule.OutputNameMasterDataAtoss.ToUpper()); //create dataset as wrapper for the data. 
                                DataTable dtMasterData = await scopedManager.GetAutomatedMasterDataExportForAtoss(holdingId: schedule.HoldingId, companyId: schedule.CompanyId, validHoldingIds: _validHoldingIds, validCompanyIds: _validCompanyIds);
                                dtMasterData.TableName = schedule.OutputNameMasterDataAtoss.ToUpper();
                                dsMasterData.Tables.Add(dtMasterData);   //retrieve data
                                datasets.Add(dsMasterData);

                                if (_debugToDbEnabled)
                                {
                                    await scopedManager.AddExportLogEvent(message: string.Format("> dsMasterData({2} - {3}) {0} - {1}", DateTime.Now.ToString(), internalGuid, dsMasterData.Tables.Count, dsMasterData.Tables.Count > 0 ? dsMasterData.Tables[0].Rows.Count : 0));
                                }

                                await scopedManager.AddExportLogEvent(message: string.Format("> MasterDataAtoss End Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));
                            }

                            /* Generation and Delivering */
                            if (schedule.DeliverySFTP == true)
                            {
                                if (schedule.OutputCSV == true) //create csv output and upload to sftp
                                {
                                    foreach (DataSet ds in datasets)
                                    {
                                        foreach (DataTable dt in ds.Tables) {
                                            //retrieve settings from env for locations and related
                                         
                                            SftpConnector sftpConnector = new SftpConnector(SFTPLocation: _configHelper.GetValueAsString(schedule.SettingSFTPLocation), SFTPUser: _configHelper.GetValueAsString(schedule.SettingSFTPUserName), SFTPPassword: _configHelper.GetValueAsString(schedule.SettingSFTPPassword));

                                            using (Stream ms = new MemoryStream()) //TODO see if this can be moved out to list of streams/dict of streams
                                            {

                                                await scopedManager.AddExportLogEvent(message: string.Format("> CsvWriter.WriteFromDataTable Start Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));
                                                //create csv
                                                await CsvWriter.WriteFromDataTable(source: dt, stream: ms, false, true);

                                                ms.Seek(0, SeekOrigin.Begin); //reset to beginning;

                                                await scopedManager.AddExportLogEvent(message: string.Format("> CsvWriter.WriteFromDataTable End Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));

                                                await scopedManager.AddExportLogEvent(message: string.Format("> SFTPConnector.SendFile Start Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));

                                                //filename based on table name.
                                                string fileName = string.Concat(dt.TableName, ".csv");

                                                //send data to sftp
                                                if (_debugToDbEnabled)
                                                {
                                                    //write to db
                                                    ms.Seek(0, SeekOrigin.Begin); //reset to beginning;
                                                    using (Stream copy = new MemoryStream())
                                                    {
                                                        ms.CopyTo(copy);
                                                        copy.Seek(0, SeekOrigin.Begin); //reset to beginning;
                                                        using StreamReader R = new StreamReader(copy);
                                                        var contentString = R.ReadToEnd();
                                                        await scopedManager.AddExportLogEvent(message: string.Format("> FILE DEBUG [{2}] {0} - {1}", DateTime.Now.ToString(), internalGuid, fileName), description: contentString);
                                                    }
                                                }

                                                if (sftpConnector.HasConfiguration())
                                                {
                                                    ms.Seek(0, SeekOrigin.Begin); //reset to beginning;
                                                    var outputSftp = sftpConnector.SendFile(file: ms, fileName.ToLower());
                                                    await scopedManager.AddExportLogEvent(message: string.Format("> SFTPConnector Output {0} - {1}", DateTime.Now.ToString(), internalGuid), description: outputSftp);
                                                }
                                                else
                                                {
                                                    await scopedManager.AddExportLogEvent(message: string.Format("> SFTPConnector No Configuration {0} - {1}", DateTime.Now.ToString(), internalGuid));
                                                }

                                                await scopedManager.AddExportLogEvent(message: string.Format("> SFTPConnector.SendFile End Data Retrieval {0} - {1}", DateTime.Now.ToString(), internalGuid));
                                            }

                                        }


                                    }

                                }

                                if(schedule.OutputXLS)
                                {
                                    // FOR CURRENT VERSION NOT IMPLEMENTED
                                }

                            }

                            if (schedule.DeliveryEmail) {
                                List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                                // FOR CURRENT VERSION NOT IMPLEMENTED
                            }

                            await LogDataSetStatistics(companyId: schedule.CompanyId, holdingId: schedule.HoldingId, dataSets: datasets, exportingManager: scopedManager);

                            /* Cleanup */
                            datasets.Clear();
                            datasets = null; //force null, for mem renewal. 
                        }

                        UpdateGenerationEndSchedule(scheduleKey: currentScheduleKey, currentSchedule: schedule);
                    }
                } catch(Exception ex)
                {
                    await scopedManager.AddExportLogEvent(message: string.Format("> ERROR Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: ex.Message);
                    _logger.LogInformation("Error occurred {0} - {1}", ex.Message, internalGuid);
                    _logger.LogError(exception: ex, message: ex.Message);
                }

                await scopedManager.AddExportLogEvent(message: string.Format("> End Scoped Holding:{0} | CompanyId:{1} - {2} - {3}", schedule.HoldingId, schedule.CompanyId, DateTime.Now.ToString(), internalGuid), description: schedule.ToString());
            }

        }


        #region - schedule creation and maintaining - 
        /// <summary>
        /// CreateSchedule(); create schedule, remove existing one to clear invalid data, based on flattend objects.
        /// </summary>
        /// <returns>true/false if successfull.</returns>
        private async Task<bool> CreateSchedule()
        {
            if (GenerationIsCurrentlyRunning() == false)
            {
                var possibleHoldingSchedules = await RetrieveHoldingSchedules();
                var possibleCompanySchedules = await RetrieveCompanySchedules();

                List<ExportSchedule> possibleSchedules = new List<ExportSchedule>();
                if (possibleHoldingSchedules != null && possibleHoldingSchedules.Count > 0) possibleSchedules.AddRange(possibleHoldingSchedules);
                if (possibleCompanySchedules != null && possibleCompanySchedules.Count > 0) possibleSchedules.AddRange(possibleCompanySchedules);

                RemoveSchedules(); //remove existing schedules before updating.

                var flattendSchedules = FlattenSchedules(possibleSchedules);
                foreach (var flattendSchedule in flattendSchedules)
                {
                    var currentRunTime = flattendSchedule.RunTime.ToString("0000");
                    if (Schedule.ContainsKey(currentRunTime))
                    {
                        if (Schedule[currentRunTime].Any() && Schedule[currentRunTime].Count > 0)
                        {
                            var possibleItem = Schedule[currentRunTime].Where(x => x.HoldingId == flattendSchedule.HoldingId && x.CompanyId == flattendSchedule.CompanyId).FirstOrDefault();
                            if (possibleItem != null && (possibleItem.HoldingId > 0 || possibleItem.CompanyId > 0))
                            {
                                possibleItem = flattendSchedule; //update existing with new
                            }
                            else
                            {
                                Schedule[currentRunTime].Add(flattendSchedule); //add new one
                            }
                        }
                        else
                        {
                            Schedule[currentRunTime].Add(flattendSchedule); //add new one
                        }
                    }
                    else
                    {
                        Schedule[currentRunTime].Add(flattendSchedule); //add new one
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// FlattenSchedules; Flatten all schedule objects for adding to schedule list
        /// </summary>
        /// <param name="schedules">Current export schedules.</param>
        /// <returns>List of ExportScheduleSettings (flattened objects, containing all relevant variables in root, specifics can still be retrieved from ExportSchedule fields if needed)</returns>
        private List<ExportScheduleSettings> FlattenSchedules(List<ExportSchedule> schedules)
        {
            var flattend = new List<ExportScheduleSettings>();

            foreach (var schedule in schedules)
            {
                if(schedule.IsActive == true)
                {
                    if (schedule.ScheduleItems != null)
                    {
                        foreach (var item in schedule.ScheduleItems)
                        {
                            flattend.Add(new ExportScheduleSettings()
                            {
                                CompanyId = schedule.CompanyId,
                                ExportSchedule = schedule,
                                HoldingId = schedule.HoldingId,
                                RunTime = CalculateRunTime(currentTime: item.ScheduleRunTime, schedule.TimeZone),
                                TimeZone = schedule.TimeZone,
                                TimeFrameInMinutes = item.TimeFrameInMinutes,
                                //BundleExports = schedule.BundleExports,
                                DayOfWeek = item.DayOfWeek,
                                DataRangeStartTime = item.StartTime,
                                DataRangeEndTime = item.EndTime,
                                DataRangeDirection = item.DateDirection,
                                /*What to use for delivery */
                                DeliveryEmail = schedule.DeliveryEmail,
                                DeliveryPost = schedule.DeliveryPost,
                                DeliverySFTP = schedule.DeliverySFTP,
                                /*What to use for output */
                                OutputCSV = schedule.OutputCSV,
                                OutputXLS = schedule.OutputXLSX,
                                OutputJSON = schedule.OutputJSON,
                                OutputSQL = schedule.OutputSQL,
                                /*What to use for data retrieval*/
                                RetrievalUserDataAtoss = schedule.RetrievalUserDataAtoss,
                                RetrievalScoreDataAtoss = schedule.RetrievalScoreDataAtoss,
                                RetrievalMasterDataAtoss = schedule.RetrievalMasterDataAtoss,
                                /*What settings to use for SFTP */
                                SettingSFTPLocation = schedule.SettingSFTPLocation,
                                SettingSFTPUserName = schedule.SettingSFTPUserName,
                                SettingSFTPPassword = schedule.SettingSFTPPassword,
                                /*Output filenames*/
                                OutputNameUserDataAtoss = schedule.OutputNameUserDataAtoss,
                                OutputNameScoreDataAtoss = schedule.OutputNameScoreDataAtoss,
                                OutputNameMasterDataAtoss = schedule.OutputNameMasterDataAtoss,
                                /*Other*/
                                IsActive = schedule.IsActive
                            });
                        }
                    }
                }
            }

            return flattend;
        }

        /// <summary>
        /// RetrieveHoldingSchedules; Retrieve schedule list based on holding configuration. (id 44 in settings table)
        /// </summary>
        /// <returns>List of export schedule items.</returns>
        private async Task<List<ExportSchedule>> RetrieveHoldingSchedules()
        {
            var output = new List<ExportSchedule>();

            await _manager.AddExportLogEvent(message: "Exporter RetrieveHoldingSchedules()");

            NpgsqlDataReader dr = null;
            NpgsqlConnection conn = null;
            try
            {
                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {

                    try
                    {
                        await conn.OpenAsync();

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_holding_setting_schedules"), conn))
                        {
                            cmd.CommandType = CommandType.Text;

                            dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                            while (await dr.ReadAsync())
                            {
                                try
                                {
                                    var exportSchedule = new ExportSchedule();
                                    if (dr["json_data"] != DBNull.Value && !string.IsNullOrEmpty(dr["json_data"].ToString()))
                                    {
                                        var json = dr["json_data"].ToString();
                                        exportSchedule = json.ToObjectFromJson<ExportSchedule>();
                                    }
                                    exportSchedule.HoldingId = Convert.ToInt32(dr["holding_id"]);

                                    output.Add(exportSchedule);
                                } catch (Exception ex)
                                {
                                    _logger.LogError(exception: ex, message: "Error occurred RetrieveHoldingSchedules()");
                                }
                                
                            }

                            await dr.CloseAsync(); dr = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred RetrieveHoldingSchedules()");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred RetrieveHoldingSchedules()");
            }
            finally
            {
                if (dr != null) await dr.CloseAsync();
                if (conn != null) await conn.CloseAsync();
            }

            return output;
        }

        /// <summary>
        /// RetrieveCompanySchedules; Retrieve schedule list based on company configuration.  (id 44 in settings table)
        /// </summary>
        /// <returns>List of export schedule items.</returns>
        private async Task<List<ExportSchedule>> RetrieveCompanySchedules()
        {
            var output = new List<ExportSchedule>();

            await _manager.AddExportLogEvent(message: "Exporter RetrieveCompanySchedules()");

            NpgsqlDataReader dr = null;
            NpgsqlConnection conn = null;
            try
            {
                using (conn = new NpgsqlConnection(_connectionHelper.GetConnectionStringReader()))
                {

                    try
                    {
                        await conn.OpenAsync();

                        using (NpgsqlCommand cmd = new NpgsqlCommand(DataConnectorHelper.WrapFunctionCommand("get_companies_setting_schedules"), conn))
                        {
                            cmd.CommandType = CommandType.Text;

                            dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                            while (await dr.ReadAsync())
                            {
                                try
                                {
                                    var exportSchedule = new ExportSchedule();
                                    if (dr["json_data"] != DBNull.Value && !string.IsNullOrEmpty(dr["json_data"].ToString()))
                                    {
                                        var json = dr["json_data"].ToString();
                                        exportSchedule = json.ToObjectFromJson<ExportSchedule>();
                                    }
                                    exportSchedule.CompanyId = Convert.ToInt32(dr["company_id"]);

                                    output.Add(exportSchedule);
                                } catch (Exception ex)
                                {
                                    _logger.LogError(exception: ex, message: "Error occurred RetrieveCompanySchedules()");
                                }
                               
                            }

                            await dr.CloseAsync(); dr = null;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(exception: ex, message: "Error occurred RetrieveCompanySchedules()");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred RetrieveCompanySchedules()");
            }
            finally
            {
                if (dr != null) await dr.CloseAsync();
                if (conn != null) await conn.CloseAsync();
            }

            return output;
        }

        /// <summary>
        /// RemoveSchedules; clear entire schedule.
        /// </summary>
        private void RemoveSchedules()
        {
            foreach (var scheduleSettingCollection in Schedule)
            {
                scheduleSettingCollection.Value.Clear();
            }
        }

        /// <summary>
        /// CalculateRunTime; Calculate runtime based on current date.
        /// Runtime will be mapped to number (e.g. time without leading 0, for easier checking)
        /// </summary>
        /// <param name="currentTime">Current time (also int, 24 our clock without leading 0)</param>
        /// <param name="timeZone">Current timezone. (from schedule)</param>
        /// <returns></returns>
        private int CalculateRunTime(int currentTime, string timeZone)
        {
            if(!string.IsNullOrEmpty(timeZone))
            {
                try
                {
                    DateTime currentDateTime = DateTime.Now.Date;
                    currentDateTime = currentDateTime.AddHours(Convert.ToInt32(currentTime.ToString("0000").Substring(0, 2)));
                    currentDateTime = currentDateTime.AddMinutes(Convert.ToInt32(currentTime.ToString("0000").Substring(2, 2)));

                    var dateTimeUnspec = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Unspecified);
                    var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspec, TimeZoneInfo.FindSystemTimeZoneById(timeZone));

                    return Convert.ToInt32(utcDateTime.ToString("HHmm"));
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    //ignore offset, timezone not correctly configured.
                }
            }
            return currentTime;
        }

        #endregion

        #region - retrieval / checks for validating for Generation RUN -
        /// <summary>
        /// UpdateGenerationStartSchedule; Update start date for certain key in schedule.
        /// NOTE! will update the Running and the Date of running.
        /// </summary>
        /// <param name="scheduleKey">Key where start date needs to be updated.</param>
        /// <param name="currentSchedule">Schedule, containing item to be updated.</param>
        private void UpdateGenerationStartSchedule(string scheduleKey, ExportScheduleSettings currentSchedule)
        {
            var s = Schedule[scheduleKey].Where(x => x.HoldingId == currentSchedule.HoldingId && x.CompanyId == currentSchedule.CompanyId).FirstOrDefault();
            if (s != null)
            {
                s.StartRunDateCurrentSession = DateTime.Now;
                s.Running = true;
            }
        }

        /// <summary>
        /// UpdateGenerationEndSchedule; Update start date for certain key in schedule.
        /// NOTE! will update the Running and the Date of running.
        /// </summary>
        /// <param name="scheduleKey">Key where start date needs to be updated.</param>
        /// <param name="currentSchedule">Schedule, containing item to be updated.</param>
        private void UpdateGenerationEndSchedule(string scheduleKey, ExportScheduleSettings currentSchedule)
        {
            var s = Schedule[scheduleKey].Where(x => x.HoldingId == currentSchedule.HoldingId && x.CompanyId == currentSchedule.CompanyId).FirstOrDefault();
            if (s != null)
            {
                s.LastRunDateCurrentSession = DateTime.Now;
                s.Running = false;
            }

        }

        /// <summary>
        /// CheckGenerationRunning; Check if a genration run is running
        /// </summary>
        /// <param name="scheduleKey">Key to check.</param>
        /// <param name="currentSchedule">Schedule, containing item to be updated.</param>
        /// <returns>true / false depending on outcome.</returns>
        private bool CheckGenerationRunning(string scheduleKey, ExportScheduleSettings currentSchedule)
        {
            var s = Schedule[scheduleKey].Where(x => x.HoldingId == currentSchedule.HoldingId && x.CompanyId == currentSchedule.CompanyId).FirstOrDefault();
            if (s != null)
            {
                return s.Running;
            }
            return false;
        }

        /// <summary>
        /// CreateValidHoldingCompanyCollections(); Get list of holdings/companies to be running.
        /// Will fill private variables used in worker.
        /// </summary>
        /// <returns>true/false depending on outcome</returns>
        private async Task<bool> CreateValidHoldingCompanyCollections()
        {
            await _manager.AddExportLogEvent(message: "Exporter CreateValidHoldingCompanyCollections()");
            _validCompanyIds = await GenerationCompanyIds();
            _validHoldingIds = await GenerationHoldingIds();

            return true;
        }

        /// <summary>
        /// GenerationIsCurrentlyRunning(); Check if current schedule is running. 
        /// </summary>
        /// <returns></returns>
        private bool GenerationIsCurrentlyRunning()
        {
            if (Schedule.AsQueryable().Where(x => x.Value.Where(y => y.Running == true).Any()).Any())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// GenerationCompanyIds; Get a list of company ids to start sync,
        /// </summary>
        /// <returns>A list of generation company ids</returns>
        private async Task<List<int>> GenerationCompanyIds()
        {
            var output = await GetResourceSettingIds(settingKey: "TECH_COMPANY_EXPORTER");
            return output;
        }

        /// <summary>
        /// GenerationHoldingIds; Get a list of holding ids to start sync,
        /// </summary>
        /// <returns>A list of generation holding ids</returns>
        private async Task<List<int>> GenerationHoldingIds()
        {
            var output = await GetResourceSettingIds(settingKey: "TECH_HOLDING_EXPORTER");
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
        #endregion

        #region - logging related -
        /// <summary>
        /// LogScheme, log the entire scheme, will be used on startup and or on scheme refresh. 
        /// Will log to the DB so it can be retrieved and checked if needed. 
        /// Should be enabled always
        /// </summary>
        /// <returns></returns>
        private async Task LogScheme()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in Schedule.OrderBy(x => x.Key))
            {

                foreach (var setting in item.Value)
                {
                    sb.AppendFormat("[{0}]<{1}>", item.Key, setting.ToString());
                    sb.AppendLine();
                }
            }

            await _manager.AddExportLogEvent(message: "Export active scheme", eventName: "HEALTHCHECK", description: sb.ToString());
        }

        /// <summary>
        /// LogDataSetStatistics; 
        /// </summary>
        /// <param name="companyId">CompanyId, id from company possibly used for in logging.</param>
        /// <param name="holdingId">HoldingId, id from holding possibly used for in logging.</param>
        /// <param name="dataSets">DataSet containing data to be logged. NOTE! depending on size this can be a large set.</param>
        /// <param name="exportingManager">Manager, containing functionalities (scoped) to be used.</param>
        /// <returns>N/A</returns>
        private async Task LogDataSetStatistics(int companyId, int holdingId, List<DataSet> dataSets, IAutomatedExportingManager exportingManager)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                if (dataSets != null)
                {

                    foreach (var ds in dataSets)
                    {

                        sb.AppendFormat("{0} : ", ds.DataSetName);

                        foreach (DataTable table in ds.Tables)
                        {
                            sb.AppendFormat("[{0}({1})]", table.TableName, table.Rows != null ? table.Rows.Count : 0);
                        }

                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("DataSet is empty");
                }
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }

            try
            {
                await exportingManager.AddExportLogEvent(message: string.Format("Export statistics holding:{0} | companyId:{1}", holdingId, companyId), eventName: "HEALTHCHECK", description: sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("EZExporter.LogDataSetStatistics(): ", ex.Message));
            }
        }

        /// <summary>
        /// WriteFileContentToLog; Send file content to log, NOTE! only use on certain parts of logic/with specific files. Can be large. 
        /// </summary>
        /// <param name="ms">MemoryStream containing file</param>
        /// <param name="scopedManager">Manager containing functionalities to be called for logging.</param>
        /// <param name="internalGuid">InternalGuid for the current run.</param>
        /// <param name="fileName">Filename of the file being logged.</param>
        /// <returns></returns>
        private async Task WriteFileContentToLog(MemoryStream ms, IAutomatedExportingManager scopedManager, string internalGuid, string fileName)
        {
            try
            {
                //write to db
                ms.Seek(0, SeekOrigin.Begin); //reset to beginning;
                using (Stream copy = new MemoryStream())
                {
                    ms.CopyTo(copy);
                    copy.Seek(0, SeekOrigin.Begin); //reset to beginning;
                    using StreamReader R = new StreamReader(copy);
                    var contentString = R.ReadToEnd();
                    await scopedManager.AddExportLogEvent(message: string.Format("> FILE DEBUG [{2}] {0} - {1}", DateTime.Now.ToString(), internalGuid, fileName), description: contentString);
                }
            } catch (Exception ex )
            {
                _logger.LogError(exception: ex, message: string.Concat("EZExporter.WriteFileContentToLog(): ", ex.Message));
            }
           
        }
        #endregion

    }


}


/* 
 Queries to be usd (without the hardcoded companeis).
 
 
SPS which will be used: 

 
--DROP FUNCTION export_data_atoss_users;
 
CREATE OR REPLACE FUNCTION public.export_data_atoss_users(_holdingid integer DEFAULT 0, _companyid integer DEFAULT 0)
 RETURNS TABLE(plant_id integer, user_id integer, fullname character varying,  employee_id character varying, companyid integer)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE 
BEGIN
	IF _holdingid = 0 AND _companyid = 0 THEN
		RETURN;
	END IF;
	IF _holdingid <> 0 AND NOT check_holding_company(_companyid, _holdingid) THEN
			RETURN;
	END IF;
	RETURN QUERY
		SELECT CS98.value::int AS plant_id, PU.id AS user_id, CONCAT(PU.last_name,', ',PU.first_name) AS fullname, pued.employee_id::varchar AS employee_id 
		FROM profiles_user pu 
		LEFT JOIN profiles_user_extended_details pued ON pued.user_id = pu.id 
		INNER JOIN companies_company cc ON CC.id = pu.company_id  AND cc.is_active = true
		INNER JOIN companies_setting CS98 ON CS98.company_id = CC.id AND CS98.resource_setting_id = 98
		WHERE pu.company_id = _companyid AND pu.is_active = TRUE --AND cc.name ILIKE 'Refresco%'
		ORDER BY cc.name;
END$function$
;
 
--DROP FUNCTION export_data_atoss_master;
 
CREATE OR REPLACE FUNCTION public.export_data_atoss_master(_holdingid integer DEFAULT 0, _companyid integer DEFAULT 0)
 RETURNS TABLE(user_skill_id integer, name character varying, description character varying)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE 
BEGIN
	IF _holdingid = 0 AND _companyid = 0 THEN
		RETURN;
	END IF;
	IF _holdingid <> 0 AND NOT check_holding_company(_companyid, _holdingid) THEN
		RETURN;
	END IF;
	RETURN QUERY
		SELECT id AS user_skill_id, CONCAT (company_short,' - ',name)::varchar AS name, CONCAT (company_short,' - ',description)::varchar AS description FROM (
			SELECT us.id, us.name, us.description, 
			CS95.Value::varchar AS company_short		
			FROM user_skills us
			INNER JOIN companies_company cc ON CC.id = us.company_id AND cc.is_active = true
			INNER JOIN companies_setting CS95 ON CS95.company_id = cc.id AND CS95.resource_setting_id = 95
			WHERE us.company_id = _companyid
			AND us.is_active  = TRUE AND us.id NOT IN (SELECT id FROM user_skills WHERE (name = '' OR name IS NULL) AND (assessment_template_id IS NULL))
			ORDER BY us.company_id, us.id
		)
		AS T;
END$function$
;
 
 
--DROP FUNCTION export_data_atoss_scores;
 
CREATE OR REPLACE FUNCTION public.export_data_atoss_scores(_holdingid integer DEFAULT 0, _companyid integer DEFAULT 0)
 RETURNS TABLE(employee_id character varying,user_skill_id integer, valid_from date, valid_to date, degree int)
 LANGUAGE plpgsql
 STABLE
AS $function$
DECLARE 
BEGIN
	IF _holdingid = 0 AND _companyid = 0 THEN
		RETURN;
	END IF;
	IF _holdingid <> 0 AND NOT check_holding_company(_companyid, _holdingid) THEN
		RETURN;
	END IF;
	RETURN QUERY
		WITH scores AS (
		    SELECT assessment_id, totalscore, resultscore, completed_for_id FROM get_matrix_assessment_scores(_companyid, 0) 
			WHERE totalscore <> 0
		)
		SELECT T.employee_id, T.user_skill_id, T.valid_from, T.valid_to, (asco.resultscore * 20)::integer AS "degree" FROM (
			SELECT pued.employee_id::varchar AS employee_id, us.id AS user_skill_id, us.valid_from::date AS valid_from, us.valid_to::date AS valid_to, pu.company_id, a.id AS assessment_id, pu.id AS user_id 
			FROM profiles_user pu 
			INNER JOIN profiles_user_extended_details pued ON pued.user_id = pu.id 
			INNER JOIN companies_company cc ON CC.id = pu.company_id  AND cc.is_active = true
			INNER JOIN assessments a ON a.completed_for_id = PU.id AND a.is_active = TRUE AND a.is_completed = true
			INNER JOIN user_skills us ON us.assessment_template_id = a.assessment_template_id AND us.company_id = _companyid AND us.is_active = true
			WHERE pu.company_id = _companyid AND pu.is_active = TRUE
			ORDER BY pu.company_id, a.completed_at DESC
		) AS T
		INNER JOIN scores asco ON asco.assessment_id = T.assessment_id AND asco.completed_for_id = T.user_id
		ORDER BY T.employee_id, T.user_skill_id;
END$function$
;
  
 
 
 */