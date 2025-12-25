using System.Threading.Tasks;
using EZGO.Api.Models.Skills;

namespace EZGO.Api.Managers
{
    /// <summary>
    /// Interface for Skills Matrix Legend business logic operations
    /// </summary>
    public interface ISkillMatrixLegendManager
    {
        /// <summary>
        /// Gets the legend configuration for a company
        /// Returns default configuration if none exists
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>The legend configuration</returns>
        Task<SkillMatrixLegendConfiguration> GetConfigurationAsync(int companyId);

        /// <summary>
        /// Saves or updates the legend configuration for a company
        /// </summary>
        /// <param name="configuration">The configuration to save</param>
        /// <param name="userId">The user ID making the change</param>
        /// <returns>The saved configuration with updated version</returns>
        Task<SkillMatrixLegendConfiguration> SaveConfigurationAsync(SkillMatrixLegendConfiguration configuration, int userId);

        /// <summary>
        /// Updates a single legend item
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="item">The item to update</param>
        /// <returns>The updated item</returns>
        Task<SkillMatrixLegendItem> UpdateItemAsync(int companyId, SkillMatrixLegendItem item);

        /// <summary>
        /// Resets the legend configuration to default values
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="userId">The user ID making the change</param>
        /// <returns>The reset configuration</returns>
        Task<SkillMatrixLegendConfiguration> ResetToDefaultAsync(int companyId, int userId);

        /// <summary>
        /// Validates a legend configuration
        /// </summary>
        /// <param name="configuration">The configuration to validate</param>
        /// <returns>Validation result with any errors</returns>
        (bool IsValid, string[] Errors) ValidateConfiguration(SkillMatrixLegendConfiguration configuration);

        /// <summary>
        /// Validates a single legend item
        /// </summary>
        /// <param name="item">The item to validate</param>
        /// <returns>Validation result with any errors</returns>
        (bool IsValid, string[] Errors) ValidateItem(SkillMatrixLegendItem item);
    }
}
