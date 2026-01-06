using EZ.Connector.Init.Interfaces;
using EZ.Connector.Init.Managers;
using EZ.Connector.SAP.Interfaces;
using EZ.Connector.SAP.Managers;
using EZ.Connector.Ultimo.Interfaces;
using EZ.Connector.Ultimo.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Init.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add logic services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        public static void AddConnectorServices(this IServiceCollection services)
        {
            services.AddScoped<IConnectorManager, ConnectorManager>(); //external connector.
            services.AddScoped<ISAPConnector, SAPConnector>(); //external EZ.Connector.SAP
            services.AddScoped<IUltimoConnector, UltimoConnector>(); //external EZ.Connector.Ultimo
        }
    }
}
