using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.TaskGeneration;
using EZGO.Api.Settings;
using EZGO.Api.TaskGeneration.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//TODO add logging.
namespace EZGO.Api.Logic.Generation
{
    /// <summary>
    /// TaskGenerationManager; TaskGeneration manager for generation of tasks within the database.
    /// The generation process will normally be company based but also 'All' generation can be called.
    /// This will loop through all companies or in case of a per company 'all' generation functionality all different types of tasks will be generated.
    /// Logging EventNumbers:
    /// 901: Start Generation Call
    /// 902: End Generation Call
    /// 911: Start Generation For Company
    /// 912: End Generation For Company
    /// 921: Start Generation All
    /// 922: End Generation All
    /// 931: Specific
    /// 961: Issue with a tasks within Generation All
    /// 962: Issue with a task with OneTimeOnly
    /// 963: Issue with a task with Weekly
    /// 964: Issue with a task with Monthly
    /// 965: Issue with a task with Shifts
    /// 999: Exception
    ///
    /// NOTE! this class is used by the API and the TaskGeneration Worker Service. Due to specific technical (built-in) limitation only.
    /// functionality from the following libraries may be used:
    /// - API.Data
    /// - API.Interface
    /// - API.Models
    /// - API.Settings
    /// If specific other logic is needed from the other projects check if it is compatible with the service worker (by running it).
    /// If an other library is not compatible duplicate the logic or replicate it's functionality specifically for service workers.
    /// Logic and or libraries that make use of:
    /// - Specific web-based functionality.
    /// - Items, objects or logic that uses the AspNetCore APP SDK.
    /// </summary>
    public class TaskGenerationManager : BaseManager<TaskGenerationManager>, ITaskGenerationManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _dataManager;
        private readonly IConfigurationHelper _configHelper;
        #endregion

        #region - constructor(s) -
        public TaskGenerationManager(IDatabaseAccessHelper dataManager, IConfigurationHelper configurationHelper,  ILogger<TaskGenerationManager> logger) : base(logger)
        {
            _dataManager = dataManager;
            _configHelper = configurationHelper;
        }
        #endregion

