using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EZGO.Api.Interfaces.Provisioner;
using EZGO.Api.Interfaces.SapPmConnector;
using Microsoft.Extensions.DependencyInjection;

namespace EZGO.Api.Logic.SapPmConnector.Helpers
{
    public static class StartupHelperExtension
    {
        public static void AddSapPMConnectionServices(this IServiceCollection services)
        {
            services.AddScoped<ISapPmConnectionManager, SapPmConnectionManager>();
        }

    }
}
