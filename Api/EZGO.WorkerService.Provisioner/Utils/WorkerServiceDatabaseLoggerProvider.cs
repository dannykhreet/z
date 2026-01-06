using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EZGO.WorkerService.Provisioner.Utils
{
    /// <summary>
    /// WorkerServiceDatabaseLoggerProvider
    /// NOTE! only for use with worker service!!
    /// </summary>
    public class WorkerServiceDatabaseLoggerProvider : ILoggerProvider
    {
        ILogger _logger;
        IConfiguration _config;
        private bool _disposed = false;

        public WorkerServiceDatabaseLoggerProvider(IConfiguration configuration)
        {
            _config = configuration;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (null == _logger)
            {
                //Add configuration, for now always enabled.

                _logger = new WorkerServiceDatabaseLogger(_config);
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
