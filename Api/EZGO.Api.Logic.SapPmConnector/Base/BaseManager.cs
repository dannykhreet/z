using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.SapPmConnector.Base
{
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
