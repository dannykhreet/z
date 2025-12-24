using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using EZGO.Api.Interfaces.Data;

//TODO determan if logger needs to be moved to other dll due to references.
namespace EZGO.Api.Utils.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        ILogger _logger;
        IConfiguration _config;
        IDatabaseLogWriter _logwriter;
        private bool _disposed = false;

        public DatabaseLoggerProvider(IConfiguration configuration, IDatabaseLogWriter logwriter)
        {
            _config = configuration;
            _logwriter = logwriter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (null == _logger)
            {
                //Add configuration, for now always enabled.

                _logger = new DatabaseLogger(_logwriter);
            }

            return _logger;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
