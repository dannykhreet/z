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
                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", companyId, DbType.Int32);

                var result = await _connection.QueryFirstOrDefaultAsync<FullConfigurationResult>(
                    "sp_get_skill_matrix_legend_full",
                    parameters,
                    commandType: CommandType.StoredProcedure);

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
                configuration.CreatedAt = DateTime.UtcNow;

                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", configuration.CompanyId, DbType.Int32);
                parameters.Add("p_version", configuration.Version, DbType.Int32);
                parameters.Add("p_created_by", configuration.CreatedBy, DbType.Int32);

                configuration.Id = await _connection.ExecuteScalarAsync<int>(
                    "sp_insert_skill_matrix_legend_configuration",
                    parameters,
                    commandType: CommandType.StoredProcedure);

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
                configuration.UpdatedAt = DateTime.UtcNow;

                var updateParams = new DynamicParameters();
                updateParams.Add("p_company_id", configuration.CompanyId, DbType.Int32);
                updateParams.Add("p_version", configuration.Version, DbType.Int32);
                updateParams.Add("p_updated_by", configuration.UpdatedBy, DbType.Int32);

                await _connection.ExecuteAsync(
                    "sp_update_skill_matrix_legend_configuration",
                    updateParams,
                    commandType: CommandType.StoredProcedure);

                // Delete existing items using stored procedure
                var deleteParams = new DynamicParameters();
                deleteParams.Add("p_company_id", configuration.CompanyId, DbType.Int32);

                await _connection.ExecuteAsync(
                    "sp_delete_skill_matrix_legend_items_by_company",
                    deleteParams,
                    commandType: CommandType.StoredProcedure);

                // Get the configuration ID using stored procedure
                var getIdParams = new DynamicParameters();
                getIdParams.Add("p_company_id", configuration.CompanyId, DbType.Int32);

                configuration.Id = await _connection.ExecuteScalarAsync<int>(
                    "sp_get_skill_matrix_legend_configuration_id",
                    getIdParams,
                    commandType: CommandType.StoredProcedure);

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
                item.UpdatedAt = DateTime.UtcNow;
                item.IsDefault = false;

                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", companyId, DbType.Int32);
                parameters.Add("p_skill_level_id", item.SkillLevelId, DbType.Int32);
                parameters.Add("p_skill_type", item.SkillType, DbType.String);
                parameters.Add("p_label", item.Label, DbType.String);
                parameters.Add("p_description", item.Description, DbType.String);
                parameters.Add("p_icon_color", item.IconColor, DbType.String);
                parameters.Add("p_background_color", item.BackgroundColor, DbType.String);
                parameters.Add("p_sort_order", item.Order, DbType.Int32);
                parameters.Add("p_score_value", item.ScoreValue, DbType.Int32);
                parameters.Add("p_icon_class", item.IconClass, DbType.String);

                await _connection.ExecuteAsync(
                    "sp_update_skill_matrix_legend_item",
                    parameters,
                    commandType: CommandType.StoredProcedure);

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
                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", companyId, DbType.Int32);

                var result = await _connection.ExecuteScalarAsync<bool>(
                    "sp_delete_skill_matrix_legend_configuration",
                    parameters,
                    commandType: CommandType.StoredProcedure);

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
            var parameters = new DynamicParameters();
            parameters.Add("p_company_id", companyId, DbType.Int32);

            var exists = await _connection.ExecuteScalarAsync<bool>(
                "sp_exists_skill_matrix_legend_configuration",
                parameters,
                commandType: CommandType.StoredProcedure);

            return exists;
        }

        /// <inheritdoc/>
        public async Task<int> GetVersionAsync(int companyId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_company_id", companyId, DbType.Int32);

            var version = await _connection.ExecuteScalarAsync<int>(
                "sp_get_skill_matrix_legend_version",
                parameters,
                commandType: CommandType.StoredProcedure);

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

                var parameters = new DynamicParameters();
                parameters.Add("p_configuration_id", item.ConfigurationId, DbType.Int32);
                parameters.Add("p_skill_level_id", item.SkillLevelId, DbType.Int32);
                parameters.Add("p_skill_type", item.SkillType, DbType.String);
                parameters.Add("p_label", item.Label, DbType.String);
                parameters.Add("p_description", item.Description, DbType.String);
                parameters.Add("p_icon_color", item.IconColor, DbType.String);
                parameters.Add("p_background_color", item.BackgroundColor, DbType.String);
                parameters.Add("p_sort_order", item.Order, DbType.Int32);
                parameters.Add("p_score_value", item.ScoreValue, DbType.Int32);
                parameters.Add("p_icon_class", item.IconClass, DbType.String);
                parameters.Add("p_is_default", item.IsDefault, DbType.Boolean);

                await _connection.ExecuteAsync(
                    "sp_insert_skill_matrix_legend_item",
                    parameters,
                    commandType: CommandType.StoredProcedure);
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
