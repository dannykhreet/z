using System.Threading.Tasks;
using EZGO.Api.Models.Skills;

namespace EZGO.Api.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for Skills Matrix Legend Configuration operations
    /// </summary>
    public interface ISkillMatrixLegendRepository
    {
        /// <summary>
        /// Gets the legend configuration for a specific company
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>The configuration or null if not found</returns>
        Task<SkillMatrixLegendConfiguration> GetByCompanyIdAsync(int companyId);

        /// <summary>
        /// Creates a new legend configuration
        /// </summary>
        /// <param name="configuration">The configuration to create</param>
        /// <returns>The created configuration with ID</returns>
        Task<SkillMatrixLegendConfiguration> CreateAsync(SkillMatrixLegendConfiguration configuration);

        /// <summary>
        /// Updates an existing legend configuration
        /// </summary>
        /// <param name="configuration">The configuration to update</param>
        /// <returns>The updated configuration</returns>
        Task<SkillMatrixLegendConfiguration> UpdateAsync(SkillMatrixLegendConfiguration configuration);

        /// <summary>
        /// Updates a single legend item
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="item">The item to update</param>
        /// <returns>The updated item</returns>
        Task<SkillMatrixLegendItem> UpdateItemAsync(int companyId, SkillMatrixLegendItem item);

        /// <summary>
        /// Deletes the legend configuration for a company
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteByCompanyIdAsync(int companyId);

        /// <summary>
        /// Checks if a configuration exists for a company
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>True if exists</returns>
        Task<bool> ExistsAsync(int companyId);

        /// <summary>
        /// Gets the current version number for a company's configuration
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>The version number or 0 if not found</returns>
        Task<int> GetVersionAsync(int companyId);
    }
}
