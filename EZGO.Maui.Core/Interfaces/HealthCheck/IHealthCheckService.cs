using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.HealthCheck
{
    public interface IHealthCheckService : IDisposable
    {
        Task<bool> ValidateTokenAsync(string token);
    }
}
