using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Logic.Templates.Base
{
    /// <summary>
    /// BaseManager; base manager, must be used with all manager within the API Logic (only do not use if there is a specif reason);
    /// Logger is used as a service (direct injection).
    /// NOTE! depending on implementations this base-class will be extended with more default properties / constructor(s).
    /// </summary>
    /// <typeparam name="T">Type of manager.</typeparam>
    public class BaseManager<T>
    {
        /// <summary>
        /// Logger will be implemented based on the standard .net core logger; 
        /// Depending on type of logger and provider used this will output to the output logging stream and database (custom logger)
        /// </summary>
        protected readonly ILogger<T> _logger;

        public BaseManager(ILogger<T> logger)
        {
            this._logger = logger;
        }
    }
}
