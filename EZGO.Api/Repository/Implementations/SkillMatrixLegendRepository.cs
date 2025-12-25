using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EZGO.Api.Models.Skills;
using EZGO.Api.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EZGO.Api.Repository.Implementations
{
    /// <summary>
    /// Repository implementation for Skills Matrix Legend Configuration operations
    /// Uses PostgreSQL stored procedures for all database operations
    /// </summary>
    public class SkillMatrixLegendRepository : ISkillMatrixLegendRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<SkillMatrixLegendRepository> _logger;

        public SkillMatrixLegendRepository(IDbConnection connection, ILogger<SkillMatrixLegendRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                // Call stored procedure to get full configuration with items
                var result = await _connection.QueryFirstOrDefaultAsync<FullConfigurationResult>(
                    "SELECT * FROM sp_get_skill_matrix_legend_full(@CompanyId)",
                    new { CompanyId = companyId });

                if (result == null || result.config_id == null)
                {
                    return null;
                }

                var configuration = new SkillMatrixLegendConfiguration
                {
                    Id = result.config_id.Value,
                    CompanyId = result.config_company_id ?? companyId,
                    Version = result.config_version ?? 1,
                    CreatedAt = result.config_created_at ?? DateTime.UtcNow,
                    UpdatedAt = result.config_updated_at,
                    CreatedBy = result.config_created_by,
                    UpdatedBy = result.config_updated_by
                };

                // Parse items from JSON
                if (!string.IsNullOrEmpty(result.items_json))
                {
                    var items = JsonConvert.DeserializeObject<List<SkillMatrixLegendItem>>(result.items_json);
                    configuration.MandatorySkills = items?.Where(i => i.SkillType == "mandatory").ToList() ?? new List<SkillMatrixLegendItem>();
                    configuration.OperationalSkills = items?.Where(i => i.SkillType == "operational").ToList() ?? new List<SkillMatrixLegendItem>();
                }
                else
                {
                    configuration.MandatorySkills = new List<SkillMatrixLegendItem>();
                    configuration.OperationalSkills = new List<SkillMatrixLegendItem>();
                }

                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting legend configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> CreateAsync(SkillMatrixLegendConfiguration configuration)
        {
            try
            {
                // Call stored procedure to insert configuration
                configuration.CreatedAt = DateTime.UtcNow;
                configuration.Id = await _connection.ExecuteScalarAsync<int>(
                    "SELECT sp_insert_skill_matrix_legend_configuration(@CompanyId, @Version, @CreatedBy)",
                    new
                    {
                        configuration.CompanyId,
                        configuration.Version,
                        configuration.CreatedBy
                    });

                // Insert all items using stored procedure
                await InsertItemsAsync(configuration);

                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating legend configuration for company {CompanyId}", configuration.CompanyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> UpdateAsync(SkillMatrixLegendConfiguration configuration)
        {
            try
            {
                // Call stored procedure to update configuration
                configuration.UpdatedAt = DateTime.UtcNow;
                await _connection.ExecuteAsync(
                    "SELECT sp_update_skill_matrix_legend_configuration(@CompanyId, @Version, @UpdatedBy)",
                    new
                    {
                        configuration.CompanyId,
                        configuration.Version,
                        configuration.UpdatedBy
                    });

                // Delete existing items using stored procedure
                await _connection.ExecuteAsync(
                    "SELECT sp_delete_skill_matrix_legend_items_by_company(@CompanyId)",
                    new { configuration.CompanyId });

                // Get the configuration ID using stored procedure
                configuration.Id = await _connection.ExecuteScalarAsync<int>(
                    "SELECT sp_get_skill_matrix_legend_configuration_id(@CompanyId)",
                    new { configuration.CompanyId });

                // Insert new items
                await InsertItemsAsync(configuration);

                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legend configuration for company {CompanyId}", configuration.CompanyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendItem> UpdateItemAsync(int companyId, SkillMatrixLegendItem item)
        {
            try
            {
                // Call stored procedure to update single item
                item.UpdatedAt = DateTime.UtcNow;
                item.IsDefault = false;

                await _connection.ExecuteAsync(
                    @"SELECT sp_update_skill_matrix_legend_item(
                        @CompanyId, @SkillLevelId, @SkillType, @Label, @Description,
                        @IconColor, @BackgroundColor, @Order, @ScoreValue, @IconClass)",
                    new
                    {
                        CompanyId = companyId,
                        item.SkillLevelId,
                        item.SkillType,
                        item.Label,
                        item.Description,
                        item.IconColor,
                        item.BackgroundColor,
                        item.Order,
                        item.ScoreValue,
                        item.IconClass
                    });

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legend item for company {CompanyId}", companyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByCompanyIdAsync(int companyId)
        {
            try
            {
                // Call stored procedure to delete configuration (cascades to items)
                var result = await _connection.ExecuteScalarAsync<bool>(
                    "SELECT sp_delete_skill_matrix_legend_configuration(@CompanyId)",
                    new { CompanyId = companyId });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting legend configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int companyId)
        {
            // Call stored procedure to check existence
            var exists = await _connection.ExecuteScalarAsync<bool>(
                "SELECT sp_exists_skill_matrix_legend_configuration(@CompanyId)",
                new { CompanyId = companyId });
            return exists;
        }

        /// <inheritdoc/>
        public async Task<int> GetVersionAsync(int companyId)
        {
            // Call stored procedure to get version
            var version = await _connection.ExecuteScalarAsync<int>(
                "SELECT sp_get_skill_matrix_legend_version(@CompanyId)",
                new { CompanyId = companyId });
            return version;
        }

        /// <summary>
        /// Inserts legend items using stored procedure
        /// </summary>
        private async Task InsertItemsAsync(SkillMatrixLegendConfiguration configuration)
        {
            var allItems = new List<SkillMatrixLegendItem>();
            if (configuration.MandatorySkills != null)
            {
                allItems.AddRange(configuration.MandatorySkills);
            }
            if (configuration.OperationalSkills != null)
            {
                allItems.AddRange(configuration.OperationalSkills);
            }

            foreach (var item in allItems)
            {
                item.ConfigurationId = configuration.Id;
                item.CreatedAt = DateTime.UtcNow;

                // Call stored procedure to insert item
                await _connection.ExecuteAsync(
                    @"SELECT sp_insert_skill_matrix_legend_item(
                        @ConfigurationId, @SkillLevelId, @SkillType, @Label, @Description,
                        @IconColor, @BackgroundColor, @Order, @ScoreValue, @IconClass, @IsDefault)",
                    new
                    {
                        item.ConfigurationId,
                        item.SkillLevelId,
                        item.SkillType,
                        item.Label,
                        item.Description,
                        item.IconColor,
                        item.BackgroundColor,
                        item.Order,
                        item.ScoreValue,
                        item.IconClass,
                        item.IsDefault
                    });
            }
        }

        /// <summary>
        /// Result class for the full configuration stored procedure
        /// </summary>
        private class FullConfigurationResult
        {
            public int? config_id { get; set; }
            public int? config_company_id { get; set; }
            public int? config_version { get; set; }
            public DateTime? config_created_at { get; set; }
            public DateTime? config_updated_at { get; set; }
            public int? config_created_by { get; set; }
            public int? config_updated_by { get; set; }
            public string items_json { get; set; }
        }
    }
}
