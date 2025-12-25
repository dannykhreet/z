using EZGO.Api.Managers;
using EZGO.Api.Repository.Implementations;
using EZGO.Api.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EZGO.Api.Extensions
{
    /// <summary>
    /// Extension methods for registering Skills Matrix Legend services
    /// </summary>
    public static class SkillMatrixLegendServiceExtensions
    {
        /// <summary>
        /// Adds Skills Matrix Legend services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSkillMatrixLegendServices(this IServiceCollection services)
        {
            // Register repository
            services.AddScoped<ISkillMatrixLegendRepository, SkillMatrixLegendRepository>();

            // Register manager
            services.AddScoped<ISkillMatrixLegendManager, SkillMatrixLegendManager>();

            return services;
        }
    }
}
