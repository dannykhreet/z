using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EZGO.Api.Models.Skills;
using EZGO.Api.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Repository.Implementations
{
    /// <summary>
    /// Repository implementation for Skills Matrix Legend Configuration operations
    /// Uses PostgreSQL compatible SQL syntax
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
                const string configSql = @"
                    SELECT id AS ""Id"", company_id AS ""CompanyId"", version AS ""Version"",
                           created_at AS ""CreatedAt"", updated_at AS ""UpdatedAt"",
                           created_by AS ""CreatedBy"", updated_by AS ""UpdatedBy""
                    FROM skill_matrix_legend_configuration
                    WHERE company_id = @CompanyId";

                var configuration = await _connection.QueryFirstOrDefaultAsync<SkillMatrixLegendConfiguration>(
                    configSql, new { CompanyId = companyId });

                if (configuration == null)
                {
                    return null;
                }

                const string itemsSql = @"
                    SELECT id AS ""Id"", configuration_id AS ""ConfigurationId"",
                           skill_level_id AS ""SkillLevelId"", skill_type AS ""SkillType"",
                           label AS ""Label"", description AS ""Description"",
                           icon_color AS ""IconColor"", background_color AS ""BackgroundColor"",
                           sort_order AS ""Order"", score_value AS ""ScoreValue"",
                           icon_class AS ""IconClass"", is_default AS ""IsDefault"",
                           created_at AS ""CreatedAt"", updated_at AS ""UpdatedAt""
                    FROM skill_matrix_legend_item
                    WHERE configuration_id = @ConfigurationId
                    ORDER BY skill_type, sort_order";

                var items = await _connection.QueryAsync<SkillMatrixLegendItem>(
                    itemsSql, new { ConfigurationId = configuration.Id });

                var itemsList = items.ToList();
                configuration.MandatorySkills = itemsList.Where(i => i.SkillType == "mandatory").ToList();
                configuration.OperationalSkills = itemsList.Where(i => i.SkillType == "operational").ToList();

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
                const string insertConfigSql = @"
                    INSERT INTO skill_matrix_legend_configuration
                        (company_id, version, created_at, created_by)
                    VALUES
                        (@CompanyId, @Version, @CreatedAt, @CreatedBy)
                    RETURNING id";

                configuration.CreatedAt = DateTime.UtcNow;
                configuration.Id = await _connection.ExecuteScalarAsync<int>(insertConfigSql, new
                {
                    configuration.CompanyId,
                    configuration.Version,
                    configuration.CreatedAt,
                    configuration.CreatedBy
                });

                // Insert all items
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
                const string updateConfigSql = @"
                    UPDATE skill_matrix_legend_configuration
                    SET version = @Version,
                        updated_at = @UpdatedAt,
                        updated_by = @UpdatedBy
                    WHERE company_id = @CompanyId";

                configuration.UpdatedAt = DateTime.UtcNow;
                await _connection.ExecuteAsync(updateConfigSql, new
                {
                    configuration.CompanyId,
                    configuration.Version,
                    configuration.UpdatedAt,
                    configuration.UpdatedBy
                });

                // Delete existing items and insert new ones
                const string deleteItemsSql = @"
                    DELETE FROM skill_matrix_legend_item
                    WHERE configuration_id = (
                        SELECT id FROM skill_matrix_legend_configuration WHERE company_id = @CompanyId
                    )";

                await _connection.ExecuteAsync(deleteItemsSql, new { configuration.CompanyId });

                // Get the configuration ID
                const string getIdSql = "SELECT id FROM skill_matrix_legend_configuration WHERE company_id = @CompanyId";
                configuration.Id = await _connection.ExecuteScalarAsync<int>(getIdSql, new { configuration.CompanyId });

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
                const string updateItemSql = @"
                    UPDATE skill_matrix_legend_item
                    SET label = @Label,
                        description = @Description,
                        icon_color = @IconColor,
                        background_color = @BackgroundColor,
                        sort_order = @Order,
                        score_value = @ScoreValue,
                        icon_class = @IconClass,
                        is_default = false,
                        updated_at = @UpdatedAt
                    WHERE configuration_id = (
                        SELECT id FROM skill_matrix_legend_configuration WHERE company_id = @CompanyId
                    )
                    AND skill_level_id = @SkillLevelId
                    AND skill_type = @SkillType";

                item.UpdatedAt = DateTime.UtcNow;
                item.IsDefault = false;

                await _connection.ExecuteAsync(updateItemSql, new
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
                    item.IconClass,
                    item.UpdatedAt
                });

                // Update configuration version
                const string updateVersionSql = @"
                    UPDATE skill_matrix_legend_configuration
                    SET version = version + 1, updated_at = @UpdatedAt
                    WHERE company_id = @CompanyId";

                await _connection.ExecuteAsync(updateVersionSql, new { CompanyId = companyId, item.UpdatedAt });

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
                const string deleteItemsSql = @"
                    DELETE FROM skill_matrix_legend_item
                    WHERE configuration_id = (
                        SELECT id FROM skill_matrix_legend_configuration WHERE company_id = @CompanyId
                    )";

                const string deleteConfigSql = @"
                    DELETE FROM skill_matrix_legend_configuration
                    WHERE company_id = @CompanyId";

                await _connection.ExecuteAsync(deleteItemsSql, new { CompanyId = companyId });
                var rows = await _connection.ExecuteAsync(deleteConfigSql, new { CompanyId = companyId });

                return rows > 0;
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
            const string sql = "SELECT COUNT(1) FROM skill_matrix_legend_configuration WHERE company_id = @CompanyId";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
            return count > 0;
        }

        /// <inheritdoc/>
        public async Task<int> GetVersionAsync(int companyId)
        {
            const string sql = "SELECT COALESCE(version, 0) FROM skill_matrix_legend_configuration WHERE company_id = @CompanyId";
            return await _connection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
        }

        /// <summary>
        /// Inserts legend items for a configuration
        /// </summary>
        private async Task InsertItemsAsync(SkillMatrixLegendConfiguration configuration)
        {
            const string insertItemSql = @"
                INSERT INTO skill_matrix_legend_item
                    (configuration_id, skill_level_id, skill_type, label, description,
                     icon_color, background_color, sort_order, score_value, icon_class, is_default, created_at)
                VALUES
                    (@ConfigurationId, @SkillLevelId, @SkillType, @Label, @Description,
                     @IconColor, @BackgroundColor, @Order, @ScoreValue, @IconClass, @IsDefault, @CreatedAt)";

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

                await _connection.ExecuteAsync(insertItemSql, new
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
                    item.IsDefault,
                    item.CreatedAt
                });
            }
        }
    }
}