        /// <summary>
        /// GenerateOneTimeOnlyCompany; Generate all 'one time only' tasks with a company.
        /// </summary>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <returns>boolean, true/false depending on success.</returns>
        public async Task<bool> GenerateOneTimeOnlyCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_onetimeonly_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_onetimeonly_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        /// <summary>
        /// GenerateWeeklyCompany; Generate all 'weekly' tasks with a company.
        /// </summary>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <returns>boolean, true/false depending on success.</returns>
        public async Task<bool> GenerateWeeklyCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_weekly_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_weekly_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        /// <summary>
        /// GenerateMonthlyCompany; Generate all 'monthly' tasks with a company.
        /// </summary>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <returns>boolean, true/false depending on success.</returns>
        public async Task<bool> GenerateMonthlyCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_monthly_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_monthly_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        /// <summary>
        /// GenerateShiftsCompany; Generate all 'shift' tasks with a company.
        /// </summary>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <returns>boolean, true/false depending on success.</returns>
        public async Task<bool> GenerateShiftsCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_shifts_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_shifts_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        public async Task<bool> GeneratePeriodDayCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_periodday_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_perdiodday_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        public async Task<bool> GeneratePeriodHourCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_periodhour_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_periodhour_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        public async Task<bool> GeneratePeriodMinuteCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_periodminute_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_periodminute_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        public async Task<bool> GenerateDynamicDayCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamicday_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_dynamicday_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        public async Task<bool> GenerateDynamicHourCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamichour_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_dynamichour_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }

        public async Task<bool> GenerateDynamicMinuteCompany(int companyId, int? templateId = null)
        {
            var succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamicminute_by_company", companyId: companyId);
            if (!succes)
            {
                succes = false; //set to false, something in company is not correctly processed.
                await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_tasks_dynamicminute_by_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            return succes;
        }


        /// <summary>
        /// GenerateSpecificTemplate; Generate specific template
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="templateId"></param>
        /// <param name="recurrencyType"></param>
        /// <returns></returns>
        public async Task<bool> GenerateSpecificTemplate(int companyId, int templateId, RecurrencyTypeEnum recurrencyType)
        {
            bool succes = false;

            if(_configHelper.GetValueAsBool("AppSettings:EnablePreGenerationCleanupTasks"))
            {
                //cleanup if needed for template specific generation.
                await PreGenerationCleanup(companyId: companyId, templateId: templateId);
            }
           
            switch (recurrencyType)
            {
                case RecurrencyTypeEnum.NoRecurrency: 
                    succes = await GenerateCall(storedProcedureName: "generate_tasks_onetimeonly_by_company", companyId: companyId, templateId: templateId);
                    break;
                case RecurrencyTypeEnum.Shifts:
                    succes = await GenerateCall(storedProcedureName: "generate_tasks_shifts_by_company", companyId: companyId, templateId: templateId);
                    break;
                case RecurrencyTypeEnum.Week:
                    succes = await GenerateCall(storedProcedureName: "generate_tasks_weekly_by_company", companyId: companyId, templateId: templateId);
                    break;
                case RecurrencyTypeEnum.Month:
                    succes = await GenerateCall(storedProcedureName: "generate_tasks_monthly_by_company", companyId: companyId, templateId: templateId);
                    break;
                case RecurrencyTypeEnum.PeriodDay:
                    succes = await GenerateCall(storedProcedureName: "generate_tasks_periodday_by_company", companyId: companyId, templateId: templateId);
                    break;
                case RecurrencyTypeEnum.DynamicDay:
                    succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamicday_by_company", companyId: companyId, templateId: templateId);
                    break;
                    //case RecurrencyTypeEnum.PeriodHour:
                    //    succes = await GenerateCall(storedProcedureName: "generate_tasks_periodhour_by_company", companyId: companyId, templateId: templateId);
                    //    break;
                    //case RecurrencyTypeEnum.PeriodMinute:
                    //    succes = await GenerateCall(storedProcedureName: "generate_tasks_periodminute_by_company", companyId: companyId, templateId: templateId);
                    //    break;
            }

            return succes;
        }

        /// <summary>
        /// GenerateSpecificTemplate; Generate specific template based on task
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public async Task<bool> GenerateSpecificTemplateBasedOnTask(int companyId, int taskId)
        {
            bool succes = false;
            string recurrencyType = await GetTemplateRecurrencyTypeBasedOnTask(companyId: companyId, taskId: taskId); //Force Dynamic Day seeing for test

            if (!string.IsNullOrEmpty(recurrencyType))
            {

                if (Enum.TryParse(value: recurrencyType, ignoreCase: true, out RecurrencyTypeEnum possibleRecurrencyType))
                {
                    switch (possibleRecurrencyType)
                    {
                        case RecurrencyTypeEnum.DynamicDay:
                            succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamicday_by_company", companyId: companyId, taskId: taskId);
                            break;
                            //case RecurrencyTypeEnum.DynamicHour:
                            //    succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamichour_by_company", companyId: companyId, taskId: taskId);
                            //    break;
                            //case RecurrencyTypeEnum.DynamicMinute:
                            //    succes = await GenerateCall(storedProcedureName: "generate_tasks_dynamicminute_by_company", companyId: companyId, taskId: taskId);
                            //    break;
                    }
                }
               
            }

            return succes;
        }

        /// <summary>
        /// GenerateAllCompany; Generate all tasks for a company. In sequence the OneTimeOnly, Weekly, Monthly and Shift tasks will be generated.
        /// </summary>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <returns>boolean, true/false depending on success.</returns>
        public async Task<bool> GenerateAllCompany(int companyId, CancellationToken stoppingToken)
        {
            var succesOneTimeOnly = false;
            var succesWeekly = false;
            var succesMonthly = false;
            var succesShifts = false;
            var succesPeriodDay = false;
            var succesDynamicDay = false;
            //var succesPeriodHour = false;
            //var succesPeriodMinute = false;
            await AddGenerationLogEvent(eventId: 911, message: string.Format("START GENERATION [generate_all_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            if (!stoppingToken.IsCancellationRequested) succesOneTimeOnly = await GenerateOneTimeOnlyCompany(companyId: companyId);
            if (!stoppingToken.IsCancellationRequested) succesWeekly = await GenerateWeeklyCompany(companyId: companyId);
            if (!stoppingToken.IsCancellationRequested) succesMonthly = await GenerateMonthlyCompany(companyId: companyId);
            if (!stoppingToken.IsCancellationRequested) succesShifts = await GenerateShiftsCompany(companyId: companyId);
            if (!stoppingToken.IsCancellationRequested) succesPeriodDay = await GeneratePeriodDayCompany(companyId: companyId);
            if (!stoppingToken.IsCancellationRequested) succesDynamicDay = await GenerateDynamicDayCompany(companyId: companyId);
            //if (!stoppingToken.IsCancellationRequested) succesPeriodHour = await GeneratePeriodHourCompany(companyId: companyId);
            //if (!stoppingToken.IsCancellationRequested) succesPeriodMinute = await GeneratePeriodMinuteCompany(companyId: companyId);
            await AddGenerationLogEvent(eventId: 912, message: string.Format("END GENERATION [generate_all_company] FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            if(succesOneTimeOnly && succesWeekly && succesMonthly && succesShifts && succesPeriodDay && succesDynamicDay) await AddGenerationLogEvent(eventId: 200, message: string.Format("SUCCES GENERATION FOR {0} | {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")), eventName: string.Concat("GENERATION_SUCCES_", companyId));
            return (succesOneTimeOnly && succesWeekly && succesMonthly && succesShifts && succesPeriodDay && succesDynamicDay);
        }

        /// <summary>
        /// GenerateAll; Will loop through all companies and generate all tasks for a company. In sequence the OneTimeOnly, Weekly, Monthly and Shift tasks will be generated.
        /// </summary>
        /// <returns>boolean, true/false depending on success.</returns>
        public async Task<bool> GenerateAll(CancellationToken stoppingToken)
        {
            var succes = true;
            await AddGenerationLogEvent(eventId: 921, message: string.Format("START GENERATION [generate_all] | {0}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            var companies = await GetGenerationCompaniesAsync();
            //TODO create call for specific generation companies (demo's and tests do not need to be generated all the time. Make exclude structure in db) for now use config
            foreach (var c in companies)
            {
                if(!stoppingToken.IsCancellationRequested)
                {
                    //only run for active companies.
                    if(await CheckGenerationCompany(c.Id)) {
                        var succesCompany = await GenerateAllCompany(companyId: c.Id, stoppingToken: stoppingToken);
                        if (!succesCompany)
                        {
                            succes = false; //set to false, something in company is not correctly processed.
                            await AddGenerationLogEvent(eventId: 921, message: string.Format("GENERATION ISSUE [generate_all] FOR {0} | {1}", c.Id, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
                        }
                        await Task.Delay(2000); //wait 2 seconds.
                    }
                }
            }
            await AddGenerationLogEvent(eventId: 922, message: string.Format("END GENERATION [generate_all] | {0}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            return succes;//Todo add somekind of check.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<string> GetTemplateRecurrencyTypeBasedOnTask(int companyId, int taskId)
        {
            var output = string.Empty;

            try
            {
                var parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_taskid", taskId));

                var possibleType = await _dataManager.ExecuteScalarAsync("generate_tasks_retrieve_type", parameters: parameters, connectionKind: Data.Enumerations.ConnectionKind.Reader);
                if (possibleType != DBNull.Value && possibleType != null)
                {
                    output = (string)possibleType;
                }

            } catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.GetTemplateRecurrencyTypeBasedOnTask(): ", ex.Message));
            }
            

            return output;
        }

        /// <summary>
        /// CheckGenerationCompany; Checks company id against generation company ids
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>true/false if company id is found.</returns>
        public async Task<bool> CheckGenerationCompany(int companyId)
        {

            var activeCompanies = await GenerationCompanyIds(); 
            return activeCompanies.Contains(companyId);
        }

        /// <summary>
        /// GetRunnableHours; Get runnable hours for task generation.
        /// </summary>
        /// <returns>list of runnable hours</returns>
        public async Task<List<int>> GetRunnableHours()
        {
            var output = new List<int>();

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", "TECH_TASKGENERATION_HOURS"));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dataManager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var nrs = dr["settingvalue"].ToString();
                            if (nrs != null)
                            {
                                output = nrs.Split(",").Where(x => int.TryParse(x, out int num)).Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.GetRunnableHours(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// GetRunnableMinutes; Get runnable minutes for task generation.
        /// </summary>
        /// <returns>List of minutes.</returns>
        public async Task<List<int>> GetRunnableMinutes()
        {
            var output = new List<int>();

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", "TECH_TASKGENERATION_MINUTES"));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dataManager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var nrs = dr["settingvalue"].ToString();
                            if (nrs != null)
                            {
                                output = nrs.Split(",").Where(x => int.TryParse(x, out int num)).Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.GetRunnableMinutes(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// GetRunnableType; Get runnable type for generation.
        /// </summary>
        /// <returns>String containing the type of generation.</returns>
        public async Task<string> GetRunnableType()
        {
            var output = string.Empty;

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", "TECH_TASKGENERATION_RUNTYPE"));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dataManager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var runtype = dr["settingvalue"].ToString();
                            if (runtype != null)
                            {
                                output = runtype;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.GetRunnableType(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// PreGenerationCleanup; cleanup tasks when certain changes are made to the template. 
        /// </summary>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <param name="templateId">Optional for running generation for an specific tasktemplate.</param>
        /// <returns></returns>
        private async Task<bool> PreGenerationCleanup(int companyId, int templateId)
        {
            var output = false;
            if(companyId > 0 && templateId > 0)
            {
                var parameters = _dataManager.GetBaseParameters(companyId: companyId);
                object numberOfItems = null;
                parameters.Add(new NpgsqlParameter("@_templateid", templateId));
                await AddGenerationLogEvent(eventId: 932, message: string.Format("SPECIFIC TEMPLATE CLEANUP {0} FOR {1} | {2}", templateId, companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));

                try
                {
                    numberOfItems = (int)await _dataManager.ExecuteScalarAsync("generate_tasks_pre_generation_cleanup", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    _logger.LogError(eventId: 999, exception: ex, "EXCEPTION OCCURRED: {0} [{1}] FOR {2} | {3}", ex.Message, "generation_cleanup_template", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"));

                }
                if(numberOfItems != null)
                {
                    output = true;
                }
            }
            return output;
        }


        /// <summary>
        /// GenerateCall; Start generation process based on a StoredProcedure and CompanyId
        /// </summary>
        /// <param name="storedProcedureName">Name of the StoredProcedure that needs to be called.</param>
        /// <param name="companyId">The companyId where tasks need to be generated.</param>
        /// <param name="templateId">Optional for running generation for an specific tasktemplate.</param>
        /// <returns>boolean, true/false depending on success.</returns>
        private async Task<bool> GenerateCall(string storedProcedureName, int companyId, int? templateId = null, int? taskId = null)
        {
            //TODO create own logging tables.
            await AddGenerationLogEvent(eventId:901, message:string.Format("START GENERATION [{0}] FOR {1} | {2}", storedProcedureName, companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            var parameters = _dataManager.GetBaseParameters(companyId: companyId);
            object numberOfItems = null;
            if (templateId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_templateid", templateId.Value));
                await AddGenerationLogEvent(eventId: 931, message: string.Format("SPECIFIC TEMPLATE {0} FOR {1} | {2}", templateId.Value, companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            if (taskId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_taskid", taskId.Value));
                await AddGenerationLogEvent(eventId: 931, message: string.Format("SPECIFIC TEMPLATE BASED ON TASK {0} FOR {1} | {2}", taskId.Value, companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            }
            try
            {
                numberOfItems = await _dataManager.ExecuteScalarAsync(storedProcedureName, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            } catch (Exception ex)
            {
                _logger.LogError(eventId: 999, exception:ex, "EXCEPTION OCCURRED: {0} [{1}] FOR {2} | {3}", ex.Message, storedProcedureName, companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"));

            }
            await AddGenerationLogEvent(eventId: 902, message: string.Format("END GENERATION [{0}]({1}) FOR {2} | {3}", storedProcedureName, numberOfItems, companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")));
            return numberOfItems == null ? false : true;

        }

        /// <summary>
        /// GetGenerationCompaniesAsync; Get compannies for generation.
        /// </summary>
        /// <returns>A list of companies to use for generation.</returns>
        private async Task<List<Company>> GetGenerationCompaniesAsync()
        {
            var output = new List<Company>();

            NpgsqlDataReader dr = null;

            try
            {
                using (dr = await _dataManager.GetDataReader("get_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var company = CreateOrFillGenerationCompanyFromReader(dr);
                        output.Add(company);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.GetGenerationCompaniesAsync(): ", ex.Message));
            }
            finally
            {

                if (dr != null)
                {
                    if (!dr.IsClosed) await dr.CloseAsync();
                    await dr.DisposeAsync();
                }

            }
            return output;
        }

        /// <summary>
        /// GenerationCompanyIds; Get a list of company ids to start sync,
        /// </summary>
        /// <returns>A list of generation ids</returns>
        private async Task<List<int>> GenerationCompanyIds()
        {
            var output = new List<int>();
            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", "TECH_TASKGENERATION"));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dataManager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if(dr["settingvalue"] != DBNull.Value)
                        {
                            var companyIds = dr["settingvalue"].ToString();
                            if (companyIds != null)
                            {
                                output = companyIds.Split(",").Where(x => int.TryParse(x, out int num)).Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.GenerationCompanyIds(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// CreateOrFillGenerationCompanyFromReader; creates and fills a Company object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="company">Company object containing all data needed for updating the database. (DB: companies_company)</param>
        /// <returns>A filled Company object.</returns>
        private Company CreateOrFillGenerationCompanyFromReader(NpgsqlDataReader dr, Company company = null)
        {
            if (company == null) company = new Company();
            company.Id = Convert.ToInt32(dr["id"]);
            company.Name = dr["name"].ToString();
            return company;
        }

        /// <summary>
        /// AddGenerationLogEvent; Add a generation event to the database. 
        /// </summary>
        public async Task<bool> AddGenerationLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "")
        {

            if (_configHelper.GetValueAsBool(ApiSettings.ENABLE_DB_LOG_CONFIG_KEY))
            {
                try
                {
                    var source = _configHelper.GetValueAsString(ApiSettings.APPLICATION_NAME_CONFIG_KEY);

                    var parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventId.ToString()));
                    parameters.Add(new NpgsqlParameter("@_eventname", eventName));
                    parameters.Add(new NpgsqlParameter("@_source", source));
                    parameters.Add(new NpgsqlParameter("@_description", string.Empty));

                    var output = await _dataManager.ExecuteScalarAsync("add_log_generation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    _logger.LogError(exception: ex, message: string.Concat("TaskGenerationManager.AddGenerationLogEvent(): ", message, " - ", ex.Message));
                }
                finally
                {

                }
            }
            return true;
        }
    }
}
