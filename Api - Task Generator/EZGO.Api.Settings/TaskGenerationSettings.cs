using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    public static class TaskGenerationSettings
    {
        /// <summary>
        /// RUNNABLE_TIME_NUMBER_CONFIG_KEY; Config key reference: comma separated list of time numbers (e.g. minutes or hours) where generation of tasks must occur.
        /// </summary>
        public const string RUNNABLE_TIME_NUMBER_CONFIG_KEY = "AppSettings:RunableTimeNumber";
        /// <summary>
        /// RUNNABLE_TIME_TYPE_CONFIG_KEY;  Config key reference: the runnable type (minute, hour); Task generation can run on every specific minute or hour located in the RunableTimeNumber;
        /// </summary>
        public const string RUNNABLE_TIME_TYPE_CONFIG_KEY = "AppSettings:RunableTimeType";
        /// <summary>
        /// TASKGENERATION_ACTIVE_CONFIG_KEY;  Config key reference: Generation active, true/false if generation process is active; (used in WS)
        /// </summary>
        public const string TASKGENERATION_ACTIVE_CONFIG_KEY = "AppSettings:GenerationActive";
        /// <summary>
        /// TASKGENERATION_AUTOMATED_LOOP_TIMEOUT_CONFIG_KEY;  Config key reference: timeout number for every run used within the WorkerService.
        /// </summary>
        public const string TASKGENERATION_AUTOMATED_LOOP_TIMEOUT_CONFIG_KEY = "AppSettings:GenerationLoopTimeInMS";
        /// <summary>
        /// TASKGENERATION_GENERATE_COMPANIES_CONFIG_KEY; Config key reference: active companies (comma separated list of ids) where task generation must occur for.
        /// </summary>
        public const string TASKGENERATION_GENERATE_COMPANIES_CONFIG_KEY = "AppSettings:GenerateForCompanies";
    }
}
